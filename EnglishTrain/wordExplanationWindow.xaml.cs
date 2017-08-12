using System.Windows;
using static EnglishTrain.DataBase;
using WMPLib;

namespace EnglishTrain
{
    public partial class wordExplanationWindow : Window
    {
        string word;
        string html;
        WindowsMediaPlayer VoiceTubePlayer;
        public wordExplanationWindow(string str,string word,string html)
        {
            InitializeComponent();
            this.word = word;
            LB.Content = str;
            this.html = html;
            VoiceTubePlayer = new WindowsMediaPlayer();
            VoiceTubePlayer.URL = $"https://tw.voicetube.com/player/{word}.mp3";
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            AddStatus success;
            addWordData(word, html, out success);
            switch (success)
            {
                case AddStatus.HaveWord:
                    MessageBox.Show($"{word}失敗，已有此單字");
                    break;
                case AddStatus.SearchFail:
                    MessageBox.Show($"{word}失敗，查無此單字");
                    break;
                default:
                    MessageBox.Show($"{word}新增成功");
                    break;
            }
                
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VoiceTubePlayer.controls.pause();
            VoiceTubePlayer.controls.currentPosition = 0;
            VoiceTubePlayer.controls.play();
        }
    }
}
