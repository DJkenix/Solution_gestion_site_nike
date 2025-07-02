using System;
using System.Windows;
using System.Windows.Controls;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour AddSizeWindow.xaml
    /// </summary>
    public partial class AddSizeWindow : Window
    {
        public int Size { get; private set; }
        public int Stock { get; private set; }

        public AddSizeWindow()
        {
            InitializeComponent();

            // Remplir le ComboBox avec les tailles standard
            for (int i = 35; i <= 47; i++)
            {
                cmbSize.Items.Add(i);
            }

            // Sélectionner une taille par défaut
            cmbSize.SelectedItem = 42;

            // Définir un stock initial par défaut
            txtStock.Text = "10";
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSize.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner une taille.",
                    "Erreur de saisie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Veuillez saisir une quantité valide (nombre entier positif).",
                    "Erreur de saisie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Size = (int)cmbSize.SelectedItem;
            Stock = stock;

            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void TxtStock_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
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
            if (int.TryParse(txtStock.Text, out int currentValue))
            {
                if (currentValue > 0)
                {
                    txtStock.Text = (currentValue - 1).ToString();
                }
            }
        }

        private void BtnIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtStock.Text, out int currentValue))
            {
                txtStock.Text = (currentValue + 1).ToString();
            }
        }
    }
}