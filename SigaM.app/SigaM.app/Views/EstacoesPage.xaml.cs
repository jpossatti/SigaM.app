using SigaM.app.ViewModels;

namespace SigaM.app.Views;

public partial class EstacoesPage : ContentPage
{
    public EstacoesPage()
    {
        InitializeComponent();
        BindingContext = new EstacoesViewModel();
    }

    // Adicione isso para que a lista atualize sozinha quando o usuário entrar na tela
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is EstacoesViewModel viewModel)
        {
            await viewModel.CarregarEstacoes();
        }
    }
}