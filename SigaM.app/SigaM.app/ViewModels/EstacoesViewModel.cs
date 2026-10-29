using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SigaM.app.Models;
using System.Text;
using System.Text.Json;

namespace SigaM.app.ViewModels
{
    public class EstacoesViewModel : INotifyPropertyChanged
    {
        // IP Local do seu computador onde o XAMPP/PHP está rodando
        private readonly string _apiBase = "http://193.168.1.71:3000/modules/sigam/api_gerenciar_estacoes.php";

        public ObservableCollection<Estacao> Estacoes { get; set; } = new();
        public ObservableCollection<Localidade> LocalidadesDisponiveis { get; set; } = new();

        public bool PodeEditar => Globals.IsAdmin;

        private bool _isModalVisivel = false;
        public bool IsModalVisivel
        {
            get => _isModalVisivel;
            set { _isModalVisivel = value; OnPropertyChanged(); }
        }

        private int _estacaoIdAtual;

        private Localidade? _localidadeSelecionada;
        public Localidade? LocalidadeSelecionada
        {
            get => _localidadeSelecionada;
            set { _localidadeSelecionada = value; OnPropertyChanged(); }
        }

        private string _ipGerenciamento = string.Empty;
        public string IpGerenciamento
        {
            get => _ipGerenciamento;
            set { _ipGerenciamento = value; OnPropertyChanged(); }
        }

        private string _observacao = string.Empty;
        public string Observacao
        {
            get => _observacao;
            set { _observacao = value; OnPropertyChanged(); }
        }

        public Command CarregarEstacoesCommand { get; }
        public Command AbrirModalNovoCommand { get; }
        public Command AbrirModalEditarCommand { get; }
        public Command SalvarCommand { get; }
        public Command ExcluirCommand { get; }
        public Command FecharModalCommand { get; }

        public EstacoesViewModel()
        {
            CarregarEstacoesCommand = new Command(async () => await CarregarEstacoes());
            AbrirModalNovoCommand = new Command(async () => await AbrirModalNovo());
            AbrirModalEditarCommand = new Command<Estacao>(async (e) => await AbrirModalEditar(e));
            SalvarCommand = new Command(async () => await Salvar());
            ExcluirCommand = new Command<Estacao>(async (e) => await Excluir(e));
            FecharModalCommand = new Command(() => IsModalVisivel = false);

            // Carrega as estações assim que o ViewModel é instanciado
            _ = CarregarEstacoes();
        }

        public async Task CarregarEstacoes()
        {
            try
            {
                using var client = new HttpClient();
                // O parâmetro 't' evita que o app carregue dados antigos do cache
                var url = $"{_apiBase}?action=listar&t={DateTime.Now.Ticks}";
                var response = await client.GetStringAsync(url);

                // Configura o leitor JSON para ignorar maiúsculas/minúsculas
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var resultado = JsonSerializer.Deserialize<EstacoesResponse>(response, options);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Estacoes.Clear();
                    if (resultado?.Estacoes != null)
                    {
                        foreach (var item in resultado.Estacoes)
                        {
                            Estacoes.Add(item);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro de Conexão", $"Falha ao carregar estações: {ex.Message}", "OK");
            }
        }

        private async Task CarregarLocalidadesLivres()
        {
            try
            {
                using var client = new HttpClient();
                var url = $"{_apiBase}?action=localidades&t={DateTime.Now.Ticks}";
                var response = await client.GetStringAsync(url);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var resultado = JsonSerializer.Deserialize<LocalidadesResponse>(response, options);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LocalidadesDisponiveis.Clear();
                    if (resultado?.Localidades != null)
                    {
                        foreach (var item in resultado.Localidades)
                        {
                            // Proteção caso o nome venha nulo do banco
                            item.Nome ??= "Sem Nome";
                            LocalidadesDisponiveis.Add(item);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar localidades: {ex.Message}");
            }
        }

        private async Task AbrirModalNovo()
        {
            if (!PodeEditar) return;
            await CarregarLocalidadesLivres();

            _estacaoIdAtual = 0;
            LocalidadeSelecionada = null;
            IpGerenciamento = string.Empty;
            Observacao = string.Empty;
            IsModalVisivel = true;
        }

        private async Task AbrirModalEditar(Estacao? est)
        {
            if (est == null || !PodeEditar) return;
            await CarregarLocalidadesLivres();

            // Adiciona a localidade atual no topo da lista do Picker
            var locAtual = new Localidade { Id = est.LocalidadeId, Nome = est.Localidade };
            LocalidadesDisponiveis.Insert(0, locAtual);
            LocalidadeSelecionada = locAtual;

            _estacaoIdAtual = est.Id;
            IpGerenciamento = est.IpGerenciamento ?? "";
            Observacao = est.Observacao ?? "";
            IsModalVisivel = true;
        }

        private async Task Salvar()
        {
            if (!PodeEditar || LocalidadeSelecionada == null)
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "Selecione uma localidade.", "OK");
                return;
            }

            try
            {
                var dados = new
                {
                    id = _estacaoIdAtual,
                    localidade_id = LocalidadeSelecionada.Id,
                    ip_gerenciamento = IpGerenciamento,
                    observacao = Observacao,
                    usuario_prontuario = Globals.UsuarioLogado // Envia o prontuário para validar no PHP
                };

                using var client = new HttpClient();
                var content = new StringContent(JsonSerializer.Serialize(dados), Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_apiBase}?action=salvar", content);

                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Erro no Servidor", $"Não foi possível salvar. Retorno: {erro}", "OK");
                    return;
                }

                IsModalVisivel = false;
                await CarregarEstacoes(); // Recarrega a lista
                await Application.Current.MainPage.DisplayAlert("Sucesso", "Estação salva com sucesso!", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro Crítico", ex.Message, "OK");
            }
        }

        private async Task Excluir(Estacao? est)
        {
            if (est == null || !PodeEditar) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Excluir", $"Deseja excluir a estação de {est.Localidade}?", "Sim", "Não");
            if (!confirm) return;

            try
            {
                var dados = new { id = est.Id, usuario_prontuario = Globals.UsuarioLogado };
                using var client = new HttpClient();
                var content = new StringContent(JsonSerializer.Serialize(dados), Encoding.UTF8, "application/json");

                await client.PostAsync($"{_apiBase}?action=excluir", content);

                await CarregarEstacoes();
                await Application.Current.MainPage.DisplayAlert("Sucesso", "Estação excluída!", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", ex.Message, "OK");
            }
        }

        // Classes Auxiliares para Deserialização (Ocultas da Interface)
        private class EstacoesResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("estacoes")]
            public List<Estacao>? Estacoes { get; set; }
        }

        private class LocalidadesResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("localidades")]
            public List<Localidade>? Localidades { get; set; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}