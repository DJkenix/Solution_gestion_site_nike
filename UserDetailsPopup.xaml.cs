using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour UserDetailsPopup.xaml
    /// </summary>
    public partial class UserDetailsPopup : Window
    {
        private MySqlConnection connection;
        private string userId;

        // Événement déclenché lorsque l'utilisateur est modifié
        public event EventHandler UserModified;

        public UserDetailsPopup(string userId)
        {
            InitializeComponent();

            this.userId = userId;
            this.connection = DB.SeConnecter();

            // Charger les détails de l'utilisateur
            LoadUserDetails();
        }

        private void LoadUserDetails()
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                // Charger les informations de base de l'utilisateur
                string userQuery = @"
                    SELECT u.identifiant, u.email, u.role, u.`created at` as created_at
                    FROM users u
                    WHERE u.identifiant = @userId";

                using (MySqlCommand cmd = new MySqlCommand(userQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Mettre à jour l'en-tête avec le nom d'utilisateur et le rôle
                            txtUserName.Text = $"Détails de l'utilisateur: {reader["identifiant"]}";
                            txtUserRole.Text = $"Rôle: {reader["role"]}";

                            // Mettre à jour les informations de base
                            txtIdentifiant.Text = reader["identifiant"].ToString();
                            txtEmail.Text = reader["email"].ToString();
                            txtCreatedAt.Text = reader["created_at"].ToString();
                        }
                    }
                }

                // Charger les détails personnels de l'utilisateur depuis user_details
                string detailsQuery = @"
                    SELECT ud.prenom, ud.nom, ud.adresse, ud.ville, ud.code_postal, ud.pays, ud.telephone
                    FROM user_details ud
                    JOIN users u ON ud.user_id = u.id_client
                    WHERE u.identifiant = @userId";

                using (MySqlCommand cmd = new MySqlCommand(detailsQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Mettre à jour les détails personnels
                            txtPrenom.Text = reader["prenom"] != DBNull.Value ? reader["prenom"].ToString() : "-";
                            txtNom.Text = reader["nom"] != DBNull.Value ? reader["nom"].ToString() : "-";
                            txtAdresse.Text = reader["adresse"] != DBNull.Value ? reader["adresse"].ToString() : "-";
                            txtVille.Text = reader["ville"] != DBNull.Value ? reader["ville"].ToString() : "-";
                            txtCodePostal.Text = reader["code_postal"] != DBNull.Value ? reader["code_postal"].ToString() : "-";
                            txtPays.Text = reader["pays"] != DBNull.Value ? reader["pays"].ToString() : "-";
                            txtTelephone.Text = reader["telephone"] != DBNull.Value ? reader["telephone"].ToString() : "-";
                        }
                        else
                        {
                            // Aucun détail personnel trouvé
                            txtPrenom.Text = "Non renseigné";
                            txtNom.Text = "Non renseigné";
                            txtAdresse.Text = "Non renseignée";
                            txtVille.Text = "Non renseignée";
                            txtCodePostal.Text = "Non renseigné";
                            txtPays.Text = "Non renseigné";
                            txtTelephone.Text = "Non renseigné";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des détails de l'utilisateur: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnModifier_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ouvrir la nouvelle fenêtre de modification complète
                EditUserComplete editWindow = new EditUserComplete(userId);

                // Configurer un gestionnaire pour l'événement UserModified
                editWindow.UserModified += (s, args) => {
                    // Recharger les détails après modification
                    LoadUserDetails();

                    // Propager l'événement de modification vers le haut
                    UserModified?.Invoke(this, EventArgs.Empty);
                };

                editWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture de la fenêtre de modification: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnFermer_Click(object sender, RoutedEventArgs e)
        {
            // Fermer simplement la fenêtre
            Close();
        }
    }
}