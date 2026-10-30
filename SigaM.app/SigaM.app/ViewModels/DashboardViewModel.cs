using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SigaM.app.Models;
using Microsoft.Maui.Graphics;

namespace SigaM.app.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        // ============================================================
        // 1. CONFIGURAÇÃO DE URLS
        // ============================================================
        private readonly string _baseUrl = "http://193.168.1.71:3000/modules/sigam/";
        private readonly string _apiDadosBase;
        private readonly string _apiEstacoes;
        private readonly string _apiEnviarComando;

        private IDispatcherTimer? _timer;

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
                    OnPropertyChanged(nameof(EstaOnline));

                    if (_estacaoSelecionada != null)
                    {
                        _ = BuscarDados();
                    }
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
                OnPropertyChanged(nameof(TextoBotaoArmar));
                OnPropertyChanged(nameof(CorBotaoArmar));

                // --- CORREÇÃO CRUCIAL: Força a atualização das propriedades internas na tela ---
                if (_leituraAtual != null)
                {
                    OnPropertyChanged(nameof(LeituraAtual.TensaoRede));
                    OnPropertyChanged(nameof(LeituraAtual.TensaoGae));
                    OnPropertyChanged(nameof(LeituraAtual.TensaoBateria));
                    OnPropertyChanged(nameof(LeituraAtual.PortaPrincipalStatus));
                    OnPropertyChanged(nameof(LeituraAtual.PortaGeradorStatus));
                    OnPropertyChanged(nameof(LeituraAtual.PortaSalaBateriasStatus));
                    OnPropertyChanged(nameof(LeituraAtual.PortaBanheiroStatus));
                    OnPropertyChanged(nameof(LeituraAtual.SistemaArmado));

                    // Antigas propriedades de armamento
                    OnPropertyChanged(nameof(LeituraAtual.TextoArmado));
                    OnPropertyChanged(nameof(LeituraAtual.CorArmado));

                    // NOVAS PROPRIEDADES DO ALARME VIOLADO
                    OnPropertyChanged(nameof(LeituraAtual.TextoStatusSeguranca));
                    OnPropertyChanged(nameof(LeituraAtual.CorStatusSeguranca));

                    OnPropertyChanged(nameof(LeituraAtual.TextoPrincipal));
                    OnPropertyChanged(nameof(LeituraAtual.TextoGerador));
                    OnPropertyChanged(nameof(LeituraAtual.TextoSalaBaterias));
                    OnPropertyChanged(nameof(LeituraAtual.TextoBanheiro));
                    OnPropertyChanged(nameof(LeituraAtual.CorRede));
                    OnPropertyChanged(nameof(LeituraAtual.CorBateria));
                    OnPropertyChanged(nameof(LeituraAtual.CorPrincipal));
                    OnPropertyChanged(nameof(LeituraAtual.CorGerador));
                    OnPropertyChanged(nameof(LeituraAtual.CorSalaBaterias));
                    OnPropertyChanged(nameof(LeituraAtual.CorBanheiro));
                    OnPropertyChanged(nameof(LeituraAtual.DataHoraLeitura));
                }
            }
        }

        // --- PROPRIEDADES VISUAIS ---
        public string TextoBotaoArmar => LeituraAtual?.SistemaArmado == 1 ? "🔓 DESARMAR SISTEMA" : "🔒 ARMAR SISTEMA";
        public Color CorBotaoArmar => LeituraAtual?.SistemaArmado == 1 ? Color.FromArgb("#dc3545") : Color.FromArgb("#198754");

        public bool EstaOnline => EstacaoSelecionada != null && !string.IsNullOrEmpty(EstacaoSelecionada.IpLocalAtual) && EstacaoSelecionada.IpLocalAtual != "Offline";

        public ICommand AtualizarCommand { get; }
        public ICommand ToggleArmarCommand { get; }

        public DashboardViewModel()
        {
            _apiDadosBase = _baseUrl + "api_dados.php?estacao_id=";
            _apiEstacoes = _baseUrl + "api_estacoes.php";
            _apiEnviarComando = _baseUrl + "api_enviar_comando.php";

            AtualizarCommand = new Command(async () => await BuscarDados());
            ToggleArmarCommand = new Command(async () => await AlternarArmamento());

            // Inicializa as chamadas na MainThread para evitar problemas de contexto
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await CarregarEstacoes();
                IniciarAtualizacaoAutomatica();
            });
        }

        private async Task CarregarEstacoes()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                var response = await client.GetStringAsync($"{_apiEstacoes}?t={DateTime.Now.Ticks}");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var resultado = JsonSerializer.Deserialize<EstacoesResponse>(response, options);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    EstacoesDisponiveis.Clear();
                    if (resultado?.Estacoes != null && resultado.Estacoes.Count > 0)
                    {
                        foreach (var est in resultado.Estacoes)
                        {
                            EstacoesDisponiveis.Add(est);
                        }

                        // Seleciona a primeira estação e já busca os dados
                        EstacaoSelecionada = EstacoesDisponiveis[0];
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERRO AO CARREGAR ESTAÇÕES: {ex.Message}");
            }
        }

        private void IniciarAtualizacaoAutomatica()
        {
            if (Application.Current?.Dispatcher == null) return;

            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += async (s, e) =>
            {
                // PROTEÇÃO: Só tenta buscar se houver uma estação selecionada
                if (EstacaoSelecionada != null)
                {
                    await BuscarDados();
                }
            };
            _timer.Start();
        }

        public async Task BuscarDados()
        {
            if (EstacaoSelecionada == null) return;

            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                var url = $"{_apiDadosBase}{EstacaoSelecionada.Id}&t={DateTime.Now.Ticks}";
                var response = await client.GetStringAsync(url);

                // Configuração que permite ler números que vieram como string do PHP
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                var json = JsonSerializer.Deserialize<DadosResponse>(response, options);

                if (json != null && json.Leitura != null)
                {
                    // Correção do erro CS0120: Usando MainThread para atualizar a tela
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LeituraAtual = json.Leitura;
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("JSON veio, mas a propriedade 'Leitura' está nula.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERRO AO BUSCAR DADOS: {ex.Message}");
            }
        }

        private async Task AlternarArmamento()
        {
            if (EstacaoSelecionada == null || LeituraAtual == null) return;

            if (!EstaOnline)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert(
                    "Estação Offline",
                    "Não é possível enviar comandos. O ESP32 desta estação não está conectado.",
                    "OK");
                return;
            }

            string comando = LeituraAtual.SistemaArmado == 1 ? "desarmar" : "armar";
            string acao = comando == "armar" ? "ARMAR" : "DESARMAR";

            bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Confirmação",
                $"Tem certeza que deseja {acao} o sistema em {EstacaoSelecionada.Localidade}?",
                "Sim", "Cancelar");

            if (!confirm) return;

            try
            {
                var dados = new
                {
                    estacao_id = EstacaoSelecionada.Id,
                    comando = comando,
                    usuario_id = Globals.UsuarioId
                };

                var content = new StringContent(JsonSerializer.Serialize(dados), Encoding.UTF8, "application/json");
                using var client = new HttpClient();

                var response = await client.PostAsync(_apiEnviarComando, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                var json = JsonSerializer.Deserialize<ComandoResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (response.IsSuccessStatusCode && json?.Status == "sucesso")
                {
                    await Application.Current!.Windows[0].Page!.DisplayAlert("Sucesso", $"Comando {acao} enfileirado! O ESP32 irá executá-lo em instantes.", "OK");
                }
                else
                {
                    await Application.Current!.Windows[0].Page!.DisplayAlert("Aviso", json?.Mensagem ?? "Falha ao enviar comando.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Erro", "Falha na conexão: " + ex.Message, "OK");
            }
        }

        // ============================================================
        // CLASSES AUXILIARES PARA LEITURA DO JSON
        // ============================================================
        private class EstacoesResponse
        {
            [JsonPropertyName("estacoes")]
            public List<Estacao>? Estacoes { get; set; }
        }

        private class DadosResponse
        {
            [JsonPropertyName("leitura")]
            public Leitura? Leitura { get; set; }
        }

        private class ComandoResponse
        {
            [JsonPropertyName("status")]
            public string? Status { get; set; }
            [JsonPropertyName("mensagem")]
            public string? Mensagem { get; set; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}