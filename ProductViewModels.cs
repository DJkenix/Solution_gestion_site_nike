using System.ComponentModel;

namespace PanelNikeStore
{
    /// <summary>
    /// Classe ViewModel pour les variantes de produit
    /// </summary>
    public class ProductVariantViewModel : INotifyPropertyChanged
    {
        private int id;
        private string color;
        private bool isMain;

        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }

        public string Color
        {
            get { return color; }
            set
            {
                color = value;
                OnPropertyChanged("Color");
            }
        }

        public bool IsMain
        {
            get { return isMain; }
            set
            {
                isMain = value;
                OnPropertyChanged("IsMain");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// Classe ViewModel pour les images de produit
    /// </summary>
    public class ProductImageViewModel
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsMain { get; set; }
    }

    /// <summary>
    /// Classe pour les éléments de filtre de variante
    /// </summary>
    public class FilterVariant
    {
        public int? Id { get; set; }  // null pour "Toutes les variantes"
        public string Nom { get; set; }

        public override string ToString() => Nom;
    }
}