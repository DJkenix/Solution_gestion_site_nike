using MySql.Data.MySqlClient;
using System.Windows;


namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour Connexion.xaml
    /// </summary>
    public partial class LoginPage : Window
    {
        public LoginPage()
        {
            InitializeComponent();
        }
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();

            string password = txtPassword.Password.Trim();

            if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(password))
            {

                MessageBox.Show("veuillez remplir les champs");
                return;
            }
            else
            {
                string connectionToDb = "server=127.0.0.1; database=nike_basketball;uid=root;pwd=";
                using (MySqlConnection connection = new MySqlConnection(connectionToDb))
                {

                    connection.Open();

                    string query = "SELECT id_client, mdp FROM users WHERE email = @email";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@email", email);

                    using (MySqlDataReader reader = cmd.ExecuteReader()) // Utilisation correcte
                    {
                        if (reader.Read()) // Vérifie si une ligne est retournée
                        {
                            string storedHash = reader["mdp"].ToString(); // Récupère le mot de passe
                            // Récupère l'ID de l'utilisateur

                            if (BCrypt.Net.BCrypt.Verify(password, storedHash)) // Vérifie le mot de passe
                            {
                                string loggedInUser = reader["id_client"].ToString();
                                DashboardWindow dashboard = new DashboardWindow(loggedInUser);
                                dashboard.Show();
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Invalid email or password.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid email or password.");
                        }
                    }
                }

            }
        }
    }


}