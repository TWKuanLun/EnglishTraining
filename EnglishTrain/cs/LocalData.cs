using NSoup;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;

namespace EnglishTrain.cs
{
    static class LocalData
    {
        static string CurrentPath = Directory.GetCurrentDirectory();
        /// <summary>例句資料庫，給key(單字)獲得例句</summary>
        public static Dictionary<string, List<Sentence>> Sentances = new Dictionary<string, List<Sentence>>();
        /// <summary>單字資料庫，給key(單字)獲得單字資料</summary>
        public static Dictionary<string, Word> Words = new Dictionary<string, Word>();

        public enum AddStatus
        {
            Success, SearchFail, HaveWord
        }
        public static void initialization()
        {
            loadLocalData();
            getVerbLemmas();
        }
        /// <summary>新增單字，將單字、單字的例句加進本地資料，需搭配getHTML</summary>
        /// <param name="word">"單字</param>
        /// <param name="htmlstr">getHTML的結果</param>
        /// <param name="success">輸出結果，方便得知失敗原因，有Success或SearchFail或HaveWord</param>
        public static void addWordData(string word, string htmlstr, out AddStatus success)
        {
            if (!Words.Keys.Contains(word))//判斷資料庫內是否已經有該單字
            {
                var htmlDoc = NSoupClient.Parse(htmlstr);
                try
                {
                    var allBlock = htmlDoc.GetElementsByTag("ol").First(x => x.Attr("class") == "mb-15 reg searchCenterMiddle");
                    var meaningBlock = allBlock.GetElementsByTag("div").First(x => x.Attr("class") == "grp grp-tab-content-explanation tabsContent tab-content-explanation tabActived");

                    var phonetic = htmlDoc.GetElementsByTag("div").First(x => x.Attr("class") == "compList ml-25 d-ib").Text();
                    phonetic = phonetic.Replace('ˋ', '`');

                    var w = new Word(word, phonetic);//給word物件單字和音標
                    var sentences = new List<Sentence>();
                    
                    var rows = meaningBlock.GetElementsByTag("li").ToArray();
                    var sentencesByPos = new Dictionary<string, Dictionary<string, List<Sentence>>>();
                    string partOfSpeech = "";
                    List<string> chiMeaning = null;
                    for (int i = 0; i < rows.Length; i++)//詞性
                    {
                        var rowStr = rows[i].Text();
                        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\d+");
                        System.Text.RegularExpressions.Match match = regex.Match(rowStr);
                        if (match.Success)
                        {
                            //中文意思
                            var meaning = rows[i].GetElementsByTag("span").First().Text();
                            chiMeaning.Add(meaning);
                            if (!sentencesByPos[partOfSpeech].ContainsKey(meaning))
                            {
                                sentencesByPos[partOfSpeech].Add(meaning, new List<Sentence>());
                            }
                            var sentenceElements = rows[i].GetElementsByTag("p");
                            foreach (var sentenceElement in sentenceElements)
                            {
                                var sentence = sentenceElement.Text();
                                int firstChineseIndex = -1;
                                for (int j = 0; j < sentence.Length; j++)
                                {
                                    UnicodeCategory cat = char.GetUnicodeCategory(sentence[j]);
                                    if (cat == UnicodeCategory.OtherLetter)
                                    {
                                        firstChineseIndex = j;
                                        break;
                                    }
                                }
                                var engSentence = sentence.Substring(0, firstChineseIndex - 1);
                                var chiSentence = sentence.Substring(firstChineseIndex);
                                var s = new Sentence(chiSentence, engSentence, word, partOfSpeech, chiMeaning.Count - 1);
                                sentencesByPos[partOfSpeech][meaning].Add(s);
                                sentences.Add(s);
                            }
                        }
                        else
                        {
                            if(partOfSpeech != string.Empty)
                                w.chineseMeaning[partOfSpeech] = chiMeaning;
                            //詞性
                            partOfSpeech = rowStr;
                            sentencesByPos[partOfSpeech] = new Dictionary<string, List<Sentence>>();
                            chiMeaning = new List<string>();
                        }
                    }
                    w.chineseMeaning[partOfSpeech] = chiMeaning;
                    Sentances[word] = sentences;
                    Words[word] = w;
                    saveDataToLocal();//儲存資料在本地端
                    success = AddStatus.Success;
                }
                catch (Exception e)
                {
                    success = AddStatus.SearchFail;
                }
            }
            else
            {
                success = AddStatus.HaveWord;
            }
        }
        /// <summary>Get HTML Source Code</summary>
        public static string getHTML(string word)
        {
            var myRequest = WebRequest.Create(@"https://tw.dictionary.search.yahoo.com/search?p=" + word + "&fr2=dict");
            myRequest.Method = "GET";
            var myResponse = myRequest.GetResponse();
            var sr = new StreamReader(myResponse.GetResponseStream());
            string htmlSourceCode = sr.ReadToEnd();
            sr.Close();
            myResponse.Close();
            return htmlSourceCode;
        }

        private static Dictionary<string, string> verb_lemmas = new Dictionary<string, string>();//動詞型態字典
        private static void getVerbLemmas()//獲得動詞型態字典
        {
            #region getData
            string[] data;
            var verb_tenses = new Dictionary<string, string[]>();

            using (var sr = new StreamReader(CurrentPath + "\\verb.txt"))
            {
                string line = sr.ReadToEnd();
                data = line.Split(new char[] { '\n' });
            }
            for (int i = 0; i < data.Length; i++)
            {
                string[] a = data[i].Split(new char[] { ',' });
                verb_tenses[a[0]] = a;
            }
            foreach (var infinitive in verb_tenses)
            {
                foreach (string tense in verb_tenses[infinitive.Key])
                {
                    if (!tense.Equals(""))
                    {
                        verb_lemmas[tense] = infinitive.Key;
                    }
                }
            }
            #endregion
        }
        /// <summary>獲得原形動詞</summary>
        public static string getVerbRoot(string v)
        {
            try
            {
                return verb_lemmas[v];
            }
            catch (Exception)
            {
                return v;
            }
        }
        /// <summary>獲得單數名詞</summary>
        public static string getSingularNoun(string n)
        {
            return System.Data.Entity.Design.PluralizationServices.PluralizationService.CreateService(System.Globalization.CultureInfo.GetCultureInfo("en-us")).Singularize(n);
        }

        /// <summary>移除單字</summary>
        public static void removeWord(string WordKey)
        {
            Words.Remove(WordKey);
            Sentances.Remove(WordKey);
            saveDataToLocal();
        }
        /// <summary>載入本地資料</summary>
        public static void loadLocalData()
        {
            try
            {
                //將檔案還原成原來的物件
                using (var oFileStream = new FileStream($"{CurrentPath}\\wordDB.txt", FileMode.Open))
                {
                    var myBinaryFormatter = new BinaryFormatter();
                    Words = (Dictionary<string, Word>)myBinaryFormatter.Deserialize(oFileStream);
                }

                using (var oFileStream = new FileStream($"{CurrentPath}\\sentanceDB.txt", FileMode.Open))
                {
                    var myBinaryFormatter = new BinaryFormatter();
                    Sentances = (Dictionary<string, List<Sentence>>)myBinaryFormatter.Deserialize(oFileStream);
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(e.ToString(), "讀檔錯誤");
            }

        }
        /// <summary>增加單字權重</summary>
        public static void weightIncrease(string word)
        {
            Words[word].weight++;
            saveDataToLocal();
        }
        /// <summary>減少單字權重</summary>
        public static void weightDecrease(string word)
        {
            Words[word].weight--;
            saveDataToLocal();
        }
        /// <summary>儲存資料</summary>
        private static void saveDataToLocal()
        {
            using (var oFileStream = new FileStream($"{CurrentPath}\\wordDB.txt", FileMode.Create))
            {
                //建立二進位格式化物件
                var myBinaryFormatter = new BinaryFormatter();
                //將物件進行二進位格式序列化，並且將之儲存成檔案
                myBinaryFormatter.Serialize(oFileStream, Words);
                oFileStream.Flush();
                oFileStream.Close();
                oFileStream.Dispose();
            }
            using (var oFileStream = new FileStream($"{CurrentPath}\\sentanceDB.txt", FileMode.Create))
            {
                //建立二進位格式化物件
                var myBinaryFormatter = new BinaryFormatter();
                //將物件進行二進位格式序列化，並且將之儲存成檔案
                myBinaryFormatter.Serialize(oFileStream, Sentances);
                oFileStream.Flush();
                oFileStream.Close();
                oFileStream.Dispose();
            }
        }
        /// <summary>儲存單字筆記</summary>
        public static void seveWordRemark(string word, string remark)
        {
            Words[word].remark = remark;
            saveDataToLocal();
        }
        /// <summary>下載網路檔案方法</summary>
        private static void WebDownloadFile(string source, string destination)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent: Other");
                try
                {
                    client.DownloadFile(new Uri(source), destination);
                }
                catch (Exception e)
                {
                    MessageBox.Show(DateTime.Now + e.Message);
                }
            }
        }
    }
}
