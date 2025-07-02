using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour UserStatsPopup.xaml
    /// </summary>
    public partial class UserStatsPopup : Window
    {
        private MySqlConnection connection;

        public UserStatsPopup()
        {
            InitializeComponent();
            this.connection = DB.SeConnecter(); // Initialiser la connexion

            if (connection.State != ConnectionState.Open)
                connection.Open();

            // Charger les données dès que le pop-up s'ouvre
            LoadUserStats();
        }

        private void LoadUserStats()
        {
            try
            {
                // Récupérer les statistiques des utilisateurs depuis la base de données
                int adminCount = GetUserCountByRole("admin");
                int employeeCount = GetUserCountByRole("employee");
                int clientCount = GetUserCountByRole("client");

                // Mettre à jour les TextBlocks dans le pop-up
                txtAdminCount.Text = adminCount.ToString();
                txtEmployeeCount.Text = employeeCount.ToString();
                txtClientCount.Text = clientCount.ToString();

                // Calculer le total
                int totalUsers = adminCount + employeeCount + clientCount;
                txtTotalUsers.Text = totalUsers.ToString();

                // Calculer les pourcentages
                txtAdminPercent.Text = totalUsers > 0 ? Math.Round((adminCount / (double)totalUsers) * 100) + "%" : "0%";
                txtEmployeePercent.Text = totalUsers > 0 ? Math.Round((employeeCount / (double)totalUsers) * 100) + "%" : "0%";
                txtClientPercent.Text = totalUsers > 0 ? Math.Round((clientCount / (double)totalUsers) * 100) + "%" : "0%";

                // Récupérer le nombre de nouveaux utilisateurs (derniers 30 jours)
                txtNewUsers.Text = GetNewUsersCount().ToString();

                // Mettre à jour les barres du graphique
                UpdateChartBars(adminCount, employeeCount, clientCount);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des statistiques : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetUserCountByRole(string role)
        {
            string query = "SELECT COUNT(*) FROM users WHERE role = @role";
            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@role", role);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int GetNewUsersCount()
        {
            string query = "SELECT COUNT(*) FROM users WHERE `created at` >= DATE_SUB(NOW(), INTERVAL 30 DAY)";
            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void UpdateChartBars(int adminCount, int employeeCount, int clientCount)
        {
            // Obtenir le total des utilisateurs
            int totalUsers = adminCount + employeeCount + clientCount;

            if (totalUsers == 0)
                return; // Éviter la division par zéro

            // Hauteur maximale disponible pour les barres (en pixels)
            const double maxBarHeight = 200;

            // Calculer la hauteur proportionnelle pour chaque barre
            // Nous utilisons une hauteur minimale de 10 pixels pour que même les petits nombres soient visibles
            double adminHeight = Math.Max(10, (adminCount / (double)totalUsers) * maxBarHeight);
            double employeeHeight = Math.Max(10, (employeeCount / (double)totalUsers) * maxBarHeight);
            double clientHeight = Math.Max(10, (clientCount / (double)totalUsers) * maxBarHeight);

            // Mettre à jour les hauteurs des barres
            AdminBar.Height = adminHeight;
            EmployeeBar.Height = employeeHeight;
            ClientBar.Height = clientHeight;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Fermer simplement la fenêtre
            this.Close();
        }
    }
}