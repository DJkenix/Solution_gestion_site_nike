using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PanelNikeStore
{
    public partial class SimpleQuantityDialog : Window
    {
        public int Quantity { get; private set; }

        public SimpleQuantityDialog(string productName, int currentQuantity)
        {
            InitializeComponent();

            // Définir les informations du produit et la quantité actuelle
            txtProductInfo.Text = $"Nouvelle quantité pour {productName} (actuellement: {currentQuantity}):";
            txtQuantity.Text = currentQuantity.ToString();

            // Sélectionner tout le texte pour faciliter la modification
            txtQuantity.SelectAll();
            txtQuantity.Focus();
        }

        private void TxtQuantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Accepter uniquement les chiffres
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtQuantity.Text, out int quantity) && quantity > 0)
            {
                Quantity = quantity;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Veuillez entrer une quantité valide (nombre entier positif).",
                    "Quantité invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        // Gérer la touche Entrée pour confirmer
        private void TxtQuantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnConfirm_Click(sender, e);
            }
        }

        // Boutons pour incrémenter/décrémenter la quantité
        private void BtnDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtQuantity.Text, out int currentValue) && currentValue > 1)
            {
                txtQuantity.Text = (currentValue - 1).ToString();
            }
        }

        private void BtnIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtQuantity.Text, out int currentValue))
            {
                txtQuantity.Text = (currentValue + 1).ToString();
            }
        }
    }
}