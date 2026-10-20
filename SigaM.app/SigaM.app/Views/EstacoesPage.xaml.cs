using SigaM.app.ViewModels;

namespace SigaM.app.Views;

public partial class EstacoesPage : ContentPage
{
    public EstacoesPage()
    {
        InitializeComponent();

        // Vincula a tela ao ViewModel programaticamente
        BindingContext = new EstacoesViewModel();
    }
}