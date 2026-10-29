using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Text.Json;
using SigaM.app.Models;

namespace SigaM.app.ViewModels
{
    public class AlarmesViewModel : INotifyPropertyChanged
    {
        private static readonly HttpClient _client = new HttpClient();
        private readonly string _apiUrl = "http://193.168.1.71:3000/modules/sigam/api_alarmes.php";
        private readonly string _apiEstacoesUrl = "http://193.168.1.71:3000/modules/sigam/api_estacoes.php";

        public ObservableCollection<Alarme> Alarmes { get; set; } = new();
        public ObservableCollection<EstacaoFiltro> EstacoesDisponiveis { get; set; } = new();
        public ObservableCollection<string> TiposDisponiveis { get; set; } = new() { "todos", "portas", "rede", "gerador", "bateria" };

        public EstacaoFiltro? EstacaoSelecionada { get; set; }
        public string TipoSelecionado { get; set; } = "todos";

        public string DataInicio { get; set; } = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
        public string DataFim { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand CarregarAlarmesCommand { get; }

        public AlarmesViewModel()
        {
            CarregarAlarmesCommand = new Command(async () => await CarregarDados());
            Task.Run(async () => await CarregarEstacoes());
        }

        public async Task CarregarDados()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                MainThread.BeginInvokeOnMainThread(() => Alarmes.Clear());

                string url = $"{_apiUrl}?data_inicio={DataInicio}&data_fim={DataFim}&tipo={TipoSelecionado}";
                if (EstacaoSelecionada != null && EstacaoSelecionada.Id > 0)
                {
                    url += $"&estacao_id={EstacaoSelecionada.Id}";
                }

                var response = await _client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                };

                var resultado = JsonSerializer.Deserialize<ApiResponse>(json, options);

                if (resultado?.Alarmes != null && resultado.Alarmes.Count > 0)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        foreach (var item in resultado.Alarmes)
                            Alarmes.Add(item);
                    });
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro de Leitura", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CarregarEstacoes()
        {
            try
            {
                var response = await _client.GetAsync(_apiEstacoesUrl);
                var json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var resultado = JsonSerializer.Deserialize<EstacoesResponse>(json, options);

                if (resultado?.Estacoes != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        EstacoesDisponiveis.Clear();
                        EstacoesDisponiveis.Add(new EstacaoFiltro { Id = 0, Localidade = "Todas as Localidades" });
                        foreach (var est in resultado.Estacoes)
                        {
                            EstacoesDisponiveis.Add(new EstacaoFiltro { Id = est.Id, Localidade = est.Localidade });
                        }
                    });
                }
            }
            catch { }
        }

        private class ApiResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("alarmes")]
            public List<Alarme>? Alarmes { get; set; }
        }

        private class EstacoesResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("estacoes")]
            public List<EstacaoFiltro>? Estacoes { get; set; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EstacaoFiltro
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public int Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("localidade")]
        public string Localidade { get; set; } = string.Empty;
    }
}