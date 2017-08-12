using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using NSoup;
using System.Text;

namespace EnglishTrain
{
    //Yahoo爬下來的資料中，一個英文單字有多種詞性，一個詞性有0~N種中文意思，一個中文意思有多個例句
    [Serializable]
    /// <summary>單字類別，含詞性、中文意思、權重等。</summary>
    class Word
    {
        /// <summary>單字權重起始值，數字越大越不熟，0=非常熟，完全不會出現在單字練習。</summary>
        public int weight = 3;
        /// <summary>英文單字</summary>
        public readonly string word;//該英文單字
        /// <summary>音標</summary>
        public readonly string phoneticSymbol;
        /// <summary>給詞性Key獲得中文解釋List</summary>
        public Dictionary<string, List<string>> chineseMeaning = new Dictionary<string, List<string>>();
        /// <summary>直接ToString的解釋</summary>
        private readonly string explanation ;
        public Word(string word,string phonetic,string exp)
        {
            this.word = word;
            this.phoneticSymbol = phonetic;
            explanation = exp;
        }
        public override string ToString()
        {
            return explanation;
        }
    }
    [Serializable]
    class Sentence
    {
        public Sentence(string chi, string eng,string w,string p,int i)
        {
            Chi = chi;
            Eng = eng;
            WordKey = w;
            PartOfSpeech = p;
            ChiMeaningIndex = i;
        }
        public readonly string Chi;
        public readonly string Eng;
        public readonly string WordKey;//屬於哪個單字的例句
        public readonly string PartOfSpeech;//屬於該單字在哪個詞性的例句
        public readonly int ChiMeaningIndex;//該單字在該詞性時屬於哪個中文意思的例句
    }
    
    static class DataBase
    {
        static string debugPath = Directory.GetCurrentDirectory();
        /// <summary>例句資料庫，給key(單字)獲得例句</summary>
        public static Dictionary<string, List<Sentence>> sentanceDB = new Dictionary<string, List<Sentence>>();
        /// <summary>單字資料庫，給key(單字)獲得單字資料</summary>
        public static Dictionary<string, Word> wordDB = new Dictionary<string, Word>();
        
        public enum AddStatus
        {
            Success,SearchFail,HaveWord
        }
        public static void initialization()
        {
            LoadDatabase();
            getVerbLemmas();
        }
        /// <summary>新增單字，將單字、單字的例句加進資料庫，需搭配getHTML</summary>
        /// <param name="word">"單字</param>
        /// <param name="htmlstr">getHTML的結果</param>
        /// <param name="success">輸出結果，方便得知失敗原因，有Success或SearchFail或HaveWord</param>
        public static void addWordData(string word, string htmlstr, out AddStatus success)
        {
            if (!wordDB.Keys.Contains(word))//判斷資料庫內是否已經有該單字
            {
                NSoup.Nodes.Document htmlDoc = NSoupClient.Parse(htmlstr);
                try
                {
                    var allBlock = htmlDoc.GetElementsByTag("div").First(x => x.Attr("class") == "dd algo explain mt-20 lst DictionaryResults");
                    var meaningBlock = allBlock.GetElementsByTag("ul").Where(x => x.Attr("class") == "compArticleList mb-15 ml-10").ToArray();
                    var phonetic = htmlDoc.GetElementsByTag("span").First(x => x.Attr("class") == "cite").Text();
                    
                    Word w = new Word(word, phonetic.Replace('ˋ', '`'), getWordExplanation(htmlstr));//給word物件單字和音標
                    List<Sentence> sentences = new List<Sentence>();

                    var parts = allBlock.GetElementsByTag("h3");
                    for (int i = 0; i < parts.Count; i++)//詞性
                    {
                        List<string> chiMeaning = new List<string>();
                        
                        var ChiMeaningElements = meaningBlock[i].GetElementsByTag("li");
                        foreach (var OneMeaning in ChiMeaningElements)
                        {
                            var sentence = OneMeaning.GetElementsByTag("span").ToArray();
                            chiMeaning.Add(sentence[0].Text());//0是中文意思，一個中文意思有多個例句
                            for (int j = 1; j < sentence.Length - 1; j += 2)
                            {
                                int index = sentence[j].Text().LastIndexOf(' ');//獲得 例句 與 例句的中文 中間的索引

                                if (index != -1)//沒例句的情形
                                {
                                    Sentence s = new Sentence(sentence[j].Text().Substring(index + 1), sentence[j].Text().Substring(0, index), word, parts[i].Text(), chiMeaning.Count - 1);
                                    sentences.Add(s);//例句
                                }
                            }
                        }
                        w.chineseMeaning[parts[i].Text()] = chiMeaning;//詞性當Key，給key獲得該詞性的所有中文意思
                    }
                    sentanceDB[word] = sentences;
                    wordDB[word] = w;
                    SaveDatabase();//儲存資料在本地端
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
            WebRequest myRequest = WebRequest.Create(@"https://tw.dictionary.search.yahoo.com/search?p=" + word + "&fr2=dict");
            myRequest.Method = "GET";
            WebResponse myResponse = myRequest.GetResponse();
            StreamReader sr = new StreamReader(myResponse.GetResponseStream());
            string htmlSourceCode = sr.ReadToEnd();
            sr.Close();
            myResponse.Close();
            return htmlSourceCode;
        }
        public static string getWordExplanation(string htmlSourceCode)//獲得Tosting的文字
        {
            NSoup.Nodes.Document htmlDoc = NSoup.NSoupClient.Parse(htmlSourceCode);
            StringBuilder strBuilder = new StringBuilder();
            try
            {
                var allBlock = htmlDoc.GetElementsByTag("div").First(x => x.Attr("class") == "dd algo explain mt-20 lst DictionaryResults");
                var meaningBlock = allBlock.GetElementsByTag("ul").Where(x => x.Attr("class") == "compArticleList mb-15 ml-10").ToArray();
                var wordstr = htmlDoc.GetElementsByTag("span").First(x => x.Attr("class") == " mr-15").Text();
                strBuilder.AppendLine(wordstr);
                var phonetic = htmlDoc.GetElementsByTag("span").First(x => x.Attr("class") == "cite").Text();
                strBuilder.AppendLine(phonetic);
                var parts = allBlock.GetElementsByTag("h3");
                for (int i = 0; i < parts.Count; i++)
                {
                    strBuilder.AppendLine(parts[i].Text());//詞性
                    var ttt = meaningBlock[i].GetElementsByTag("li");
                    foreach (var sss in ttt)
                    {
                        var sentence = sss.GetElementsByTag("span").ToArray();
                        strBuilder.Append("　　");
                        strBuilder.AppendLine(sentence[0].Text());//中文意思
                        for (int j = 1; j < sentence.Length - 1; j += 2)
                        {
                            strBuilder.Append("　　　　");
                            strBuilder.AppendLine(sentence[j].Text());//例句
                        }
                    }
                }
                return strBuilder.ToString();
            }
            catch (Exception e)
            {
                return $"查無此單字解釋，{e}";
            }
        }

        private static Dictionary<string, string> verb_lemmas = new Dictionary<string, string>();//動詞型態字典
        private static void getVerbLemmas()//獲得動詞型態字典
        {
            #region getData
            string[] data;
            Dictionary<string, string[]> verb_tenses = new Dictionary<string, string[]>();

            using (StreamReader sr = new StreamReader(debugPath + "\\verb.txt"))
            {
                string line = sr.ReadToEnd();
                data = line.Split(new char[] { '\n' });
            }
            for (int i = 0; i < data.Length; i++)
            {
                string[] a = data[i].Split(new char[] { ',' });
                verb_tenses[a[0]] = a;
            }
            foreach (KeyValuePair<string, string[]> infinitive in verb_tenses)
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
            wordDB.Remove(WordKey);
            sentanceDB.Remove(WordKey);
            SaveDatabase();
        }
        /// <summary>載入資料庫</summary>
        public static void LoadDatabase()
        {
            try
            {
                //將檔案還原成原來的物件
                using (FileStream oFileStream = new FileStream($"{debugPath}\\wordDB.txt", FileMode.Open))
                {
                    BinaryFormatter myBinaryFormatter = new BinaryFormatter();
                    wordDB = (Dictionary<string, Word>)myBinaryFormatter.Deserialize(oFileStream);
                }
                
                using (FileStream oFileStream = new FileStream($"{debugPath}\\sentanceDB.txt", FileMode.Open))
                {
                    BinaryFormatter myBinaryFormatter = new BinaryFormatter();
                    sentanceDB = (Dictionary<string, List<Sentence>>)myBinaryFormatter.Deserialize(oFileStream);
                }
            }
            catch(Exception e)
            {
                //MessageBox.Show(e.ToString(), "讀檔錯誤");
            }
            
        }
        /// <summary>增加單字權重</summary>
        public static void weightIncrease(string word)
        {
            wordDB[word].weight++;
            SaveDatabase();
        }
        /// <summary>減少單字權重</summary>
        public static void weightDecrease(string word)
        {
            wordDB[word].weight--;
            SaveDatabase();
        }
        /// <summary>儲存資料庫</summary>
        private static void SaveDatabase()
        {
            using (FileStream oFileStream = new FileStream($"{debugPath}\\wordDB.txt", FileMode.Create))
            {
                //建立二進位格式化物件
                BinaryFormatter myBinaryFormatter = new BinaryFormatter();
                //將物件進行二進位格式序列化，並且將之儲存成檔案
                myBinaryFormatter.Serialize(oFileStream, wordDB);
                oFileStream.Flush();
                oFileStream.Close();
                oFileStream.Dispose();
            }
            using (FileStream oFileStream = new FileStream($"{debugPath}\\sentanceDB.txt", FileMode.Create))
            {
                //建立二進位格式化物件
                BinaryFormatter myBinaryFormatter = new BinaryFormatter();
                //將物件進行二進位格式序列化，並且將之儲存成檔案
                myBinaryFormatter.Serialize(oFileStream, sentanceDB);
                oFileStream.Flush();
                oFileStream.Close();
                oFileStream.Dispose();
            }
        }
    } 
}
