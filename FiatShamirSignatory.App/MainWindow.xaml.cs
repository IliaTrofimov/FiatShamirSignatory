using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

using Org.BouncyCastle.Math;

using FiatShamirSignatory.Models;
using FiatShamirSignatory.Algorythm;
using FiatShamirSignatory.App.Utils;


namespace FiatShamirSignatory.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string publicKeyExt = "publickey.xml";
        private const string privateKeyExt = "privatekey.txt";
        private const string signedMsgExt = "signed.xml";
        private const int keysCount = 5;
        private const int nLength = 8;
        
        internal ViewModels.MainWindowVM ViewModel { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }


        private void ErrorMessage(Exception ex)
        {
            MessageBox.Show($"{ex.Message}\n\n{ex.ToString()}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        private void ClearKeys_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PublicKey = null;
            ViewModel.PrivateKey = null;
            ViewModel.IsVerified = null;
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PublicKey = null;
            ViewModel.PrivateKey = null;
            ViewModel.IsVerified = null;
            ViewModel.Text = "";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Расчётное задание по КМЗИ\nЗадание: Реализация протокола цифровой подписи Фиата-Шамира\nВыполнил: Трофимов И.С., А-05-19",
                "О программе",
                MessageBoxButton.OK,
                MessageBoxImage.Question);
        }

        private void GenerateKeys_Click(object sender, RoutedEventArgs e)
        {
            var fs = new FSSignatory(keysCount, nLength);
            ViewModel.PublicKey = fs.PublicKey;
            ViewModel.PrivateKey = fs.PrivateA;
            ViewModel.SignatoryName = null;
        }

        private void SaveKeys_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.PublicKey == null)
            {
                MessageBox.Show("Невозможно сохранить пустой открытый ключ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (ViewModel.PrivateKey == null)
            {
                MessageBox.Show("Невозможно сохранить пустой закрытый ключ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var filename = SavePublicKeyDialog();
            if (filename != null)
            {
                try
                {
                    FilesService.SavePublickKey(ViewModel.PublicKey, filename);
                }
                catch (Exception ex)
                {
                    ErrorMessage(ex);
                }
            }

            filename = SavePrivateKeyDialog();
            if (filename != null)
            {
                try
                {
                    FilesService.SavePrivateKey(ViewModel.PrivateKey, filename);
                }
                catch (Exception ex)
                {
                    ErrorMessage(ex);
                }
            }
        }

        private void Verify_Click(object sender, RoutedEventArgs e)
        { 
            var pk = TryReadPublicKey();
            ViewModel.PrivateKey = null;
            ViewModel.PublicKey = pk;

            var message = ReadSignedMessage();
            if (message != null && ViewModel.PublicKey != null)
            {
                ViewModel.IsVerified = FSSignatory.Verify(message, ViewModel.PublicKey);
                ViewModel.Text = message.Message.UTF8();
            }
        }

        internal void Sign_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.PublicKey == null || ViewModel.PrivateKey == null)
            {
                ErrorMessage(new Exception("Невозможно подписать сообщение!\nНе заданы ключи."));
                return;
            }

            var filename = SaveSignedMsgDialog();
            if (filename == null)
                return;

            var fs = new FSSignatory(ViewModel.PublicKey, ViewModel.PrivateKey);
            
            var signed = fs.Sign(ViewModel.Text);
            try
            {
                FilesService.SaveSignedMessage(signed, filename);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        internal void LoadKeys_Click(object sender, RoutedEventArgs e)
        {
            var pk = TryReadPublicKey();
            if (pk == null) return;
            
            var a = TryReadPrivateKey();
            if (a == null) return;

            try
            {
                FSSignatory.VerifyKeys(a, pk);
                ViewModel.PublicKey = pk;
                ViewModel.PrivateKey = a;
            }
            catch (ArgumentException ex)
            {
                ErrorMessage(new Exception("Недопустимые ключи", ex));
            }
        }


        private PublicKey? TryReadPublicKey()
        {
            var filename = LoadPublicKeyDialog();
            if (filename == null)
                return null;

            try
            {
                return FilesService.ReadPublickKey(filename);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
                return null;
            }
        }
        
        private BigInteger[]? TryReadPrivateKey()
        {
            var filename = LoadPrivateKeyDialog();
            if (filename == null)
                return null;

            try
            {
                return FilesService.ReadPrivateKey(filename);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
                return null;
            }
        }

        private SignedMessage? ReadSignedMessage()
        {
            var filename = LoadSignedMsgDialog();
            if (filename == null)
                return null;

            try
            {
                return FilesService.ReadSignedMessage(filename);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
                return null;
            }
        }


        #region OpenFile dialogs
        private string? SavePublicKeyDialog()
        {
            SaveFileDialog dialog = new()
            {
                Title = "Сохранить открытый ключ для протокола Фиата-Шамира",
                AddExtension = true,
                FileName = "key",
                Filter = $"Открытый ключ|*.{publicKeyExt}"
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        private string? SavePrivateKeyDialog()
        {
            SaveFileDialog dialog = new()
            {
                Title = "Сохранить закрытый ключ для протокола Фиата-Шамира",
                AddExtension = true,
                FileName = "key",
                Filter = $"Закрытый ключ|*.{privateKeyExt}"
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        private string? LoadPublicKeyDialog()
        {
            OpenFileDialog dialog = new()
            {
                Title = "Загрузить открытый ключ для протокола Фиата-Шамира",
                AddExtension = true,
                Filter = $"Закрытый ключ|*.{publicKeyExt}"
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        private string? LoadPrivateKeyDialog()
        {
            OpenFileDialog dialog = new()
            {
                Title = "Загрузить закрытый ключ для протокола Фиата-Шамира",
                AddExtension = true,
                Filter = $"Закрытый ключ|*.{privateKeyExt}"
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        private string? LoadSignedMsgDialog()
        {
            OpenFileDialog dialog = new()
            {
                Title = "Загрузить подписанное сообщение",
                AddExtension = true,
                Filter = $"Подписанное сообщение|*.{signedMsgExt}"
            };
            if (dialog.ShowDialog() == true)
            {
                ViewModel.SignatoryName = dialog.FileName;
                return dialog.FileName;
            }
            return null;
        }

        private string? SaveSignedMsgDialog()
        {
            SaveFileDialog dialog = new()
            {
                Title = "Сохранить подписанное сообщение",
                AddExtension = true,
                FileName = "message",
                Filter = $"Подписанное сообщение|*.{signedMsgExt}"
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
        #endregion
    }
}
