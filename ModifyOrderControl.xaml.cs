using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PanelNikeStore
{
    public partial class ModifyOrderControl : UserControl
    {
        private MySqlConnection connection;
        private int currentOrderId = 0;
        private string adminUsername;

        // Listes pour suivre les modifications
        private List<int> itemsToDelete = new List<int>();
        private Dictionary<int, ItemChange> itemChanges = new Dictionary<int, ItemChange>();
        private List<NewOrderItem> newItems = new List<NewOrderItem>();

        public ModifyOrderControl(string adminUsername)
        {
            InitializeComponent();
            this.connection = DB.SeConnecter();
            this.adminUsername = adminUsername;

            // Initialiser le placeholder de recherche
            txtSearchOrder.Text = "Rechercher par n° commande ou identifiant client...";
            txtSearchOrder.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#757575"));
        }

        #region Gestion de l'interface utilisateur
        private void TxtSearchOrder_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearchOrder.Text == "Rechercher par n° commande ou identifiant client...")
            {
                txtSearchOrder.Text = "";
                txtSearchOrder.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void TxtSearchOrder_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchOrder.Text))
            {
                txtSearchOrder.Text = "Rechercher par n° commande ou identifiant client...";
                txtSearchOrder.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#757575"));
            }
        }

        private void TxtSearchOrder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSearchOrder_Click(sender, e);
            }
        }
        #endregion

        #region Recherche
        private void BtnSearchOrder_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearchOrder.Text.Trim();

            // Ignorer la recherche si c'est le texte placeholder
            if (string.IsNullOrEmpty(searchText) ||
                searchText == "Rechercher par n° commande ou identifiant client...")
            {
                MessageBox.Show("Veuillez entrer un numéro de commande ou un identifiant client.",
                    "Recherche vide", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Essayer d'abord comme un numéro de commande
            if (int.TryParse(searchText, out int orderId))
            {
                FindOrderById(orderId);
            }
            else
            {
                // Rechercher par identifiant client
                FindOrdersByClientId(searchText);
            }
        }

        private void FindOrderById(int orderId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(DB.GetConnectionString()))
                {
                    conn.Open();

                    string query = @"
                    SELECT c.id, c.user_id, c.montant_total, c.statut, c.commande_le, u.identifiant
                    FROM commande c
                    JOIN users u ON c.user_id = u.id_client
                    WHERE c.id = @orderId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentOrderId = orderId;
                                string clientId = reader["identifiant"].ToString();
                                string status = reader["statut"].ToString();
                                DateTime orderDate = Convert.ToDateTime(reader["commande_le"]);
                                decimal amount = Convert.ToDecimal(reader["montant_total"]);

                                bool isModifiable = IsOrderModifiable(status);
                                LoadOrderDetails(orderId, clientId, status, orderDate, amount, isModifiable);
                                return;
                            }
                        }
                    }

                    MessageBox.Show($"Aucune commande trouvée avec le numéro {orderId}.",
                        "Aucun résultat", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la recherche: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FindOrdersByClientId(string clientId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(DB.GetConnectionString()))
                {
                    conn.Open();

                    string query = @"
                    SELECT c.id, c.montant_total, c.statut, c.commande_le
                    FROM commande c
                    JOIN users u ON c.user_id = u.id_client
                    WHERE u.identifiant = @clientId
                    ORDER BY c.commande_le DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@clientId", clientId);

                        DataTable ordersTable = new DataTable();
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            adapter.Fill(ordersTable);
                        }

                        if (ordersTable.Rows.Count > 0)
                        {
                            OrdersDataGrid.ItemsSource = ordersTable.DefaultView;
                            DefaultMessagePanel.Visibility = Visibility.Collapsed;
                            OrdersSelectionPanel.Visibility = Visibility.Visible;
                            OrderDetailsPanel.Visibility = Visibility.Collapsed;
                            txtClientName.Text = clientId;
                            return;
                        }
                    }

                    MessageBox.Show($"Aucune commande trouvée pour le client {clientId}.",
                        "Aucun résultat", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la recherche: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsOrderModifiable(string status)
        {
            return status.ToLower() == "en attente" || status.ToLower() == "en préparation";
        }
        #endregion

        #region Chargement des détails
        private void LoadOrderDetails(int orderId, string clientId, string status, DateTime orderDate, decimal amount, bool isModifiable)
        {
            // Mettre à jour l'en-tête
            txtOrderTitle.Text = $"Commande n°{orderId}";
            txtOrderDate.Text = $"Commandé le {orderDate.ToString("dd/MM/yyyy")}";
            txtOrderClientId.Text = $"Client: {clientId}";
            txtOrderStatus.Text = FormatOrderStatus(status);
            bdOrderStatus.Background = GetStatusBrush(status);

            // Charger les articles
            LoadOrderItems(orderId);

            // Configurer les boutons selon l'état
            btnSaveChanges.IsEnabled = isModifiable;
            btnAddProduct.IsEnabled = isModifiable;

            if (!isModifiable)
            {
                txtModificationWarning.Text = "Cette commande ne peut plus être modifiée car elle a déjà été envoyée ou livrée.";
                txtModificationWarning.Visibility = Visibility.Visible;
            }
            else
            {
                txtModificationWarning.Visibility = Visibility.Collapsed;
            }

            // Afficher le panneau de détails
            DefaultMessagePanel.Visibility = Visibility.Collapsed;
            OrdersSelectionPanel.Visibility = Visibility.Collapsed;
            OrderDetailsPanel.Visibility = Visibility.Visible;

            // Réinitialiser les modifications 
            itemsToDelete.Clear();
            itemChanges.Clear();
            newItems.Clear();
            txtEstimatedTotal.Visibility = Visibility.Collapsed;
            btnCancelChanges.Visibility = Visibility.Collapsed;
        }

        private void LoadOrderItems(int orderId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(DB.GetConnectionString()))
                {
                    conn.Open();

                    string query = @"
                    SELECT 
                        dc.id as detail_id,
                        dc.product_id,
                        dc.variante_id,
                        dc.produit_taille_id,
                        dc.quantite,
                        dc.prix_achat,
                        p.nom AS product_name,
                        pv.couleur AS variant_name,
                        pt.taille AS size
                    FROM 
                        details_commande dc
                    JOIN 
                        produits p ON dc.product_id = p.id
                    LEFT JOIN 
                        produit_variantes pv ON dc.variante_id = pv.id
                    LEFT JOIN 
                        produit_tailles pt ON dc.produit_taille_id = pt.id
                    WHERE 
                        dc.commande_id = @orderId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);

                        DataTable itemsTable = new DataTable();
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            adapter.Fill(itemsTable);
                        }

                        // Ajouter une colonne pour le statut
                        if (!itemsTable.Columns.Contains("Statut"))
                        {
                            itemsTable.Columns.Add("Statut", typeof(string));
                        }

                        OrderItemsGrid.ItemsSource = itemsTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des articles: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string FormatOrderStatus(string status)
        {
            switch (status.ToLower())
            {
                case "en attente": return "En attente";
                case "en préparation": return "En préparation";
                case "envoyé": return "Envoyé";
                case "livré": return "Livré";
                default: return status;
            }
        }

        private SolidColorBrush GetStatusBrush(string status)
        {
            switch (status.ToLower())
            {
                case "en attente": return new SolidColorBrush(Color.FromRgb(117, 117, 117)); // Gris
                case "en préparation": return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                case "envoyé": return new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Bleu
                case "livré": return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Vert
                default: return new SolidColorBrush(Colors.Gray);
            }
        }
        #endregion

        #region Actions sur les articles
        private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int detailId = Convert.ToInt32(btn.Tag);

                MessageBoxResult result = MessageBox.Show(
                    "Êtes-vous sûr de vouloir supprimer cet article de la commande ?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Ajouter à la liste d'articles à supprimer
                    if (!itemsToDelete.Contains(detailId))
                    {
                        itemsToDelete.Add(detailId);
                    }

                    // Mettre à jour l'affichage
                    UpdateItemsDisplay();
                }
            }
        }

        private void BtnChangeItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int detailId = Convert.ToInt32(btn.Tag);

                // Trouver l'article dans la grille
                foreach (DataRowView rowView in (DataView)OrderItemsGrid.ItemsSource)
                {
                    if (Convert.ToInt32(rowView["detail_id"]) == detailId)
                    {
                        string productName = rowView["product_name"].ToString();
                        int currentQuantity = Convert.ToInt32(rowView["quantite"]);
                        int productId = Convert.ToInt32(rowView["product_id"]);
                        int variantId = Convert.ToInt32(rowView["variante_id"]);
                        int sizeId = Convert.ToInt32(rowView["produit_taille_id"]);

                        // Ouvrir notre boîte de dialogue personnalisée
                        SimpleQuantityDialog dialog = new SimpleQuantityDialog(productName, currentQuantity);
                        dialog.Owner = Window.GetWindow(this);

                        if (dialog.ShowDialog() == true)
                        {
                            int newQuantity = dialog.Quantity;

                            // Enregistrer la modification
                            itemChanges[detailId] = new ItemChange
                            {
                                ProductId = productId,
                                VariantId = variantId,
                                SizeId = sizeId,
                                Quantity = newQuantity
                            };

                            // Mettre à jour l'affichage
                            UpdateItemsDisplay();
                        }

                        break;
                    }
                }
            }
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            // Ouvrir la boîte de dialogue pour ajouter un produit
            AddProductToOrderDialog dialog = new AddProductToOrderDialog();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                int productId = dialog.SelectedProductId;
                int variantId = dialog.SelectedVariantId;
                int sizeId = dialog.SelectedSizeId;
                int quantity = dialog.SelectedQuantity;
                decimal price = dialog.SelectedPrice;

                // Ajouter le nouvel article
                newItems.Add(new NewOrderItem
                {
                    ProductId = productId,
                    VariantId = variantId,
                    SizeId = sizeId,
                    Quantity = quantity,
                    Price = price
                });

                // Récupérer le nom du produit
                string productName = GetProductInfo(productId, "nom");

                MessageBox.Show($"Le produit '{productName}' a été ajouté à la commande.",
                    "Produit ajouté", MessageBoxButton.OK, MessageBoxImage.Information);

                // Mettre à jour l'affichage
                UpdateItemsDisplay();
            }
        }

        private void UpdateItemsDisplay()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(DB.GetConnectionString()))
                {
                    conn.Open();

                    string query = @"
                    SELECT 
                        dc.id as detail_id,
                        dc.product_id,
                        dc.variante_id,
                        dc.produit_taille_id,
                        dc.quantite,
                        dc.prix_achat,
                        p.nom AS product_name,
                        pv.couleur AS variant_name,
                        pt.taille AS size
                    FROM 
                        details_commande dc
                    JOIN 
                        produits p ON dc.product_id = p.id
                    LEFT JOIN 
                        produit_variantes pv ON dc.variante_id = pv.id
                    LEFT JOIN 
                        produit_tailles pt ON dc.produit_taille_id = pt.id
                    WHERE 
                        dc.commande_id = @orderId";

                    DataTable itemsTable = new DataTable();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@orderId", currentOrderId);
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            adapter.Fill(itemsTable);
                        }
                    }

                    // Ajouter une colonne pour le statut
                    if (!itemsTable.Columns.Contains("Statut"))
                    {
                        itemsTable.Columns.Add("Statut", typeof(string));
                    }

                    // Marquer les articles à supprimer ou modifier
                    foreach (DataRow row in itemsTable.Rows)
                    {
                        int detailId = Convert.ToInt32(row["detail_id"]);

                        if (itemsToDelete.Contains(detailId))
                        {
                            row["Statut"] = "À supprimer";
                        }
                        else if (itemChanges.ContainsKey(detailId))
                        {
                            // Mettre à jour la quantité
                            row["quantite"] = itemChanges[detailId].Quantity;
                            row["Statut"] = "Modifié";
                        }
                        else
                        {
                            row["Statut"] = "";
                        }
                    }

                    // Ajouter les nouveaux articles
                    foreach (var newItem in newItems)
                    {
                        // Récupérer les informations du produit
                        string productName = GetProductInfo(newItem.ProductId, "nom");
                        string variantName = GetVariantInfo(newItem.VariantId, "couleur");
                        string size = GetSizeInfo(newItem.SizeId, "taille");

                        DataRow newRow = itemsTable.NewRow();
                        newRow["detail_id"] = -1; // ID temporaire négatif
                        newRow["product_id"] = newItem.ProductId;
                        newRow["variante_id"] = newItem.VariantId;
                        newRow["produit_taille_id"] = newItem.SizeId;
                        newRow["quantite"] = newItem.Quantity;
                        newRow["prix_achat"] = newItem.Price;
                        newRow["product_name"] = productName;
                        newRow["variant_name"] = variantName;
                        newRow["size"] = size;
                        newRow["Statut"] = "Nouveau";

                        itemsTable.Rows.Add(newRow);
                    }

                    // Mettre à jour la source de données
                    OrderItemsGrid.ItemsSource = itemsTable.DefaultView;

                    // Calculer le total estimé
                    decimal totalEstimated = CalculateEstimatedTotal(itemsTable);
                    txtEstimatedTotal.Text = $"Total estimé après modifications: {totalEstimated:C2}";

                    // Afficher ou masquer les éléments en fonction des modifications
                    bool hasChanges = itemsToDelete.Count > 0 || itemChanges.Count > 0 || newItems.Count > 0;
                    txtEstimatedTotal.Visibility = hasChanges ? Visibility.Visible : Visibility.Collapsed;
                    btnCancelChanges.Visibility = hasChanges ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour de l'affichage: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Méthodes auxiliaires
        private string GetProductInfo(int productId, string field)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(DB.GetConnectionString()))
                {
                    conn.Open();
                    string query = $"SELECT {field} FROM produits WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", productId);
                        object result = cmd.ExecuteScalar();
                        return result != null ? result.ToString() : "";
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        private string GetVariantInfo(int variantId, string field)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(DB.GetConnectionString()))
                {
                    conn.Open();
                    string query = $"SELECT {field} FROM produit_variantes WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", variantId);
                        object result = cmd.ExecuteScalar();
                        return result != null ? result.ToString() : "";
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        private string GetSizeInfo(int sizeId, string field)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(DB.GetConnectionString()))
                {
                    conn.Open();
                    string query = $"SELECT {field} FROM produit_tailles WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", sizeId);
                        object result = cmd.ExecuteScalar();
                        return result != null ? result.ToString() : "";
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        private decimal CalculateEstimatedTotal(DataTable itemsTable)
        {
            decimal total = 0;

            foreach (DataRow row in itemsTable.Rows)
            {
                string status = row["Statut"].ToString();
                if (status != "À supprimer")
                {
                    int quantity = Convert.ToInt32(row["quantite"]);
                    decimal price = Convert.ToDecimal(row["prix_achat"]);
                    total += quantity * price;
                }
            }

            return total;
        }
        #endregion

        #region Enregistrement des modifications
        private void BtnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            // Vérifier s'il y a des modifications
            if (itemsToDelete.Count == 0 && itemChanges.Count == 0 && newItems.Count == 0)
            {
                MessageBox.Show("Aucune modification n'a été effectuée.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Ouvrir la boîte de dialogue pour la raison
            ModificationReasonDialog dialog = new ModificationReasonDialog();
            dialog.Owner = Window.GetWindow(this); // Pour centrer correctement

            if (dialog.ShowDialog() == true)
            {
                string reason = dialog.ModificationReason;

                // Appliquer les modifications
                ApplyChangesToOrder(reason);
            }
        }

        private void ApplyChangesToOrder(string reason)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(DB.GetConnectionString()))
                {
                    conn.Open();

                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Supprimer les articles marqués
                            foreach (int detailId in itemsToDelete)
                            {
                                string deleteQuery = "DELETE FROM details_commande WHERE id = @detailId";
                                using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@detailId", detailId);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // 2. Mettre à jour les articles modifiés
                            foreach (var change in itemChanges)
                            {
                                string updateQuery = "UPDATE details_commande SET quantite = @quantite WHERE id = @detailId";
                                using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@quantite", change.Value.Quantity);
                                    cmd.Parameters.AddWithValue("@detailId", change.Key);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // 3. Ajouter les nouveaux articles
                            foreach (var item in newItems)
                            {
                                string insertQuery = @"
                                INSERT INTO details_commande
                                    (commande_id, product_id, quantite, prix_achat, variante_id, produit_taille_id)
                                VALUES
                                    (@commandeId, @productId, @quantite, @prixAchat, @varianteId, @produitTailleId)";

                                using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@commandeId", currentOrderId);
                                    cmd.Parameters.AddWithValue("@productId", item.ProductId);
                                    cmd.Parameters.AddWithValue("@quantite", item.Quantity);
                                    cmd.Parameters.AddWithValue("@prixAchat", item.Price);
                                    cmd.Parameters.AddWithValue("@varianteId", item.VariantId);
                                    cmd.Parameters.AddWithValue("@produitTailleId", item.SizeId);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // 4. Calculer le nouveau montant total
                            decimal newTotal = CalculateNewTotal(conn, transaction);

                            // 5. Mettre à jour le montant total de la commande
                            string updateOrderQuery = "UPDATE commande SET montant_total = @montantTotal WHERE id = @orderId";
                            using (MySqlCommand cmd = new MySqlCommand(updateOrderQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@montantTotal", newTotal);
                                cmd.Parameters.AddWithValue("@orderId", currentOrderId);
                                cmd.ExecuteNonQuery();
                            }

                            // 6. Enregistrer dans un journal de modifications
                            string logQuery = @"
                            INSERT INTO order_modifications
                                (order_id, admin_username, modification_date, reason, details)
                            VALUES
                                (@orderId, @adminUsername, NOW(), @reason, @details)";

                            string details = CreateModificationDetails();

                            using (MySqlCommand cmd = new MySqlCommand(logQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@orderId", currentOrderId);
                                cmd.Parameters.AddWithValue("@adminUsername", adminUsername);
                                cmd.Parameters.AddWithValue("@reason", reason);
                                cmd.Parameters.AddWithValue("@details", details);
                                cmd.ExecuteNonQuery();
                            }

                            // Valider la transaction
                            transaction.Commit();

                            MessageBox.Show($"Les modifications ont été enregistrées avec succès.\nRaison: {reason}",
                                "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Recharger les données
                            LoadOrderDetails(currentOrderId,
                                txtOrderClientId.Text.Replace("Client: ", ""),
                                txtOrderStatus.Text,
                                DateTime.Parse(txtOrderDate.Text.Replace("Commandé le ", "")),
                                newTotal,
                                true);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'application des modifications: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private decimal CalculateNewTotal(MySqlConnection conn, MySqlTransaction transaction)
        {
            string query = "SELECT SUM(quantite * prix_achat) FROM details_commande WHERE commande_id = @orderId";
            using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@orderId", currentOrderId);
                object result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToDecimal(result);
                }

                return 0;
            }
        }

        private string CreateModificationDetails()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (itemsToDelete.Count > 0)
            {
                sb.AppendLine("Articles supprimés:");
                foreach (int detailId in itemsToDelete)
                {
                    // Trouver les détails de l'article supprimé
                    string productName = "ID: " + detailId;
                    foreach (DataRowView row in ((DataView)OrderItemsGrid.ItemsSource))
                    {
                        if (Convert.ToInt32(row["detail_id"]) == detailId && row["Statut"].ToString() == "À supprimer")
                        {
                            productName = $"{row["product_name"]} ({row["variant_name"]}, taille {row["size"]})";
                            break;
                        }
                    }
                    sb.AppendLine($"- {productName}");
                }
                sb.AppendLine();
            }

            if (itemChanges.Count > 0)
            {
                sb.AppendLine("Articles modifiés:");
                foreach (var change in itemChanges)
                {
                    string productName = "ID: " + change.Key;
                    int oldQuantity = 0;

                    foreach (DataRowView row in ((DataView)OrderItemsGrid.ItemsSource))
                    {
                        if (Convert.ToInt32(row["detail_id"]) == change.Key && row["Statut"].ToString() == "Modifié")
                        {
                            productName = $"{row["product_name"]} ({row["variant_name"]}, taille {row["size"]})";
                            oldQuantity = Convert.ToInt32(row["quantite"]);
                            break;
                        }
                    }

                    sb.AppendLine($"- {productName}, Nouvelle quantité: {change.Value.Quantity} (précédemment: {oldQuantity})");
                }
                sb.AppendLine();
            }

            if (newItems.Count > 0)
            {
                sb.AppendLine("Articles ajoutés:");
                foreach (var item in newItems)
                {
                    string productName = GetProductInfo(item.ProductId, "nom");
                    string variantName = GetVariantInfo(item.VariantId, "couleur");
                    string size = GetSizeInfo(item.SizeId, "taille");
                    sb.AppendLine($"- {productName} ({variantName}, taille {size}), Quantité: {item.Quantity}, Prix: {item.Price:C2}");
                }
            }

            return sb.ToString();
        }
        #endregion

        #region Navigation
        private void BtnSelectOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int orderId = Convert.ToInt32(btn.Tag);
                FindOrderById(orderId);
            }
        }

        private void BtnBackToSearch_Click(object sender, RoutedEventArgs e)
        {
            // Vérifier s'il y a des modifications non enregistrées
            if (itemsToDelete.Count > 0 || itemChanges.Count > 0 || newItems.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Des modifications n'ont pas été enregistrées. Voulez-vous vraiment quitter sans enregistrer ?",
                    "Modifications non enregistrées", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            // Réinitialiser l'interface
            currentOrderId = 0;
            itemsToDelete.Clear();
            itemChanges.Clear();
            newItems.Clear();

            DefaultMessagePanel.Visibility = Visibility.Visible;
            OrdersSelectionPanel.Visibility = Visibility.Collapsed;
            OrderDetailsPanel.Visibility = Visibility.Collapsed;
        }

        private void BtnCancelChanges_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Voulez-vous vraiment annuler toutes les modifications ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Réinitialiser les modifications
                itemsToDelete.Clear();
                itemChanges.Clear();
                newItems.Clear();

                // Masquer les éléments liés aux modifications
                txtEstimatedTotal.Visibility = Visibility.Collapsed;
                btnCancelChanges.Visibility = Visibility.Collapsed;

                // Rafraîchir la vue
                LoadOrderItems(currentOrderId);
            }
        }
        #endregion

        // Classes pour les modifications
        private class ItemChange
        {
            public int ProductId { get; set; }
            public int VariantId { get; set; }
            public int SizeId { get; set; }
            public int Quantity { get; set; }
        }

        private class NewOrderItem
        {
            public int ProductId { get; set; }
            public int VariantId { get; set; }
            public int SizeId { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }
    }
}