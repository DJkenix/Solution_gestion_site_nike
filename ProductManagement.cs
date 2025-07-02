using MySql.Data.MySqlClient;
using System;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows;

namespace PanelNikeStore
{
    /// <summary>
    /// Classe utilitaire pour la gestion des produits
    /// </summary>
    public static class ProductManagement
    {
        /// <summary>
        /// Valide les données d'un produit
        /// </summary>
        public static bool ValidateProduct(string name, string priceText, int imageCount)
        {
            // Vérifier le nom du produit
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Veuillez saisir un nom pour le produit.",
                    "Champ obligatoire", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Vérifier le prix
            if (!ParsePrice(priceText, out decimal price) || price <= 0)
            {
                MessageBox.Show("Veuillez saisir un prix valide (nombre positif).",
                    "Prix invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Vérifier les images (minimum 2)
            if (imageCount < 2)
            {
                MessageBox.Show("Veuillez ajouter au moins 2 images pour le produit.",
                    "Images insuffisantes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parse un prix à partir d'une chaîne de caractères
        /// </summary>
        public static bool ParsePrice(string priceText, out decimal price)
        {
            // Remplacer la virgule par un point pour le parsing décimal
            string normalizedPrice = priceText.Replace(',', '.');

            // Essayer de parser en utilisant la culture invariante
            return decimal.TryParse(normalizedPrice,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out price);
        }

        /// <summary>
        /// Insère un produit dans la base de données
        /// </summary>
        public static int InsertProduct(MySqlConnection connection, MySqlTransaction transaction,
            string nom, string description, decimal prix, string typeProduit, string genre)
        {
            string query = @"
                INSERT INTO produits (nom, description, prix, type_produit, genre)
                VALUES (@nom, @description, @prix, @typeProduit, @genre);
                SELECT LAST_INSERT_ID();";

            using (MySqlCommand cmd = new MySqlCommand(query, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@description", description);
                cmd.Parameters.AddWithValue("@prix", prix);
                cmd.Parameters.AddWithValue("@typeProduit", typeProduit);
                cmd.Parameters.AddWithValue("@genre", genre);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// Insère des tailles par défaut pour une variante
        /// </summary>
        public static void InsertDefaultSizes(MySqlConnection connection, MySqlTransaction transaction, int variantId)
        {
            // Tailles standards pour les chaussures
            int[] defaultSizes = { 39, 40, 41, 42, 43, 44, 45 };

            foreach (int size in defaultSizes)
            {
                string query = @"
                    INSERT INTO produit_tailles (variante_id, taille, stock)
                    VALUES (@varianteId, @taille, @stock);";

                using (MySqlCommand cmd = new MySqlCommand(query, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@varianteId", variantId);
                    cmd.Parameters.AddWithValue("@taille", size);
                    cmd.Parameters.AddWithValue("@stock", 0); // Stock à 0 par défaut

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}