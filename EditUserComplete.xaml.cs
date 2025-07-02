using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour EditUserComplete.xaml
    /// </summary>
    public partial class EditUserComplete : Window
    {
        private MySqlConnection connection;
        private string userId;
        private int userDbId; // ID numérique de l'utilisateur dans la base de données

        // Événement déclenché lorsque l'utilisateur est modifié
        public event EventHandler UserModified;

        public EditUserComplete(string userId)
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
                    SELECT u.id_client, u.identifiant, u.email, u.role
                    FROM users u
                    WHERE u.identifiant = @userId";

                using (MySqlCommand cmd = new MySqlCommand(userQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Stocker l'ID numérique
                            userDbId = Convert.ToInt32(reader["id_client"]);

                            // Mettre à jour l'en-tête avec le nom d'utilisateur et le rôle
                            txtUserName.Text = $"Modifier l'utilisateur: {reader["identifiant"]}";
                            txtUserRole.Text = $"Rôle: {reader["role"]}";

                            // Mettre à jour les champs du formulaire
                            txtIdentifiant.Text = reader["identifiant"].ToString();
                            txtEmail.Text = reader["email"].ToString();
                            txtRole.Text = reader["role"].ToString();
                        }
                    }
                }

                // Charger les détails personnels de l'utilisateur depuis user_details
                string detailsQuery = @"
                    SELECT ud.prenom, ud.nom, ud.adresse, ud.ville, ud.code_postal, ud.pays, ud.telephone
                    FROM user_details ud
                    WHERE ud.user_id = @userDbId";

                using (MySqlCommand cmd = new MySqlCommand(detailsQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@userDbId", userDbId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Remplir les champs avec les données existantes
                            txtPrenom.Text = reader["prenom"] != DBNull.Value ? reader["prenom"].ToString() : "";
                            txtNom.Text = reader["nom"] != DBNull.Value ? reader["nom"].ToString() : "";
                            txtAdresse.Text = reader["adresse"] != DBNull.Value ? reader["adresse"].ToString() : "";
                            txtVille.Text = reader["ville"] != DBNull.Value ? reader["ville"].ToString() : "";
                            txtCodePostal.Text = reader["code_postal"] != DBNull.Value ? reader["code_postal"].ToString() : "";
                            txtPays.Text = reader["pays"] != DBNull.Value ? reader["pays"].ToString() : "";
                            txtTelephone.Text = reader["telephone"] != DBNull.Value ? reader["telephone"].ToString() : "";
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

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                // Commencer une transaction pour s'assurer que toutes les mises à jour sont effectuées
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 1. Mettre à jour l'email dans la table users
                    string updateUserQuery = "UPDATE users SET email = @email WHERE id_client = @userId";
                    using (MySqlCommand cmd = new MySqlCommand(updateUserQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@userId", userDbId);
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Vérifier si l'utilisateur a déjà des détails personnels
                    string checkDetailsQuery = "SELECT COUNT(*) FROM user_details WHERE user_id = @userId";
                    bool hasDetails = false;

                    using (MySqlCommand cmd = new MySqlCommand(checkDetailsQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@userId", userDbId);
                        hasDetails = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }

                    // 3. Insérer ou mettre à jour les détails personnels
                    string detailsQuery;

                    if (hasDetails)
                    {
                        // Mettre à jour les détails existants
                        detailsQuery = @"
                            UPDATE user_details SET 
                                prenom = @prenom,
                                nom = @nom,
                                adresse = @adresse,
                                ville = @ville,
                                code_postal = @codePostal,
                                pays = @pays,
                                telephone = @telephone
                            WHERE user_id = @userId";
                    }
                    else
                    {
                        // Insérer de nouveaux détails
                        detailsQuery = @"
                            INSERT INTO user_details
                                (user_id, prenom, nom, adresse, ville, code_postal, pays, telephone)
                            VALUES
                                (@userId, @prenom, @nom, @adresse, @ville, @codePostal, @pays, @telephone)";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(detailsQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@userId", userDbId);
                        cmd.Parameters.AddWithValue("@prenom", txtPrenom.Text);
                        cmd.Parameters.AddWithValue("@nom", txtNom.Text);
                        cmd.Parameters.AddWithValue("@adresse", txtAdresse.Text);
                        cmd.Parameters.AddWithValue("@ville", txtVille.Text);
                        cmd.Parameters.AddWithValue("@codePostal", txtCodePostal.Text);
                        cmd.Parameters.AddWithValue("@pays", txtPays.Text);
                        cmd.Parameters.AddWithValue("@telephone", txtTelephone.Text);

                        cmd.ExecuteNonQuery();
                    }

                    // Valider la transaction
                    transaction.Commit();

                    // Afficher un message de succès
                    txtStatus.Text = "Modifications enregistrées avec succès!";
                    MessageBox.Show("Les informations de l'utilisateur ont été mises à jour avec succès!",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Déclencher l'événement de modification
                    UserModified?.Invoke(this, EventArgs.Empty);

                    // Fermer la fenêtre
                    Close();
                }
                catch (Exception ex)
                {
                    // En cas d'erreur, annuler la transaction
                    transaction.Rollback();
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'enregistrement des modifications: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Erreur lors de l'enregistrement des modifications.";
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            // Fermer la fenêtre sans enregistrer
            Close();
        }
    }
}