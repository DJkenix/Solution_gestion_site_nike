using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour KPISectionControl.xaml
    /// </summary>
    public partial class KPISectionControl : UserControl
    {
        private MySqlConnection connection;

        public KPISectionControl()
        {
            InitializeComponent();
            this.connection = DB.SeConnecter();

            // Attendre que le contrôle soit chargé pour dessiner le graphique
            this.Loaded += (s, e) => {
                LoadDashboardData();
                DrawSalesChart();
            };
        }

        private void LoadDashboardData()
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                // Charger les statistiques utilisateurs
                int userCount = GetCount("users");
                txtUserCount.Text = userCount.ToString();

                // Charger les statistiques produits
                int productCount = GetCount("produits");
                txtProductCount.Text = productCount.ToString();

                // Charger les statistiques commandes
                int orderCount = GetCount("commande");
                txtOrderCount.Text = orderCount.ToString();

                // Charger les commandes récentes
                int newOrdersToday = GetTodayOrdersCount();
                txtNewOrdersCount.Text = newOrdersToday.ToString();

                // Charger le revenu total
                decimal totalRevenue = GetTotalRevenue();
                txtRevenue.Text = totalRevenue.ToString("N0") + " €";

                // Charger les commandes récentes dans la ListView
                LoadRecentOrders();

                // Charger les alertes de stock dans la ListView
                LoadStockAlerts();

                // Charger les produits les plus vendus dans la ListView
                LoadTopProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des données: " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetCount(string tableName)
        {
            string query = $"SELECT COUNT(*) FROM {tableName}";
            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private int GetTodayOrdersCount()
        {
            string query = "SELECT COUNT(*) FROM commande WHERE DATE(commande_le) = CURDATE()";
            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private decimal GetTotalRevenue()
        {
            string query = "SELECT SUM(montant_total) FROM commande";
            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                object result = cmd.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }
        }

        private void LoadRecentOrders()
        {
            // Cette méthode charge les commandes récentes dans la ListView
            ObservableCollection<RecentOrder> orders = new ObservableCollection<RecentOrder>();

            string query = @"
                SELECT c.id, c.montant_total, c.statut, c.commande_le, u.identifiant
                FROM commande c
                JOIN users u ON c.user_id = u.id_client
                ORDER BY c.commande_le DESC
                LIMIT 5";

            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(new RecentOrder
                        {
                            OrderId = Convert.ToInt32(reader["id"]),
                            ClientName = reader["identifiant"].ToString(),
                            OrderDate = Convert.ToDateTime(reader["commande_le"]).ToString("dd/MM/yyyy"),
                            Amount = Convert.ToDecimal(reader["montant_total"]).ToString("C2"),
                            Status = reader["statut"].ToString(),
                            StatusColor = GetStatusColor(reader["statut"].ToString())
                        });
                    }
                }
            }

            // Affecter la source de données à la ListView
            lvRecentOrders.ItemsSource = orders;
        }

        private SolidColorBrush GetStatusColor(string status)
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

        private void LoadStockAlerts()
        {
            // Cette méthode charge les alertes de stock dans la ListView
            ObservableCollection<StockAlert> alerts = new ObservableCollection<StockAlert>();

            string query = @"
                SELECT p.id, p.nom, pt.taille, pt.stock
                FROM produit_tailles pt
                JOIN produit_variantes pv ON pt.variante_id = pv.id
                JOIN produits p ON pv.produit_id = p.id
                WHERE pt.stock <= 5 AND pt.stock > 0
                ORDER BY pt.stock ASC
                LIMIT 5";

            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        alerts.Add(new StockAlert
                        {
                            ProductId = Convert.ToInt32(reader["id"]),
                            ProductName = reader["nom"].ToString(),
                            AlertMessage = $"Taille {reader["taille"]} - Stock bas: {reader["stock"]} restant(s)"
                        });
                    }
                }
            }

            // Afficher les produits en rupture de stock
            string outOfStockQuery = @"
                SELECT DISTINCT p.id, p.nom, pv.couleur
                FROM produits p
                JOIN produit_variantes pv ON p.id = pv.produit_id
                LEFT JOIN produit_tailles pt ON pv.id = pt.variante_id
                WHERE pt.stock = 0 OR pt.id IS NULL
                LIMIT 3";

            using (MySqlCommand cmd = new MySqlCommand(outOfStockQuery, connection))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        alerts.Add(new StockAlert
                        {
                            ProductId = Convert.ToInt32(reader["id"]),
                            ProductName = reader["nom"].ToString(),
                            AlertMessage = $"Variante {reader["couleur"]} - En rupture de stock!"
                        });
                    }
                }
            }

            // Affecter la source de données à la ListView
            lvStockAlerts.ItemsSource = alerts;
        }

        private void LoadTopProducts()
        {
            // Cette méthode charge les produits les plus vendus dans la ListView
            ObservableCollection<TopProduct> products = new ObservableCollection<TopProduct>();

            string query = @"
                SELECT p.id, p.nom, p.type_produit, COUNT(dc.id) as count
                FROM produits p
                JOIN details_commande dc ON p.id = dc.product_id
                GROUP BY p.id, p.nom, p.type_produit
                ORDER BY count DESC
                LIMIT 5";

            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    int rank = 1;
                    while (reader.Read())
                    {
                        string productName = reader["nom"].ToString();
                        int salesCount = Convert.ToInt32(reader["count"]);

                        products.Add(new TopProduct
                        {
                            Rank = rank++,
                            ProductId = Convert.ToInt32(reader["id"]),
                            ProductName = productName,
                            Category = reader["type_produit"].ToString(),
                            // Format: "X vendus" à afficher après le nom du produit
                            SalesCount = $"{salesCount} vendus"
                        });
                    }
                }
            }

            // Affecter la source de données à la ListView
            lvTopProducts.ItemsSource = products;
        }

        private List<SalesData> GetMonthlySales()
        {
            List<SalesData> result = new List<SalesData>();

            try
            {
                // Obtenir le mois actuel (nous sommes en avril 2025)
                int currentMonth = 4; // Avril 2025

                // Nous voulons afficher jusqu'à 4 mois (janvier, février, mars, avril)
                int monthLimit = 4;

                string query = @"
                    SELECT MONTH(commande_le) as month, SUM(montant_total) as total
                    FROM commande
                    WHERE YEAR(commande_le) = YEAR(CURDATE()) AND MONTH(commande_le) <= @monthLimit
                    GROUP BY MONTH(commande_le)
                    ORDER BY month ASC";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@monthLimit", monthLimit);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new SalesData
                            {
                                Month = Convert.ToInt32(reader["month"]),
                                Amount = Convert.ToDecimal(reader["total"])
                            });
                        }
                    }
                }

                // Si nous n'avons pas de données pour certains mois, ajouter des mois avec des ventes à 0
                // Créer un dictionnaire pour faciliter la vérification
                Dictionary<int, SalesData> monthData = new Dictionary<int, SalesData>();
                foreach (var data in result)
                {
                    monthData[data.Month] = data;
                }

                // S'assurer que tous les mois sont présents (janvier à avril)
                for (int i = 1; i <= monthLimit; i++)
                {
                    if (!monthData.ContainsKey(i))
                    {
                        result.Add(new SalesData
                        {
                            Month = i,
                            Amount = 0
                        });
                    }
                }

                // Trier par mois
                result.Sort((a, b) => a.Month.CompareTo(b.Month));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la récupération des ventes mensuelles: " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Si aucune donnée n'a été trouvée, générer des données d'exemple
            if (result.Count == 0)
            {
                result = GenerateSampleSalesData();
            }

            return result;
        }

        private List<SalesData> GenerateSampleSalesData()
        {
            // Cette méthode génère des données d'exemple pour le graphique
            List<SalesData> sampleData = new List<SalesData>();

            // Données d'exemple pour les 4 premiers mois de 2025
            sampleData.Add(new SalesData { Month = 1, Amount = 5000 }); // Janvier
            sampleData.Add(new SalesData { Month = 2, Amount = 6200 }); // Février
            sampleData.Add(new SalesData { Month = 3, Amount = 5500 }); // Mars
            sampleData.Add(new SalesData { Month = 4, Amount = 7100 }); // Avril

            return sampleData;
        }

        private void DrawSalesChart()
        {
            try
            {
                // Vérifier que le canvas existe
                if (ChartCanvas == null) return;

                // Vider le canvas
                ChartCanvas.Children.Clear();

                // Récupérer les données de ventes par mois
                List<SalesData> salesData = GetMonthlySales();
                // Si aucune donnée, afficher un message
                if (salesData.Count == 0)
                {
                    TextBlock noData = new TextBlock
                    {
                        Text = "Aucune donnée de vente disponible",
                        Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 12
                    };
                    ChartCanvas.Children.Add(noData);
                    Canvas.SetLeft(noData, ChartCanvas.ActualWidth / 2 - 90);
                    Canvas.SetTop(noData, ChartCanvas.ActualHeight / 2 - 10);
                    return;
                }

                // Trouver la valeur maximale pour l'échelle
                decimal maxValue = 0;
                foreach (var data in salesData)
                {
                    if (data.Amount > maxValue)
                        maxValue = data.Amount;
                }

                // Arrondir le maximum pour une meilleure lisibilité et ajouter une marge de 20%
                maxValue = Math.Ceiling(maxValue / 1000) * 1000;
                maxValue = maxValue * 1.2m; // Ajoute 20% d'espace au-dessus de la valeur max

                // Éviter la division par zéro
                if (maxValue == 0) maxValue = 1000;

                // Dimensions du graphique
                double chartWidth = ChartCanvas.ActualWidth;
                double chartHeight = ChartCanvas.ActualHeight;
                double chartMargin = 30; // Marge pour les étiquettes et valeurs
                double barPadding = 10; // Espace entre les barres

                // Espace disponible pour les barres
                double availableWidth = chartWidth - (2 * chartMargin);
                double availableHeight = chartHeight - chartMargin;
                double barWidth = (availableWidth / salesData.Count) - barPadding;

                // Dessiner les lignes horizontales de référence (25%, 50%, 75%, 100%)
                for (int i = 0; i <= 4; i++)
                {
                    double lineY = availableHeight - (i * (availableHeight / 4));

                    // Ligne horizontale
                    Line gridLine = new Line
                    {
                        X1 = chartMargin,
                        Y1 = lineY,
                        X2 = chartWidth - chartMargin,
                        Y2 = lineY,
                        Stroke = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                        StrokeThickness = 1
                    };
                    ChartCanvas.Children.Add(gridLine);

                    // Étiquette de valeur
                    if (i > 0) // Ne pas afficher 0
                    {
                        decimal value = (maxValue / 4) * i;
                        TextBlock valueLabel = new TextBlock
                        {
                            Text = value.ToString("N0"),
                            FontSize = 9,
                            Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150))
                        };
                        ChartCanvas.Children.Add(valueLabel);
                        Canvas.SetLeft(valueLabel, 0);
                        Canvas.SetTop(valueLabel, lineY - 10);
                    }
                }

                // Créer un dictionnaire pour les noms des mois
                Dictionary<int, string> monthNames = new Dictionary<int, string>()
                {
                    { 1, "Janvier" },
                    { 2, "Février" },
                    { 3, "Mars" },
                    { 4, "Avril" }
                };

                // Dessiner les barres du graphique
                for (int i = 0; i < salesData.Count; i++)
                {
                    var data = salesData[i];

                    // Calculer la hauteur de la barre proportionnellement à la valeur max
                    double barHeight = data.Amount <= 0 ? 0 : (double)(data.Amount / maxValue) * availableHeight;
                    if (barHeight < 2 && data.Amount > 0) barHeight = 2; // Hauteur minimale visible

                    // Position X de la barre (centrer dans l'espace disponible)
                    double barX = chartMargin + (i * (availableWidth / salesData.Count)) + (barPadding / 2);

                    // Position Y de la barre (à partir du bas du graphique)
                    double barY = availableHeight - barHeight;

                    // Dessiner le dégradé pour la barre
                    LinearGradientBrush gradientBrush = new LinearGradientBrush();
                    gradientBrush.StartPoint = new Point(0, 0);
                    gradientBrush.EndPoint = new Point(0, 1);
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(33, 150, 243), 0.0)); // Bleu principal
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(100, 181, 246), 1.0)); // Bleu plus clair

                    // Créer la barre
                    Rectangle bar = new Rectangle
                    {
                        Width = barWidth,
                        Height = barHeight,
                        Fill = gradientBrush,
                        RadiusX = 4,
                        RadiusY = 4,
                        Effect = new System.Windows.Media.Effects.DropShadowEffect
                        {
                            ShadowDepth = 1,
                            BlurRadius = 4,
                            Opacity = 0.15,
                            Color = Colors.Black
                        }
                    };

                    // Animer l'apparition de la barre
                    System.Windows.Media.Animation.DoubleAnimation animation =
                        new System.Windows.Media.Animation.DoubleAnimation(0, barHeight,
                            new System.Windows.Duration(TimeSpan.FromMilliseconds(500)))
                        {
                            EasingFunction = new System.Windows.Media.Animation.QuadraticEase()
                            {
                                EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut
                            }
                        };

                    // Positionner la barre
                    ChartCanvas.Children.Add(bar);
                    Canvas.SetLeft(bar, barX);
                    Canvas.SetTop(bar, barY);

                    // Démarrer l'animation
                    bar.BeginAnimation(Rectangle.HeightProperty, animation);

                    // Ajouter l'étiquette de valeur au-dessus de la barre
                    if (data.Amount > 0)
                    {
                        TextBlock valueLabel = new TextBlock
                        {
                            Text = data.Amount.ToString("N0") + " €",
                            FontSize = 10,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243))
                        };
                        ChartCanvas.Children.Add(valueLabel);

                        // Placer l'étiquette de valeur
                        double labelX = barX + (barWidth / 2) - 20; // Centrer sur la barre
                        double labelY;

                        // Si la barre est très haute, placer l'étiquette à l'intérieur de la barre
                        if (barY < 25)
                        {
                            labelY = barY + 5; // Juste à l'intérieur du haut de la barre
                            valueLabel.Foreground = new SolidColorBrush(Colors.White); // Texte blanc
                        }
                        else
                        {
                            labelY = barY - 20; // Au-dessus de la barre
                        }

                        Canvas.SetLeft(valueLabel, labelX);
                        Canvas.SetTop(valueLabel, labelY);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du dessin du graphique: " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Gestionnaire d'événement pour le bouton Détails des commandes
        private void BtnOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                try
                {
                    // Récupérer l'ID de la commande à partir du Tag du bouton
                    int orderId = Convert.ToInt32(btn.Tag);

                    // Créer et afficher la fenêtre de détails de commande
                    OrderDetailsWindow detailsWindow = new OrderDetailsWindow(orderId);
                    detailsWindow.Owner = Window.GetWindow(this); // Définir la fenêtre parent
                    detailsWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'ouverture des détails de commande: {ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Gestionnaire pour le bouton d'info des stats utilisateurs
        private void BtnUserStatsInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ouvrir la fenêtre popup des statistiques utilisateurs
                UserStatsPopup popup = new UserStatsPopup();
                popup.Owner = Window.GetWindow(this); // Définir la fenêtre parent
                popup.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture des statistiques utilisateurs: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Classes de données pour les différentes listes
    public class RecentOrder
    {
        public int OrderId { get; set; }
        public string ClientName { get; set; }
        public string OrderDate { get; set; }
        public string Amount { get; set; }
        public string Status { get; set; }
        public SolidColorBrush StatusColor { get; set; }
    }

    public class StockAlert
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string AlertMessage { get; set; }
    }

    public class TopProduct
    {
        public int Rank { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public string SalesCount { get; set; }
    }

    public class SalesData
    {
        public int Month { get; set; }
        public decimal Amount { get; set; }
    }
}