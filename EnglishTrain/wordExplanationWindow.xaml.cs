using EnglishTrain.cs;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using static EnglishTrain.cs.LocalData;

namespace EnglishTrain
{
    public partial class wordExplanationWindow : Window
    {
        string word;
        bool save = false;
        
        public wordExplanationWindow(string word)
        {
            InitializeComponent();
            word = Regex.Replace(word, "[.,']", "", RegexOptions.IgnoreCase);//去除'和.和,
            word = getSingularNoun(getVerbRoot(word));//獲得原型動詞與單數
            if (Words.Keys.Contains(word))
            {
                var showWordExplain = new ShowWordExplain(word, mainGrid);
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
                    var showWordExplain = new ShowWordExplain(word, mainGrid);
                }
            }
            this.word = word;
            var acwfs = new AutoChangeWindowsFontSize(this,2880);
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            save = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            removeWord(word);
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!save)
                removeWord(word);
        }
    }
}
