using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using static EnglishTrain.DataBase;

namespace EnglishTrain
{
    /// <summary>
    /// Dictionary.xaml 的互動邏輯
    /// </summary>
    public partial class Dictionary : Window
    {
        public Dictionary()
        {
            InitializeComponent();
            AutoChangeWindowsFontSize acwf = new AutoChangeWindowsFontSize(this, 1920);
        }
        private void process()
        {
            string word = Regex.Replace(wordTextBox.Text, "[.,']", "", RegexOptions.IgnoreCase);//去除'和.和,
            word = getSingularNoun(getVerbRoot(word));//獲得原型動詞與單數
            if (word.Equals(String.Empty))
                return;
            if (wordDB.Keys.Contains(word))
            {
                ShowWordExplain showWordExplain = new ShowWordExplain(word, ShowGrid);
            }
            else
            {
                AddStatus addStatus;
                addWordData(word, getHTML(word), out addStatus);
                if (addStatus == AddStatus.SearchFail)
                {
                    MessageBox.Show($"Yahoo查無此單字：{word}\n");
                }else
                {
                    ShowWordExplain showWordExplain = new ShowWordExplain(word, ShowGrid);
                }
            }
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            process();
        }

        private void wordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                process();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow();
            Close();
            mw.Show();
        }
    }
}
