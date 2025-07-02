using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour RoleManagementControl.xaml
    /// </summary>
    public partial class RoleManagementControl : UserControl
    {
        private MySqlConnection connection;
        private string adminUsername; // Identifiant de l'administrateur connecté

        // Liste des rôles disponibles
        public List<string> RoleList { get; private set; }

        // Constructeur par défaut (nécessaire pour XAML)
        public RoleManagementControl()
        {
            InitializeComponent();

            this.connection = DB.SeConnecter();

            // Initialiser la liste des rôles
            RoleList = new List<string> { "client", "employee", "admin" };

            // Définir le contexte de données pour le binding
            this.DataContext = this;
        }

        // Constructeur avec paramètre
        public RoleManagementControl(string adminUsername) : this()
        {
            this.adminUsername = adminUsername;
        }

        // Méthode pour mettre à jour le nom d'utilisateur de l'administrateur
        public void SetAdminUsername(string username)
        {
            this.adminUsername = username;
        }

        // Méthode pour charger les données lorsque le contrôle est chargé
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUsers();
            LoadRoleLogs();
        }

        private void LoadUsers(string searchText = "")
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                // Requête SQL avec filtre
                string query = "SELECT identifiant, email, role FROM users";
                if (!string.IsNullOrEmpty(searchText))
                {
                    query += " WHERE identifiant LIKE @search OR email LIKE @search OR role LIKE @search";
                }
                query += " ORDER BY role, identifiant"; // Trier par rôle puis par identifiant

                MySqlCommand cmd = new MySqlCommand(query, connection);
                if (!string.IsNullOrEmpty(searchText))
                {
                    cmd.Parameters.AddWithValue("@search", $"%{searchText}%");
                }

                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                // Mettre à jour la source de données
                UsersDataGrid.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des utilisateurs: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Nouvelle méthode pour charger les logs
        private void LoadRoleLogs(string searchText = "")
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                // Requête SQL avec filtre optionnel
                string query = @"
                    SELECT 
                        log_id, 
                        admin_username, 
                        user_modified, 
                        new_role, 
                        DATE_FORMAT(change_date, '%d/%m/%Y %H:%i:%s') as change_date 
                    FROM role_change_logs";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += @" WHERE 
                        admin_username LIKE @search OR 
                        user_modified LIKE @search OR 
                        new_role LIKE @search OR 
                        DATE_FORMAT(change_date, '%d/%m/%Y') LIKE @search";
                }

                query += " ORDER BY change_date DESC LIMIT 100"; // Limiter à 100 dernières entrées

                MySqlCommand cmd = new MySqlCommand(query, connection);
                if (!string.IsNullOrEmpty(searchText))
                {
                    cmd.Parameters.AddWithValue("@search", $"%{searchText}%");
                }

                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                // Mettre à jour la source de données
                LogsDataGrid.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des logs: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Recherche dynamique pour les logs
        private void TxtLogSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadRoleLogs(txtLogSearch.Text);
        }

        // Recherche dynamique pour les utilisateurs
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadUsers(txtSearch.Text);
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers(txtSearch.Text);
        }

        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Cette méthode peut être utilisée pour mettre à jour des informations supplémentaires
            // sur l'utilisateur sélectionné si nécessaire
        }

        private void BtnModifyRole_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag != null)
            {
                string userId = textBlock.Tag.ToString();

                // Ouvrir une fenêtre de dialogue simple pour choisir le rôle
                ChangeRoleDialog dialog = new ChangeRoleDialog(userId);

                if (dialog.ShowDialog() == true)
                {
                    string newRole = dialog.SelectedRole;

                    try
                    {
                        // Mettre à jour le rôle
                        UpdateUserRole(userId, newRole);

                        // Recharger les utilisateurs pour refléter les changements
                        LoadUsers(txtSearch.Text);

                        // Recharger l'historique des logs
                        LoadRoleLogs(txtLogSearch.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la modification du rôle: {ex.Message}",
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void UpdateUserRole(string userId, string newRole)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                // Commencer une transaction
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 1. Mettre à jour le rôle de l'utilisateur
                    string updateRoleQuery = "UPDATE users SET role = @role WHERE identifiant = @userId";
                    using (MySqlCommand cmd = new MySqlCommand(updateRoleQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@role", newRole);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Enregistrer l'action dans un journal
                    string logQuery = @"
                        INSERT INTO role_change_logs 
                            (admin_username, user_modified, new_role, change_date) 
                        VALUES 
                            (@adminUsername, @userModified, @newRole, NOW())";

                    using (MySqlCommand cmd = new MySqlCommand(logQuery, connection, transaction))
                    {
                        // Vérifier que adminUsername n'est pas null ou vide
                        cmd.Parameters.AddWithValue("@adminUsername", string.IsNullOrEmpty(adminUsername) ? "Unknown" : adminUsername);
                        cmd.Parameters.AddWithValue("@userModified", userId);
                        cmd.Parameters.AddWithValue("@newRole", newRole);
                        cmd.ExecuteNonQuery();
                    }

                    // Valider la transaction
                    transaction.Commit();

                    MessageBox.Show($"Le rôle de l'utilisateur {userId} a été modifié en {newRole}.",
                        "Modification réussie", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception)
                {
                    // Annuler la transaction en cas d'erreur
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la mise à jour du rôle: {ex.Message}", ex);
            }
        }
    }
}