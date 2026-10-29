using SigaM.app.ViewModels;

namespace SigaM.app.Views
{
    public partial class AlarmesPage : ContentPage
    {
        public AlarmesPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Carrega os dados assim que a tela é exibida
            if (BindingContext is AlarmesViewModel viewModel)
            {
                await viewModel.CarregarDados();
            }
        }
    }
}