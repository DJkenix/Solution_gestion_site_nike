using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace PanelNikeStore
{
    public partial class ModifierClient : Window
    {
        private MySqlConnection connection;
        private string originalId;
        private string originalEmail;
        private string originalRole;

        public ModifierClient(string id, string email, string role)
        {
            InitializeComponent();
            this.connection = DB.SeConnecter();

            // Stocker les valeurs originales
            originalId = id;
            originalEmail = email;
            originalRole = role;

            // Remplir les champs
            modifierid.Text = id;
            modifieremail.Text = email;
        }

        private void EnregistrerModifications(object sender, RoutedEventArgs e)
        {
            string nouvelEmail = modifieremail.Text.Trim();

            // Validation de base pour l'email
            if (string.IsNullOrWhiteSpace(nouvelEmail) || !nouvelEmail.Contains("@"))
            {
                MessageBox.Show("Veuillez saisir une adresse email valide.",
                    "Erreur de validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Ouvrir la connexion si elle est fermée
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                // Mettre à jour l'utilisateur
                string requete = "UPDATE users SET email = @Email WHERE identifiant = @Id";
                using (MySqlCommand cmd = new MySqlCommand(requete, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", nouvelEmail);
                    cmd.Parameters.AddWithValue("@Id", originalId);

                    int lignesAffectees = cmd.ExecuteNonQuery();

                    if (lignesAffectees > 0)
                    {
                        MessageBox.Show("Les informations de l'utilisateur ont été mises à jour avec succès !",
                            "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Aucune mise à jour effectuée. Vérifiez les données.",
                            "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la mise à jour : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AnnulerModification(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}