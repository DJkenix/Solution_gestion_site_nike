using System.Windows;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace PanelNikeStore
{
    public partial class ChangeRoleDialog : Window
    {
        public string SelectedRole { get; private set; }
        private string userId;
        private MySqlConnection connection;

        public ChangeRoleDialog(string userId)
        {
            InitializeComponent();

            this.userId = userId;
            this.connection = DB.SeConnecter();

            // Initialiser la ComboBox avec les rôles disponibles
            cmbRole.Items.Add("client");
            cmbRole.Items.Add("employee");
            cmbRole.Items.Add("admin");

            // Sélectionner le rôle actuel de l'utilisateur
            string currentRole = GetCurrentRole(userId);
            cmbRole.SelectedItem = currentRole;

            // Afficher l'identifiant de l'utilisateur
            txtUserId.Text = userId;
        }

        private string GetCurrentRole(string userId)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = "SELECT role FROM users WHERE identifiant = @userId";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    return cmd.ExecuteScalar()?.ToString() ?? "client";
                }
            }
            catch (Exception)
            {
                return "client"; // Valeur par défaut
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbRole.SelectedItem != null)
            {
                string newRole = cmbRole.SelectedItem.ToString();
                string currentRole = GetCurrentRole(userId);

                // Vérifier si le rôle a changé
                if (newRole == currentRole)
                {
                    MessageBox.Show("Le rôle sélectionné est identique au rôle actuel.",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Confirmation
                MessageBoxResult result = MessageBox.Show(
                    $"Êtes-vous sûr de vouloir modifier le rôle de l'utilisateur {userId} de {currentRole} à {newRole} ?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SelectedRole = newRole;
                    DialogResult = true;
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un rôle.",
                    "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}