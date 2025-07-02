using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace PanelNikeStore
{
    public partial class EditProductWindow : Window
    {
        private MySqlConnection connection;
        private ProductItem product;

        // Collection observable pour les images du produit
        public ObservableCollection<ProductImageViewModel> ProductImages { get; set; }

        // L'image actuellement sélectionnée
        private ProductImageViewModel currentSelectedImage;

        // Définition de la classe ProductImageViewModel
        public class ProductImageViewModel
        {
            public int Id { get; set; }
            public int VariantId { get; set; }
            public string ImageUrl { get; set; }
            public bool IsMain { get; set; }
        }

        public EditProductWindow(ProductItem product)
        {
            InitializeComponent();
            this.connection = DB.SeConnecter();
            this.product = product;

            // Initialiser la collection d'images
            ProductImages = new ObservableCollection<ProductImageViewModel>();

            // Définir le contexte de données pour le binding
            this.DataContext = this;

            // Remplir les champs avec les données du produit
            LoadProductData();

            // Charger les images du produit
            LoadProductImages();
        }

        private void LoadProductData()
        {
            txtId.Text = product.Id.ToString();
            txtName.Text = product.Name;
            txtDescription.Text = product.Description;
            txtPrice.Text = product.Price.ToString("0.00");
            txtStock.Text = product.TotalStock.ToString();

            // Sélectionner le type de produit dans la combobox
            foreach (ComboBoxItem item in cmbType.Items)
            {
                if (item.Content.ToString() == product.ProductType)
                {
                    cmbType.SelectedItem = item;
                    break;
                }
            }

            // Sélectionner le genre dans la combobox
            // Récupérer d'abord le genre du produit
            string genre = GetProductGenre(product.Id);
            foreach (ComboBoxItem item in cmbGenre.Items)
            {
                if (item.Content.ToString() == genre)
                {
                    cmbGenre.SelectedItem = item;
                    break;
                }
            }
        }

        private string GetProductGenre(int productId)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = "SELECT genre FROM produits WHERE id = @productId";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@productId", productId);
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "Homme"; // Par défaut "Homme" si null
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la récupération du genre: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return "Homme"; // Valeur par défaut en cas d'erreur
            }
        }

        private void LoadProductImages()
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                // Récupérer toutes les variantes du produit
                string variantsQuery = "SELECT id FROM produit_variantes WHERE produit_id = @productId";

                using (MySqlCommand variantsCmd = new MySqlCommand(variantsQuery, connection))
                {
                    variantsCmd.Parameters.AddWithValue("@productId", product.Id);

                    using (MySqlDataReader variantsReader = variantsCmd.ExecuteReader())
                    {
                        // Collecter tous les IDs de variantes
                        var variantIds = new System.Collections.Generic.List<int>();
                        while (variantsReader.Read())
                        {
                            variantIds.Add(Convert.ToInt32(variantsReader["id"]));
                        }
                        variantsReader.Close();

                        // Pour chaque variante, récupérer ses images
                        foreach (int variantId in variantIds)
                        {
                            string imagesQuery = @"
                                SELECT id, url_image, is_main 
                                FROM produit_images 
                                WHERE variante_id = @variantId";

                            using (MySqlCommand imagesCmd = new MySqlCommand(imagesQuery, connection))
                            {
                                imagesCmd.Parameters.AddWithValue("@variantId", variantId);

                                using (MySqlDataReader imagesReader = imagesCmd.ExecuteReader())
                                {
                                    while (imagesReader.Read())
                                    {
                                        var image = new ProductImageViewModel
                                        {
                                            Id = Convert.ToInt32(imagesReader["id"]),
                                            VariantId = variantId,
                                            ImageUrl = imagesReader["url_image"].ToString(),
                                            IsMain = Convert.ToBoolean(imagesReader["is_main"])
                                        };

                                        ProductImages.Add(image);

                                        // Si c'est l'image principale, la définir comme sélectionnée
                                        if (image.IsMain)
                                        {
                                            currentSelectedImage = image;
                                            LoadImagePreview(image.ImageUrl);
                                            txtMainImageIndicator.Visibility = Visibility.Visible;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Si aucune image n'a été définie comme principale, utiliser la première image
                if (currentSelectedImage == null && ProductImages.Count > 0)
                {
                    currentSelectedImage = ProductImages[0];
                    LoadImagePreview(currentSelectedImage.ImageUrl);
                    txtMainImageIndicator.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des images: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadImagePreview(string imageUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imageUrl, UriKind.Absolute);
                    bitmap.EndInit();
                    imgPreview.Source = bitmap;
                }
                else
                {
                    imgPreview.Source = null;
                }
            }
            catch
            {
                imgPreview.Source = null;
            }
        }

        // Méthode corrigée pour la sélection d'image depuis un TextBlock
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag is ProductImageViewModel image)
            {
                // Mettre à jour l'image sélectionnée
                currentSelectedImage = image;

                // Mettre à jour l'aperçu avec cette image
                LoadImagePreview(image.ImageUrl);

                // Afficher l'indicateur d'image principale si nécessaire
                txtMainImageIndicator.Visibility = image.IsMain ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProductImageViewModel image)
            {
                // Mettre à jour l'image sélectionnée
                currentSelectedImage = image;

                // Mettre à jour l'aperçu avec cette image
                LoadImagePreview(image.ImageUrl);

                // Afficher l'indicateur d'image principale si nécessaire
                txtMainImageIndicator.Visibility = image.IsMain ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void BtnSetAsMain_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProductImageViewModel image)
            {
                // Définir cette image comme principale dans la base de données
                SetImageAsMain(image);

                // Mettre à jour les statuts localement
                foreach (var img in ProductImages)
                {
                    img.IsMain = (img.Id == image.Id);
                }

                // Mettre à jour l'aperçu avec cette image
                currentSelectedImage = image;
                LoadImagePreview(image.ImageUrl);
                txtMainImageIndicator.Visibility = Visibility.Visible;

                // Rafraîchir la liste pour mettre à jour les indicateurs d'image principale
                lstImages.Items.Refresh();
            }
        }

        private void BtnEditImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProductImageViewModel image)
            {
                // Ouvrir une boîte de dialogue pour modifier l'URL de l'image
                EditImageDialog dialog = new EditImageDialog(image.ImageUrl);
                dialog.Owner = this; // Définir l'Owner pour un bon positionnement

                if (dialog.ShowDialog() == true)
                {
                    // Si confirmé, mettre à jour l'URL dans la base de données
                    UpdateImageUrl(image.Id, dialog.ImageUrl);

                    // Mettre à jour l'URL dans la collection
                    image.ImageUrl = dialog.ImageUrl;

                    // Si c'est l'image actuellement affichée, mettre à jour l'aperçu
                    if (currentSelectedImage == image)
                    {
                        LoadImagePreview(dialog.ImageUrl);
                    }

                    // Rafraîchir la liste
                    lstImages.Items.Refresh();
                }
            }
        }

        private void BtnDeleteImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProductImageViewModel image)
            {
                // Confirmation avant suppression
                MessageBoxResult result = MessageBox.Show(
                    "Êtes-vous sûr de vouloir supprimer cette image ?",
                    "Confirmation de suppression",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Supprimer l'image de la base de données
                    DeleteImage(image.Id, image.IsMain);

                    // Si c'était l'image principale et qu'il y a d'autres images, définir une nouvelle image principale
                    if (image.IsMain && ProductImages.Count > 1)
                    {
                        // Trouver la première image différente de celle supprimée
                        ProductImageViewModel newMain = null;
                        foreach (var img in ProductImages)
                        {
                            if (img.Id != image.Id)
                            {
                                newMain = img;
                                break;
                            }
                        }

                        if (newMain != null)
                        {
                            SetImageAsMain(newMain);
                            newMain.IsMain = true;
                        }
                    }

                    // Supprimer l'image de la collection
                    ProductImages.Remove(image);

                    // Si c'était l'image actuellement affichée
                    if (currentSelectedImage == image)
                    {
                        if (ProductImages.Count > 0)
                        {
                            // Sélectionner la première image disponible
                            currentSelectedImage = ProductImages[0];
                            LoadImagePreview(currentSelectedImage.ImageUrl);
                            txtMainImageIndicator.Visibility = currentSelectedImage.IsMain ? Visibility.Visible : Visibility.Collapsed;
                        }
                        else
                        {
                            // Aucune image disponible
                            currentSelectedImage = null;
                            imgPreview.Source = null;
                            txtMainImageIndicator.Visibility = Visibility.Collapsed;
                        }
                    }

                    // Rafraîchir la liste
                    lstImages.Items.Refresh();
                }
            }
        }

        private void BtnAddNewImage_Click(object sender, RoutedEventArgs e)
        {
            // Ouvrir une boîte de dialogue pour ajouter une nouvelle image
            EditImageDialog dialog = new EditImageDialog("");
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                // Si confirmé, ajouter l'image à la base de données
                int variantId = GetMainVariantId(product.Id);

                if (variantId > 0)
                {
                    // Vérifier si c'est la première image (sera définie comme principale)
                    bool isFirstImage = ProductImages.Count == 0;

                    int newImageId = AddNewImage(variantId, dialog.ImageUrl, isFirstImage);

                    if (newImageId > 0)
                    {
                        // Ajouter l'image à la collection
                        var newImage = new ProductImageViewModel
                        {
                            Id = newImageId,
                            VariantId = variantId,
                            ImageUrl = dialog.ImageUrl,
                            IsMain = isFirstImage
                        };

                        ProductImages.Add(newImage);

                        // Si c'est la première image ou s'il n'y a pas d'image principale
                        if (isFirstImage || currentSelectedImage == null)
                        {
                            currentSelectedImage = newImage;
                            LoadImagePreview(dialog.ImageUrl);
                            txtMainImageIndicator.Visibility = isFirstImage ? Visibility.Visible : Visibility.Collapsed;
                        }

                        // Rafraîchir la liste
                        lstImages.Items.Refresh();
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation des données
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Le nom du produit ne peut pas être vide.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
                {
                    MessageBox.Show("Le prix doit être un nombre positif.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Récupérer le type de produit sélectionné
                string productType = ((ComboBoxItem)cmbType.SelectedItem).Content.ToString();

                // Récupérer le genre sélectionné
                string genre = "Homme"; // Valeur par défaut
                if (cmbGenre.SelectedItem != null)
                {
                    genre = ((ComboBoxItem)cmbGenre.SelectedItem).Content.ToString();
                }

                // Mettre à jour le produit dans la base de données
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = @"
                    UPDATE produits 
                    SET nom = @nom, 
                        description = @description, 
                        prix = @prix, 
                        type_produit = @typeProduit,
                        genre = @genre
                    WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@nom", txtName.Text);
                    cmd.Parameters.AddWithValue("@description", txtDescription.Text);
                    cmd.Parameters.AddWithValue("@prix", price);
                    cmd.Parameters.AddWithValue("@typeProduit", productType);
                    cmd.Parameters.AddWithValue("@genre", genre);
                    cmd.Parameters.AddWithValue("@id", product.Id);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        // Mise à jour réussie
                        MessageBox.Show("Le produit a été mis à jour avec succès!",
                            "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                        this.DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show("Aucune modification effectuée.",
                            "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour du produit: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Fermer la fenêtre sans enregistrer
            this.DialogResult = false;
        }

        // Méthodes pour gérer les images
        private int GetMainVariantId(int productId)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = "SELECT id FROM produit_variantes WHERE produit_id = @productId AND is_main = 1";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@productId", productId);

                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    // Si aucune variante principale n'est trouvée, on prend la première variante
                    query = "SELECT id FROM produit_variantes WHERE produit_id = @productId LIMIT 1";
                    cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@productId", productId);

                    result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la récupération de la variante principale: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        private int AddNewImage(int variantId, string imageUrl, bool isMain)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = @"
                    INSERT INTO produit_images (variante_id, url_image, is_main)
                    VALUES (@variantId, @url, @isMain);
                    SELECT LAST_INSERT_ID();";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@variantId", variantId);
                cmd.Parameters.AddWithValue("@url", imageUrl);
                cmd.Parameters.AddWithValue("@isMain", isMain);

                int newImageId = Convert.ToInt32(cmd.ExecuteScalar());
                return newImageId;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ajout de l'image: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        private void UpdateImageUrl(int imageId, string newUrl)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = "UPDATE produit_images SET url_image = @newUrl WHERE id = @imageId";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@newUrl", newUrl);
                cmd.Parameters.AddWithValue("@imageId", imageId);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour de l'URL: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetImageAsMain(ProductImageViewModel image)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                // Commencer une transaction
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Réinitialiser tous les is_main à 0 pour cette variante
                        string resetQuery = "UPDATE produit_images SET is_main = 0 WHERE variante_id = @variantId";
                        using (MySqlCommand cmd = new MySqlCommand(resetQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@variantId", image.VariantId);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Définir cette image comme principale
                        string setMainQuery = "UPDATE produit_images SET is_main = 1 WHERE id = @imageId";
                        using (MySqlCommand cmd = new MySqlCommand(setMainQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@imageId", image.Id);
                            cmd.ExecuteNonQuery();
                        }

                        // Valider la transaction
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // En cas d'erreur, annuler la transaction
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la définition de l'image principale: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteImage(int imageId, bool isMain)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = "DELETE FROM produit_images WHERE id = @imageId";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@imageId", imageId);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression de l'image: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}