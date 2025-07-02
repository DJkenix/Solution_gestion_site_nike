using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PanelNikeStore
{
    public partial class ChangeProductDialog : Window
    {
        private MySqlConnection connection;

        public int SelectedProductId { get; private set; }
        public int SelectedVariantId { get; private set; }
        public int SelectedSizeId { get; private set; }
        public int SelectedQuantity { get; private set; }

        public ChangeProductDialog()
        {
            InitializeComponent();
            this.connection = DB.SeConnecter();

            // Initialiser les contrôles
            cmbQuantity.SelectedIndex = 0; // 1 par défaut

            // Charger les produits
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = "SELECT id, nom FROM produits ORDER BY nom";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    DataTable productsTable = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(productsTable);
                    }

                    cmbProduct.ItemsSource = productsTable.DefaultView;
                    cmbProduct.DisplayMemberPath = "nom";
                    cmbProduct.SelectedValuePath = "id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des produits: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProduct.SelectedValue != null)
            {
                int productId = Convert.ToInt32(cmbProduct.SelectedValue);
                LoadVariants(productId);
            }
            else
            {
                cmbVariant.ItemsSource = null;
                cmbSize.ItemsSource = null;
            }
        }

        private void LoadVariants(int productId)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = "SELECT id, couleur FROM produit_variantes WHERE produit_id = @productId";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@productId", productId);

                    DataTable variantsTable = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(variantsTable);
                    }

                    cmbVariant.ItemsSource = variantsTable.DefaultView;
                    cmbVariant.DisplayMemberPath = "couleur";
                    cmbVariant.SelectedValuePath = "id";

                    // Sélectionner la première variante par défaut
                    if (variantsTable.Rows.Count > 0)
                    {
                        cmbVariant.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des variantes: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbVariant_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbVariant.SelectedValue != null)
            {
                int variantId = Convert.ToInt32(cmbVariant.SelectedValue);
                LoadSizes(variantId);

                // Charger l'image de la variante
                LoadVariantImage(variantId);
            }
            else
            {
                cmbSize.ItemsSource = null;
                imgProduct.Source = null;
            }
        }

        private void LoadSizes(int variantId)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = "SELECT id, taille, stock FROM produit_tailles WHERE variante_id = @variantId AND stock > 0";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@variantId", variantId);

                    DataTable sizesTable = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(sizesTable);
                    }

                    cmbSize.ItemsSource = sizesTable.DefaultView;
                    cmbSize.DisplayMemberPath = "taille";
                    cmbSize.SelectedValuePath = "id";

                    // Sélectionner la première taille par défaut
                    if (sizesTable.Rows.Count > 0)
                    {
                        cmbSize.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des tailles: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadVariantImage(int variantId)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = "SELECT url_image FROM produit_images WHERE variante_id = @variantId AND is_main = 1 LIMIT 1";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@variantId", variantId);

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        string imageUrl = result.ToString();

                        try
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(imageUrl);
                            bitmap.EndInit();
                            imgProduct.Source = bitmap;
                        }
                        catch
                        {
                            // Si l'image ne peut pas être chargée
                            imgProduct.Source = null;
                        }
                    }
                    else
                    {
                        imgProduct.Source = null;
                    }
                }
            }
            catch (Exception)
            {
                imgProduct.Source = null;
            }
        }

        private void CmbSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSize.SelectedItem != null)
            {
                // Récupérer le stock disponible
                int stock = Convert.ToInt32(((DataRowView)cmbSize.SelectedItem)["stock"]);

                // Mettre à jour les quantités disponibles
                cmbQuantity.Items.Clear();
                for (int i = 1; i <= Math.Min(stock, 10); i++) // Limiter à 10 maximum
                {
                    cmbQuantity.Items.Add(i);
                }

                // Sélectionner la première quantité
                cmbQuantity.SelectedIndex = 0;
            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProduct.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner un produit.",
                    "Sélection requise", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbVariant.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner une variante.",
                    "Sélection requise", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbSize.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner une taille.",
                    "Sélection requise", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbQuantity.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner une quantité.",
                    "Sélection requise", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Enregistrer les sélections
            SelectedProductId = Convert.ToInt32(cmbProduct.SelectedValue);
            SelectedVariantId = Convert.ToInt32(cmbVariant.SelectedValue);
            SelectedSizeId = Convert.ToInt32(cmbSize.SelectedValue);
            SelectedQuantity = Convert.ToInt32(cmbQuantity.SelectedItem);

            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}