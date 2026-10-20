using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SigaM.app.Models;
using Newtonsoft.Json;
using Microsoft.Maui.ApplicationModel;

namespace SigaM.app.ViewModels
{
    public class AlarmesViewModel : INotifyPropertyChanged
    {
        private readonly string _apiAlarmesBase = "http://localhost:3000/modules/sigam/api_alarmes.php";
        private readonly string _apiEstacoes = "http://localhost:3000/modules/sigam/api_estacoes.php";

        // Listas
        public ObservableCollection<Alarme> Alarmes { get; set; } = new();
        public ObservableCollection<Estacao> EstacoesDisponiveis { get; set; } = new();

        // Filtros
        private string _dataInicio = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
        public string DataInicio
        {
            get => _dataInicio;
            set { _dataInicio = value; OnPropertyChanged(); }
        }

        private string _dataFim = DateTime.Now.ToString("yyyy-MM-dd");
        public string DataFim
        {
            get => _dataFim;
            set { _dataFim = value; OnPropertyChanged(); }
        }

        public List<string> TiposDisponiveis { get; } = new() { "todos", "portas", "rede", "gerador", "bateria" };
        private string _tipoSelecionado = "todos";
        public string TipoSelecionado
        {
            get => _tipoSelecionado;
            set { _tipoSelecionado = value; OnPropertyChanged(); }
        }

        private Estacao? _estacaoSelecionada;
        public Estacao? EstacaoSelecionada
        {
            get => _estacaoSelecionada;
            set { _estacaoSelecionada = value; OnPropertyChanged(); }
        }

        public Command CarregarAlarmesCommand { get; }

        public AlarmesViewModel()
        {
            CarregarAlarmesCommand = new Command(async () => await CarregarAlarmes());
            _ = Inicializar();
        }

        private async Task Inicializar()
        {
            await CarregarEstacoes();
            await CarregarAlarmes();
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
                            // Adiciona uma opção "Todas" (ID 0) no topo da lista
                            EstacoesDisponiveis.Add(new Estacao { Id = 0, Localidade = "Todas as Localidades" });

                            foreach (var est in lista) EstacoesDisponiveis.Add(est);

                            // Seleciona "Todas" por padrão
                            EstacaoSelecionada = EstacoesDisponiveis[0];
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar estações: {ex.Message}");
            }
        }

        public async Task CarregarAlarmes()
        {
            try
            {
                using var client = new HttpClient();

                // Pega o ID da estação. Se for 0, manda vazio para trazer todas
                string idParam = (EstacaoSelecionada != null && EstacaoSelecionada.Id > 0) ? EstacaoSelecionada.Id.ToString() : "";

                // Monta a URL com os parâmetros
                string url = $"{_apiAlarmesBase}?data_inicio={DataInicio}&data_fim={DataFim}&tipo={TipoSelecionado}&estacao_id={idParam}";

                var response = await client.GetStringAsync(url);
                dynamic? json = JsonConvert.DeserializeObject(response);

                if (json != null && json.alarmes != null)
                {
                    var lista = JsonConvert.DeserializeObject<List<Alarme>>(json.alarmes.ToString());

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Alarmes.Clear();
                        if (lista != null)
                        {
                            foreach (var item in lista) Alarmes.Add(item);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}