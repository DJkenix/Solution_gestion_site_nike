using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour EditImageDialog.xaml
    /// </summary>
    public partial class EditImageDialog : Window
    {
        public string ImageUrl { get; private set; }

        public EditImageDialog(string currentUrl)
        {
            InitializeComponent();

            // Initialiser avec l'URL actuelle si elle existe
            ImageUrl = currentUrl;
            txtImageUrl.Text = currentUrl;

            // Activer le support du copier-coller
            txtImageUrl.KeyDown += (s, e) => {
                if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (Clipboard.ContainsText())
                    {
                        txtImageUrl.Text = Clipboard.GetText();
                        e.Handled = true;
                    }
                }
            };

            // Charger l'aperçu si une URL est fournie
            if (!string.IsNullOrEmpty(currentUrl))
            {
                LoadImagePreview(currentUrl);
            }
        }

        private void BtnPaste_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                txtImageUrl.Text = Clipboard.GetText();
                txtImageUrl.Focus();
                txtImageUrl.SelectionStart = txtImageUrl.Text.Length;
            }
        }

        private void LoadImagePreview(string url)
        {
            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(url, UriKind.Absolute);
                    bitmap.EndInit();
                    imgPreview.Source = bitmap;
                }
                else
                {
                    imgPreview.Source = null;
                }
            }
            catch
            {
                imgPreview.Source = null;
                // Ne pas afficher de message d'erreur à chaque frappe
            }
        }

        private void TxtImageUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Mettre à jour l'aperçu à chaque changement d'URL
            LoadImagePreview(txtImageUrl.Text);
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // Validation de l'URL
            if (string.IsNullOrWhiteSpace(txtImageUrl.Text))
            {
                MessageBox.Show("Veuillez saisir une URL valide pour l'image.",
                                "URL requise", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Stocker l'URL et fermer la boîte de dialogue
            ImageUrl = txtImageUrl.Text;
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}