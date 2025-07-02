using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour StockDetailsPopup.xaml
    /// </summary>
    public partial class StockDetailsPopup : Window
    {
        private MySqlConnection connection;
        private int productId;
        private string productName;

        // Événement déclenché lorsque le stock est mis à jour
        public event EventHandler StockUpdated;

        // Collection observable pour les tailles et stocks
        public ObservableCollection<SizeStockViewModel> SizesStock { get; set; }

        public StockDetailsPopup(int productId, string productName)
        {
            InitializeComponent();

            this.productId = productId;
            this.productName = productName;
            this.connection = DB.SeConnecter();

            // Initialiser la collection observable
            SizesStock = new ObservableCollection<SizeStockViewModel>();

            // Définir le contexte de données
            this.DataContext = this;

            // Mettre à jour le titre
            txtProductName.Text = productName;

            // Charger les données
            LoadProductVariants();
        }

        private void LoadProductVariants()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                // Récupérer les variantes du produit
                string variantsQuery = @"
                    SELECT 
                        pv.id AS variante_id, 
                        pv.couleur,
                        pi.url_image
                    FROM 
                        produit_variantes pv
                    LEFT JOIN 
                        produit_images pi ON pv.id = pi.variante_id AND pi.is_main = 1
                    WHERE 
                        pv.produit_id = @productId
                    ORDER BY 
                        pv.is_main DESC, pv.couleur";

                MySqlCommand variantsCmd = new MySqlCommand(variantsQuery, connection);
                variantsCmd.Parameters.AddWithValue("@productId", productId);

                // Liste pour stocker les variantes
                List<VariantViewModel> variants = new List<VariantViewModel>();

                using (MySqlDataReader reader = variantsCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        variants.Add(new VariantViewModel
                        {
                            Id = Convert.ToInt32(reader["variante_id"]),
                            Color = reader["couleur"].ToString(),
                            ImageUrl = reader["url_image"] != DBNull.Value ? reader["url_image"].ToString() : ""
                        });
                    }
                }

                // Remplir le ComboBox des variantes
                cmbVariants.ItemsSource = variants;

                // Sélectionner la première variante si disponible
                if (variants.Count > 0)
                {
                    cmbVariants.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des variantes: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSizesForVariant(int variantId)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                // Vider la collection actuelle
                SizesStock.Clear();

                // Récupérer les tailles et stocks pour cette variante
                string sizesQuery = @"
                    SELECT 
                        pt.id AS taille_id,
                        pt.taille,
                        pt.stock
                    FROM 
                        produit_tailles pt
                    WHERE 
                        pt.variante_id = @variantId
                    ORDER BY 
                        pt.taille";

                MySqlCommand sizesCmd = new MySqlCommand(sizesQuery, connection);
                sizesCmd.Parameters.AddWithValue("@variantId", variantId);

                using (MySqlDataReader reader = sizesCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SizesStock.Add(new SizeStockViewModel
                        {
                            Id = Convert.ToInt32(reader["taille_id"]),
                            Size = Convert.ToInt32(reader["taille"]),
                            Stock = Convert.ToInt32(reader["stock"]),
                            VariantId = variantId
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des tailles: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbVariants_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbVariants.SelectedItem is VariantViewModel selectedVariant)
            {
                // Charger les tailles pour cette variante
                LoadSizesForVariant(selectedVariant.Id);

                // Mettre à jour l'image si disponible
                if (!string.IsNullOrEmpty(selectedVariant.ImageUrl))
                {
                    try
                    {
                        imgProduct.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(selectedVariant.ImageUrl));
                    }
                    catch
                    {
                        // Si l'image ne peut pas être chargée, utiliser une image par défaut
                        imgProduct.Source = null;
                    }
                }
                else
                {
                    imgProduct.Source = null;
                }
            }
        }

        private void BtnUpdateStock_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is SizeStockViewModel sizeStock)
            {
                // Créer une fenêtre pour mettre à jour le stock
                UpdateStockWindow updateWindow = new UpdateStockWindow(sizeStock.Size, sizeStock.Stock);

                if (updateWindow.ShowDialog() == true)
                {
                    int newStock = updateWindow.NewStock;

                    // Mettre à jour dans la base de données
                    UpdateStockInDatabase(sizeStock.Id, newStock);

                    // Mettre à jour l'UI
                    sizeStock.Stock = newStock;

                    // Déclencher l'événement de mise à jour
                    StockUpdated?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void BtnAddSize_Click(object sender, RoutedEventArgs e)
        {
            if (cmbVariants.SelectedItem is VariantViewModel selectedVariant)
            {
                // Créer une fenêtre pour ajouter une nouvelle taille
                AddSizeWindow addWindow = new AddSizeWindow();

                if (addWindow.ShowDialog() == true)
                {
                    int newSize = addWindow.Size;
                    int initialStock = addWindow.Stock;

                    // Vérifier si cette taille existe déjà
                    bool sizeExists = false;
                    foreach (var item in SizesStock)
                    {
                        if (item.Size == newSize)
                        {
                            sizeExists = true;
                            MessageBox.Show($"La taille {newSize} existe déjà pour cette variante.",
                                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        }
                    }

                    if (!sizeExists)
                    {
                        // Ajouter dans la base de données
                        int newSizeId = AddSizeToDatabase(selectedVariant.Id, newSize, initialStock);

                        if (newSizeId > 0)
                        {
                            // Ajouter à la liste
                            SizesStock.Add(new SizeStockViewModel
                            {
                                Id = newSizeId,
                                Size = newSize,
                                Stock = initialStock,
                                VariantId = selectedVariant.Id
                            });

                            // Déclencher l'événement de mise à jour
                            StockUpdated?.Invoke(this, EventArgs.Empty);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Veuillez d'abord sélectionner une variante.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateStockInDatabase(int sizeId, int newStock)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string updateQuery = @"
                    UPDATE produit_tailles 
                    SET stock = @newStock 
                    WHERE id = @sizeId";

                MySqlCommand cmd = new MySqlCommand(updateQuery, connection);
                cmd.Parameters.AddWithValue("@newStock", newStock);
                cmd.Parameters.AddWithValue("@sizeId", sizeId);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Stock mis à jour avec succès!",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Aucune mise à jour effectuée.",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour du stock: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int AddSizeToDatabase(int variantId, int size, int stock)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string insertQuery = @"
                    INSERT INTO produit_tailles (variante_id, taille, stock)
                    VALUES (@variantId, @taille, @stock);
                    SELECT LAST_INSERT_ID();";

                MySqlCommand cmd = new MySqlCommand(insertQuery, connection);
                cmd.Parameters.AddWithValue("@variantId", variantId);
                cmd.Parameters.AddWithValue("@taille", size);
                cmd.Parameters.AddWithValue("@stock", stock);

                int newId = Convert.ToInt32(cmd.ExecuteScalar());

                if (newId > 0)
                {
                    MessageBox.Show($"Taille {size} ajoutée avec succès!",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    return newId;
                }
                else
                {
                    MessageBox.Show("Erreur lors de l'ajout de la taille.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ajout de la taille: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class VariantViewModel
    {
        public int Id { get; set; }
        public string Color { get; set; }
        public string ImageUrl { get; set; }

        public override string ToString()
        {
            return Color;
        }
    }

    public class SizeStockViewModel
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public int Size { get; set; }
        public int Stock { get; set; }
    }
}