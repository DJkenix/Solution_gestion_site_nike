using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour OrderManagementControl.xaml
    /// </summary>
    public partial class OrderManagementControl : UserControl
    {
        private MySqlConnection connection;
        private int currentClientId = 0;

        public OrderManagementControl()
        {
            InitializeComponent();
            this.connection = DB.SeConnecter();
        }

        #region Recherche client
        private void TxtSearchClient_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Recherche dynamique optionnelle - peut être implémentée si souhaité
        }

        private void BtnSearchClient_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearchClient.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Veuillez entrer un identifiant, un email ou un numéro de commande.",
                    "Recherche vide", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Créer une nouvelle connexion à chaque fois
                using (MySqlConnection newConnection = new MySqlConnection(DB.GetConnectionString()))
                {
                    newConnection.Open();

                    // Recherche par identifiant ou email
                    string query = @"
                SELECT id_client, identifiant, email, role, `created at` as created_date
                FROM users
                WHERE identifiant LIKE @search OR email LIKE @search";

                    using (MySqlCommand cmd = new MySqlCommand(query, newConnection))
                    {
                        cmd.Parameters.AddWithValue("@search", $"%{searchText}%");

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Utilisateur trouvé, chargez ses données
                                currentClientId = Convert.ToInt32(reader["id_client"]);
                                string userIdentifiant = reader["identifiant"].ToString();
                                string userEmail = reader["email"].ToString();
                                string userRole = reader["role"].ToString();
                                DateTime userCreatedDate = Convert.ToDateTime(reader["created_date"]);

                                // Fermer le reader et la connexion avant de charger le dashboard
                                reader.Close();
                                newConnection.Close();

                                // Maintenant charger le dashboard
                                LoadClientDashboard(
                                    currentClientId,
                                    userIdentifiant,
                                    userEmail,
                                    userRole,
                                    userCreatedDate
                                );
                                return;
                            }
                        }
                    }

                    // Pas trouvé par identifiant ou email, essayez le numéro de commande
                    // Vérifiez si le texte de recherche est un numéro
                    if (int.TryParse(searchText, out int orderId))
                    {
                        FindClientByOrderId(orderId);
                    }
                    else
                    {
                        ShowNoResultsMessage();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la recherche : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void FindClientByOrderId(int orderId)
        {
            try
            {
                // Utiliser une nouvelle connexion
                using (MySqlConnection newConnection = new MySqlConnection(DB.GetConnectionString()))
                {
                    newConnection.Open();

                    string query = @"
                SELECT u.id_client, u.identifiant, u.email, u.role, u.`created at` as created_date
                FROM users u
                JOIN commande c ON u.id_client = c.user_id
                WHERE c.id = @orderId";

                    using (MySqlCommand cmd = new MySqlCommand(query, newConnection))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Utilisateur trouvé via la commande
                                currentClientId = Convert.ToInt32(reader["id_client"]);
                                string userIdentifiant = reader["identifiant"].ToString();
                                string userEmail = reader["email"].ToString();
                                string userRole = reader["role"].ToString();
                                DateTime userCreatedDate = Convert.ToDateTime(reader["created_date"]);

                                // Fermer le reader et la connexion avant de charger le dashboard
                                reader.Close();
                                newConnection.Close();

                                LoadClientDashboard(
                                    currentClientId,
                                    userIdentifiant,
                                    userEmail,
                                    userRole,
                                    userCreatedDate
                                );
                                return;
                            }
                        }
                    }

                    ShowNoResultsMessage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la recherche par commande : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowNoResultsMessage()
        {
            MessageBox.Show("Aucun client trouvé avec ces critères de recherche.",
                "Aucun résultat", MessageBoxButton.OK, MessageBoxImage.Information);

            // Réinitialiser l'interface
            DefaultMessagePanel.Visibility = Visibility.Visible;
            ClientDashboardPanel.Visibility = Visibility.Collapsed;
            currentClientId = 0;
        }
        #endregion

        #region Chargement du tableau de bord client
        private void LoadClientDashboard(int clientId, string identifiant, string email, string role, DateTime createdDate)
        {
            try
            {
                // Mettre à jour les informations du client
                txtClientName.Text = identifiant;
                txtClientEmail.Text = email;
                txtClientRole.Text = role;
                txtClientSince.Text = $"Client depuis le {createdDate.ToString("dd/MM/yyyy")}";
                txtClientInitials.Text = GetInitials(identifiant);

                // Charger les KPIs du client
                LoadClientKPIs(clientId);

                // Charger les commandes du client
                LoadClientOrders(clientId);

                // Afficher le tableau de bord client
                DefaultMessagePanel.Visibility = Visibility.Collapsed;
                ClientDashboardPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du tableau de bord : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "??";

            if (name.Length <= 2)
                return name.ToUpper();

            // Prenez les deux premiers caractères ou la première lettre si le nom est court
            return name.Substring(0, Math.Min(2, name.Length)).ToUpper();
        }

        private void LoadClientKPIs(int clientId)
        {
            try
            {
                // 1. Nombre total de commandes
                using (MySqlConnection newConnection = new MySqlConnection(DB.GetConnectionString()))
                {
                    newConnection.Open();
                    string totalOrdersQuery = "SELECT COUNT(*) FROM commande WHERE user_id = @clientId";
                    using (MySqlCommand cmd = new MySqlCommand(totalOrdersQuery, newConnection))
                    {
                        cmd.Parameters.AddWithValue("@clientId", clientId);
                        int totalOrders = Convert.ToInt32(cmd.ExecuteScalar());
                        txtTotalOrders.Text = totalOrders.ToString();
                    }
                }

                // 2. Montant total dépensé
                using (MySqlConnection newConnection = new MySqlConnection(DB.GetConnectionString()))
                {
                    newConnection.Open();
                    string totalSpentQuery = "SELECT COALESCE(SUM(montant_total), 0) FROM commande WHERE user_id = @clientId";
                    using (MySqlCommand cmd = new MySqlCommand(totalSpentQuery, newConnection))
                    {
                        cmd.Parameters.AddWithValue("@clientId", clientId);
                        decimal totalSpent = Convert.ToDecimal(cmd.ExecuteScalar());
                        txtTotalSpent.Text = totalSpent.ToString("0.00", CultureInfo.InvariantCulture) + " €";
                    }
                }

                // 3. Dernière commande
                using (MySqlConnection newConnection = new MySqlConnection(DB.GetConnectionString()))
                {
                    newConnection.Open();
                    string lastOrderQuery = @"
                SELECT id, montant_total, statut, commande_le
                FROM commande 
                WHERE user_id = @clientId 
                ORDER BY commande_le DESC 
                LIMIT 1";

                    using (MySqlCommand cmd = new MySqlCommand(lastOrderQuery, newConnection))
                    {
                        cmd.Parameters.AddWithValue("@clientId", clientId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                DateTime orderDate = Convert.ToDateTime(reader["commande_le"]);
                                decimal amount = Convert.ToDecimal(reader["montant_total"]);
                                string status = reader["statut"].ToString();

                                txtLastOrderDate.Text = orderDate.ToString("dd/MM/yyyy");
                                txtLastOrderAmount.Text = amount.ToString("0.00", CultureInfo.InvariantCulture) + " €";
                                txtLastOrderStatus.Text = FormatOrderStatus(status);

                                // Définir la couleur du statut
                                bdLastOrderStatus.Background = GetStatusBrush(status);
                            }
                            else
                            {
                                // Pas de commande
                                txtLastOrderDate.Text = "Aucune";
                                txtLastOrderAmount.Text = "0.00 €";
                                txtLastOrderStatus.Text = "N/A";
                                bdLastOrderStatus.Background = new SolidColorBrush(Colors.Gray);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des KPIs : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadClientOrders(int clientId)
        {
            try
            {
                string ordersQuery = @"
                    SELECT id, montant_total, statut, commande_le
                    FROM commande 
                    WHERE user_id = @clientId 
                    ORDER BY commande_le DESC";

                using (MySqlCommand cmd = new MySqlCommand(ordersQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@clientId", clientId);

                    List<OrderViewModel> orders = new List<OrderViewModel>();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new OrderViewModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Amount = Convert.ToDecimal(reader["montant_total"]),
                                Status = reader["statut"].ToString(),
                                OrderDate = Convert.ToDateTime(reader["commande_le"]).ToString("dd/MM/yyyy")
                            });
                        }
                    }

                    OrdersList.ItemsSource = orders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des commandes : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
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
        #endregion

        #region Actions sur les commandes
        private void BtnOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int orderId = Convert.ToInt32(btn.Tag);
                OpenOrderDetailsWindow(orderId);
            }
        }

        private void BtnChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int orderId = Convert.ToInt32(btn.Tag);
                OpenChangeStatusDialog(orderId);
            }
        }

        private void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int orderId = Convert.ToInt32(btn.Tag);
                ConfirmDeleteOrder(orderId);
            }
        }

        private void OpenOrderDetailsWindow(int orderId)
        {
            try
            {
                // Ouvrir une fenêtre de détails de commande
                OrderDetailsWindow detailsWindow = new OrderDetailsWindow(orderId);

                // Mettre à jour le tableau de bord après fermeture de la fenêtre
                detailsWindow.Closed += (s, args) =>
                {
                    if (currentClientId > 0)
                    {
                        LoadClientKPIs(currentClientId);
                        LoadClientOrders(currentClientId);
                    }
                };

                detailsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture des détails : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenChangeStatusDialog(int orderId)
        {
            try
            {
                // Ouvrir une fenêtre de changement de statut
                ChangeOrderStatusDialog statusDialog = new ChangeOrderStatusDialog(orderId);

                if (statusDialog.ShowDialog() == true)
                {
                    // Statut modifié, rafraîchir les données
                    if (currentClientId > 0)
                    {
                        LoadClientKPIs(currentClientId);
                        LoadClientOrders(currentClientId);
                    }

                    MessageBox.Show("Le statut de la commande a été mis à jour avec succès.",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du changement de statut : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfirmDeleteOrder(int orderId)
        {
            // Demander confirmation avant suppression
            MessageBoxResult result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer la commande n°{orderId} ?\n\nCette action est irréversible et supprimera également tous les détails associés à cette commande.",
                "Confirmation de suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                DeleteOrder(orderId);
            }
        }

        private void DeleteOrder(int orderId)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                // Utiliser une transaction pour s'assurer que tout est supprimé ou rien
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Supprimer les détails de la commande
                        string deleteDetailsQuery = "DELETE FROM details_commande WHERE commande_id = @orderId";
                        using (MySqlCommand cmd = new MySqlCommand(deleteDetailsQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@orderId", orderId);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Supprimer les paiements associés
                        string deletePaymentsQuery = "DELETE FROM paiements WHERE commande_id = @orderId";
                        using (MySqlCommand cmd = new MySqlCommand(deletePaymentsQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@orderId", orderId);
                            cmd.ExecuteNonQuery();
                        }

                        // 3. Supprimer la commande elle-même
                        string deleteOrderQuery = "DELETE FROM commande WHERE id = @orderId";
                        using (MySqlCommand cmd = new MySqlCommand(deleteOrderQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@orderId", orderId);
                            cmd.ExecuteNonQuery();
                        }

                        // Valider la transaction
                        transaction.Commit();

                        // Rafraîchir les données
                        if (currentClientId > 0)
                        {
                            LoadClientKPIs(currentClientId);
                            LoadClientOrders(currentClientId);
                        }

                        MessageBox.Show($"La commande n°{orderId} a été supprimée avec succès.",
                            "Suppression réussie", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        // Annuler la transaction en cas d'erreur
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression de la commande : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }

    // ViewModel pour les commandes
    public class OrderViewModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string OrderDate { get; set; }
    }

    // Convertisseur pour les couleurs de statut
    // Dans OrderHistoryControl.xaml.cs, renommez la classe
    public class OrderHistoryStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
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

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}