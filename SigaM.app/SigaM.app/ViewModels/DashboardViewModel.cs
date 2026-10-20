using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SigaM.app.Models;
using Newtonsoft.Json;
using Microsoft.Maui.ApplicationModel;

namespace SigaM.app.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        // URLs base
        private readonly string _apiDadosBase = "http://localhost:3000/modules/sigam/api_dados.php?estacao_id=";
        private readonly string _apiEstacoes = "http://localhost:3000/modules/sigam/api_estacoes.php";

        // Lista de estações para o Picker
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
                    // Quando o usuário muda a estação, busca os novos dados imediatamente
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
            }
        }

        public ICommand AtualizarCommand { get; }

        public DashboardViewModel()
        {
            AtualizarCommand = new Command(async () => await BuscarDados());

            // Carrega as estações ao iniciar e depois inicia o loop
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
                            foreach (var est in lista)
                            {
                                EstacoesDisponiveis.Add(est);
                            }

                            // Seleciona a primeira estação por padrão
                            if (EstacoesDisponiveis.Count > 0)
                            {
                                EstacaoSelecionada = EstacoesDisponiveis[0];
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar estações: {ex.Message}");
            }
        }

        private async Task IniciarAtualizacaoAutomatica()
        {
            while (true)
            {
                await Task.Delay(5000); // Atualiza a cada 5 segundos
                await BuscarDados();
            }
        }

        public async Task BuscarDados()
        {
            if (EstacaoSelecionada == null) return;

            try
            {
                using var client = new HttpClient();
                // Usa o ID da estação selecionada na URL
                var response = await client.GetStringAsync(_apiDadosBase + EstacaoSelecionada.Id);
                dynamic? json = JsonConvert.DeserializeObject(response);

                if (json != null && json.leitura != null)
                {
                    var novaLeitura = JsonConvert.DeserializeObject<Leitura>(json.leitura.ToString());

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LeituraAtual = novaLeitura;
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar dados: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}