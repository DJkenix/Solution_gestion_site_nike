using System;
using System.Windows;
using System.Windows.Controls;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour UpdateStockWindow.xaml
    /// </summary>
    public partial class UpdateStockWindow : Window
    {
        public int NewStock { get; private set; }
        private int currentSize;
        private int currentStock;

        public UpdateStockWindow(int size, int currentStock)
        {
            InitializeComponent();

            this.currentSize = size;
            this.currentStock = currentStock;

            // Mettre à jour l'interface utilisateur
            txtSize.Text = size.ToString();
            txtCurrentStock.Text = currentStock.ToString();
            txtNewStock.Text = currentStock.ToString();

            // Définir le focus sur le champ de saisie
            txtNewStock.Focus();
            txtNewStock.SelectAll();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtNewStock.Text, out int newStock))
            {
                if (newStock < 0)
                {
                    MessageBox.Show("Le stock ne peut pas être négatif.",
                        "Erreur de saisie", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                NewStock = newStock;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Veuillez saisir une valeur numérique valide.",
                    "Erreur de saisie", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void TxtNewStock_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Autoriser uniquement les chiffres
            e.Handled = !IsNumeric(e.Text);
        }

        private bool IsNumeric(string text)
        {
            return int.TryParse(text, out _);
        }

        // Boutons pour incrémenter/décrémenter
        private void BtnDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtNewStock.Text, out int currentValue))
            {
                if (currentValue > 0)
                {
                    txtNewStock.Text = (currentValue - 1).ToString();
                }
            }
        }

        private void BtnIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtNewStock.Text, out int currentValue))
            {
                txtNewStock.Text = (currentValue + 1).ToString();
            }
        }
    }
}