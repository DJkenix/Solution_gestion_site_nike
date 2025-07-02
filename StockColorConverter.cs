using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PanelNikeStore
{
    public class StockColorConverter : IValueConverter
    {
        // Seuils de stock avec des valeurs ajustées
        private const int LOW_STOCK_THRESHOLD = 10;
        private const int CRITICAL_STOCK_THRESHOLD = 3;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Vérifier si la valeur est null
            if (value == null)
                return new SolidColorBrush(Colors.Gray); // Couleur par défaut

            int stockLevel;

            // Essayer de convertir la valeur en entier
            if (value is int)
            {
                stockLevel = (int)value;
            }
            else if (!int.TryParse(value.ToString(), out stockLevel))
            {
                return new SolidColorBrush(Colors.Gray); // Couleur par défaut si conversion impossible
            }

            // Stock critique - Rouge
            if (stockLevel <= CRITICAL_STOCK_THRESHOLD)
            {
                return new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Rouge Material Design
            }
            // Stock faible - Orange
            else if (stockLevel <= LOW_STOCK_THRESHOLD)
            {
                return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange Material Design
            }
            // Stock suffisant - Vert
            else
            {
                return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Vert Material Design
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}