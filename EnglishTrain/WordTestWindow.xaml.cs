using EnglishTrain.cs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EnglishTrain
{
    public partial class WordTestWindow : Window
    {
        /// <summary>單字Yahoo發音</summary>
        MediaPlayerHelper VoiceTubePlayer;
        /// <summary>單字Google發音</summary>
        MediaPlayerHelper GooglePlayer;
        /// <summary>例句慢速發音</summary>
        List<MediaPlayerHelper> SlowPlayer = new List<MediaPlayerHelper>();
        /// <summary>例句常速發音</summary>
        List<MediaPlayerHelper> NormalPlayer = new List<MediaPlayerHelper>();
        /// <summary>目前的狀態</summary>
        UIVisibilityState CurrentState = UIVisibilityState.OnlyVoice;
        /// <summary>UI的狀態種類</summary>
        enum UIVisibilityState
        {
            OnlyVoice, VoiceAndEng, All
        }
        public WordTestWindow()
        {
            InitializeComponent();
            setTest();
            var autoChangeFontSize = new AutoChangeWindowsFontSize(this, 1920);
        }
        /// <summary>以權重值獲得隨機的單字</summary>
        private string GetRandomWord()
        {
            var ran = new Random();
            var randomBox = new List<string>();//產生權重數目的單字
            foreach (var word in LocalData.Words)
            {
                for (int i = 0; i < word.Value.weight; i++)
                {
                    randomBox.Add(word.Key);
                }
            }
            return randomBox.Count == 0 ? "沒單字" : randomBox[ran.Next(randomBox.Count)];
        }
        /// <summary>設置題目，包含控制項改動</summary>
        private void setTest()
        {
            NormalPlayer.Clear();
            SlowPlayer.Clear();

            string wordstr = GetRandomWord();
            
            if(wordstr!= "沒單字")
            {
                wordLabel.Content = wordstr;
                var word = LocalData.Words[wordstr];

                //設題目時先載入音檔，避免每次播放都要載入。
                GooglePlayer = new MediaPlayerHelper($"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl=en&q={wordstr}");
                VoiceTubePlayer = new MediaPlayerHelper($"https://tw.voicetube.com/player/{wordstr}.mp3");
                phoneticSymbolLabel.Content = word.phoneticSymbol;//show音標
                ChiTextBlock.Text = "";
                foreach (var s in word.chineseMeaning)//show詞性、中文意思
                {
                    ChiTextBlock.Text += $"{s.Key}\n";
                    for(int i = 0; i < s.Value.Count; i++)
                        ChiTextBlock.Text += $"　 {s.Value[i]}\n";
                }
                for (int i = 0; i < LocalData.Sentances[wordstr].Count; i++)
                {
                    //載入例句音檔
                    NormalPlayer.Add(new MediaPlayerHelper($"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl=en&q={LocalData.Sentances[wordstr][i].Eng}"));
                    SlowPlayer.Add(new MediaPlayerHelper($"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl=en&ttsspeed=0.1&q={LocalData.Sentances[wordstr][i].Eng}"));
                    //新增英文和中文的Row
                    SentenceGrid.RowDefinitions.Add(new RowDefinition());
                    SentenceGrid.RowDefinitions.Add(new RowDefinition());
                    var grid = new Grid();
                    var label = new Label();
                    label.Content = LocalData.Sentances[wordstr][i].Chi;
                    label.Visibility = Visibility.Hidden;
                    label.Foreground = Brushes.Gainsboro;
                    label.FontSize = 45;
                    string[] sentanceWords = LocalData.Sentances[wordstr][i].Eng.Split(new char[] {' '});
                    grid.HorizontalAlignment = HorizontalAlignment.Left;
                    grid.Visibility = Visibility.Hidden;
                    #region 創建例句內所有單字按紐
                    for (int j = 0; j < sentanceWords.Length; j++)
                    {

                        var column = new ColumnDefinition();
                        grid.ColumnDefinitions.Add(column);
                        grid.ColumnDefinitions[j].Width = GridLength.Auto;//給予grid寬度自動，防止按鈕部分遮蓋
                        var button = new Button();
                        button.Content = sentanceWords[j]+" ";
                        button.HorizontalContentAlignment = HorizontalAlignment.Left;
                        button.BorderThickness = new Thickness(0);//按鈕框線粗細，0=看不到框線
                        button.Background = Brushes.Black;
                        button.Foreground = Brushes.Pink;
                        button.FontSize = 45;
                        button.Click += Button_Click;
                        Grid.SetColumn(button, j);
                        grid.Children.Add(button);
                    }
                    #endregion
                    #region 創建例句播放聲音按紐
                    var column2 = new ColumnDefinition();
                    grid.ColumnDefinitions.Add(column2);
                    var sentanceVoiceButton = new Button();
                    sentanceVoiceButton.Content = $"_{i}Play";
                    sentanceVoiceButton.FontSize = 45;
                    sentanceVoiceButton.Tag = i;
                    sentanceVoiceButton.VerticalAlignment = VerticalAlignment.Center;
                    sentanceVoiceButton.Click += SentanceVoiceButton_Click;
                    Grid.SetColumn(sentanceVoiceButton, sentanceWords.Length);
                    grid.Children.Add(sentanceVoiceButton);
                    #endregion
                    Grid.SetRow(grid, i * 2);
                    Grid.SetRow(label, i * 2 + 1);
                    SentenceGrid.Children.Add(grid);
                    SentenceGrid.Children.Add(label);
                }
            }
            else//沒有不熟單字的情況
            {
                ruleLabel1.Content = "目前沒有不熟的單字";
                ruleLabel2.Content = "請先去新增單字";
                NextButton.IsEnabled = false;
            }
        }

        /// <summary>例句英文單字按紐，點擊查詢單字</summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            string word = b.Content.ToString().Substring(0, b.Content.ToString().Length-1);//去除空白
            var wordWindow = new wordExplanationWindow(word);
            wordWindow.Show();
        }
        /// <summary>例句聲音播放按紐</summary>
        private void SentanceVoiceButton_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            foreach (var slowPlayer in SlowPlayer)
            {
                slowPlayer.Pause();
            }
            foreach (var normalPlayer in NormalPlayer)
            {
                normalPlayer.Pause();
            }
            if ((bool)SlowVoiceCheckBox.IsChecked)
            {
                
                SlowPlayer[(int)b.Tag].PlayFromStart();
            }else
            {
                NormalPlayer[(int)b.Tag].PlayFromStart();
            }
        }

        private void ReplayButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)GoogleRadioButton.IsChecked)
            {
                GooglePlayer.Pause();
                VoiceTubePlayer.Pause();
                GooglePlayer.PlayFromStart();
            }
            else//否則Yahoo發音
            {
                GooglePlayer.Pause();
                VoiceTubePlayer.Pause();
                VoiceTubePlayer.PlayFromStart();
            }
        }
        
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mw = new MainWindow();
            Close();
            mw.Show();
        }

        private void UnfamiliarButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeWeight((Button)sender);
        }

        private void MediumButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeWeight((Button)sender);
        }

        private void FamiliarButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeWeight((Button)sender);
        }
        private void ChangeWeight(Button bt)
        {
            switch (bt.Name)
            {
                case "UnfamiliarButton":
                    LocalData.weightIncrease(wordLabel.Content.ToString());
                    break;
                case "MediumButton":
                    break;
                case "FamiliarButton":
                    LocalData.weightDecrease(wordLabel.Content.ToString());
                    break;
                default:
                    MessageBox.Show("Error in ChangeWeight");
                    break;
            }
            #region UI控制
            //按完後進到下一題
            CurrentState = UIVisibilityState.OnlyVoice;
            UnfamiliarButton.IsEnabled = false;
            MediumButton.IsEnabled = false;
            FamiliarButton.IsEnabled = false;
            NextButton.IsEnabled = true;
            NextButton.Visibility = Visibility.Visible;
            ruleLabel1.Visibility = Visibility.Visible;
            ruleLabel2.Visibility = Visibility.Visible;
            wordLabel.Visibility = Visibility.Hidden;
            phoneticSymbolLabel.Visibility = Visibility.Hidden;
            chiMeanScrollViewer.Visibility = Visibility.Hidden;
            SentenceGrid.RowDefinitions.Clear();
            SentenceGrid.Children.Clear();
            #endregion
            setTest();
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            switch (CurrentState)
            {
                case UIVisibilityState.OnlyVoice:
                    for(int i=0;i< SentenceGrid.Children.Count; i += 2)
                    {
                        SentenceGrid.Children[i].Visibility = Visibility.Visible;
                    }
                    ruleLabel1.Visibility = Visibility.Hidden;
                    ruleLabel2.Visibility = Visibility.Hidden;
                    ruleTextBlock1.Visibility = Visibility.Visible;
                    wordLabel.Visibility = Visibility.Visible;
                    phoneticSymbolLabel.Visibility = Visibility.Visible;
                    CurrentState = UIVisibilityState.VoiceAndEng;

                    break;
                case UIVisibilityState.VoiceAndEng:
                    for (int i = 1; i < SentenceGrid.Children.Count; i += 2)
                    {
                        SentenceGrid.Children[i].Visibility = Visibility.Visible;
                    }
                    ruleTextBlock1.Visibility = Visibility.Hidden;
                    NextButton.IsEnabled = false;
                    NextButton.Visibility = Visibility.Hidden;
                    UnfamiliarButton.IsEnabled = true;
                    MediumButton.IsEnabled = true;
                    FamiliarButton.IsEnabled = true;
                    chiMeanScrollViewer.Visibility = Visibility.Visible;
                    CurrentState = UIVisibilityState.All;
                    break;
                default:
                    MessageBox.Show("Error in CurrentState");
                    break;
            }
        }
    }
}
