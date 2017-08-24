using System.Windows;
using static EnglishTrain.DataBase;
using System.Linq;
using System.Text.RegularExpressions;

namespace EnglishTrain
{
    public partial class wordExplanationWindow : Window
    {
        string word;
        
        public wordExplanationWindow(string word)
        {
            InitializeComponent();
            word = Regex.Replace(word, "[.']", "", RegexOptions.IgnoreCase);//去除'和.
            word = getSingularNoun(getVerbRoot(word));//獲得原型動詞與單數
            if (wordDB.Keys.Contains(word))
            {
                ShowWordExplain showWordExplain = new ShowWordExplain(word, mainGrid);
            }
            else
            {
                AddStatus status;
                addWordData(word, getHTML(word), out status);
                if (status == AddStatus.SearchFail)
                {
                    MessageBox.Show($"Yahoo查無此單字：{word}\n");
                    Close();
                }
                else
                {
                    ShowWordExplain showWordExplain = new ShowWordExplain(word, mainGrid);
                }
            }
            this.word = word;
            //AutoChangeWindowsFontSize acwfs = new AutoChangeWindowsFontSize(this,1080);
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            removeWord(word);
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            removeWord(word);
            Close();
        }
    }
}
