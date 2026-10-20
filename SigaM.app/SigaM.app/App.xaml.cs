using SigaM.app.Views;

namespace SigaM.app
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Inicia o app na tela de Login
            return new Window(new NavigationPage(new LoginPage()));
        }
    }
}