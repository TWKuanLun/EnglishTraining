using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using NSoup;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using WMPLib;

namespace EnglishTrain
{
    //新構想: 靜態只儲存List<string> words，當作Key，其餘資料都去資料夾內取得，下此單字時全部下載下來
    
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
        /// <summary>備註</summary>
        public string remark { get; set; } = string.Empty;
        public Word(string word,string phonetic)
        {
            this.word = word;
            this.phoneticSymbol = phonetic;
        }
        public override string ToString()
        {
            return word;
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
            loadDatabase();
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
                    
                    Word w = new Word(word, phonetic.Replace('ˋ', '`'));//給word物件單字和音標
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
                    saveDatabase();//儲存資料在本地端
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
            saveDatabase();
        }
        /// <summary>載入資料庫</summary>
        public static void loadDatabase()
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
            saveDatabase();
        }
        /// <summary>減少單字權重</summary>
        public static void weightDecrease(string word)
        {
            wordDB[word].weight--;
            saveDatabase();
        }
        /// <summary>儲存資料庫</summary>
        private static void saveDatabase()
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
        /// <summary>儲存單字筆記</summary>
        public static void seveWordRemark(string word, string remark)
        {
            wordDB[word].remark = remark;
            saveDatabase();
        }
        /// <summary>下載網路檔案方法</summary>
        private static void WebDownloadFile(string source, string destination)
        {
            using (WebClient client = new WebClient())
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

    /// <summary>自動改變WPF視窗字體大小的Class。</summary>
    class AutoChangeWindowsFontSize
    {
        private double UserResolutionWidth, defaultResolutionWidth, scale, preWindowWidth;
        Window window;
        /// <summary>取得所有相依物件。</summary>
        public static IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                foreach (object rawChild in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (rawChild is DependencyObject)
                    {
                        DependencyObject child = (DependencyObject)rawChild;
                        if (child is T)
                        {
                            yield return (T)child;
                        }

                        foreach (T childOfChild in FindLogicalChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }
        public AutoChangeWindowsFontSize(Window window, double defaultResolutionWidth)
        {
            UserResolutionWidth = SystemParameters.PrimaryScreenWidth;
            this.defaultResolutionWidth = defaultResolutionWidth;
            preWindowWidth = window.Width;
            this.window = window;
            this.window.SizeChanged += Window_SizeChanged;
            scale = UserResolutionWidth / defaultResolutionWidth;
            StringBuilder str = new StringBuilder();
            foreach (var c in FindLogicalChildren<Control>(window))
            {
                c.FontSize = c.FontSize * scale;
            }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double scaleWidth = window.ActualWidth / preWindowWidth;
            preWindowWidth = window.ActualWidth;
            foreach (var c in FindLogicalChildren<Control>(window))
            {
                c.FontSize = c.FontSize * scaleWidth;
            }
        }
    }
    /// <summary>自動產生Word所有資料與畫面設定的Class。</summary>
    class ShowWordExplain
    {
        /// <summary>單字Yahoo發音</summary>
        WindowsMediaPlayer YahooPlayer;
        /// <summary>單字Yahoo發音2，當有兩種發音方式時出現</summary>
        WindowsMediaPlayer YahooPlayer2;
        /// <summary>單字Google發音</summary>
        WindowsMediaPlayer GooglePlayer;
        /// <summary>單字VoiceTube發音</summary>
        WindowsMediaPlayer VoiceTubePlayer;
        /// <summary>句子Google發音</summary>
        List<WindowsMediaPlayer> SentencePlayer = new List<WindowsMediaPlayer>();
        string word;
        Button yahooButton2;
        public ShowWordExplain(string word, Grid mainGrid)
        {
            this.word = word;
            #region 播放按鈕設定
            YahooPlayer = new WindowsMediaPlayer();
            GooglePlayer = new WindowsMediaPlayer();
            YahooPlayer2 = new WindowsMediaPlayer();
            VoiceTubePlayer = new WindowsMediaPlayer();
            GooglePlayer.URL = $"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl=en&q={word}";
            YahooPlayer.URL = $"https://s.yimg.com/tn/dict/dreye/live/f/{word}.mp3";//有多種念法or男生發音時會失效
            YahooPlayer2.URL = $"https://s.yimg.com/tn/dict/dreye/live/f/{word}@2.mp3";
            VoiceTubePlayer.URL = $"https://tw.voicetube.com/player/{word}.mp3";
            GooglePlayer.controls.stop();
            //YahooPlayer.controls.stop();//為了測試該單字音檔在yahoo屬於哪種網址
            YahooPlayer2.controls.stop();
            VoiceTubePlayer.controls.stop();
            #endregion
            YahooPlayer.MediaError += YahooPlayer_MediaError;
            mainGrid.Children.Clear();
            mainGrid.RowDefinitions.Clear();
            mainGrid.RowDefinitions.Add(new RowDefinition());
            mainGrid.RowDefinitions.Add(new RowDefinition());
            mainGrid.RowDefinitions.Add(new RowDefinition());
            mainGrid.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Auto);
            mainGrid.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Auto);
            mainGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
            Grid titleGrid = new Grid();
            #region 音標Label Name:phoneticLabel
            Label phoneticLabel = new Label();
            phoneticLabel.Content = DataBase.wordDB[word].phoneticSymbol;
            phoneticLabel.Foreground = Brushes.SpringGreen;
            phoneticLabel.FontSize = 35;
            #endregion
            ScrollViewer scrollViewer = new ScrollViewer();
            Grid dataGrid = new Grid();
            dataGrid.VerticalAlignment = VerticalAlignment.Top;
            scrollViewer.Content = dataGrid;
            Grid.SetRow(titleGrid, 0);
            Grid.SetRow(phoneticLabel, 1);
            Grid.SetRow(scrollViewer, 2);
            mainGrid.Children.Add(titleGrid);
            mainGrid.Children.Add(phoneticLabel);
            mainGrid.Children.Add(scrollViewer);
            #region titleGrid設定 含wordLabel、googleButton、voiceTubeButton、yahooButton
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition());
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition());
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition());
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition());
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition());
            titleGrid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Auto);
            titleGrid.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Auto);
            titleGrid.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Auto);
            titleGrid.ColumnDefinitions[3].Width = new GridLength(0, GridUnitType.Auto);
            titleGrid.ColumnDefinitions[4].Width = new GridLength(0, GridUnitType.Auto);
            Label wordLabel = new Label();
            wordLabel.Content = DataBase.wordDB[word].word;
            wordLabel.Foreground = Brushes.SkyBlue;
            wordLabel.FontSize = 85;
            Button googleButton = new Button();
            googleButton.Content = "_Google(▶)";
            googleButton.Background = Brushes.Black;
            googleButton.Foreground = Brushes.White;
            googleButton.FontSize = 47;
            googleButton.Click += GoogleButton_Click;
            Button voiceTubeButton = new Button();
            voiceTubeButton.Content = "_VoiceTube(▶)";
            voiceTubeButton.Background = Brushes.Black;
            voiceTubeButton.Foreground = Brushes.White;
            voiceTubeButton.FontSize = 47;
            voiceTubeButton.Click += VoiceTubeButton_Click;
            Button yahooButton = new Button();
            yahooButton.Content = "_Yahoo(▶)";
            yahooButton.Background = Brushes.Black;
            yahooButton.Foreground = Brushes.White;
            yahooButton.FontSize = 47;
            yahooButton.Click += YahooButton_Click;
            Button yahooButton2 = new Button();
            this.yahooButton2 = yahooButton2;
            yahooButton2.Content = "_Yahoo2(▶)";
            yahooButton2.Background = Brushes.Black;
            yahooButton2.Foreground = Brushes.White;
            yahooButton2.FontSize = 47;
            yahooButton2.Visibility = Visibility.Hidden;
            yahooButton2.Click += YahooButton2_Click;
            Grid.SetColumn(wordLabel, 0);
            Grid.SetColumn(googleButton, 1);
            Grid.SetColumn(voiceTubeButton, 2);
            Grid.SetColumn(yahooButton, 3);
            Grid.SetColumn(yahooButton2, 4);
            titleGrid.Children.Add(wordLabel);
            titleGrid.Children.Add(googleButton);
            titleGrid.Children.Add(voiceTubeButton);
            titleGrid.Children.Add(yahooButton);
            titleGrid.Children.Add(yahooButton2);
            #endregion
            #region dataGrid設定 含詞性、中文意思、例句
            int gridRowIndex = 0;
            int sentenceCount = 0;
            foreach (KeyValuePair<string, List<string>> m in DataBase.wordDB[word].chineseMeaning)
            {
                #region 詞性設定
                dataGrid.RowDefinitions.Add(new RowDefinition());
                var partOfSpeechlabel = new Label();
                partOfSpeechlabel.Content = m.Key;
                partOfSpeechlabel.Foreground = Brushes.Lavender;
                partOfSpeechlabel.FontSize = 47;
                Grid.SetRow(partOfSpeechlabel, gridRowIndex);
                dataGrid.Children.Add(partOfSpeechlabel);
                gridRowIndex++;
                #endregion
                for (int i = 0; i < m.Value.Count; i++)
                {
                    #region 中文意思設定
                    dataGrid.RowDefinitions.Add(new RowDefinition());
                    var chiMeaninglabel = new Label();
                    chiMeaninglabel.Content = $"　{m.Value[i]}";
                    chiMeaninglabel.Foreground = Brushes.PapayaWhip;
                    chiMeaninglabel.FontSize = 47;
                    Grid.SetRow(chiMeaninglabel, gridRowIndex);
                    dataGrid.Children.Add(chiMeaninglabel);
                    gridRowIndex++;
                    #endregion

                    var searchSentence = DataBase.sentanceDB[word].Select(x => x).Where(x => (x.PartOfSpeech == m.Key && x.ChiMeaningIndex == i));
                    foreach (Sentence s in searchSentence)
                    {
                        #region 英文例句設定
                        dataGrid.RowDefinitions.Add(new RowDefinition());
                        var sentenceGrid = new Grid();
                        string[] sentanceWords = s.Eng.Split(new char[] { ' ' });
                        sentenceGrid.HorizontalAlignment = HorizontalAlignment.Left;
                        #region 加上前面的空白間隔
                        sentenceGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        sentenceGrid.ColumnDefinitions[0].Width = GridLength.Auto;
                        var Emptylabel = new Label();
                        Emptylabel.Content = $"　　　";
                        Emptylabel.FontSize = 35;
                        Grid.SetRow(Emptylabel, 0);
                        sentenceGrid.Children.Add(Emptylabel);
                        #endregion
                        for (int j = 0; j < sentanceWords.Length; j++)
                        {
                            sentenceGrid.ColumnDefinitions.Add(new ColumnDefinition());
                            sentenceGrid.ColumnDefinitions[j + 1].Width = GridLength.Auto;//給予grid寬度自動，防止按鈕部分遮蓋
                            var button = new Button();
                            button.Content = sentanceWords[j] + " ";
                            button.BorderThickness = new Thickness(0);//按鈕框線粗細，0=看不到框線
                            button.Background = Brushes.Black;
                            button.Foreground = Brushes.Pink;
                            button.FontSize = 35;
                            button.Click += Button_Click;
                            Grid.SetColumn(button, j + 1);
                            sentenceGrid.Children.Add(button);
                        }
                        #region 句子發音
                        SentencePlayer.Add(new WMPLib.WindowsMediaPlayer { URL = $"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl=en&q={s.Eng}" });
                        SentencePlayer[sentenceCount].controls.stop();
                        sentenceGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        sentenceGrid.ColumnDefinitions[sentanceWords.Length + 1].Width = GridLength.Auto;
                        var SentenceVoiceButton = new Button();
                        SentenceVoiceButton.Tag = sentenceCount;
                        SentenceVoiceButton.Content = $"_{sentenceCount}Play";
                        SentenceVoiceButton.Background = Brushes.Black;
                        SentenceVoiceButton.Foreground = Brushes.White;
                        SentenceVoiceButton.FontSize = 35;
                        SentenceVoiceButton.Click += SentenceVoiceButton_Click;
                        Grid.SetColumn(SentenceVoiceButton, sentanceWords.Length + 1);
                        sentenceGrid.Children.Add(SentenceVoiceButton);
                        #endregion
                        Grid.SetRow(sentenceGrid, gridRowIndex);
                        dataGrid.Children.Add(sentenceGrid);
                        gridRowIndex++;
                        #endregion
                        #region 中文例句設定
                        dataGrid.RowDefinitions.Add(new RowDefinition());
                        var clabel = new Label();
                        clabel.Content = $"　　　 {s.Chi}";
                        clabel.Foreground = Brushes.Gainsboro;
                        clabel.FontSize = 35;
                        Grid.SetRow(clabel, gridRowIndex);
                        dataGrid.Children.Add(clabel);
                        gridRowIndex++;
                        #endregion
                        sentenceCount++;
                    }
                }
            }
            #endregion
        }

        private void SentenceVoiceButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            foreach (WMPLib.WindowsMediaPlayer p in SentencePlayer)
            {
                p.controls.pause();
            }
            SentencePlayer[(int)b.Tag].controls.currentPosition = 0;
            SentencePlayer[(int)b.Tag].controls.play();
        }

        int status = 0;
        private void YahooPlayer_MediaError(object pMediaObject)
        {
            if (status == 0)//女生發音失敗時
            {
                YahooPlayer.URL = $"https://s.yimg.com/tn/dict/dreye/live/m/{word}.mp3";
                status++;
            }
            else//男女生都發音失敗時
            {
                YahooPlayer.URL = $"https://s.yimg.com/tn/dict/dreye/live/f/{word}@1.mp3";
                yahooButton2.Visibility = Visibility.Visible;
                status = 0;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            string word = b.Content.ToString().Substring(0, b.Content.ToString().Length - 1);//去除空白
            wordExplanationWindow wordWindow = new wordExplanationWindow(word);
            wordWindow.Show();
        }

        private void VoiceTubeButton_Click(object sender, RoutedEventArgs e)
        {
            GooglePlayer.controls.pause();
            YahooPlayer.controls.pause();
            YahooPlayer2.controls.pause();
            VoiceTubePlayer.controls.pause();
            VoiceTubePlayer.controls.currentPosition = 0;
            VoiceTubePlayer.controls.play();
        }

        private void YahooButton2_Click(object sender, RoutedEventArgs e)
        {
            GooglePlayer.controls.pause();
            YahooPlayer.controls.pause();
            YahooPlayer2.controls.pause();
            VoiceTubePlayer.controls.pause();
            YahooPlayer2.controls.currentPosition = 0;
            YahooPlayer2.controls.play();
        }

        private void YahooButton_Click(object sender, RoutedEventArgs e)
        {
            GooglePlayer.controls.pause();
            YahooPlayer.controls.pause();
            YahooPlayer2.controls.pause();
            VoiceTubePlayer.controls.pause();
            YahooPlayer.controls.currentPosition = 0;
            YahooPlayer.controls.play();
        }

        private void GoogleButton_Click(object sender, RoutedEventArgs e)
        {
            GooglePlayer.controls.pause();
            YahooPlayer.controls.pause();
            YahooPlayer2.controls.pause();
            VoiceTubePlayer.controls.pause();
            GooglePlayer.controls.currentPosition = 0;
            GooglePlayer.controls.play();
        }
    }
}
