using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour AddUserControl.xaml
    /// </summary>
    public partial class AddUserControl : UserControl
    {
        private MySqlConnection connection;

        // Événement pour signaler qu'un utilisateur a été ajouté
        public event EventHandler UserAdded;

        public AddUserControl()
        {
            InitializeComponent();
            this.connection = DB.SeConnecter();
        }

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer les valeurs des champs
            string identifiant = txtIdentifiant.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password.Trim();

            // Déterminer le rôle sélectionné
            string role = "client"; // Valeur par défaut
            if (rbAdmin.IsChecked == true)
                role = "admin";
            else if (rbEmployee.IsChecked == true)
                role = "employee";

            // Validation de base
            if (string.IsNullOrWhiteSpace(identifiant) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Veuillez remplir tous les champs obligatoires.",
                    "Champs manquants", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!email.Contains("@"))
            {
                MessageBox.Show("Veuillez saisir une adresse email valide.",
                    "Email invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Vérifier si l'identifiant existe déjà
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                // Vérifier si l'identifiant existe déjà
                string checkQuery = "SELECT COUNT(*) FROM users WHERE identifiant = @identifiant";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@identifiant", identifiant);
                    int userCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (userCount > 0)
                    {
                        MessageBox.Show("Cet identifiant existe déjà. Veuillez en choisir un autre.",
                            "Identifiant existant", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Vérifier si l'email existe déjà
                string checkEmailQuery = "SELECT COUNT(*) FROM users WHERE email = @email";
                using (MySqlCommand checkCmd = new MySqlCommand(checkEmailQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@email", email);
                    int emailCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (emailCount > 0)
                    {
                        MessageBox.Show("Cette adresse email est déjà utilisée. Veuillez en choisir une autre.",
                            "Email existant", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Hacher le mot de passe avec BCrypt
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                // Générer un code secret aléatoire (pour l'authentification à deux facteurs)
                string secret = GenerateRandomSecret();

                // Insérer le nouvel utilisateur
                string insertQuery = @"
            INSERT INTO users (identifiant, email, mdp, role, secret) 
            VALUES (@identifiant, @email, @mdp, @role, @secret)";

                using (MySqlCommand cmd = new MySqlCommand(insertQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@identifiant", identifiant);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@mdp", hashedPassword);
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@secret", secret);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        txtStatus.Text = "Utilisateur ajouté avec succès!";

                        // Réinitialiser le formulaire
                        ResetForm();

                        // Notifier que l'utilisateur a été ajouté
                        UserAdded?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        txtStatus.Text = "Erreur: Aucune ligne affectée.";
                    }
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Erreur lors de l'ajout de l'utilisateur.";
                MessageBox.Show($"Erreur lors de l'ajout de l'utilisateur: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void ResetForm()
        {
            // Réinitialiser tous les champs
            txtIdentifiant.Text = "";
            txtEmail.Text = "";
            txtPassword.Password = "";
            rbClient.IsChecked = true;
            rbAdmin.IsChecked = false;
            rbEmployee.IsChecked = false;
        }

        private string GenerateRandomSecret()
        {
            // Générer une chaîne aléatoire de 16 caractères pour le secret
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            char[] result = new char[16];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }
    }
}