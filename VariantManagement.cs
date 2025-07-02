using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace PanelNikeStore
{
    /// <summary>
    /// Classe utilitaire pour la gestion des variantes de produit
    /// </summary>
    public static class VariantManagement
    {
        /// <summary>
        /// Vérifie si une couleur existe déjà dans la liste des variantes
        /// </summary>
        public static bool ColorExists(ObservableCollection<ProductVariantViewModel> variants, string color)
        {
            foreach (var variant in variants)
            {
                if (variant.Color.Equals(color, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Vérifie si une variante principale existe
        /// </summary>
        public static bool HasMainVariant(ObservableCollection<ProductVariantViewModel> variants)
        {
            foreach (var variant in variants)
            {
                if (variant.IsMain)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Insère une variante dans la base de données
        /// </summary>
        public static int InsertVariant(MySqlConnection connection, MySqlTransaction transaction,
            int productId, string couleur, bool isMain)
        {
            string variantQuery = @"
                INSERT INTO produit_variantes (produit_id, couleur, is_main)
                VALUES (@produitId, @couleur, @isMain);
                SELECT LAST_INSERT_ID();";

            using (MySqlCommand cmd = new MySqlCommand(variantQuery, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@produitId", productId);
                cmd.Parameters.AddWithValue("@couleur", couleur);
                cmd.Parameters.AddWithValue("@isMain", isMain);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// Insère une variante standard pour un produit sans variantes
        /// </summary>
        public static int InsertDefaultVariant(MySqlConnection connection, MySqlTransaction transaction, int productId)
        {
            string variantQuery = @"
                INSERT INTO produit_variantes (produit_id, couleur, is_main)
                VALUES (@produitId, 'Standard', 1);
                SELECT LAST_INSERT_ID();";

            using (MySqlCommand cmd = new MySqlCommand(variantQuery, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@produitId", productId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}