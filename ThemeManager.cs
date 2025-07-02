using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

namespace PanelNikeStore
{
    public enum ThemeType
    {
        Light,
        Dark
    }

    public class ThemeManager
    {
        private static ThemeManager _instance;
        private ThemeType _currentTheme = ThemeType.Light;

        // Événement pour notifier les abonnés lorsque le thème change
        public event EventHandler ThemeChanged;

        // Pattern Singleton
        public static ThemeManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ThemeManager();
                return _instance;
            }
        }

        private ThemeManager()
        {
            // Initialiser avec le thème par défaut (Clair)
        }

        public ThemeType CurrentTheme
        {
            get { return _currentTheme; }
        }

        // Couleurs du thème sombre
        public static class DarkTheme
        {
            // Couleurs principales
            public static readonly Color PrimaryBackground = (Color)ColorConverter.ConvertFromString("#262624");
            public static readonly Color SecondaryBackground = (Color)ColorConverter.ConvertFromString("#323230");
            public static readonly Color TertiaryBackground = (Color)ColorConverter.ConvertFromString("#3E3E3C");

            // Couleurs sidebar
            public static readonly Color SidebarBackground = (Color)ColorConverter.ConvertFromString("#212121");
            public static readonly Color SidebarText = (Color)ColorConverter.ConvertFromString("#E0E0E0");
            public static readonly Color SidebarSubMenuText = (Color)ColorConverter.ConvertFromString("#ADADAD");

            // Couleurs de texte
            public static readonly Color PrimaryText = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            public static readonly Color SecondaryText = (Color)ColorConverter.ConvertFromString("#CCCCCC");
            public static readonly Color DisabledText = (Color)ColorConverter.ConvertFromString("#888888");

            // Couleurs d'accent
            public static readonly Color Accent = (Color)ColorConverter.ConvertFromString("#2196F3"); // Bleu

            // Couleurs de bordure
            public static readonly Color Border = (Color)ColorConverter.ConvertFromString("#444444");

            // Couleurs de statut (garder la cohérence avec le mode clair pour la clarté)
            public static readonly Color Success = (Color)ColorConverter.ConvertFromString("#4CAF50");
            public static readonly Color Warning = (Color)ColorConverter.ConvertFromString("#FF9800");
            public static readonly Color Error = (Color)ColorConverter.ConvertFromString("#F44336");
            public static readonly Color Info = (Color)ColorConverter.ConvertFromString("#2196F3");

            // Menu hover
            public static readonly Color MenuHover = (Color)ColorConverter.ConvertFromString("#404040");
            public static readonly Color MenuPressed = (Color)ColorConverter.ConvertFromString("#505050");
        }

        // Couleurs du thème clair (couleurs d'origine de l'application)
        public static class LightTheme
        {
            // Couleurs principales
            public static readonly Color PrimaryBackground = (Color)ColorConverter.ConvertFromString("#F8F9FA");
            public static readonly Color SecondaryBackground = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            public static readonly Color TertiaryBackground = (Color)ColorConverter.ConvertFromString("#F5F5F5");

            // Couleurs sidebar
            public static readonly Color SidebarBackground = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            public static readonly Color SidebarText = (Color)ColorConverter.ConvertFromString("#495057");
            public static readonly Color SidebarSubMenuText = (Color)ColorConverter.ConvertFromString("#6C757D");

            // Couleurs de texte
            public static readonly Color PrimaryText = (Color)ColorConverter.ConvertFromString("#212529");
            public static readonly Color SecondaryText = (Color)ColorConverter.ConvertFromString("#6C757D");
            public static readonly Color DisabledText = (Color)ColorConverter.ConvertFromString("#ADB5BD");

            // Couleurs d'accent
            public static readonly Color Accent = (Color)ColorConverter.ConvertFromString("#007BFF");

            // Couleurs de bordure
            public static readonly Color Border = (Color)ColorConverter.ConvertFromString("#E9ECEF");

            // Couleurs de statut
            public static readonly Color Success = (Color)ColorConverter.ConvertFromString("#28A745");
            public static readonly Color Warning = (Color)ColorConverter.ConvertFromString("#FFC107");
            public static readonly Color Error = (Color)ColorConverter.ConvertFromString("#DC3545");
            public static readonly Color Info = (Color)ColorConverter.ConvertFromString("#17A2B8");

            // Menu hover
            public static readonly Color MenuHover = (Color)ColorConverter.ConvertFromString("#F1F3F5");
            public static readonly Color MenuPressed = (Color)ColorConverter.ConvertFromString("#E9ECEF");
        }

        // Basculer entre thème clair et sombre
        public void ToggleTheme()
        {
            _currentTheme = (_currentTheme == ThemeType.Light) ? ThemeType.Dark : ThemeType.Light;
            ApplyTheme();

            // Notifier les abonnés que le thème a changé
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        // Appliquer le thème actuel à l'application
        public void ApplyTheme()
        {
            ResourceDictionary resources = Application.Current.Resources;

            if (_currentTheme == ThemeType.Dark)
            {
                // Appliquer le thème sombre

                // Couleurs principales
                resources["WindowBackground"] = new SolidColorBrush(DarkTheme.PrimaryBackground);
                resources["ContentBackground"] = new SolidColorBrush(DarkTheme.SecondaryBackground);
                resources["ControlBackground"] = new SolidColorBrush(DarkTheme.TertiaryBackground);

                // Sidebar
                resources["SidebarBackground"] = new SolidColorBrush(DarkTheme.SidebarBackground);
                resources["SidebarText"] = new SolidColorBrush(DarkTheme.SidebarText);
                resources["SidebarSubMenuText"] = new SolidColorBrush(DarkTheme.SidebarSubMenuText);

                // Texte
                resources["PrimaryText"] = new SolidColorBrush(DarkTheme.PrimaryText);
                resources["SecondaryText"] = new SolidColorBrush(DarkTheme.SecondaryText);
                resources["DisabledText"] = new SolidColorBrush(DarkTheme.DisabledText);

                // Bordures
                resources["BorderBrush"] = new SolidColorBrush(DarkTheme.Border);

                // Menu hover
                resources["MenuHover"] = new SolidColorBrush(DarkTheme.MenuHover);
                resources["MenuPressed"] = new SolidColorBrush(DarkTheme.MenuPressed);

                // Accent et statuts (conserver les mêmes pour cohérence visuelle)
                resources["AccentBrush"] = new SolidColorBrush(DarkTheme.Accent);
                resources["SuccessBrush"] = new SolidColorBrush(DarkTheme.Success);
                resources["WarningBrush"] = new SolidColorBrush(DarkTheme.Warning);
                resources["ErrorBrush"] = new SolidColorBrush(DarkTheme.Error);
                resources["InfoBrush"] = new SolidColorBrush(DarkTheme.Info);
            }
            else
            {
                // Appliquer le thème clair (par défaut)

                // Couleurs principales
                resources["WindowBackground"] = new SolidColorBrush(LightTheme.PrimaryBackground);
                resources["ContentBackground"] = new SolidColorBrush(LightTheme.SecondaryBackground);
                resources["ControlBackground"] = new SolidColorBrush(LightTheme.TertiaryBackground);

                // Sidebar
                resources["SidebarBackground"] = new SolidColorBrush(LightTheme.SidebarBackground);
                resources["SidebarText"] = new SolidColorBrush(LightTheme.SidebarText);
                resources["SidebarSubMenuText"] = new SolidColorBrush(LightTheme.SidebarSubMenuText);

                // Texte
                resources["PrimaryText"] = new SolidColorBrush(LightTheme.PrimaryText);
                resources["SecondaryText"] = new SolidColorBrush(LightTheme.SecondaryText);
                resources["DisabledText"] = new SolidColorBrush(LightTheme.DisabledText);

                // Bordures
                resources["BorderBrush"] = new SolidColorBrush(LightTheme.Border);

                // Menu hover
                resources["MenuHover"] = new SolidColorBrush(LightTheme.MenuHover);
                resources["MenuPressed"] = new SolidColorBrush(LightTheme.MenuPressed);

                // Accent et statuts
                resources["AccentBrush"] = new SolidColorBrush(LightTheme.Accent);
                resources["SuccessBrush"] = new SolidColorBrush(LightTheme.Success);
                resources["WarningBrush"] = new SolidColorBrush(LightTheme.Warning);
                resources["ErrorBrush"] = new SolidColorBrush(LightTheme.Error);
                resources["InfoBrush"] = new SolidColorBrush(LightTheme.Info);
            }
        }
    }
}