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
        
        public DatabaseWindow()
        {
            InitializeComponent();
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
                ShowWordExplain showWordExplain = new ShowWordExplain(word, showWordGrid);
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
        

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            updataList();
        }
    }
}
