using System.Windows;
using System.Windows.Controls;

namespace PanelNikeStore
{
    public partial class ModificationReasonDialog : Window
    {
        public string ModificationReason { get; private set; }

        public ModificationReasonDialog()
        {
            InitializeComponent();

            // Sélectionner la première raison par défaut
            cmbReason.SelectedIndex = 0;
        }

        private void CmbReason_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Vérifier si "Autre" est sélectionné
            if (cmbReason.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Content.ToString() == "Autre")
            {
                // Afficher les contrôles pour la raison personnalisée
                txtCustomReasonLabel.Visibility = Visibility.Visible;
                txtCustomReason.Visibility = Visibility.Visible;
                txtCustomReason.Focus();
            }
            else
            {
                // Masquer les contrôles pour la raison personnalisée
                txtCustomReasonLabel.Visibility = Visibility.Collapsed;
                txtCustomReason.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // S'assurer qu'une raison est sélectionnée
            if (cmbReason.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedReason = selectedItem.Content.ToString();

                if (selectedReason == "Autre")
                {
                    // Vérifier si une raison personnalisée a été saisie
                    if (string.IsNullOrWhiteSpace(txtCustomReason.Text))
                    {
                        MessageBox.Show("Veuillez préciser la raison.",
                            "Raison requise", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtCustomReason.Focus();
                        return;
                    }

                    ModificationReason = txtCustomReason.Text.Trim();
                }
                else
                {
                    ModificationReason = selectedReason;
                }

                // Fermer la boîte de dialogue avec succès
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une raison.",
                    "Raison requise", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Fermer la boîte de dialogue sans succès
            DialogResult = false;
        }
    }
}