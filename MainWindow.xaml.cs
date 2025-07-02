using System.Windows;


namespace PanelNikeStore
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Redirection_To_Login_Click(object sender, RoutedEventArgs e)
        {
            LoginPage loginpage = new LoginPage();
            loginpage.Show();
            this.Close();
        }

    }
}
