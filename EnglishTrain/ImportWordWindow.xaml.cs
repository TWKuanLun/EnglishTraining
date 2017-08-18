using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using static EnglishTrain.DataBase;

namespace EnglishTrain
{
    public partial class ImportWordWindow : Window
    {
        
        public ImportWordWindow()
        {
            InitializeComponent();
            AutoChangeWindowsFontSize autoChangeFontSize = new AutoChangeWindowsFontSize(this, 1920);
        }
        private void DataProcessing(string wordsString)//處理TextBox文字(將新單字匯入資料庫)
        {
            string[] words = wordsString.Split(new char[] { '\n' }); //TextBox文字以\n劃分各單字
            
            StringBuilder message = new StringBuilder();
            StringBuilder newText = new StringBuilder(); //讓成功新增的單字在Textbox上除去
            for (int i = 0; i < words.Length; i++)
            {
                string word = getVerbRoot(getSingularNoun(words[i]));//獲得動詞原形、單數名詞
                
                AddStatus success;
                addWordData(word, getHTML(word),out success);
                double pVlaue = (double)(i + 1) / words.Length * 100;
                //更新進度條
                pbStatus.Dispatcher.Invoke(() => pbStatus.Value = pVlaue, DispatcherPriority.Background);

                switch (success)
                {
                    case AddStatus.HaveWord:
                        message.Append($"{words[i]}失敗，已有此單字資料\n");
                        newText.Append($"{words[i]}\r\n");
                        break;
                    case AddStatus.SearchFail:
                        if (!words[i].Equals(String.Empty))
                        {
                            message.Append($"{words[i]}失敗，Yahoo查無此單字\n");
                            newText.Append($"{words[i]}\r\n");
                        }
                        break;
                    default:
                        break;
                }
                Thread.Sleep(500);//防止被誤認為DDOS
            }
            if(!message.ToString().Equals(String.Empty))
                MessageBox.Show(message.ToString(), "新增失敗");
            //if (!newText.ToString().Equals(String.Empty))
                inputTextBox.Text = newText.ToString();

            importButton.IsEnabled = true;
            BackButton.IsEnabled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            importButton.IsEnabled = false;
            BackButton.IsEnabled = false;
            DataProcessing(Regex.Replace(inputTextBox.Text, "[\r]", "", RegexOptions.IgnoreCase));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow();
            Close();
            mw.Show();
        }
    }
}
