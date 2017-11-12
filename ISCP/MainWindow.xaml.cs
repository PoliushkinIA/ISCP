using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ISCP
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        EditMessageWindow editMessageWindow;

        public MainWindow()
        {
            InitializeComponent();
            editMessageWindow = new EditMessageWindow();
        }

        private void quit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonChooseSIF_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonOpenMessageText_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonEditMessageText_Click(object sender, RoutedEventArgs e)
        {
            editMessageWindow.Owner = this;
            editMessageWindow.Show();
        }

        private void new_Click(object sender, RoutedEventArgs e)
        {
            editMessageWindow.message.Text = "";
        }

        private void buttonSaveMessageText_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonHideMessage_Click(object sender, RoutedEventArgs e)
        {
            PassPhraseWindow passPhraseWindow = new PassPhraseWindow();
            passPhraseWindow.Owner = this;
            passPhraseWindow.ShowDialog();
        }

        private void buttonGetMessage_Click(object sender, RoutedEventArgs e)
        {
            PassPhraseWindow passPhraseWindow = new PassPhraseWindow();
            passPhraseWindow.Owner = this;
            passPhraseWindow.ShowDialog();
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
