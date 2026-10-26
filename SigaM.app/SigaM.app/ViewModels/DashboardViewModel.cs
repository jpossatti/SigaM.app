using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SigaM.app.Models;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Maui.ApplicationModel;

namespace SigaM.app.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly string _apiDadosBase = "http://localhost:3000/modules/sigam/api_dados.php?estacao_id=";
        private readonly string _apiEstacoes = "http://localhost:3000/modules/sigam/api_estacoes.php";
        private readonly string _apiEnviarComando = "http://localhost:3000/modules/sigam/api_enviar_comando.php";

        public ObservableCollection<Estacao> EstacoesDisponiveis { get; set; } = new();

        private Estacao? _estacaoSelecionada;
        public Estacao? EstacaoSelecionada
        {
            get => _estacaoSelecionada;
            set
            {
                if (_estacaoSelecionada != value)
                {
                    _estacaoSelecionada = value;
                    OnPropertyChanged();
                    if (_estacaoSelecionada != null) _ = BuscarDados();
                }
            }
        }

        private Leitura? _leituraAtual;
        public Leitura? LeituraAtual
        {
            get => _leituraAtual;
            set
            {
                _leituraAtual = value;
                OnPropertyChanged();
                // Notifica o XAML que o texto do botão pode ter mudado
                OnPropertyChanged(nameof(TextoBotaoArmar));
                OnPropertyChanged(nameof(CorBotaoArmar));
            }
        }

        // Propriedades para o Botão
        public string TextoBotaoArmar => LeituraAtual?.SistemaArmado == 1 ? "DESARMAR SISTEMA" : "ARMAR SISTEMA";
        public Color CorBotaoArmar => LeituraAtual?.SistemaArmado == 1 ? Colors.DarkRed : Colors.Green;

        public ICommand AtualizarCommand { get; }
        public ICommand ToggleArmarCommand { get; }

        public DashboardViewModel()
        {
            AtualizarCommand = new Command(async () => await BuscarDados());
            ToggleArmarCommand = new Command(async () => await AlternarArmamento());
            _ = Inicializar();
        }

        private async Task Inicializar()
        {
            await CarregarEstacoes();
            _ = IniciarAtualizacaoAutomatica();
        }

        private async Task CarregarEstacoes()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetStringAsync(_apiEstacoes);
                dynamic? json = JsonConvert.DeserializeObject(response);

                if (json != null && json.estacoes != null)
                {
                    var lista = JsonConvert.DeserializeObject<List<Estacao>>(json.estacoes.ToString());
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        EstacoesDisponiveis.Clear();
                        if (lista != null)
                        {
                            foreach (var est in lista) EstacoesDisponiveis.Add(est);
                            if (EstacoesDisponiveis.Count > 0) EstacaoSelecionada = EstacoesDisponiveis[0];
                        }
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine($"Erro ao carregar estações: {ex.Message}"); }
        }

        private async Task IniciarAtualizacaoAutomatica()
        {
            while (true)
            {
                await Task.Delay(5000);
                await BuscarDados();
            }
        }

        public async Task BuscarDados()
        {
            if (EstacaoSelecionada == null) return;

            try
            {
                using var client = new HttpClient();
                var response = await client.GetStringAsync(_apiDadosBase + EstacaoSelecionada.Id);
                dynamic? json = JsonConvert.DeserializeObject(response);

                if (json != null && json.leitura != null)
                {
                    var novaLeitura = JsonConvert.DeserializeObject<Leitura>(json.leitura.ToString());
                    MainThread.BeginInvokeOnMainThread(() => { LeituraAtual = novaLeitura; });
                }
            }
            catch (Exception ex) { Console.WriteLine($"Erro ao buscar dados: {ex.Message}"); }
        }

        private async Task AlternarArmamento()
        {
            if (EstacaoSelecionada == null || LeituraAtual == null) return;

            string comando = LeituraAtual.SistemaArmado == 1 ? "desarmar" : "armar";
            string acao = comando == "armar" ? "ARMAR" : "DESARMAR";

            // 1. Pede confirmação (Modal nativo do MAUI)
            bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Confirmação",
                $"Tem certeza que deseja {acao} o sistema?",
                "Sim", "Cancelar");

            if (!confirm) return;

            try
            {
                // 2. Envia o comando para o PHP
                var dados = new
                {
                    estacao_id = EstacaoSelecionada.Id,
                    comando = comando,
                    usuario_id = 1 // Aqui poderia usar Globals.UsuarioId
                };

                var content = new StringContent(JsonConvert.SerializeObject(dados), Encoding.UTF8, "application/json");
                using var client = new HttpClient();
                await client.PostAsync(_apiEnviarComando, content);

                await Application.Current!.Windows[0].Page!.DisplayAlert("Sucesso", $"Comando {acao} enviado com sucesso!", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Erro", "Falha ao enviar comando: " + ex.Message, "OK");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}