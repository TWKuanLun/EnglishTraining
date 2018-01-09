using EnglishTrain.cs;
using System.Windows;

namespace EnglishTrain
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            LocalData.initialization();
            var autoChangeFontSize = new AutoChangeWindowsFontSize(this,1920);
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            var wordTestwindow = new WordTestWindow();
            Close();
            wordTestwindow.Show();
        }

        private void ImportWordButton_Click(object sender, RoutedEventArgs e)
        {
            var imw = new ImportWordWindow();
            Close();
            imw.Show();
        }

        private void LocalDataButton_Click(object sender, RoutedEventArgs e)
        {
            var dbw = new LocalDataWindow();
            Close();
            dbw.Show();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var d = new Dictionary();
            Close();
            d.Show();
        }
    }
}
