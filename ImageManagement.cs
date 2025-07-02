using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using static PanelNikeStore.EditProductWindow;

namespace PanelNikeStore
{
    /// <summary>
    /// Classe utilitaire pour la gestion des images de produit
    /// </summary>
    public static class ImageManagement
    {
        /// <summary>
        /// Vérifie si une image principale existe pour une variante
        /// </summary>
        public static bool HasMainImage(ObservableCollection<ProductImageViewModel> images, int variantId)
        {
            foreach (var image in images)
            {
                if (image.VariantId == variantId && image.IsMain)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Vérifie si des images existent pour une variante
        /// </summary>
        public static bool HasImagesForVariant(ObservableCollection<ProductImageViewModel> images, int variantId)
        {
            foreach (var image in images)
            {
                if (image.VariantId == variantId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Vérifie si chaque variante a des images et une image principale
        /// </summary>
        public static bool ValidateVariantImages(ObservableCollection<ProductImageViewModel> images,
                                                ObservableCollection<ProductVariantViewModel> variants)
        {
            foreach (var variant in variants)
            {
                // Vérifier que la variante a au moins une image
                if (!HasImagesForVariant(images, variant.Id))
                {
                    MessageBox.Show($"La variante '{variant.Color}' n'a pas d'images. Veuillez ajouter au moins une image.",
                        "Images manquantes", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Vérifier que la variante a une image principale
                if (!HasMainImage(images, variant.Id))
                {
                    MessageBox.Show($"La variante '{variant.Color}' n'a pas d'image principale. Veuillez définir une image principale.",
                        "Image principale manquante", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Insère une image dans la base de données
        /// </summary>
        public static void InsertImage(MySqlConnection connection, MySqlTransaction transaction,
            int variantId, string imageUrl, bool isMain)
        {
            string query = @"
                INSERT INTO produit_images (variante_id, url_image, is_main)
                VALUES (@varianteId, @urlImage, @isMain);";

            using (MySqlCommand cmd = new MySqlCommand(query, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@varianteId", variantId);
                cmd.Parameters.AddWithValue("@urlImage", imageUrl);
                cmd.Parameters.AddWithValue("@isMain", isMain);

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Met à jour le statut principal des images pour une variante
        /// </summary>
        public static void UpdateMainStatus(ObservableCollection<ProductImageViewModel> images, int imageId, int variantId)
        {
            foreach (var image in images)
            {
                if (image.VariantId == variantId)
                {
                    image.IsMain = (image.Id == imageId);
                }
            }
        }
    }
}