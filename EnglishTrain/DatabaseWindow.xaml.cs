using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WMPLib;

namespace EnglishTrain
{
    public partial class DatabaseWindow : Window
    {
        /// <summary>單字Yahoo發音</summary>
        WindowsMediaPlayer YahooPlayer;
        /// <summary>單字Yahoo發音2，當有兩種發音方式時出現</summary>
        WindowsMediaPlayer YahooPlayer2;
        /// <summary>單字Google發音</summary>
        WindowsMediaPlayer GooglePlayer;

        WindowsMediaPlayer VoiceTubePlayer;
        public DatabaseWindow()
        {
            InitializeComponent();
            YahooPlayer = new WindowsMediaPlayer();
            GooglePlayer = new WindowsMediaPlayer();
            YahooPlayer2 = new WindowsMediaPlayer();
            VoiceTubePlayer = new WindowsMediaPlayer();
            YahooPlayer.MediaError += YahooPlayer_MediaError;//如果播放失敗
            updataList();
            AutoChangeWindowsFontSize autoChangeFontSize = new AutoChangeWindowsFontSize(this, 1920);
        }

        private void updataList()//更新listbox
        {
            WordListBox.Items.Clear();
            var search = DataBase.wordDB.Select(x => x.Value.word).Where(x => x.Contains(SearchTextBox.Text)).OrderByDescending(x => x);
            foreach (string w in search)
            {
                WordListBox.Items.Add(w);
            }
            WordListBox.SelectionChanged += WordListBox_SelectionChanged;
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            updataList();
        }

        private void WordListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WordListBox.SelectedValue != null)
            {
                string word = WordListBox.SelectedValue.ToString();
                dataGrid.Children.Clear();
                dataGrid.RowDefinitions.Clear();
                //顏色範例
                //https://msdn.microsoft.com/zh-tw/library/system.windows.media.brushes(v=vs.110).aspx

                wordlabel.Content = word;//單字設定
                phoneticlabel.Content = DataBase.wordDB[word].phoneticSymbol;//音標Label設定
                #region 播放按鈕設定
                GooglePlayer.URL = $"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl=en&q={word}";
                YahooPlayer.URL = $"https://s.yimg.com/tn/dict/dreye/live/f/{word}.mp3";//有多種念法or男生發音時會失效
                YahooPlayer2.URL = $"https://s.yimg.com/tn/dict/dreye/live/f/{word}@2.mp3";
                VoiceTubePlayer.URL = $"https://tw.voicetube.com/player/{word}.mp3";
                GooglePlayer.controls.stop();
                //YahooPlayer.controls.stop();//為了測試該單字音檔在yahoo屬於哪種網址
                YahooPlayer2.controls.stop();
                VoiceTubePlayer.controls.stop();
                yahooButton2.Visibility = Visibility.Hidden;
                #endregion


                int gridRowIndex = 0;
                foreach (KeyValuePair<string, List<string>> m in DataBase.wordDB[word].chineseMeaning)
                {
                    #region 詞性設定
                    dataGrid.RowDefinitions.Add(new RowDefinition());
                    var partOfSpeechlabel = new Label();
                    partOfSpeechlabel.Content = m.Key;
                    partOfSpeechlabel.Foreground = Brushes.Lavender;
                    partOfSpeechlabel.FontSize = 30;
                    Grid.SetRow(partOfSpeechlabel, gridRowIndex);
                    dataGrid.Children.Add(partOfSpeechlabel);
                    gridRowIndex++;
                    #endregion
                    for(int i = 0; i < m.Value.Count; i++)
                    {
                        #region 中文意思設定
                        dataGrid.RowDefinitions.Add(new RowDefinition());
                        var chiMeaninglabel = new Label();
                        chiMeaninglabel.Content = $"　{m.Value[i]}";
                        chiMeaninglabel.Foreground = Brushes.PapayaWhip;
                        chiMeaninglabel.FontSize = 30;
                        Grid.SetRow(chiMeaninglabel, gridRowIndex);
                        dataGrid.Children.Add(chiMeaninglabel);
                        gridRowIndex++;
                        #endregion

                        var searchSentence = DataBase.sentanceDB[word].Select(x => x).Where(x => (x.PartOfSpeech == m.Key && x.ChiMeaningIndex == i));
                        foreach(Sentence s in searchSentence)
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
                            Emptylabel.FontSize = 22;
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
                                button.FontSize = 22;
                                button.Click += Button_Click;
                                Grid.SetColumn(button, j + 1);
                                sentenceGrid.Children.Add(button);
                            }
                            Grid.SetRow(sentenceGrid, gridRowIndex);
                            dataGrid.Children.Add(sentenceGrid);
                            gridRowIndex++;
                            #endregion
                            #region 中文例句設定
                            dataGrid.RowDefinitions.Add(new RowDefinition());
                            var clabel = new Label();
                            clabel.Content = $"　　　 {s.Chi}";
                            clabel.Foreground = Brushes.Gainsboro;
                            clabel.FontSize = 22;
                            Grid.SetRow(clabel, gridRowIndex);
                            dataGrid.Children.Add(clabel);
                            gridRowIndex++;
                            #endregion
                        }
                    }
                }

            }
        }

        private void YahooButton2_Click(object sender, RoutedEventArgs e)
        {
            if (WordListBox.SelectedValue != null)
            {
                GooglePlayer.controls.pause();
                YahooPlayer.controls.pause();
                YahooPlayer2.controls.pause();
                VoiceTubePlayer.controls.pause();
                YahooPlayer2.controls.currentPosition = 0;
                YahooPlayer2.controls.play();
            }
        }
        int status = 0;
        private void YahooPlayer_MediaError(object pMediaObject)
        {
            if (status == 0)//女生發音失敗時
            {
                YahooPlayer.URL = $"https://s.yimg.com/tn/dict/dreye/live/m/{WordListBox.SelectedValue.ToString()}.mp3";
                status++;
            }else//男女生都發音失敗時
            {
                YahooPlayer.URL = $"https://s.yimg.com/tn/dict/dreye/live/f/{WordListBox.SelectedValue.ToString()}@1.mp3";
                yahooButton2.Visibility = Visibility.Visible;
                status = 0;
            }
            
        }

        private void YahooButton_Click(object sender, RoutedEventArgs e)
        {
            if (WordListBox.SelectedValue != null)
            {
                GooglePlayer.controls.pause();
                YahooPlayer.controls.pause();
                YahooPlayer2.controls.pause();
                VoiceTubePlayer.controls.pause();
                YahooPlayer.controls.currentPosition = 0;
                YahooPlayer.controls.play();
            }
        }

        private void GoogleButton_Click(object sender, RoutedEventArgs e)
        {
            if (WordListBox.SelectedValue != null)
            {
                GooglePlayer.controls.pause();
                YahooPlayer.controls.pause();
                YahooPlayer2.controls.pause();
                VoiceTubePlayer.controls.pause();
                GooglePlayer.controls.currentPosition = 0;
                GooglePlayer.controls.play();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            string word = b.Content.ToString().Substring(0, b.Content.ToString().Length - 1);//去除空白
            word = Regex.Replace(word, "[.']", "", RegexOptions.IgnoreCase);//去除'和.
            word = DataBase.getVerbRoot(word);//給出動詞字根
            word = DataBase.getSingularNoun(word);//給出單數名詞
            string html = DataBase.getHTML(word);
            string result = DataBase.getWordExplanation(html);
            if (result.Equals("search error"))
            {
                MessageBox.Show("找不到該單字！");
            }
            else
            {
                wordExplanationWindow wordWindow = new wordExplanationWindow(result, word, html);//取得HTML並用python擷取單字解釋
                wordWindow.Show();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow();
            Close();
            mw.Show();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataBase.removeWord(WordListBox.SelectedValue.ToString());
                updataList();
                WordListBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("錯誤，無選取單字");
            }
            
        }

        private void VoiceTubeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WordListBox.SelectedValue != null)
            {
                GooglePlayer.controls.pause();
                YahooPlayer.controls.pause();
                YahooPlayer2.controls.pause();
                VoiceTubePlayer.controls.pause();
                VoiceTubePlayer.controls.currentPosition = 0;
                VoiceTubePlayer.controls.play();
            }
        }
    }
}
