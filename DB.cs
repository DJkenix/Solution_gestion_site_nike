using MySql.Data.MySqlClient;
using System.Configuration;
namespace PanelNikeStore
{
    public class DB
    {
        private static MySqlConnection connection;
        public static MySqlConnection SeConnecter()
        {
            if (connection == null)
            {
                string connString = ConfigurationManager.ConnectionStrings["MaconnexionDB"].ConnectionString;
                connection = new MySqlConnection(connString);
            }
            return connection;
        }
        public static void OpenDb()
        {
            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();
        }
        public static void CloseDb()
        {
            if (connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }

        // Ajoutez cette nouvelle méthode
        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["MaconnexionDB"].ConnectionString;
        }
    }
}