using System.Windows;

namespace EnglishTrain
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataBase.initialization();
        }

        private void WordButton_Click(object sender, RoutedEventArgs e)
        {
            WordTestWindow wordTestwindow = new WordTestWindow();
            Close();
            wordTestwindow.Show();
        }

        private void AddWordButton_Click(object sender, RoutedEventArgs e)
        {
            ImportWordWindow imw = new ImportWordWindow();
            Close();
            imw.Show();
        }

        private void DatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            DatabaseWindow dbw = new DatabaseWindow();
            Close();
            dbw.Show();
        }
    }
}
