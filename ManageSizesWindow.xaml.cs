using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour ManageSizesWindow.xaml
    /// </summary>
    public partial class ManageSizesWindow : Window
    {
        public List<ProductSizeViewModel> Sizes { get; private set; }
        private ObservableCollection<ProductSizeViewModel> sizeItems;
        private string variantColor;

        public ManageSizesWindow(string variantColor, List<ProductSizeViewModel> existingSizes)
        {
            InitializeComponent();

            this.variantColor = variantColor;

            // Mettre à jour l'info de la variante
            txtVariantInfo.Text = $"Variante: {variantColor}";

            // Initialiser la collection des tailles
            sizeItems = new ObservableCollection<ProductSizeViewModel>();

            // Ajouter les tailles existantes si présentes
            if (existingSizes != null && existingSizes.Count > 0)
            {
                foreach (var size in existingSizes)
                {
                    sizeItems.Add(new ProductSizeViewModel
                    {
                        Id = size.Id,
                        VariantId = size.VariantId,
                        Size = size.Size,
                        Stock = size.Stock
                    });
                }
            }

            // Associer la collection à la ListView
            lstSizes.ItemsSource = sizeItems;

            // Sélectionner la première taille disponible dans le ComboBox
            cmbSize.SelectedIndex = 0;
        }

        // Valider la saisie pour le champ stock (uniquement des nombres)
        private void TxtStock_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Ajouter une taille
        private void BtnAddSize_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer la taille sélectionnée
            if (cmbSize.SelectedItem is ComboBoxItem selectedItem)
            {
                int size = int.Parse(selectedItem.Content.ToString());

                // Vérifier si cette taille existe déjà
                if (sizeItems.Any(s => s.Size == size))
                {
                    MessageBox.Show($"La taille {size} existe déjà pour cette variante.",
                        "Taille existante", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Vérifier le stock
                if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
                {
                    MessageBox.Show("Veuillez saisir un stock valide (nombre entier positif).",
                        "Stock invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Ajouter la nouvelle taille
                sizeItems.Add(new ProductSizeViewModel
                {
                    Id = 0, // ID temporaire qui sera mis à jour lors de la sauvegarde
                    Size = size,
                    Stock = stock,
                    VariantId = 0 // VariantId temporaire qui sera mis à jour lors de la sauvegarde
                });

                // Sélectionner la taille suivante dans le ComboBox si possible
                int nextIndex = cmbSize.SelectedIndex + 1;
                if (nextIndex < cmbSize.Items.Count)
                {
                    cmbSize.SelectedIndex = nextIndex;
                }

                // Réinitialiser le stock à la valeur par défaut
                txtStock.Text = "10";
            }
        }

        // Supprimer une taille
        private void BtnRemoveSize_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                int sizeToRemove = int.Parse(button.Tag.ToString());

                // Trouver la taille à supprimer
                var sizeItem = sizeItems.FirstOrDefault(s => s.Size == sizeToRemove);

                if (sizeItem != null)
                {
                    // Confirmation
                    MessageBoxResult result = MessageBox.Show(
                        $"Êtes-vous sûr de vouloir supprimer la taille {sizeToRemove} ?",
                        "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Supprimer la taille
                        sizeItems.Remove(sizeItem);
                    }
                }
            }
        }

        // Valider et fermer
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // Vérifier qu'il y a au moins une taille
            if (sizeItems.Count == 0)
            {
                MessageBox.Show("Veuillez ajouter au moins une taille.",
                    "Aucune taille", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Copier les tailles dans la liste de retour
            Sizes = new List<ProductSizeViewModel>(sizeItems);

            // Fermer la fenêtre avec succès
            DialogResult = true;
        }

        // Annuler
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }

    // Classe pour représenter une taille de produit
    public class ProductSizeViewModel
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public int Size { get; set; }
        public int Stock { get; set; }
    }
}