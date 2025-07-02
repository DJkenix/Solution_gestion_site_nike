using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PanelNikeStore
{
    public partial class UserTableControl : UserControl
    {
        // Définir un événement personnalisé pour la suppression
        public event EventHandler<string> UserDeleted;

        // Définir un événement personnalisé pour la modification
        public event EventHandler<string> UserModified;

        private MySqlConnection connection;

        public UserTableControl()
        {
            InitializeComponent();
            this.connection = DB.SeConnecter();
        }

        public void LoadClientsTable(string searchText = "")
        {
            try
            {
                if (connection == null || connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                // Requête SQL avec filtre
                string query = "SELECT identifiant, email, role FROM users";
                if (!string.IsNullOrEmpty(searchText))
                {
                    query += " WHERE identifiant LIKE @search OR email LIKE @search OR role LIKE @search";
                }

                MySqlCommand cmd = new MySqlCommand(query, connection);
                if (!string.IsNullOrEmpty(searchText))
                {
                    cmd.Parameters.AddWithValue("@search", $"%{searchText}%");
                }

                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                // Mettre à jour la source de données
                ClientTable.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des clients : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadClientsTable(txtSearch.Text);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadClientsTable(txtSearch.Text);
        }

        private void ShowUserDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                string userId = button.Tag.ToString();

                try
                {
                    // Ouvrir la fenêtre de détails
                    UserDetailsPopup detailsWindow = new UserDetailsPopup(userId);

                    // Abonner à l'événement UserModified
                    detailsWindow.UserModified += (s, args) => {
                        // Recharger les données
                        LoadClientsTable(txtSearch.Text);

                        // Propager l'événement
                        UserModified?.Invoke(this, userId);
                    };

                    detailsWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'affichage des détails: {ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ModifyUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                string userId = button.Tag.ToString();

                // Trouver la ligne correspondante
                foreach (DataRowView row in ClientTable.Items)
                {
                    if (row["identifiant"].ToString() == userId)
                    {
                        try
                        {
                            // Ouvrir la fenêtre de modification
                            ModifierClient modifierWindow = new ModifierClient(
                                row["identifiant"].ToString(),
                                row["email"].ToString(),
                                row["role"].ToString()
                            );

                            // Configurer un gestionnaire pour la fermeture
                            modifierWindow.Closed += (s, args) => {
                                // Déclencher l'événement UserModified
                                UserModified?.Invoke(this, userId);

                                // Recharger les données
                                LoadClientsTable(txtSearch.Text);
                            };

                            modifierWindow.ShowDialog();
                            return;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Erreur lors de la modification : {ex.Message}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                string userId = button.Tag.ToString();

                // Demander confirmation
                MessageBoxResult result = MessageBox.Show(
                    $"Êtes-vous sûr de vouloir supprimer l'utilisateur {userId} ?",
                    "Confirmation de suppression",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        // Exécuter la suppression
                        string query = "DELETE FROM users WHERE identifiant = @userId";
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@userId", userId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show($"L'utilisateur {userId} a été supprimé avec succès.",
                                "Suppression réussie", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Déclencher l'événement UserDeleted
                            UserDeleted?.Invoke(this, userId);

                            // Recharger les données
                            LoadClientsTable(txtSearch.Text);
                        }
                        else
                        {
                            MessageBox.Show("Aucun utilisateur n'a été supprimé. Vérifiez l'identifiant.",
                                "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la suppression : {ex.Message}",
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}