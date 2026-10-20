using SigaM.app.ViewModels;

namespace SigaM.app;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new DashboardViewModel();
    }
}