using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour ChangeOrderStatusDialog.xaml
    /// </summary>
    public partial class ChangeOrderStatusDialog : Window
    {
        private MySqlConnection connection;
        private int orderId;
        private string currentStatus;

        public ChangeOrderStatusDialog(int orderId)
        {
            InitializeComponent();

            this.orderId = orderId;
            this.connection = DB.SeConnecter();

            // Afficher l'ID de commande
            txtOrderId.Text = orderId.ToString();

            // Charger le statut actuel
            LoadCurrentStatus();
        }

        private void LoadCurrentStatus()
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = "SELECT statut FROM commande WHERE id = @orderId";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    currentStatus = cmd.ExecuteScalar()?.ToString();
                }

                // Sélectionner le RadioButton correspondant au statut actuel
                if (!string.IsNullOrEmpty(currentStatus))
                {
                    switch (currentStatus.ToLower())
                    {
                        case "en attente":
                            rbWaiting.IsChecked = true;
                            break;
                        case "en préparation":
                            rbPreparation.IsChecked = true;
                            break;
                        case "envoyé":
                            rbShipped.IsChecked = true;
                            break;
                        case "livré":
                            rbDelivered.IsChecked = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du statut actuel : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // Vérifier qu'un statut est sélectionné
            RadioButton selectedRadioButton = GetSelectedStatusRadioButton();
            if (selectedRadioButton == null)
            {
                MessageBox.Show("Veuillez sélectionner un statut.",
                    "Statut non sélectionné", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newStatus = selectedRadioButton.Tag.ToString();

            // Vérifier si le statut a changé
            if (newStatus == currentStatus)
            {
                MessageBox.Show("Le statut sélectionné est identique au statut actuel.",
                    "Aucun changement", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Mettre à jour le statut
            UpdateOrderStatus(newStatus);
        }

        private RadioButton GetSelectedStatusRadioButton()
        {
            if (rbWaiting.IsChecked == true) return rbWaiting;
            if (rbPreparation.IsChecked == true) return rbPreparation;
            if (rbShipped.IsChecked == true) return rbShipped;
            if (rbDelivered.IsChecked == true) return rbDelivered;
            return null;
        }

        private void UpdateOrderStatus(string newStatus)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = "UPDATE commande SET statut = @newStatus WHERE id = @orderId";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@newStatus", newStatus);
                    cmd.Parameters.AddWithValue("@orderId", orderId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        // Mise à jour réussie
                        DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show("Aucune modification n'a été effectuée. Veuillez réessayer.",
                            "Erreur de mise à jour", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour du statut : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}