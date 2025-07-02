using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour DashboardWindow.xaml
    /// </summary>
    public partial class DashboardWindow : Window
    {
        private string LoggedInUser;
        private MySqlConnection connection;
        private StockManagementControl StockManagementPanel;
        private OrderManagementControl OrderManagementPanel;
        private ProductManagementControl ProductManagementPanel;
        private NewProductControl AddProductPanel;
        private ModifyOrderControl ModifyOrderPanel;
        private OrderHistoryControl OrderHistoryPanel;


        public DashboardWindow(string loggedInUser)
        {
            InitializeComponent();
            LoggedInUser = loggedInUser;
            this.connection = DB.SeConnecter();
            DB.OpenDb();

            // Par défaut, on masque tout sauf le tableau de bord
            HideAllControls();
            KPISection.Visibility = Visibility.Visible;

            // Initialiser les KPIs au démarrage
            InitializeKPIs();

            // Configurer les gestionnaires d'événements
            SetupEventHandlers();

            // Initialiser le contrôle de gestion des stocks
            StockManagementPanel = new StockManagementControl();

            // Trouver le Grid approprié dans la structure XAML
            // Dans votre XAML, il s'agit du Grid à l'intérieur du ScrollViewer
            // Ce Grid contient tous vos autres contrôles (KPISection, UserTable, etc.)
            var mainGrid = (Grid)((ScrollViewer)((Grid)((Grid)this.Content).Children[1]).Children[1]).Content;

            if (mainGrid != null)
            {
                // Ajouter le panneau de gestion des stocks au Grid
                mainGrid.Children.Add(StockManagementPanel);

                // Cacher le panneau par défaut
                StockManagementPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Erreur: Impossible de trouver le grid principal pour ajouter le panneau de stock.",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            OrderManagementPanel = new OrderManagementControl();
            if (mainGrid != null)
            {
                // Ajouter le panneau de gestion des commandes au Grid
                mainGrid.Children.Add(OrderManagementPanel);

                // Cacher le panneau par défaut
                OrderManagementPanel.Visibility = Visibility.Collapsed;
            }

            // 3. Ajouter cette ligne dans la méthode HideAllControls() :
            if (OrderManagementPanel != null)
                OrderManagementPanel.Visibility = Visibility.Collapsed;

            ProductManagementPanel = new ProductManagementControl();
            if (mainGrid != null)
            {
                // Ajouter le panneau de gestion des produits au Grid
                mainGrid.Children.Add(ProductManagementPanel);

                // Cacher le panneau par défaut
                ProductManagementPanel.Visibility = Visibility.Collapsed;
            }
            // Initialiser le contrôle d'ajout de produit
            AddProductPanel = new NewProductControl();
            if (mainGrid != null)
            {
                // Ajouter le panneau d'ajout de produit au Grid
                mainGrid.Children.Add(AddProductPanel);

                // Cacher le panneau par défaut
                AddProductPanel.Visibility = Visibility.Collapsed;
            }

            ModifyOrderPanel = new ModifyOrderControl(LoggedInUser);
            if (mainGrid != null)
            {
                // Ajouter le panneau de modification de commande au Grid
                mainGrid.Children.Add(ModifyOrderPanel);

                // Cacher le panneau par défaut
                ModifyOrderPanel.Visibility = Visibility.Collapsed;
            }

            OrderHistoryPanel = new OrderHistoryControl();
            if (mainGrid != null)
            {
                // Ajouter le panneau d'historique des commandes au Grid
                mainGrid.Children.Add(OrderHistoryPanel);

                // Cacher le panneau par défaut
                OrderHistoryPanel.Visibility = Visibility.Collapsed;
            }


        }

        // Méthode centralisée pour masquer tous les contrôles
        private void HideAllControls()
        {
            // Masquer tous les contrôles principaux
            if (KPISection != null)
                KPISection.Visibility = Visibility.Collapsed;

            if (UserTable != null)
                UserTable.Visibility = Visibility.Collapsed;

            if (InfoCard != null)
                InfoCard.Visibility = Visibility.Collapsed;

            if (AddUserPanel != null)
                AddUserPanel.Visibility = Visibility.Collapsed;

            if (RoleManagementPanel != null)
                RoleManagementPanel.Visibility = Visibility.Collapsed;

            // Assurez-vous que les panneaux de gestion sont également masqués
            if (StockManagementPanel != null)
                StockManagementPanel.Visibility = Visibility.Collapsed;

            if (OrderManagementPanel != null)
                OrderManagementPanel.Visibility = Visibility.Collapsed;

            if (ProductManagementPanel != null)
                ProductManagementPanel.Visibility = Visibility.Collapsed;

            if (AddProductPanel != null)
                AddProductPanel.Visibility = Visibility.Collapsed;

            if (ModifyOrderPanel != null)
                ModifyOrderPanel.Visibility = Visibility.Collapsed;

            if (OrderHistoryPanel != null)
                OrderHistoryPanel.Visibility = Visibility.Collapsed;
        }

        // Méthode séparée pour la configuration des gestionnaires d'événements
        private void SetupEventHandlers()
        {
            // KPI Section
            if (KPISection != null && KPISection.btnUserStatsInfo != null)
            {
                KPISection.btnUserStatsInfo.Click += btnUserStatsInfo_Click;
            }

            // User Table
            if (UserTable != null)
            {
                UserTable.txtSearch.TextChanged += txtSearch_TextChanged;
                UserTable.btnSearch.Click += btnSearch_Click;
                ConfigureDataGridButtons();
            }
        }

        // Méthode spécifique pour configurer les boutons de la DataGrid
        private void ConfigureDataGridButtons()
        {
            if (UserTable != null)
            {
                // Abonner aux événements personnalisés
                UserTable.UserDeleted += (sender, userId) => {
                    // Le rechargement est déjà géré par UserTableControl
                };

                UserTable.UserModified += (sender, userId) => {
                    // Le rechargement est déjà géré par UserTableControl
                };
            }
        }


        private Button FindButtonInRow(DataGridRow row, string content)
        {
            if (row.DataContext != null)
            {
                // Trouver la cellule qui contient les boutons
                DataGridCell cell = FindVisualChild<DataGridCell>(row);
                if (cell != null)
                {
                    // Trouver le StackPanel contenant les boutons
                    StackPanel panel = FindVisualChild<StackPanel>(cell);
                    if (panel != null)
                    {
                        // Recherche parmi les enfants du StackPanel
                        foreach (var child in panel.Children)
                        {
                            if (child is Button button && button.Content.ToString() == content)
                                return button;
                        }
                    }
                }
            }
            return null;
        }

        // Méthode helper pour trouver un élément visuel enfant d'un certain type
        private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void InitializeKPIs()
        {
            try
            {
                // Initialiser les KPIs avec des données de la base de données
                if (KPISection != null)
                {
                    string queryUserCount = "SELECT COUNT(*) FROM users";
                    MySqlCommand cmdUserCount = new MySqlCommand(queryUserCount, connection);
                    int userCount = Convert.ToInt32(cmdUserCount.ExecuteScalar());

                    // À adapter selon votre structure de UserControl
                    // KPISection.UsersCard.txtCardTitle.Text = "Utilisateurs Inscrits";
                    // KPISection.UsersCard.txtCardCount.Text = userCount.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'initialisation des KPIs : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Gestion du menu
        // Gestion des sous-menus
        private void ToggleUsersSubMenu(object sender, RoutedEventArgs e)
        {
            UsersSubMenu.Visibility = UsersSubMenu.Visibility == Visibility.Visible ?
                                      Visibility.Collapsed : Visibility.Visible;

            // Optionnel: Masquer les autres sous-menus
            ProductsSubMenu.Visibility = Visibility.Collapsed;
            OrdersSubMenu.Visibility = Visibility.Collapsed;
        }

        private void ToggleProductsSubMenu(object sender, RoutedEventArgs e)
        {
            ProductsSubMenu.Visibility = ProductsSubMenu.Visibility == Visibility.Visible ?
                                         Visibility.Collapsed : Visibility.Visible;

            // Masquer les autres sous-menus
            UsersSubMenu.Visibility = Visibility.Collapsed;
            OrdersSubMenu.Visibility = Visibility.Collapsed;
        }

        private void ToggleOrdersSubMenu(object sender, RoutedEventArgs e)
        {
            OrdersSubMenu.Visibility = OrdersSubMenu.Visibility == Visibility.Visible ?
                                       Visibility.Collapsed : Visibility.Visible;

            // Masquer les autres sous-menus
            UsersSubMenu.Visibility = Visibility.Collapsed;
            ProductsSubMenu.Visibility = Visibility.Collapsed;
        }

        private void ShowDashboard_Click(object sender, RoutedEventArgs e)
        {
            // Masquer tout d'abord
            HideAllControls();

            // Afficher le tableau de bord
            KPISection.Visibility = Visibility.Visible;

            // Masquer tous les sous-menus
            UsersSubMenu.Visibility = Visibility.Collapsed;
            ProductsSubMenu.Visibility = Visibility.Collapsed;
            OrdersSubMenu.Visibility = Visibility.Collapsed;
        }
        #endregion

        private void btnUserStatsInfo_Click(object sender, RoutedEventArgs e)
        {
            // Ouvrir le pop-up
            UserStatsPopup popup = new UserStatsPopup();
            popup.ShowDialog(); // Le pop-up se charge et affiche les données automatiquement
        }

        // Gestion des Produits
        private void btnManageStock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Masquer tous les contrôles d'abord
                HideAllControls();

                // Afficher uniquement le panneau de gestion des stocks
                if (StockManagementPanel != null)
                {
                    StockManagementPanel.Visibility = Visibility.Visible;

                    // S'assurer que le panneau est bien mis à jour
                    StockManagementPanel.UpdateLayout();
                }
                else
                {
                    MessageBox.Show("Erreur: Le panneau de gestion des stocks n'a pas été initialisé.",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Masquer tous les sous-menus
                UsersSubMenu.Visibility = Visibility.Collapsed;
                ProductsSubMenu.Visibility = Visibility.Collapsed;
                OrdersSubMenu.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'affichage du panneau de stock: {ex.Message}",
                              "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnModifyProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Masquer tous les contrôles d'abord
                HideAllControls();

                // Afficher uniquement le panneau de gestion des produits
                if (ProductManagementPanel != null)
                {
                    ProductManagementPanel.Visibility = Visibility.Visible;
                    ProductManagementPanel.UpdateLayout();
                }
                else
                {
                    MessageBox.Show("Erreur: Le panneau de gestion des produits n'a pas été initialisé.",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Masquer tous les sous-menus
                UsersSubMenu.Visibility = Visibility.Collapsed;
                ProductsSubMenu.Visibility = Visibility.Collapsed;
                OrdersSubMenu.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'affichage du panneau des produits: {ex.Message}",
                              "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Masquer tous les contrôles d'abord
                HideAllControls();

                // Afficher uniquement le panneau d'ajout de produit
                if (AddProductPanel != null)
                {
                    AddProductPanel.Visibility = Visibility.Visible;
                    AddProductPanel.UpdateLayout();
                }
                else
                {
                    MessageBox.Show("Erreur: Le panneau d'ajout de produit n'a pas été initialisé.",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Masquer tous les sous-menus
                UsersSubMenu.Visibility = Visibility.Collapsed;
                ProductsSubMenu.Visibility = Visibility.Collapsed;
                OrdersSubMenu.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'affichage du panneau d'ajout de produit: {ex.Message}",
                              "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadClientsTable(string searchText = "")
        {
            try
            {
                if (UserTable != null)
                {
                    UserTable.LoadClientsTable(searchText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des clients : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Méthode pour afficher des statistiques utilisateur
        private void ShowUserStats()
        {
            if (InfoCard != null)
            {
                try
                {
                    string queryUserCount = "SELECT COUNT(*) FROM users";
                    MySqlCommand cmdUserCount = new MySqlCommand(queryUserCount, connection);
                    int userCount = Convert.ToInt32(cmdUserCount.ExecuteScalar());

                    // Adapter à votre structure de contrôle
                    // InfoCard.txtCardTitle.Text = "Utilisateurs Inscrits";
                    // InfoCard.txtCardCount.Text = userCount.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur lors du chargement des statistiques : " + ex.Message);
                }
            }
        }

        // Gestion des Utilisateurs
        private void btnModifyClient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Masquer tous les contrôles d'abord
                HideAllControls();

                // Afficher uniquement le tableau des utilisateurs
                UserTable.Visibility = Visibility.Visible;

                // Charger les utilisateurs dans le tableau
                LoadClientsTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'affichage des clients : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDeleteClient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button deleteButton && deleteButton.Tag != null)
                {
                    string userId = deleteButton.Tag.ToString();

                    // Demander une confirmation avant de supprimer
                    MessageBoxResult result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cet utilisateur ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Exécuter la requête SQL pour supprimer l'utilisateur
                        MySqlCommand cmd = new MySqlCommand("DELETE FROM users WHERE identifiant = @userId", connection);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.ExecuteNonQuery();

                        // Recharger le tableau après la suppression
                        LoadClientsTable();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la suppression de l'utilisateur : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UserTable != null && UserTable.txtSearch != null)
            {
                LoadClientsTable(UserTable.txtSearch.Text);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (UserTable != null && UserTable.txtSearch != null)
            {
                LoadClientsTable(UserTable.txtSearch.Text);
            }
        }

        private void btnAddClient_Click(object sender, RoutedEventArgs e)
        {
            // Masquer tous les contrôles d'abord
            HideAllControls();

            // Afficher uniquement le panneau d'ajout d'utilisateur
            AddUserPanel.Visibility = Visibility.Visible;
        }

        private void btnManageRoles_Click(object sender, RoutedEventArgs e)
        {
            // Masquer tous les contrôles d'abord
            HideAllControls();

            // Initialiser le panneau de gestion des rôles avec l'identifiant de l'administrateur
            if (RoleManagementPanel == null)
            {
                RoleManagementPanel = new RoleManagementControl(LoggedInUser);
            }
            else
            {
                // Si le panneau existe déjà, assurez-vous qu'il a le bon adminUsername
                RoleManagementPanel.SetAdminUsername(LoggedInUser);
            }

            // Afficher uniquement le panneau de gestion des rôles
            RoleManagementPanel.Visibility = Visibility.Visible;
        }

        // Gestion des Commandes
        private void btnOrderTracking_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Masquer tous les contrôles d'abord
                HideAllControls();

                // Afficher uniquement le panneau de gestion des commandes
                if (OrderManagementPanel != null)
                {
                    OrderManagementPanel.Visibility = Visibility.Visible;
                    OrderManagementPanel.UpdateLayout();
                }
                else
                {
                    MessageBox.Show("Erreur: Le panneau de gestion des commandes n'a pas été initialisé.",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Masquer tous les sous-menus
                UsersSubMenu.Visibility = Visibility.Collapsed;
                ProductsSubMenu.Visibility = Visibility.Collapsed;
                OrdersSubMenu.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'affichage du panneau de commandes: {ex.Message}",
                              "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnModifyOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Masquer tous les contrôles d'abord
                HideAllControls();

                // Afficher uniquement le panneau de modification de commande
                if (ModifyOrderPanel != null)
                {
                    ModifyOrderPanel.Visibility = Visibility.Visible;
                    ModifyOrderPanel.UpdateLayout();
                }
                else
                {
                    MessageBox.Show("Erreur: Le panneau de modification de commande n'a pas été initialisé.",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Masquer tous les sous-menus
                UsersSubMenu.Visibility = Visibility.Collapsed;
                ProductsSubMenu.Visibility = Visibility.Collapsed;
                OrdersSubMenu.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'affichage du panneau de modification de commande: {ex.Message}",
                              "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnOrderHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Masquer tous les contrôles d'abord
                HideAllControls();

                // Afficher uniquement le panneau d'historique des commandes
                if (OrderHistoryPanel != null)
                {
                    OrderHistoryPanel.Visibility = Visibility.Visible;
                    OrderHistoryPanel.UpdateLayout();
                }
                else
                {
                    MessageBox.Show("Erreur: Le panneau d'historique des commandes n'a pas été initialisé.",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Masquer tous les sous-menus
                UsersSubMenu.Visibility = Visibility.Collapsed;
                ProductsSubMenu.Visibility = Visibility.Collapsed;
                OrdersSubMenu.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'affichage du panneau d'historique des commandes: {ex.Message}",
                              "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Mode Sombre
        private void DarkModeToggle_Click(object sender, RoutedEventArgs e)
        {
            // Basculer entre les thèmes
            ThemeManager.Instance.ToggleTheme();

            // Mettre à jour le texte du bouton en fonction du thème actuel
            if (ThemeManager.Instance.CurrentTheme == ThemeType.Dark)
            {
                // Mode sombre activé
                DarkModeToggle.Content = "🌞"; // Emoji soleil pour retourner en mode clair

                // Mettre à jour le texte à côté du toggle
                var textBlock = DarkModeToggle.Parent as DockPanel;
                if (textBlock != null && textBlock.Children.Count > 1)
                {
                    if (textBlock.Children[1] is TextBlock darkModeText)
                        darkModeText.Text = "Mode Clair";
                }
            }
            else
            {
                // Mode clair activé 
                DarkModeToggle.Content = "🌙"; // Emoji lune pour passer en mode sombre

                // Mettre à jour le texte à côté du toggle
                var dockPanel = DarkModeToggle.Parent as DockPanel;
                if (dockPanel != null && dockPanel.Children.Count > 1)
                {
                    if (dockPanel.Children[1] is TextBlock darkModeText)
                        darkModeText.Text = "Mode Sombre";
                }
            }
        }

        // Ajoutez ce code dans le constructeur après InitializeComponent()
        private void InitializeTheme()
        {
            // Appliquer le thème par défaut
            ThemeManager.Instance.ApplyTheme();

            // Initialiser l'apparence du toggle selon le thème actuel
            if (ThemeManager.Instance.CurrentTheme == ThemeType.Dark)
            {
                DarkModeToggle.Content = "🌞";

                // Mettre à jour le texte à côté du toggle
                var dockPanel = DarkModeToggle.Parent as DockPanel;
                if (dockPanel != null && dockPanel.Children.Count > 1)
                {
                    if (dockPanel.Children[1] is TextBlock darkModeText)
                        darkModeText.Text = "Mode Clair";
                }
            }
            else
            {
                DarkModeToggle.Content = "🌙";

                // Mettre à jour le texte à côté du toggle
                var dockPanel = DarkModeToggle.Parent as DockPanel;
                if (dockPanel != null && dockPanel.Children.Count > 1)
                {
                    if (dockPanel.Children[1] is TextBlock darkModeText)
                        darkModeText.Text = "Mode Sombre";
                }
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Code pour déconnecter l'utilisateur
            MessageBox.Show("Vous avez été déconnecté.", "Déconnexion", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close(); // Fermer la fenêtre actuelle

            // Optionnel: Afficher la page de connexion
            LoginPage loginPage = new LoginPage();
            loginPage.Show();
        }

        private void ModifierUser(object sender, RoutedEventArgs e)
        {
            try
            {
                // Récupérer l'utilisateur à modifier
                DataRowView selectedRow = null;

                // Vérifier si l'événement vient d'un bouton dans le DataGrid
                if (sender is Button button && button.Tag != null)
                {
                    string userId = button.Tag.ToString();

                    // Trouver la ligne correspondante dans les items du DataGrid
                    selectedRow = UserTable.ClientTable.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(row => row["identifiant"].ToString() == userId);
                }
                // Sinon, vérifier si une ligne est sélectionnée directement
                else if (UserTable.ClientTable.SelectedItem != null)
                {
                    selectedRow = (DataRowView)UserTable.ClientTable.SelectedItem;
                }

                // Si aucune ligne n'est sélectionnée ou trouvée
                if (selectedRow == null)
                {
                    MessageBox.Show("Veuillez sélectionner un utilisateur à modifier.",
                        "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Ouvrir la fenêtre de modification
                ModifierClient modifierWindow = new ModifierClient(
                    selectedRow["identifiant"].ToString(),
                    selectedRow["email"].ToString(),
                    selectedRow["role"].ToString()
                );

                // Ajouter un gestionnaire d'événements pour actualiser après modification
                modifierWindow.Closed += (s, args) => {
                    // Recharger le tableau des utilisateurs
                    LoadClientsTable();
                };

                modifierWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la modification de l'utilisateur : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}