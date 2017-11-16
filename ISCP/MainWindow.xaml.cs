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
using System.Drawing;
using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;

namespace ISCP
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        EditMessageWindow editMessageWindow;
        Bitmap bitmap;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void quit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonChooseSIF_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
               sourceImageFileName.Text = ofd.FileName;
        }

        private void buttonOpenMessageText_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (ofd.ShowDialog() == true)
                editMessageWindow.message.Text = File.ReadAllText(ofd.FileName, Encoding.Default);
        }

        private void buttonEditMessageText_Click(object sender, RoutedEventArgs e)
        {
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
            try
            {
                bitmap = new Bitmap(sourceImageFileName.Text);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Path entered is not valid or chosen file is not a correct image file!", "Invalid path or file", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (((editMessageWindow.message.Text.Length) / 16 + 1) * 128 + 8 > bitmap.Size.Height * bitmap.Size.Width)
            {
                MessageBox.Show("Message is too long to fit the container!", "Not enough space", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PassPhraseWindow passPhraseWindow = new PassPhraseWindow();
            passPhraseWindow.Owner = this;
            if (passPhraseWindow.ShowDialog() == false)
                return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap files (*.bmp)|*.bmp|All files (*.*)|*.*";
            if (sfd.ShowDialog() == false)
                return;
            string filename = sfd.FileName;

            byte[] key = null;
            try
            {
                MD5Cng hasher = new MD5Cng();
                key = hasher.ComputeHash(Encoding.Default.GetBytes(passPhraseWindow.password.Password));
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("Passphrase cannot be empty!" + ex.ToString(), "Empty password", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            byte[] encryptedMessage;
            using (RijndaelManaged cipher = new RijndaelManaged())
            {
                cipher.BlockSize = 128;
                cipher.Key = key;
                cipher.Mode = CipherMode.ECB;
                ICryptoTransform t = cipher.CreateEncryptor();

                byte[] message = Encoding.Default.GetBytes(editMessageWindow.message.Text);
                encryptedMessage = t.TransformFinalBlock(message, 0, message.Length);
            }

            byte[] bits = BytesToBits(encryptedMessage);

            // Hiding the message in a bitmap
            int i = 0;
            for (int y = 0; y < bitmap.Height && i < bits.Length; y++)
                for (int x = 0; x < bitmap.Width && i < bits.Length; x++, i++)
                {
                    int pixel = bitmap.GetPixel(x, y).ToArgb();
                    pixel = pixel & 244 + bits[i];
                    bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(pixel));
                }

            bitmap.Save(filename);

            MessageBox.Show("Message has been successfully hidden", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private byte[] BytesToBits(byte[] input)
        {
            byte[] output = new byte[(input.Length + 1) * 8];
            for (int i = 0; i < input.Length * 8; i++)
            {
                byte current = Convert.ToByte(input[i / 8] & Convert.ToByte(Math.Pow(2, 7 - i % 8)));
                output[i] = current == 0 ? (byte)1 : (byte)0;
            }
            for (int i = input.Length * 8; i < (input.Length + 1) * 8; i++)
                output[i] = 0;
            return output;
        }

        private void buttonGetMessage_Click(object sender, RoutedEventArgs e)
        {
            PassPhraseWindow passPhraseWindow = new PassPhraseWindow();
            passPhraseWindow.Owner = this;
            passPhraseWindow.ShowDialog();

           /* using (RijndaelManaged cipher = new RijndaelManaged())
            {
                cipher.BlockSize = 128;
                cipher.Key = key;
                cipher.Mode = CipherMode.ECB;
                ICryptoTransform t2 = cipher.CreateDecryptor();

                byte[] message = t2.TransformFinalBlock(encryptedMessage, 0, encryptedMessage.Length);
                MessageBox.Show(Encoding.Default.GetString(message));
            } */
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            editMessageWindow = new EditMessageWindow();
            editMessageWindow.Owner = this;
        }
    }
}
