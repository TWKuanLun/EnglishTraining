using EnglishTrain.cs;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static EnglishTrain.cs.LocalData;

namespace EnglishTrain
{
    public partial class LocalDataWindow : Window
    {
        string oldWord;
        public LocalDataWindow()
        {
            InitializeComponent();
            updataList();
            var autoChangeFontSize = new AutoChangeWindowsFontSize(this, 1920);
            oldWord = string.Empty;
        }

        private void updataList()//更新listbox
        {
            WordListBox.Items.Clear();
            var search = LocalData.Words.Select(x => x.Value.word).Where(x => x.Contains(SearchTextBox.Text)).OrderByDescending(x => x);
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
                checkWordRemark(oldWord);
                string word = WordListBox.SelectedValue.ToString();
                oldWord = word;
                var showWordExplain = new ShowWordExplain(word, showWordGrid);
                remarkTB.Text = Words[word].remark;
                remarkTB.IsEnabled = true;
            }
            else
            {
                oldWord = string.Empty;
                showWordGrid.Children.Clear();
                remarkTB.Text = "";
                remarkTB.IsEnabled = false;
            }
        }
        

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mw = new MainWindow();
            Close();
            mw.Show();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                removeWord(WordListBox.SelectedValue.ToString());
                updataList();
                WordListBox.SelectedIndex = 0;
                oldWord = string.Empty;
            }
            catch (Exception)
            {
                MessageBox.Show("錯誤，無選取單字");
            }
            
        }
        

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            updataList();
        }

        private void checkWordRemark(string word)
        {
            if (!oldWord.Equals(string.Empty) && !Words[word].remark.Equals(remarkTB.Text))
                if (MessageBox.Show("保存筆記/註解/備忘錄?", "您有做了些修改，是否需要保存?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    seveWordRemark(word, remarkTB.Text);
                }
        }
    }
}
