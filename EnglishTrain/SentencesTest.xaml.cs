using EnglishTrain.cs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EnglishTrain
{
    public partial class SentencesTest : Window
    {
        /// <summary>例句慢速發音</summary>
        MediaPlayerHelper SlowPlayer;
        /// <summary>例句常速發音</summary>
        MediaPlayerHelper NormalPlayer;
        List<Sentence> sentences = new List<Sentence>();
        int nowIndex = 0;
        NowStatus nowStatus = NowStatus.NoEngNoChi;
        enum NowStatus
        {
            NoEngNoChi,
            NoChi,
            ShowAll
        }
        public SentencesTest()
        {
            InitializeComponent();
            var autoChangeFontSize = new AutoChangeWindowsFontSize(this, 1920);
           
            foreach (var wordSentences in LocalData.Sentances)
            {
                foreach(var sentence in wordSentences.Value)
                {
                    sentences.Add(sentence);
                }
            }
            sentences = Shuffle(sentences) as List<Sentence>;

            setTest();
        }
        
        public void setTest()
        {
            if (sentences.Count == 0)
                return;
            EngGrid.Children.Clear();
            var nowSentence = sentences[nowIndex];
            string[] sentanceWords = nowSentence.Eng.Split(new char[] { ' ' });
            EngGrid.Visibility = Visibility.Hidden;
            ChiLB.Visibility = Visibility.Hidden;
            #region 創建例句內所有單字按紐
            for (int j = 0; j < sentanceWords.Length; j++)
            {
                var column = new ColumnDefinition();
                EngGrid.ColumnDefinitions.Add(column);
                EngGrid.ColumnDefinitions[j].Width = GridLength.Auto;//給予grid寬度自動，防止按鈕部分遮蓋
                var button = new Button();
                button.Content = sentanceWords[j] + " ";
                button.HorizontalContentAlignment = HorizontalAlignment.Left;
                button.BorderThickness = new Thickness(0);//按鈕框線粗細，0=看不到框線
                button.Background = Brushes.Black;
                button.Foreground = Brushes.Pink;
                button.FontSize = 50;
                button.Click += Button_Click;
                Grid.SetColumn(button, j);
                EngGrid.Children.Add(button);
            }
            #endregion
            ChiLB.Content = nowSentence.Chi;
            NormalPlayer = new MediaPlayerHelper($"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl=en&q={nowSentence.Eng}");
            SlowPlayer = new MediaPlayerHelper($"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl=en&ttsspeed=0.1&q={nowSentence.Eng}");
            if ((bool)SlowVoiceCheckBox.IsChecked)
                SlowPlayer.Play();
            else
                NormalPlayer.Play();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            string word = b.Content.ToString().Substring(0, b.Content.ToString().Length - 1);//去除空白
            var wordWindow = new wordExplanationWindow(word);
            wordWindow.Show();
        }
        /// <summary>
        /// 將IList內所有元素隨機位置
        /// </summary>
        public IList<T> Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mw = new MainWindow();
            Close();
            mw.Show();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Right)
            {
                nextTest();
            }
            else if(e.Key == Key.Left)
            {
                backTest();
            }
        }
        private void nextTest()
        {
            switch (nowStatus)
            {
                case NowStatus.NoEngNoChi:
                    EngGrid.Visibility = Visibility.Visible;
                    nowStatus = NowStatus.NoChi;
                    break;
                case NowStatus.NoChi:
                    ChiLB.Visibility = Visibility.Visible;
                    nowStatus = NowStatus.ShowAll;
                    break;
                case NowStatus.ShowAll:
                    if (nowIndex == sentences.Count - 1)
                        return;
                    nowIndex++;
                    setTest();
                    nowStatus = NowStatus.NoEngNoChi;
                    break;
            }
        }
        private void backTest()
        {
            switch (nowStatus)
            {
                case NowStatus.NoEngNoChi:
                    if (nowIndex == 0)
                        return;
                    nowIndex--;
                    setTest();
                    break;
                case NowStatus.NoChi:
                    EngGrid.Visibility = Visibility.Hidden;
                    nowStatus = NowStatus.NoEngNoChi;
                    break;
                case NowStatus.ShowAll:
                    ChiLB.Visibility = Visibility.Hidden;
                    nowStatus = NowStatus.NoChi;
                    break;
            }
        }

        private void NextBT_Click(object sender, RoutedEventArgs e)
        {
            nextTest();
        }

        private void BackBT_Click(object sender, RoutedEventArgs e)
        {
            backTest();
        }

        private void VoiceBT_Click(object sender, RoutedEventArgs e)
        {
            SlowPlayer.Pause();
            NormalPlayer.Pause();
            if ((bool)SlowVoiceCheckBox.IsChecked)
            {
                SlowPlayer.PlayFromStart();
            }
            else
            {
                NormalPlayer.PlayFromStart();
            }
        }
    }
}
