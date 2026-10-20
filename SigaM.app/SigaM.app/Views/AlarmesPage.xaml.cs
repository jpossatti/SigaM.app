using SigaM.app.ViewModels;

namespace SigaM.app.Views;

public partial class AlarmesPage : ContentPage
{
    public AlarmesPage()
    {
        InitializeComponent();

        // Vincula a tela ao ViewModel programaticamente
        BindingContext = new AlarmesViewModel();
    }
}