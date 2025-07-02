using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour NewProductControl.xaml
    /// </summary>
    public partial class NewProductControl : UserControl
    {
        #region Propriétés privées
        private MySqlConnection connection;
        private ObservableCollection<ProductVariantViewModel> variants;
        private ObservableCollection<ProductImageViewModel> productImages;
        private int nextVariantId = -1;
        private int nextImageId = -1;
        private int selectedVariantId = -1;  // ID de la variante actuellement sélectionnée
        #endregion

        #region Constructeur
        public NewProductControl()
        {
            InitializeComponent();

            // Initialiser la connexion à la base de données
            this.connection = DB.SeConnecter();

            // Initialiser les collections observables
            variants = new ObservableCollection<ProductVariantViewModel>();
            productImages = new ObservableCollection<ProductImageViewModel>();

            // Lier les collections aux contrôles UI
            variantList.ItemsSource = variants;

            // Initialiser les valeurs par défaut pour les ComboBox
            cmbProductType.SelectedIndex = 0;  // Lifestyle par défaut
            cmbGender.SelectedIndex = 0;       // Homme par défaut

            // Initialiser le filtre de variantes
            CreateFilterButtons();

            // Masquer le panneau de variantes par défaut
            variantsPanel.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Gestion de l'interface
        private void CreateFilterButtons()
        {
            // Vider le panneau existant
            filterButtonsPanel.Children.Clear();

            // Ajouter le bouton "Toutes les variantes"
            ToggleButton allButton = new ToggleButton
            {
                Content = "Toutes les variantes",
                Style = FindResource("FilterButton") as Style,
                Tag = "-1",
                IsChecked = true
            };
            allButton.Click += BtnFilter_Click;
            filterButtonsPanel.Children.Add(allButton);

            // Ajouter un bouton pour chaque variante
            foreach (var variant in variants)
            {
                ToggleButton variantButton = new ToggleButton
                {
                    Content = variant.Color,
                    Style = FindResource("FilterButton") as Style,
                    Tag = variant.Id.ToString(),
                    Margin = new Thickness(3, 0, 3, 0)
                };

                variantButton.Click += BtnFilter_Click;
                filterButtonsPanel.Children.Add(variantButton);
            }
        }

        private void RefreshVariantFilter()
        {
            CreateFilterButtons();

            // Réinitialiser à 'Toutes les variantes'
            selectedVariantId = -1;

            // Mettre à jour l'affichage des images
            UpdateImagesDisplay();
        }

        private void UpdateImageMainStatus()
        {
            // Mettre à jour l'indicateur d'image principale dans l'interface
            foreach (var child in imagesPanel.Children)
            {
                if (child is Border imageCard && imageCard.Tag != null && imageCard.Child is Grid cardGrid)
                {
                    int imageId = Convert.ToInt32(imageCard.Tag);
                    var image = productImages.FirstOrDefault(i => i.Id == imageId);

                    if (image != null)
                    {
                        // Supprimer le badge existant s'il y en a un
                        UIElement badgeToRemove = null;
                        foreach (UIElement element in cardGrid.Children)
                        {
                            if (element is Border border &&
                                border.Child is TextBlock textBlock &&
                                textBlock.Text == "Principale")
                            {
                                badgeToRemove = element;
                                break;
                            }
                        }

                        if (badgeToRemove != null)
                        {
                            cardGrid.Children.Remove(badgeToRemove);
                        }

                        // Ajouter un nouveau badge si l'image est principale
                        if (image.IsMain)
                        {
                            Border mainBadge = CreateMainImageBadge();
                            Grid.SetRow(mainBadge, 0);
                            cardGrid.Children.Add(mainBadge);
                        }
                    }
                }
            }
        }

        private Border CreateMainImageBadge()
        {
            Border mainBadge = new Border
            {
                Style = (Style)FindResource("MainImageBadge")
            };

            TextBlock badgeText = new TextBlock
            {
                Text = "Principale",
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 10,
                FontWeight = FontWeights.Bold
            };

            mainBadge.Child = badgeText;
            return mainBadge;
        }

        private void ShowStatusMessage(string message, bool isSuccess)
        {
            // Définir les couleurs appropriées en fonction du statut
            StatusMessage.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(isSuccess ? "#f1f8e9" : "#ffebee"));

            StatusMessage.BorderBrush = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(isSuccess ? "#c5e1a5" : "#ffcdd2"));

            StatusMessageText.Text = message;
            StatusMessageText.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(isSuccess ? "#558b2f" : "#c62828"));

            StatusMessage.Visibility = Visibility.Visible;
        }
        #endregion

        #region Validation de saisie
        private void TxtPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Accepter uniquement les chiffres et un seul point ou virgule
            Regex regex = new Regex(@"^[0-9]*(\,|\.)?[0-9]*$");
            e.Handled = !regex.IsMatch(((TextBox)sender).Text + e.Text);
        }

        private bool ValidateProduct()
        {
            // Vérifier le nom du produit
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Veuillez saisir un nom pour le produit.",
                    "Champ obligatoire", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtProductName.Focus();
                return false;
            }

            // Vérifier le prix
            if (!decimal.TryParse(txtPrice.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal price) || price <= 0)
            {
                MessageBox.Show("Veuillez saisir un prix valide (nombre positif).",
                    "Prix invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return false;
            }

            // Vérifier si le produit a suffisamment d'images
            if (productImages.Count < 2)
            {
                MessageBox.Show("Veuillez ajouter au moins 2 images pour le produit.",
                    "Images insuffisantes", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtImageError.Visibility = Visibility.Visible;
                return false;
            }

            // Vérifier si chaque variante a une image principale
            bool hasVariants = chkHasVariants.IsChecked ?? false;
            if (hasVariants && variants.Count > 0)
            {
                foreach (var variant in variants)
                {
                    // Vérifier si la variante a une image principale
                    bool hasMainImage = productImages.Any(img => img.VariantId == variant.Id && img.IsMain);
                    if (!hasMainImage)
                    {
                        MessageBox.Show($"La variante '{variant.Color}' n'a pas d'image principale. Veuillez définir une image principale pour chaque variante.",
                            "Image principale manquante", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
            }

            return true;
        }
        #endregion

        #region Gestion des variantes
        private void ChkHasVariants_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool hasVariants = chkHasVariants.IsChecked ?? false;
            variantsPanel.Visibility = hasVariants ? Visibility.Visible : Visibility.Collapsed;

            if (!hasVariants)
            {
                // Si on désactive les variantes, on réinitialise le filtre
                RefreshVariantFilter();

                // Si on a des images, on les assigne toutes à la variante par défaut (id 0)
                if (productImages.Count > 0)
                {
                    foreach (var img in productImages)
                    {
                        img.VariantId = 0;
                    }

                    // S'assurer qu'il y a une image principale
                    if (!productImages.Any(img => img.IsMain))
                    {
                        productImages[0].IsMain = true;
                    }

                    // Mettre à jour l'affichage
                    UpdateImagesDisplay();
                }
            }
        }

        private void BtnAddVariant_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputColorDialog();

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.SelectedColor))
            {
                // Vérifier si cette couleur existe déjà
                if (variants.Any(v => v.Color.Equals(dialog.SelectedColor, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"La variante de couleur '{dialog.SelectedColor}' existe déjà.",
                        "Variante existante", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Créer une nouvelle variante
                var variant = new ProductVariantViewModel
                {
                    Id = nextVariantId--,
                    Color = dialog.SelectedColor,
                    IsMain = variants.Count == 0 // Première variante est principale par défaut
                };

                // Ajouter à la collection
                variants.Add(variant);

                // Mettre à jour le filtre
                RefreshVariantFilter();
            }
        }

        private void BtnRemoveVariant_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int variantId = Convert.ToInt32(btn.Tag);
                var variantToRemove = variants.FirstOrDefault(v => v.Id == variantId);

                if (variantToRemove == null) return;

                MessageBoxResult result = MessageBox.Show(
                    $"Êtes-vous sûr de vouloir supprimer la variante '{variantToRemove.Color}'?\n\n" +
                    "Cette action supprimera également toutes les images associées.",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;

                // Supprimer les images associées
                var imagesToRemove = productImages.Where(img => img.VariantId == variantId).ToList();
                foreach (var img in imagesToRemove)
                {
                    productImages.Remove(img);
                }

                // Supprimer la variante
                bool wasMain = variantToRemove.IsMain;
                variants.Remove(variantToRemove);

                // Si c'était la variante principale, définir une nouvelle principale
                if (wasMain && variants.Count > 0)
                {
                    variants[0].IsMain = true;
                }

                // Mettre à jour l'interface
                RefreshVariantFilter();
                UpdateImagesDisplay();
            }
        }

        private void VariantMainRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag != null)
            {
                int selectedVariantId = Convert.ToInt32(rb.Tag);

                // Mettre à jour le statut de toutes les variantes
                foreach (var variant in variants)
                {
                    variant.IsMain = (variant.Id == selectedVariantId);
                }
            }
        }
        #endregion

        #region Gestion des images
        private void BtnAddImage_Click(object sender, RoutedEventArgs e)
        {
            // Vérifier si le produit a des variantes
            bool hasVariants = chkHasVariants.IsChecked ?? false;
            int variantId = 0;

            if (hasVariants)
            {
                // Si le produit a des variantes mais qu'aucune n'a été créée
                if (variants.Count == 0)
                {
                    MessageBox.Show("Veuillez d'abord ajouter au moins une variante de produit.",
                        "Aucune variante", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Vérifier si une variante spécifique est sélectionnée dans le filtre
                if (selectedVariantId > -1)
                {
                    variantId = selectedVariantId;
                }
                else
                {
                    // Afficher une boîte de dialogue avec les variantes disponibles
                    SelectVariantDialog variantDialog = new SelectVariantDialog(variants.ToList());

                    if (variantDialog.ShowDialog() == true && variantDialog.SelectedVariantId != 0)
                    {
                        variantId = variantDialog.SelectedVariantId;
                    }
                    else
                    {
                        return; // Annulation par l'utilisateur
                    }
                }
            }

            // Afficher la boîte de dialogue pour entrer l'URL de l'image
            EditImageDialog dialog = new EditImageDialog("");
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ImageUrl))
            {
                // Créer une nouvelle image
                var newImage = new ProductImageViewModel
                {
                    Id = nextImageId--,
                    VariantId = variantId,
                    ImageUrl = dialog.ImageUrl,
                    IsMain = !productImages.Any(img => img.VariantId == variantId) // Première image principale par défaut
                };

                // Ajouter à la collection
                productImages.Add(newImage);

                // Masquer le message d'erreur si suffisamment d'images
                if (productImages.Count >= 2)
                {
                    txtImageError.Visibility = Visibility.Collapsed;
                }

                // Mettre à jour l'affichage
                UpdateImagesDisplay();
            }
        }

        private void BtnSetMainImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int imageId = Convert.ToInt32(btn.Tag);
                var image = productImages.FirstOrDefault(img => img.Id == imageId);

                if (image == null) return;

                // Mettre à jour le statut dans la collection
                int variantId = image.VariantId;
                foreach (var img in productImages)
                {
                    if (img.VariantId == variantId)
                    {
                        img.IsMain = (img.Id == imageId);
                    }
                }

                // Mettre à jour les badges dans l'UI
                UpdateImageMainStatus();

                MessageBox.Show("Image définie comme principale pour cette variante.",
                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnRemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int imageId = Convert.ToInt32(btn.Tag);
                var image = productImages.FirstOrDefault(img => img.Id == imageId);

                if (image == null) return;

                MessageBoxResult result = MessageBox.Show(
                    "Êtes-vous sûr de vouloir supprimer cette image?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    bool wasMain = image.IsMain;
                    int variantId = image.VariantId;

                    // Supprimer l'image de la collection
                    productImages.Remove(image);

                    // Si c'était l'image principale, définir une nouvelle principale
                    if (wasMain)
                    {
                        var nextImage = productImages.FirstOrDefault(img => img.VariantId == variantId);
                        if (nextImage != null)
                        {
                            nextImage.IsMain = true;
                        }
                    }

                    // Afficher le message d'erreur si nécessaire
                    if (productImages.Count < 2)
                    {
                        txtImageError.Visibility = Visibility.Visible;
                    }

                    // Mettre à jour l'affichage
                    UpdateImagesDisplay();
                }
            }
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton clickedButton)
            {
                // Désactiver tous les autres boutons
                foreach (var child in filterButtonsPanel.Children)
                {
                    if (child is ToggleButton btn && btn != clickedButton)
                    {
                        btn.IsChecked = false;
                    }
                }

                // S'assurer que le bouton cliqué reste coché
                clickedButton.IsChecked = true;

                // Récupérer l'ID de la variante à partir du Tag du bouton
                if (int.TryParse(clickedButton.Tag.ToString(), out int variantId))
                {
                    // Mettre à jour la variante sélectionnée
                    selectedVariantId = variantId;

                    // Filtrer les images (variantId = -1 signifie "Toutes les variantes")
                    UpdateImagesDisplay();
                }
            }
        }

        private void UpdateImagesDisplay()
        {
            // Vider le panneau d'images
            imagesPanel.Children.Clear();

            // Filtrer les images selon la variante sélectionnée
            IEnumerable<ProductImageViewModel> filteredImages = productImages;

            if (selectedVariantId > -1)
            {
                // Filtrer pour une variante spécifique
                filteredImages = productImages.Where(img => img.VariantId == selectedVariantId);
            }

            // Ajouter chaque image au panneau
            foreach (var image in filteredImages)
            {
                AddImageToPanel(image);
            }
        }

        private void AddImageToPanel(ProductImageViewModel image)
        {
            // Créer une nouvelle carte d'image
            Border imageCard = new Border
            {
                Style = (Style)FindResource("ImageCard"),
                Tag = image.Id
            };

            // Créer un Grid pour le contenu
            Grid cardGrid = new Grid();
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            try
            {
                // Ajouter l'image
                Image imageElement = new Image
                {
                    Source = new BitmapImage(new Uri(image.ImageUrl, UriKind.Absolute)),
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(5)
                };
                Grid.SetRow(imageElement, 0);
                cardGrid.Children.Add(imageElement);
            }
            catch
            {
                // En cas d'erreur de chargement
                TextBlock errorText = new TextBlock
                {
                    Text = "Erreur de chargement",
                    Foreground = new SolidColorBrush(Colors.Red),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(errorText, 0);
                cardGrid.Children.Add(errorText);
            }

            // Obtenir la variante si applicable
            if (image.VariantId != 0)
            {
                var variant = variants.FirstOrDefault(v => v.Id == image.VariantId);
                if (variant != null)
                {
                    TextBlock variantText = new TextBlock
                    {
                        Text = $"Variante: {variant.Color}",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(5, 0, 5, 5),
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555"))
                    };
                    Grid.SetRow(variantText, 1);
                    cardGrid.Children.Add(variantText);
                }
            }

            // Ajouter les boutons d'action
            StackPanel buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };

            Button btnSetMain = new Button
            {
                Content = "Définir comme principale",
                Style = (Style)FindResource("SecondaryButton"),
                Margin = new Thickness(0, 0, 5, 0),
                Height = 30,
                Padding = new Thickness(5, 0, 5, 0),
                Tag = image.Id
            };
            btnSetMain.Click += BtnSetMainImage_Click;

            Button btnRemove = new Button
            {
                Content = "Supprimer",
                Style = (Style)FindResource("SecondaryButton"),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffebee")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c62828")),
                Height = 30,
                Padding = new Thickness(5, 0, 5, 0),
                Tag = image.Id
            };
            btnRemove.Click += BtnRemoveImage_Click;

            buttonsPanel.Children.Add(btnSetMain);
            buttonsPanel.Children.Add(btnRemove);
            Grid.SetRow(buttonsPanel, 2);
            cardGrid.Children.Add(buttonsPanel);

            // Si c'est l'image principale, ajouter le badge
            if (image.IsMain)
            {
                Border mainBadge = CreateMainImageBadge();
                Grid.SetRow(mainBadge, 0);
                cardGrid.Children.Add(mainBadge);
            }

            // Ajouter le grid à la carte
            imageCard.Child = cardGrid;

            // Ajouter la carte au panneau
            imagesPanel.Children.Add(imageCard);
        }
        #endregion

        #region Sauvegarde et réinitialisation
        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Valider les données
                if (!ValidateProduct())
                {
                    return;
                }

                // Ouvrir la connexion si nécessaire
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                // Utiliser une transaction pour garantir l'intégrité des données
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Insérer le produit
                        string productName = txtProductName.Text.Trim();
                        string description = txtDescription.Text.Trim();
                        string typeProduit = ((ComboBoxItem)cmbProductType.SelectedItem).Content.ToString();
                        string genre = ((ComboBoxItem)cmbGender.SelectedItem).Content.ToString();

                        decimal prix = decimal.Parse(txtPrice.Text.Replace(',', '.'),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture);

                        string productQuery = @"
                            INSERT INTO produits (nom, description, prix, type_produit, genre)
                            VALUES (@nom, @description, @prix, @typeProduit, @genre);
                            SELECT LAST_INSERT_ID();";

                        int productId;
                        using (MySqlCommand cmd = new MySqlCommand(productQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@nom", productName);
                            cmd.Parameters.AddWithValue("@description", description);
                            cmd.Parameters.AddWithValue("@prix", prix);
                            cmd.Parameters.AddWithValue("@typeProduit", typeProduit);
                            cmd.Parameters.AddWithValue("@genre", genre);

                            productId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // 2. Insérer les variantes et les images
                        bool hasVariants = chkHasVariants.IsChecked ?? false;
                        Dictionary<int, int> tempToRealVariantIds = new Dictionary<int, int>();

                        if (hasVariants && variants.Count > 0)
                        {
                            // Insérer chaque variante
                            foreach (var variant in variants)
                            {
                                string variantQuery = @"
                                    INSERT INTO produit_variantes (produit_id, couleur, is_main)
                                    VALUES (@produitId, @couleur, @isMain);
                                    SELECT LAST_INSERT_ID();";

                                using (MySqlCommand cmd = new MySqlCommand(variantQuery, connection, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@produitId", productId);
                                    cmd.Parameters.AddWithValue("@couleur", variant.Color);
                                    cmd.Parameters.AddWithValue("@isMain", variant.IsMain);

                                    int realVariantId = Convert.ToInt32(cmd.ExecuteScalar());
                                    tempToRealVariantIds[variant.Id] = realVariantId;

                                    // Insérer des tailles par défaut
                                    InsertDefaultSizes(realVariantId, transaction);
                                }
                            }
                        }
                        else
                        {
                            // Créer une variante standard pour les produits sans variantes
                            string variantQuery = @"
                                INSERT INTO produit_variantes (produit_id, couleur, is_main)
                                VALUES (@produitId, 'Standard', 1);
                                SELECT LAST_INSERT_ID();";

                            using (MySqlCommand cmd = new MySqlCommand(variantQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@produitId", productId);

                                int standardVariantId = Convert.ToInt32(cmd.ExecuteScalar());
                                tempToRealVariantIds[0] = standardVariantId;

                                // Insérer des tailles par défaut
                                InsertDefaultSizes(standardVariantId, transaction);
                            }
                        }

                        // 3. Insérer les images
                        foreach (var image in productImages)
                        {
                            // Récupérer l'ID réel de la variante
                            int realVariantId;

                            // Gérer le cas où la variante n'existe pas dans le dictionnaire
                            if (!tempToRealVariantIds.TryGetValue(image.VariantId, out realVariantId))
                            {
                                // Utiliser la première variante disponible
                                if (tempToRealVariantIds.Count > 0)
                                {
                                    realVariantId = tempToRealVariantIds.Values.First();
                                }
                                else
                                {
                                    // Cas improbable: aucune variante n'a été créée
                                    continue;
                                }
                            }

                            string imageQuery = @"
                                INSERT INTO produit_images (variante_id, url_image, is_main)
                                VALUES (@varianteId, @urlImage, @isMain);";

                            using (MySqlCommand cmd = new MySqlCommand(imageQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@varianteId", realVariantId);
                                cmd.Parameters.AddWithValue("@urlImage", image.ImageUrl);
                                cmd.Parameters.AddWithValue("@isMain", image.IsMain);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Valider la transaction
                        transaction.Commit();

                        // Afficher le message de succès
                        ShowStatusMessage("Le produit a été ajouté avec succès!", true);

                        // Demander si l'utilisateur souhaite ajouter des tailles
                        AskToAddSizes();

                        // Réinitialiser le formulaire
                        ResetForm();
                    }
                    catch (Exception ex)
                    {
                        // Annuler la transaction en cas d'erreur
                        transaction.Rollback();
                        ShowStatusMessage($"Erreur lors de l'ajout du produit: {ex.Message}", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erreur: {ex.Message}", false);
            }
        }

        private void InsertDefaultSizes(int variantId, MySqlTransaction transaction)
        {
            // Tailles standards pour les chaussures
            int[] defaultSizes = { 39, 40, 41, 42, 43, 44, 45 };

            foreach (int size in defaultSizes)
            {
                string query = @"
                    INSERT INTO produit_tailles (variante_id, taille, stock)
                    VALUES (@varianteId, @taille, @stock);";

                using (MySqlCommand cmd = new MySqlCommand(query, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@varianteId", variantId);
                    cmd.Parameters.AddWithValue("@taille", size);
                    cmd.Parameters.AddWithValue("@stock", 0); // Stock à 0 par défaut

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void AskToAddSizes()
        {
            var result = MessageBox.Show(
                "Voulez-vous ajouter des tailles et du stock maintenant?",
                "Gestion des tailles",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show(
                    "Le produit a été créé avec des tailles par défaut (39-45) avec un stock à 0. " +
                    "Vous pouvez maintenant modifier les stocks depuis la section 'Gestion des stocks'.",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            // Confirmation avant réinitialisation
            MessageBoxResult result = MessageBox.Show(
                "Êtes-vous sûr de vouloir réinitialiser le formulaire ? Toutes les données saisies seront perdues.",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                ResetForm();
                StatusMessage.Visibility = Visibility.Collapsed;
            }
        }

        private void ResetForm()
        {
            // Réinitialiser les champs de formulaire
            txtProductName.Text = "";
            txtDescription.Text = "";
            txtPrice.Text = "";
            cmbProductType.SelectedIndex = 0;
            cmbGender.SelectedIndex = 0;
            chkHasVariants.IsChecked = false;

            // Vider les collections
            variants.Clear();
            productImages.Clear();

            // Réinitialiser les IDs temporaires
            nextVariantId = -1;
            nextImageId = -1;
            selectedVariantId = -1;

            // Réinitialiser l'interface
            imagesPanel.Children.Clear();
            txtImageError.Visibility = Visibility.Collapsed;
            variantsPanel.Visibility = Visibility.Collapsed;

            // Réinitialiser le filtre de variantes
            CreateFilterButtons();
        }

        private void BtnReuseImage_Click(object sender, RoutedEventArgs e)
        {
            // Cette fonctionnalité pourrait être implémentée pour permettre
            // de réutiliser des images entre différentes variantes
            if (productImages.Count == 0)
            {
                MessageBox.Show("Aucune image disponible à réutiliser. Veuillez d'abord ajouter une image.",
                    "Aucune image", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Vérifier si le produit a des variantes
            bool hasVariants = chkHasVariants.IsChecked ?? false;
            if (!hasVariants)
            {
                MessageBox.Show("La réutilisation d'images n'est utile que pour les produits avec variantes.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBox.Show("Fonctionnalité de réutilisation d'images disponible prochainement.",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion
    }
}