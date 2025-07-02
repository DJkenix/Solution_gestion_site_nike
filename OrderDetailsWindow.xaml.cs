using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour OrderDetailsWindow.xaml
    /// </summary>
    public partial class OrderDetailsWindow : Window
    {
        private MySqlConnection connection;
        private int orderId;

        public OrderDetailsWindow(int orderId)
        {
            InitializeComponent();

            this.orderId = orderId;
            this.connection = DB.SeConnecter();

            // Charger les détails de la commande
            LoadOrderDetails();
        }

        private void LoadOrderDetails()
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                // 1. Charger les informations de base de la commande
                LoadOrderBasicInfo();

                // 2. Charger les informations du client
                LoadClientInfo();

                // 3. Charger les informations de paiement
                LoadPaymentInfo();

                // 4. Charger les détails des produits commandés
                LoadOrderItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des détails de la commande : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrderBasicInfo()
        {
            string query = @"
                SELECT c.id, c.montant_total, c.statut, c.commande_le
                FROM commande c
                WHERE c.id = @orderId";

            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@orderId", orderId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Mettre à jour les informations de base
                        txtOrderTitle.Text = $"Commande n°{reader["id"]}";
                        txtOrderDate.Text = $"Commandé le {Convert.ToDateTime(reader["commande_le"]).ToString("dd/MM/yyyy")}";

                        // Mettre à jour le statut
                        string status = reader["statut"].ToString();
                        txtOrderStatus.Text = FormatOrderStatus(status);
                        bdOrderStatus.Background = GetStatusBrush(status);
                    }
                }
            }
        }

        private void LoadClientInfo()
        {
            string query = @"
                SELECT u.identifiant, u.email, u.id_client
                FROM users u
                JOIN commande c ON u.id_client = c.user_id
                WHERE c.id = @orderId";

            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@orderId", orderId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Mettre à jour les informations client
                        txtClientName.Text = reader["identifiant"].ToString();
                        txtClientEmail.Text = reader["email"].ToString();
                        txtClientId.Text = reader["id_client"].ToString();
                    }
                }
            }
        }

        private void LoadPaymentInfo()
        {
            string query = @"
                SELECT c.montant_total, p.mode_paiement, p.transaction_id
                FROM commande c
                LEFT JOIN paiements p ON c.id = p.commande_id
                WHERE c.id = @orderId
                ORDER BY p.date_paiement DESC
                LIMIT 1";

            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@orderId", orderId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Mettre à jour les informations de paiement
                        decimal amount = Convert.ToDecimal(reader["montant_total"]);
                        txtTotalAmount.Text = amount.ToString("0.00") + " €";

                        txtPaymentMethod.Text = reader["mode_paiement"] != DBNull.Value
                            ? reader["mode_paiement"].ToString()
                            : "Information non disponible";

                        txtTransactionId.Text = reader["transaction_id"] != DBNull.Value
                            ? reader["transaction_id"].ToString()
                            : "Information non disponible";
                    }
                }
            }
        }

        private void LoadOrderItems()
        {
            string query = @"
                SELECT 
                    dc.quantite,
                    dc.prix_achat,
                    p.nom AS product_name,
                    pv.couleur AS variant_name,
                    pt.taille AS size
                FROM 
                    details_commande dc
                JOIN 
                    produits p ON dc.product_id = p.id
                LEFT JOIN 
                    produit_variantes pv ON dc.variante_id = pv.id
                LEFT JOIN 
                    produit_tailles pt ON dc.produit_taille_id = pt.id
                WHERE 
                    dc.commande_id = @orderId";

            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@orderId", orderId);

                List<OrderItemViewModel> orderItems = new List<OrderItemViewModel>();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int quantity = Convert.ToInt32(reader["quantite"]);
                        decimal unitPrice = Convert.ToDecimal(reader["prix_achat"]);

                        orderItems.Add(new OrderItemViewModel
                        {
                            ProductName = reader["product_name"].ToString(),
                            VariantName = reader["variant_name"] != DBNull.Value
                                ? reader["variant_name"].ToString()
                                : "",
                            Size = reader["size"] != DBNull.Value
                                ? reader["size"].ToString()
                                : "N/A",
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = unitPrice * quantity
                        });
                    }
                }

                // Mettre à jour la liste des produits
                OrderItemsList.ItemsSource = orderItems;
            }
        }

        private string FormatOrderStatus(string status)
        {
            // Formater le statut de la commande pour l'affichage
            switch (status.ToLower())
            {
                case "en attente":
                    return "En attente";
                case "en préparation":
                    return "En préparation";
                case "envoyé":
                    return "Envoyé";
                case "livré":
                    return "Livré";
                default:
                    return status;
            }
        }

        private SolidColorBrush GetStatusBrush(string status)
        {
            // Retourner la couleur correspondant au statut
            switch (status.ToLower())
            {
                case "en attente":
                    return new SolidColorBrush(Color.FromRgb(117, 117, 117)); // Gris
                case "en préparation":
                    return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                case "envoyé":
                    return new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Bleu
                case "livré":
                    return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Vert
                default:
                    return new SolidColorBrush(Colors.Gray);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // ViewModel pour les éléments de commande
    public class OrderItemViewModel
    {
        public string ProductName { get; set; }
        public string VariantName { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}