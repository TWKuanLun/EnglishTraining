using EnglishTrain.cs;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WMPLib;

namespace EnglishTrain
{
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
            wordLabel.Content = LocalData.Words[word].word;
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
