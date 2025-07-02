using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using Microsoft.Win32;

namespace PanelNikeStore
{
    public partial class OrderHistoryControl : UserControl
    {
        private MySqlConnection connection;
        private ObservableCollection<OrderHistoryViewModel> allOrders;
        private ObservableCollection<OrderAlertViewModel> alertOrders;

        public OrderHistoryControl()
        {
            InitializeComponent();
            this.connection = DB.SeConnecter();

            // Initialiser les collections
            allOrders = new ObservableCollection<OrderHistoryViewModel>();
            alertOrders = new ObservableCollection<OrderAlertViewModel>();

            // Lier la collection aux contrôles
            AllOrdersGrid.ItemsSource = allOrders;
            AlertOrdersList.ItemsSource = alertOrders;

            // Initialiser la recherche
            txtSearchOrder.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#757575"));

            // Charger les données
            LoadDashboardData();
            LoadFilteredOrders();
            LoadAlerts();
        }

        #region Chargement des données

        private void LoadDashboardData()
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                // 1. Total des commandes
                string totalOrdersQuery = "SELECT COUNT(*) FROM commande";
                using (MySqlCommand cmd = new MySqlCommand(totalOrdersQuery, connection))
                {
                    int totalOrders = Convert.ToInt32(cmd.ExecuteScalar());
                    txtTotalOrders.Text = totalOrders.ToString("N0");
                }

                // 2. Commandes récentes (7 derniers jours)
                string recentOrdersQuery = "SELECT COUNT(*) FROM commande WHERE commande_le >= DATE_SUB(NOW(), INTERVAL 7 DAY)";
                using (MySqlCommand cmd = new MySqlCommand(recentOrdersQuery, connection))
                {
                    int recentOrders = Convert.ToInt32(cmd.ExecuteScalar());
                    txtRecentOrders.Text = recentOrders.ToString("N0");
                }

                // 3. Revenu total
                string totalRevenueQuery = "SELECT SUM(montant_total) FROM commande";
                using (MySqlCommand cmd = new MySqlCommand(totalRevenueQuery, connection))
                {
                    object result = cmd.ExecuteScalar();
                    decimal totalRevenue = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                    txtTotalRevenue.Text = totalRevenue.ToString("N0", CultureInfo.InvariantCulture);
                }

                // 4. Valeur moyenne des commandes
                string averageValueQuery = "SELECT AVG(montant_total) FROM commande";
                using (MySqlCommand cmd = new MySqlCommand(averageValueQuery, connection))
                {
                    object result = cmd.ExecuteScalar();
                    decimal averageValue = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                    txtAverageValue.Text = averageValue.ToString("N0", CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données du tableau de bord: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFilteredOrders(string searchQuery = "", string statusFilter = "Tous les statuts", DateTime? startDate = null)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                allOrders.Clear();

                string query = @"
                    SELECT 
                        c.id, 
                        c.montant_total, 
                        c.statut, 
                        c.commande_le,
                        u.identifiant as client_name,
                        (SELECT COUNT(*) FROM details_commande dc WHERE dc.commande_id = c.id) as item_count
                    FROM 
                        commande c
                    LEFT JOIN 
                        users u ON c.user_id = u.id_client
                    WHERE 
                        1=1";

                // Ajouter le filtre de recherche
                if (!string.IsNullOrEmpty(searchQuery) && searchQuery != "Rechercher par n° commande ou client...")
                {
                    query += @" AND (c.id LIKE @search OR u.identifiant LIKE @search)";
                }

                // Ajouter le filtre de statut
                if (statusFilter != "Tous les statuts")
                {
                    query += @" AND c.statut = @status";
                }

                // Ajouter le filtre de date
                if (startDate.HasValue)
                {
                    query += @" AND c.commande_le >= @startDate";
                }

                query += " ORDER BY c.commande_le DESC";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    // Paramètres de recherche
                    if (!string.IsNullOrEmpty(searchQuery) && searchQuery != "Rechercher par n° commande ou client...")
                    {
                        cmd.Parameters.AddWithValue("@search", $"%{searchQuery}%");
                    }

                    // Paramètre de statut
                    if (statusFilter != "Tous les statuts")
                    {
                        cmd.Parameters.AddWithValue("@status", statusFilter);
                    }

                    // Paramètre de date
                    if (startDate.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate.Value.Date);
                    }

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = Convert.ToInt32(reader["id"]);
                            string clientName = reader["client_name"].ToString();
                            string status = reader["statut"].ToString();
                            DateTime orderDate = Convert.ToDateTime(reader["commande_le"]);
                            decimal amount = Convert.ToDecimal(reader["montant_total"]);
                            int itemCount = Convert.ToInt32(reader["item_count"]);

                            bool canChangeStatus = status.ToLower() != "livré";

                            allOrders.Add(new OrderHistoryViewModel
                            {
                                Id = id,
                                ClientName = clientName,
                                Status = status,
                                OrderDate = orderDate.ToString("dd/MM/yyyy"),
                                Amount = amount.ToString("C2", CultureInfo.CreateSpecificCulture("fr-FR")),
                                ItemCount = itemCount,
                                CanChangeStatus = canChangeStatus
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des commandes: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAlerts()
        {
            try
            {
                // Ajouter des données d'exemple aux alertes
                alertOrders.Clear();

                // Ajouter les alertes d'exemple
                alertOrders.Add(new OrderAlertViewModel
                {
                    OrderId = 19,
                    AlertTitle = "Commande #19 en attente depuis 5 jours",
                    AlertMessage = "Client: Joco - Montant: 389,98 €"
                });

                alertOrders.Add(new OrderAlertViewModel
                {
                    OrderId = 21,
                    AlertTitle = "Commande #21 avec problème de paiement",
                    AlertMessage = "Client: client - Montant: 459,97 € - Paiement refusé"
                });

                // Mettre à jour l'affichage
                if (alertOrders.Count == 0)
                {
                    txtNoAlerts.Visibility = Visibility.Visible;
                    AlertOrdersList.Visibility = Visibility.Collapsed;
                }
                else
                {
                    txtNoAlerts.Visibility = Visibility.Collapsed;
                    AlertOrdersList.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des alertes: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Filtres et recherche

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) // Pour éviter que ça s'exécute pendant l'initialisation
                ApplyFilters();
        }

        private void CmbDateFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) // Pour éviter que ça s'exécute pendant l'initialisation
                ApplyFilters();
        }

        private void ApplyFilters()
        {
            string statusFilter = "Tous les statuts";
            DateTime? startDate = null;

            // Récupérer le filtre de statut
            if (cmbStatusFilter.SelectedItem is ComboBoxItem statusItem)
            {
                statusFilter = statusItem.Content.ToString();
            }

            // Récupérer le filtre de date
            if (cmbDateFilter.SelectedItem is ComboBoxItem dateItem)
            {
                string dateFilter = dateItem.Content.ToString();

                switch (dateFilter)
                {
                    case "7 derniers jours":
                        startDate = DateTime.Now.AddDays(-7);
                        break;
                    case "30 derniers jours":
                        startDate = DateTime.Now.AddDays(-30);
                        break;
                    case "3 derniers mois":
                        startDate = DateTime.Now.AddMonths(-3);
                        break;
                    case "Année en cours":
                        startDate = new DateTime(DateTime.Now.Year, 1, 1);
                        break;
                }
            }

            // Appliquer les filtres
            LoadFilteredOrders(txtSearchOrder.Text, statusFilter, startDate);
        }

        #endregion

        #region Gestion des événements de recherche

        private void TxtSearchOrder_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearchOrder.Text == "Rechercher par n° commande ou client...")
            {
                txtSearchOrder.Text = "";
                txtSearchOrder.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void TxtSearchOrder_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchOrder.Text))
            {
                txtSearchOrder.Text = "Rechercher par n° commande ou client...";
                txtSearchOrder.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#757575"));
            }
        }

        private void TxtSearchOrder_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Si le texte n'est pas le placeholder et le TextBox a le focus
            if (txtSearchOrder.IsFocused && txtSearchOrder.Foreground.ToString() != "#FF757575")
            {
                ApplyFilters();
            }
        }

        private void TxtSearchOrder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSearchOrder_Click(sender, e);
            }
        }

        private void BtnSearchOrder_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        #endregion

        #region Actions sur les commandes

        private void BtnOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int orderId = Convert.ToInt32(btn.Tag);

                // Ouvrir la fenêtre de détails de commande
                OrderDetailsWindow detailsWindow = new OrderDetailsWindow(orderId);
                detailsWindow.ShowDialog();
            }
        }

        private void BtnChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int orderId = Convert.ToInt32(btn.Tag);

                // Ouvrir la boîte de dialogue pour changer le statut
                ChangeOrderStatusDialog statusDialog = new ChangeOrderStatusDialog(orderId);

                if (statusDialog.ShowDialog() == true)
                {
                    // Rafraîchir les données après mise à jour
                    LoadDashboardData();
                    ApplyFilters();
                    LoadAlerts();

                    MessageBox.Show("Le statut de la commande a été mis à jour avec succès.",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnViewAlertOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int orderId = Convert.ToInt32(btn.Tag);

                // Ouvrir la fenêtre de détails de commande
                OrderDetailsWindow detailsWindow = new OrderDetailsWindow(orderId);
                detailsWindow.ShowDialog();
            }
        }

        private void BtnResolveAlert_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int orderId = Convert.ToInt32(btn.Tag);

                // Dans une application réelle, on marquerait cette alerte comme résolue dans la BDD
                MessageBox.Show($"L'alerte pour la commande #{orderId} a été marquée comme résolue.",
                    "Alerte résolue", MessageBoxButton.OK, MessageBoxImage.Information);

                // Supprimer l'alerte de la liste
                OrderAlertViewModel alertToRemove = null;
                foreach (var alert in alertOrders)
                {
                    if (alert.OrderId == orderId)
                    {
                        alertToRemove = alert;
                        break;
                    }
                }

                if (alertToRemove != null)
                {
                    alertOrders.Remove(alertToRemove);
                }

                // Mettre à jour l'affichage des alertes
                if (alertOrders.Count == 0)
                {
                    txtNoAlerts.Visibility = Visibility.Visible;
                    AlertOrdersList.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void BtnExportOrders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Créer une boîte de dialogue pour sauvegarder le fichier
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Fichier CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = "historique_commandes_" + DateTime.Now.ToString("yyyyMMdd")
                };

                // Afficher la boîte de dialogue
                if (saveFileDialog.ShowDialog() == true)
                {
                    // Chemin du fichier à exporter
                    string filePath = saveFileDialog.FileName;

                    // Créer le contenu du CSV
                    using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                    {
                        // Écrire les en-têtes
                        sw.WriteLine("N°;Date;Client;Articles;Montant;Statut");

                        // Écrire les données
                        foreach (var order in allOrders)
                        {
                            sw.WriteLine($"{order.Id};{order.OrderDate};{order.ClientName};{order.ItemCount};" +
                                         $"{order.Amount.Replace(" €", "").Replace(",", ".")};{order.Status}");
                        }
                    }

                    MessageBox.Show($"L'historique des commandes a été exporté avec succès vers {filePath}",
                        "Export réussi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'exportation des commandes: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }

    #region Classes de modèle

    // Modèle de vue pour les commandes
    public class OrderHistoryViewModel
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string Status { get; set; }
        public string OrderDate { get; set; }
        public string Amount { get; set; }
        public int ItemCount { get; set; }
        public bool CanChangeStatus { get; set; }
    }

    // Modèle de vue pour les alertes
    public class OrderAlertViewModel
    {
        public int OrderId { get; set; }
        public string AlertTitle { get; set; }
        public string AlertMessage { get; set; }
    }

    #endregion

    // Ajouter cette classe à la fin de votre fichier
    public class OrderStatusColorConverter : IValueConverter
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