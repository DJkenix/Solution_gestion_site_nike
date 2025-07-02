using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour SelectVariantDialog.xaml
    /// </summary>
    public partial class SelectVariantDialog : Window
    {
        public int SelectedVariantId { get; private set; }

        public SelectVariantDialog(List<ProductVariantViewModel> variants)
        {
            InitializeComponent();

            // Initialiser la liste des variantes
            lstVariants.ItemsSource = variants;

            // Si une seule variante est disponible, la sélectionner automatiquement
            if (variants.Count == 1)
            {
                lstVariants.SelectedIndex = 0;
                btnSelect.IsEnabled = true;
            }
        }

        private void LstVariants_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Activer le bouton de sélection uniquement si une variante est sélectionnée
            btnSelect.IsEnabled = lstVariants.SelectedItem != null;
        }

        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (lstVariants.SelectedItem is ProductVariantViewModel selectedVariant)
            {
                SelectedVariantId = selectedVariant.Id;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une variante.",
                    "Sélection requise", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}