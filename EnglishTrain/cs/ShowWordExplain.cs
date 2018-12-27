using EnglishTrain.cs;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WMPLib;

namespace EnglishTrain
{
    /// <summary>自動產生Word所有資料與畫面設定的Class。</summary>
    class ShowWordExplain
    {
        /// <summary>單字發音</summary>
        List<MediaPlayerHelper> WordsPlayer = new List<MediaPlayerHelper>();

        /// <summary>句子Google發音</summary>
        List<MediaPlayerHelper> SentencesPlayer = new List<MediaPlayerHelper>();
        string word;
        Button yahooButton2;
        public ShowWordExplain(string word, Grid mainGrid)
        {
            this.word = word;
            #region 播放按鈕設定
            var wordsPlayerURL = new (string Name, string URL)[]
            {
                ("Google", $"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl=en&q={word}"),
                ("VoiceTube", $"https://tw.voicetube.com/player/{word}.mp3"),
                ("YahooNormal", $"https://s.yimg.com/bg/dict/dreye/live/f/{word}.mp3"),
                ("YahooNormal2", $"https://s.yimg.com/bg/dict/dreye/live/f/{word}@2.mp3"),
                ("YahooNormal3", $"https://s.yimg.com/bg/dict/dreye/live/f/{word}@3.mp3"),
                ("YahooUS1", $"https://s.yimg.com/bg/dict/ox/mp3/v1/{word}@_us_1.mp3"),
                ("YahooUS2", $"https://s.yimg.com/bg/dict/ox/mp3/v1/{word}@_us_2.mp3"),
                ("YahooUS3", $"https://s.yimg.com/bg/dict/ox/mp3/v1/{word}@_us_3.mp3"),
                ("YahooGB1", $"https://s.yimg.com/bg/dict/ox/mp3/v1/{word}@_gb_1.mp3"),
                ("YahooGB2", $"https://s.yimg.com/bg/dict/ox/mp3/v1/{word}@_gb_2.mp3"),
                ("YahooGB3", $"https://s.yimg.com/bg/dict/ox/mp3/v1/{word}@_gb_3.mp3")
            }.Where(x =>
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(x.URL);
                request.Method = "HEAD";
                try
                {
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        // Code here 
                    }
                    request.GetResponse();
                }
                catch (WebException we)
                {
                    HttpWebResponse errorResponse = we.Response as HttpWebResponse;
                    if (errorResponse.StatusCode == HttpStatusCode.Forbidden ||
                        errorResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        return false;
                    }
                }
                return true;
            }).ToArray();
            foreach (var wordPlayerURL in wordsPlayerURL)
            {
                WordsPlayer.Add(new MediaPlayerHelper(wordPlayerURL.URL));
            }
            #endregion
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
            phoneticLabel.Content = LocalData.Words[word].phoneticSymbol;
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
            titleGrid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Auto);
            Label wordLabel = new Label();
            wordLabel.Content = LocalData.Words[word].word;
            wordLabel.Foreground = Brushes.SkyBlue;
            wordLabel.FontSize = 85;
            Grid.SetColumn(wordLabel, 0);
            titleGrid.Children.Add(wordLabel);
            for (int i = 0; i < wordsPlayerURL.Length; i++)
            {
                titleGrid.ColumnDefinitions.Add(new ColumnDefinition());
                titleGrid.ColumnDefinitions[i + 1].Width = new GridLength(0, GridUnitType.Auto);
                Button button = new Button();
                button.Content = $"_{wordsPlayerURL[i].Name}(▶)";
                button.Background = Brushes.Black;
                button.Foreground = Brushes.White;
                button.FontSize = 47;
                button.Click += WordsPlayerButton_Click;
                button.Tag = i;
                Grid.SetColumn(button, i + 1);
                titleGrid.Children.Add(button);
            }
            #endregion
            #region dataGrid設定 含詞性、中文意思、例句
            int gridRowIndex = 0;
            int sentenceCount = 0;
            foreach (KeyValuePair<string, List<string>> m in LocalData.Words[word].chineseMeaning)
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

                    var searchSentence = LocalData.Sentances[word].Select(x => x).Where(x => (x.PartOfSpeech == m.Key && x.ChiMeaningIndex == i));
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
                        SentencesPlayer.Add(new MediaPlayerHelper($"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl=en&q={s.Eng}"));
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
            Button button = (Button)sender;
            foreach (var sentencePlayer in SentencesPlayer)
            {
                sentencePlayer.Pause();
            }
            SentencesPlayer[(int)button.Tag].PlayFromStart();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            string word = b.Content.ToString().Substring(0, b.Content.ToString().Length - 1);//去除空白
            wordExplanationWindow wordWindow = new wordExplanationWindow(word);
            wordWindow.Show();
        }

        private void WordsPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            foreach (var wordPlayer in WordsPlayer)
            {
                wordPlayer.Pause();
            }
            WordsPlayer[(int)button.Tag].PlayFromStart();
        }
    }
}
