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
        const uint mask = 4294967294;

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
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (sfd.ShowDialog() == true)
                File.WriteAllText(sfd.FileName, editMessageWindow.message.Text);
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
                    uint pixel = (uint)bitmap.GetPixel(x, y).ToArgb();
                    pixel = (pixel & mask) + bits[i];
                    bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb((int)pixel));
                }

            bitmap.Save(filename);

            MessageBox.Show("Message has been successfully hidden", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private byte[] BytesToBits(byte[] input)
        {
            byte[] output = new byte[(input.Length + 2) * 8];
            for (int i = 0; i < 16; i++)
            {
                int current = input.Length & Convert.ToInt32(Math.Pow(2, 15 - i % 16));
                output[i] = current == 0 ? (byte)0 : (byte)1;
            }
            for (int i = 16; i < (input.Length + 2) * 8; i++)
            {
                byte current = Convert.ToByte(input[(i / 8) - 2] & Convert.ToByte(Math.Pow(2, 7 - i % 8)));
                output[i] = current == 0 ? (byte)0 : (byte)1;
            }
            return output;
        }

        private void buttonGetMessage_Click(object sender, RoutedEventArgs e)
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

            PassPhraseWindow passPhraseWindow = new PassPhraseWindow();
            passPhraseWindow.Owner = this;
            if (passPhraseWindow.ShowDialog() == false)
                return;

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

            byte[] bits = new byte[bitmap.Width * bitmap.Height];
            // Getting message from the bitmap
            int i = 0;
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++, i++)
                {
                    bits[i] = (byte)(bitmap.GetPixel(x, y).ToArgb() & 1);
                }

            try
            {
                byte[] encryptedMessage = BitsToBytes(bits);

                using (RijndaelManaged cipher = new RijndaelManaged())
                {
                    cipher.BlockSize = 128;
                    cipher.Key = key;
                    cipher.Mode = CipherMode.ECB;
                    ICryptoTransform t = cipher.CreateDecryptor();

                    try
                    {
                        byte[] message = t.TransformFinalBlock(encryptedMessage, 0, encryptedMessage.Length);
                        editMessageWindow.message.Text = Encoding.Default.GetString(message);
                    }
                    catch (CryptographicException)
                    {
                        MessageBox.Show("Unable to decrypt a message. Either a passphrase is incorrect, or message has been corrupted", "Decryption error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Message has been corrupted or this image does not contain a message", "Read fail", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private byte[] BitsToBytes(byte[] bits)
        {
            int length = 0;
            for (int i = 0; i < 16; i++)
            {
                length <<= 1;
                length += bits[i];
            }

            byte[] res = new byte[length];

            try
            {
                for (int i = 0; i < length; i++)
                {
                    byte current = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        current <<= 1;
                        current += bits[(i + 2) * 8 + j];
                    }
                    res[i] = current;
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new ArgumentException();
            }
            return res;
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            editMessageWindow = new EditMessageWindow();
            editMessageWindow.Owner = this;
        }
    }
}
