using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PanelNikeStore
{
    public partial class ProductManagementControl : UserControl
    {
        private MySqlConnection connection;
        private List<ProductItem> products = new List<ProductItem>();

        public ProductManagementControl()
        {
            InitializeComponent();
            this.connection = DB.SeConnecter();

            // Charger la liste des produits au démarrage
            LoadProducts();
        }

        private void LoadProducts(string searchQuery = "")
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = @"
                    SELECT p.id, p.nom, p.description, p.prix, p.stock_total, p.type_produit,
                           pi.url_image 
                    FROM produits p
                    LEFT JOIN produit_variantes pv ON p.id = pv.produit_id AND pv.is_main = 1
                    LEFT JOIN produit_images pi ON pv.id = pi.variante_id AND pi.is_main = 1
                    WHERE 1=1";

                // Ajouter la recherche si nécessaire
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query += " AND (p.nom LIKE @search OR p.description LIKE @search OR p.type_produit LIKE @search)";
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
                        products.Add(new ProductItem
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["nom"].ToString(),
                            Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : "",
                            Price = Convert.ToDecimal(reader["prix"]),
                            TotalStock = Convert.ToInt32(reader["stock_total"]),
                            ProductType = reader["type_produit"].ToString(),
                            ImageUrl = reader["url_image"] != DBNull.Value ? reader["url_image"].ToString() : "",
                        });
                    }
                }

                // Mettre à jour la ListView
                ProductsList.ItemsSource = null;
                ProductsList.ItemsSource = products;

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

        private void TxtSearchProduct_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Si le textbox a le focus et n'est pas en placeholder
            if (txtSearchProduct.IsFocused && txtSearchProduct.Foreground.ToString() != "#FFBDBDBD")
            {
                LoadProducts(txtSearchProduct.Text);
            }
        }

        private void BtnSearchProduct_Click(object sender, RoutedEventArgs e)
        {
            // Réinitialiser la couleur du texte
            if (txtSearchProduct.Foreground.ToString() == "#FFBDBDBD")
            {
                txtSearchProduct.Text = "";
                txtSearchProduct.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#212121"));
            }

            LoadProducts(txtSearchProduct.Text);
        }

        private void BtnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int productId = Convert.ToInt32(btn.Tag);

                // Trouver le produit correspondant dans la liste
                ProductItem product = products.Find(p => p.Id == productId);

                if (product != null)
                {
                    // Demander confirmation avant de supprimer
                    MessageBoxResult result = MessageBox.Show(
                        $"Êtes-vous sûr de vouloir supprimer le produit '{product.Name}' ?\n\nCette action est irréversible et supprimera également toutes les variantes, tailles et images associées.",
                        "Confirmation de suppression",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Ouvrir la connexion si nécessaire
                            if (connection.State != System.Data.ConnectionState.Open)
                                connection.Open();

                            // Utiliser une transaction pour s'assurer que tout est supprimé ou rien
                            using (MySqlTransaction transaction = connection.BeginTransaction())
                            {
                                try
                                {
                                    // 1. Supprimer les images des variantes du produit
                                    string deleteImagesQuery = @"
                                DELETE pi FROM produit_images pi
                                INNER JOIN produit_variantes pv ON pi.variante_id = pv.id
                                WHERE pv.produit_id = @productId";

                                    using (MySqlCommand cmd = new MySqlCommand(deleteImagesQuery, connection, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@productId", productId);
                                        cmd.ExecuteNonQuery();
                                    }

                                    // 2. Supprimer les tailles des variantes du produit
                                    string deleteSizesQuery = @"
                                DELETE pt FROM produit_tailles pt
                                INNER JOIN produit_variantes pv ON pt.variante_id = pv.id
                                WHERE pv.produit_id = @productId";

                                    using (MySqlCommand cmd = new MySqlCommand(deleteSizesQuery, connection, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@productId", productId);
                                        cmd.ExecuteNonQuery();
                                    }

                                    // 3. Supprimer les variantes du produit
                                    string deleteVariantsQuery = @"
                                DELETE FROM produit_variantes
                                WHERE produit_id = @productId";

                                    using (MySqlCommand cmd = new MySqlCommand(deleteVariantsQuery, connection, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@productId", productId);
                                        cmd.ExecuteNonQuery();
                                    }

                                    // 4. Vérifier si le produit est utilisé dans des commandes
                                    string checkOrdersQuery = @"
                                SELECT COUNT(*) FROM details_commande
                                WHERE product_id = @productId";

                                    int orderCount = 0;
                                    using (MySqlCommand cmd = new MySqlCommand(checkOrdersQuery, connection, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@productId", productId);
                                        orderCount = Convert.ToInt32(cmd.ExecuteScalar());
                                    }

                                    if (orderCount > 0)
                                    {
                                        // Afficher un avertissement si le produit est utilisé dans des commandes
                                        MessageBoxResult orderResult = MessageBox.Show(
                                            $"Le produit '{product.Name}' est utilisé dans {orderCount} commande(s). " +
                                            $"Souhaitez-vous quand même le supprimer ? Cela pourrait affecter l'historique des commandes.",
                                            "Avertissement",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Warning);

                                        if (orderResult == MessageBoxResult.No)
                                        {
                                            // Annuler la transaction si l'utilisateur ne souhaite pas continuer
                                            transaction.Rollback();
                                            return;
                                        }
                                    }

                                    // 5. Finalement, supprimer le produit lui-même
                                    string deleteProductQuery = @"
                                DELETE FROM produits
                                WHERE id = @productId";

                                    using (MySqlCommand cmd = new MySqlCommand(deleteProductQuery, connection, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@productId", productId);
                                        cmd.ExecuteNonQuery();
                                    }

                                    // Valider toutes les suppressions
                                    transaction.Commit();

                                    // Notifier l'utilisateur
                                    MessageBox.Show(
                                        $"Le produit '{product.Name}' a été supprimé avec succès.",
                                        "Suppression réussie",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);

                                    // Mettre à jour la liste des produits
                                    LoadProducts(txtSearchProduct.Text);
                                }
                                catch (Exception ex)
                                {
                                    // Annuler toutes les modifications en cas d'erreur
                                    transaction.Rollback();
                                    throw ex;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Erreur lors de la suppression du produit: {ex.Message}",
                                "Erreur",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void BtnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int productId = Convert.ToInt32(btn.Tag);

                // Trouver le produit correspondant
                ProductItem product = products.Find(p => p.Id == productId);

                if (product != null)
                {
                    // Ouvrir la fenêtre de modification
                    EditProductWindow editWindow = new EditProductWindow(product);

                    // Réagir à la fermeture de la fenêtre
                    editWindow.Closed += (s, args) => {
                        // Recharger la liste des produits pour afficher les modifications
                        LoadProducts(txtSearchProduct.Text);
                    };

                    editWindow.ShowDialog();
                }
            }
        }
    }


    // Classe pour représenter un produit dans la liste
    public class ProductItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int TotalStock { get; set; }
        public string ProductType { get; set; }
        public string ImageUrl { get; set; }
    }
}