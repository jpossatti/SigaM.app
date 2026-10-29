using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Newtonsoft.Json;
using SigaM.app.Models;
using System.Text;
using System.Net.Http; // Necessário para o HttpClient
using System.Net;      // Necessário para HttpStatusCode

namespace SigaM.app.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        // Instância estática do HttpClient para evitar esgotamento de sockets
        private static readonly HttpClient _client = new HttpClient();

        private readonly string _apiUrl = "http://193.168.1.71:3000/modules/sigam/api_login.php";

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
            EntrarCommand = new Command(async () => await RealizarLogin(), () => !IsBusy);
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
                var json = JsonConvert.SerializeObject(dados);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Usa a instância estática do _client
                var response = await _client.PostAsync(_apiUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic? jsonResponse = JsonConvert.DeserializeObject(responseString);

                // Verifica se o PHP retornou status 200 (OK) E se o JSON diz "sucesso"
                if (response.IsSuccessStatusCode && jsonResponse != null && jsonResponse.status == "sucesso")
                {
                    var usuario = JsonConvert.DeserializeObject<Usuario>(jsonResponse.usuario.ToString());

                    // SALVA O USUÁRIO LOGADO USANDO O MÉTODO DA GLOBALS
                    Globals.DefinirUsuario(
                        usuario?.Id ?? 0,
                        usuario?.Prontuario ?? "",
                        usuario?.Nome ?? ""
                    );

                    // Vai para o Dashboard (AppShell)
                    // Nota: Em versões mais recentes do MAUI, é recomendado usar Shell.Current.GoToAsync()
                    Application.Current!.Windows[0].Page = new AppShell();
                }
                else
                {
                    // Se o PHP retornou 401 ou 400, a mensagem estará no JSON
                    MensagemErro = jsonResponse?.mensagem ?? "Prontuário ou senha inválidos.";
                }
            }
            catch (HttpRequestException ex)
            {
                // Erro específico de rede (Wi-Fi desligado, servidor fora do ar, IP errado)
                MensagemErro = "Não foi possível conectar ao servidor. Verifique sua rede e se o servidor está ligado.";
                Console.WriteLine($"Erro de rede: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Outros erros (JSON inválido, etc)
                MensagemErro = "Ocorreu um erro inesperado. Tente novamente.";
                Console.WriteLine($"Erro: {ex.Message}");
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