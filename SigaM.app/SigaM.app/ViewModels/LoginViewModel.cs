using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Newtonsoft.Json;
using SigaM.app.Models;
using System.Text;

namespace SigaM.app.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly string _apiUrl = "http://localhost:3000/modules/sigam/api_login.php";

        public string Prontuario { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;

        private string _mensagemErro = string.Empty;
        public string MensagemErro
        {
            get => _mensagemErro;
            set { _mensagemErro = value; OnPropertyChanged(); OnPropertyChanged(nameof(TemErro)); }
        }

        public bool TemErro => !string.IsNullOrEmpty(MensagemErro);

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand EntrarCommand { get; }

        public LoginViewModel()
        {
            EntrarCommand = new Command(async () => await RealizarLogin());
        }

        private async Task RealizarLogin()
        {
            if (string.IsNullOrWhiteSpace(Prontuario) || string.IsNullOrWhiteSpace(Senha))
            {
                MensagemErro = "Preencha todos os campos.";
                return;
            }

            try
            {
                IsBusy = true;
                MensagemErro = "";

                var dados = new { prontuario = Prontuario, senha = Senha };
                var content = new StringContent(JsonConvert.SerializeObject(dados), Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                var response = await client.PostAsync(_apiUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic? json = JsonConvert.DeserializeObject(responseString);

                if (json != null && json.status == "sucesso")
                {
                    var usuario = JsonConvert.DeserializeObject<Usuario>(json.usuario.ToString());

                    // SALVA O USUÁRIO LOGADO USANDO O MÉTODO DA GLOBALS
                    Globals.DefinirUsuario(
                        usuario?.Id ?? 0,
                        usuario?.Prontuario ?? "",
                        usuario?.Nome ?? ""
                    );

                    // Vai para o Dashboard (AppShell)
                    Application.Current!.Windows[0].Page = new AppShell();
                }
                else
                {
                    MensagemErro = json?.mensagem ?? "Erro desconhecido no login.";
                }
            }
            catch (Exception ex)
            {
                MensagemErro = "Falha na conexão com o servidor: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}