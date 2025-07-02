using System.Windows;
using System.Windows.Controls;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour InputColorDialog.xaml
    /// </summary>
    public partial class InputColorDialog : Window
    {
        public string SelectedColor { get; private set; }

        public InputColorDialog()
        {
            InitializeComponent();

            // Ajouter un gestionnaire d'événements pour la sélection de couleur
            cmbColorSelect.SelectionChanged += CmbColorSelect_SelectionChanged;
        }

        private void CmbColorSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Vérifier si "Autre..." est sélectionné
            if (cmbColorSelect.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag != null && selectedItem.Tag.ToString() == "other")
            {
                // Afficher le champ de texte personnalisé
                txtCustomColorLabel.Visibility = Visibility.Visible;
                txtCustomColor.Visibility = Visibility.Visible;
                txtCustomColor.Focus();
            }
            else
            {
                // Masquer le champ de texte personnalisé
                txtCustomColorLabel.Visibility = Visibility.Collapsed;
                txtCustomColor.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // Si "Autre..." est sélectionné, utiliser la valeur personnalisée
            if (cmbColorSelect.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag != null && selectedItem.Tag.ToString() == "other")
            {
                if (string.IsNullOrWhiteSpace(txtCustomColor.Text))
                {
                    MessageBox.Show("Veuillez saisir une couleur personnalisée.",
                        "Couleur requise", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtCustomColor.Focus();
                    return;
                }

                SelectedColor = txtCustomColor.Text.Trim();
            }
            else if (cmbColorSelect.SelectedItem is ComboBoxItem item)
            {
                // Utiliser la couleur sélectionnée dans la liste
                SelectedColor = item.Content.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une couleur.",
                    "Couleur requise", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbColorSelect.Focus();
                return;
            }

            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}