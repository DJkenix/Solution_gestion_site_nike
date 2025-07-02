using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour StockManagementControl.xaml
    /// </summary>
    public partial class StockManagementControl : UserControl
    {
        private MySqlConnection connection;
        private List<ProductViewModel> products = new List<ProductViewModel>();

        public StockManagementControl()
        {
            InitializeComponent();
            this.connection = DB.SeConnecter();
            // Initialiser la recherche avec un texte vide pour charger tous les produits
            LoadProducts();
        }

        private void LoadProducts(string searchQuery = "")
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = @"
                    SELECT p.id, p.nom, p.description, p.prix, p.stock_total, 
                           pi.url_image 
                    FROM produits p
                    LEFT JOIN produit_variantes pv ON p.id = pv.produit_id AND pv.is_main = 1
                    LEFT JOIN produit_images pi ON pv.id = pi.variante_id AND pi.is_main = 1
                    WHERE 1=1";

                // Ajouter la recherche si nécessaire
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query += " AND (p.nom LIKE @search OR p.description LIKE @search)";
                }

                query += " GROUP BY p.id ORDER BY p.nom";

                MySqlCommand cmd = new MySqlCommand(query, connection);

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    cmd.Parameters.AddWithValue("@search", $"%{searchQuery}%");
                }

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    products.Clear();

                    while (reader.Read())
                    {
                        products.Add(new ProductViewModel
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["nom"].ToString(),
                            Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : "",
                            Price = Convert.ToDecimal(reader["prix"]),
                            TotalStock = Convert.ToInt32(reader["stock_total"]),
                            ImageUrl = reader["url_image"] != DBNull.Value ? reader["url_image"].ToString() : "",
                        });
                    }
                }

                // Mettre à jour la vue
                ProductsItemsControl.ItemsSource = null;
                ProductsItemsControl.ItemsSource = products;

                // Si pas de produits trouvés, afficher un message
                if (products.Count == 0 && !string.IsNullOrEmpty(searchQuery))
                {
                    MessageBox.Show($"Aucun produit trouvé pour la recherche : {searchQuery}",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des produits: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            // Réinitialiser la couleur du texte car l'utilisateur a cliqué explicitement pour rechercher
            if (txtSearch.Foreground.ToString() == "#FF757575")
            {
                txtSearch.Text = "";
                txtSearch.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#212121"));
            }

            LoadProducts(txtSearch.Text);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Si le texte n'est pas le placeholder et le TextBox a le focus
            if (txtSearch.IsFocused && txtSearch.Foreground.ToString() != "#FF757575")
            {
                LoadProducts(txtSearch.Text);
            }
        }

        private void ViewProductDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int productId = Convert.ToInt32(btn.Tag);

                // Trouver le produit correspondant
                var product = products.FirstOrDefault(p => p.Id == productId);

                if (product != null)
                {
                    StockDetailsPopup detailsPopup = new StockDetailsPopup(productId, product.Name);

                    // Configurer le gestionnaire d'événements pour les mises à jour
                    detailsPopup.StockUpdated += (s, args) => {
                        // Recharger les produits après modification
                        LoadProducts(txtSearch.Text);
                    };

                    detailsPopup.ShowDialog();
                }
            }
        }
    }

    // Modèle de vue pour les produits
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int TotalStock { get; set; }
        public string ImageUrl { get; set; }
    }
}