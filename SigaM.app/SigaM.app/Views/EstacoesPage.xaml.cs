using SigaM.app.ViewModels;

namespace SigaM.app.Views;

public partial class EstacoesPage : ContentPage
{
    public EstacoesPage()
    {
        InitializeComponent();

        // Vincula a ViewModel programaticamente (Evita o erro XLS0414)
        BindingContext = new EstacoesViewModel();
    }
}