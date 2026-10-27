using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SigaM.app.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; // Necessário para ler o JSON manualmente
using System.Text;
using Microsoft.Maui.ApplicationModel;

namespace SigaM.app.ViewModels
{
    public class EstacoesViewModel : INotifyPropertyChanged
    {
        private readonly string _apiBase = "http://localhost:3000/modules/sigam/api_gerenciar_estacoes.php";

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

            _ = CarregarEstacoes();
        }

        public async Task CarregarEstacoes()
        {
            try
            {
                using var client = new HttpClient();
                // Cache killer
                var url = $"{_apiBase}?action=listar&t={DateTime.Now.Ticks}";
                var response = await client.GetStringAsync(url);

                // LENDO O JSON NA MARRA (JObject)
                JObject json = JObject.Parse(response);
                JArray estacoesArray = (JArray)json["estacoes"];

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Estacoes.Clear();
                    if (estacoesArray != null)
                    {
                        foreach (var item in estacoesArray)
                        {
                            // Criando o objeto e preenchendo campo por campo
                            var novaEstacao = new Estacao
                            {
                                Id = (int)item["id"],
                                LocalidadeId = (int)item["localidade_id"],
                                Localidade = (string)item["localidade"],
                                Cidade = (string)item["cidade"],
                                Estado = (string)item["estado"],
                                IpGerenciamento = (string)item["ip_gerenciamento"],
                                IpLocalAtual = (string)item["ip_local_atual"], // AQUI ESTÁ O SEGREDO
                                VersaoFirmware = (string)item["versao_firmware"],
                                UltimaConexao = (string)item["ultima_conexao"],
                                Observacao = (string)item["observacao"]
                            };

                            Estacoes.Add(novaEstacao);
                        }

                        // ALERTA DE DEBUG
                        if (Estacoes.Count > 0)
                        {
                            var primeira = Estacoes[0];
                            Application.Current!.Windows[0].Page!.DisplayAlert(
                                "Debug de Leitura JSON",
                                $"Estação: {primeira.Localidade}\nIP Local: {primeira.IpLocalAtual ?? "NULO"}\nVersão: {primeira.VersaoFirmware ?? "NULA"}",
                                "OK");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar estações: {ex.Message}");
            }
        }

        private async Task CarregarLocalidadesLivres()
        {
            try
            {
                using var client = new HttpClient();
                var url = $"{_apiBase}?action=localidades&t={DateTime.Now.Ticks}";
                var response = await client.GetStringAsync(url);

                JObject json = JObject.Parse(response);
                JArray locArray = (JArray)json["localidades"];

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LocalidadesDisponiveis.Clear();
                    if (locArray != null)
                    {
                        foreach (var item in locArray)
                        {
                            LocalidadesDisponiveis.Add(new Localidade
                            {
                                Id = (int)item["id"],
                                Nome = (string)item["nome"]
                            });
                        }
                    }
                });
            }
            catch (Exception ex) { Console.WriteLine($"Erro ao carregar localidades: {ex.Message}"); }
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
            if (!PodeEditar || LocalidadeSelecionada == null) return;

            try
            {
                var dados = new
                {
                    id = _estacaoIdAtual,
                    localidade_id = LocalidadeSelecionada.Id,
                    ip_gerenciamento = IpGerenciamento,
                    observacao = Observacao,
                    usuario_prontuario = Globals.UsuarioLogado
                };

                using var client = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(dados), Encoding.UTF8, "application/json");
                await client.PostAsync($"{_apiBase}?action=salvar", content);

                IsModalVisivel = false;
                await CarregarEstacoes();
            }
            catch (Exception ex) { Console.WriteLine($"Erro ao salvar: {ex.Message}"); }
        }

        private async Task Excluir(Estacao? est)
        {
            if (est == null || !PodeEditar) return;

            bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert("Excluir", $"Deseja excluir a estação de {est.Localidade}?", "Sim", "Não");
            if (!confirm) return;

            try
            {
                var dados = new { id = est.Id, usuario_prontuario = Globals.UsuarioLogado };
                using var client = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(dados), Encoding.UTF8, "application/json");
                await client.PostAsync($"{_apiBase}?action=excluir", content);

                await CarregarEstacoes();
            }
            catch (Exception ex) { Console.WriteLine($"Erro ao excluir: {ex.Message}"); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}