using SigaM.app.ViewModels;

namespace SigaM.app;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        // Define o BindingContext AQUI (e não mais no XAML)
        BindingContext = new DashboardViewModel();
    }
}