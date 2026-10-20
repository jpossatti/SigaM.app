using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SigaM.app.Models;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Maui.ApplicationModel;

namespace SigaM.app.ViewModels
{
    public class EstacoesViewModel : INotifyPropertyChanged
    {
        private readonly string _apiBase = "http://localhost:3000/modules/sigam/api_gerenciar_estacoes.php";

        public ObservableCollection<Estacao> Estacoes { get; set; } = new();
        public ObservableCollection<Localidade> LocalidadesDisponiveis { get; set; } = new();

        // --- CONTROLE DE PERMISSÃO ---
        public bool PodeEditar => Globals.IsAdmin;

        // --- PROPRIEDADES DO FORMULÁRIO MODAL ---
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

        // --- COMANDOS ---
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
                var response = await client.GetStringAsync($"{_apiBase}?action=listar");
                dynamic? json = JsonConvert.DeserializeObject(response);

                if (json != null && json.estacoes != null)
                {
                    var lista = JsonConvert.DeserializeObject<List<Estacao>>(json.estacoes.ToString());
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Estacoes.Clear();
                        if (lista != null)
                        {
                            foreach (var item in lista) Estacoes.Add(item);
                        }
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine($"Erro: {ex.Message}"); }
        }

        private async Task CarregarLocalidadesLivres()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetStringAsync($"{_apiBase}?action=localidades");
                dynamic? json = JsonConvert.DeserializeObject(response);

                if (json != null && json.localidades != null)
                {
                    var lista = JsonConvert.DeserializeObject<List<Localidade>>(json.localidades.ToString());
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LocalidadesDisponiveis.Clear();
                        if (lista != null)
                        {
                            foreach (var item in lista) LocalidadesDisponiveis.Add(item);
                        }
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine($"Erro: {ex.Message}"); }
        }

        private async Task AbrirModalNovo()
        {
            if (!PodeEditar) return; // Dupla checagem

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
            if (!PodeEditar) return;

            if (LocalidadeSelecionada == null)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Aviso", "Selecione uma localidade.", "OK");
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
                    usuario_prontuario = Globals.UsuarioLogado // ENVIA O USUÁRIO PARA A API
                };

                using var client = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(dados), Encoding.UTF8, "application/json");
                await client.PostAsync($"{_apiBase}?action=salvar", content);

                IsModalVisivel = false;
                await CarregarEstacoes();
            }
            catch (Exception ex) { Console.WriteLine($"Erro: {ex.Message}"); }
        }

        private async Task Excluir(Estacao? est)
        {
            if (est == null || !PodeEditar) return;

            bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert("Excluir", $"Deseja excluir a estação de {est.Localidade}?", "Sim", "Não");
            if (!confirm) return;

            try
            {
                var dados = new
                {
                    id = est.Id,
                    usuario_prontuario = Globals.UsuarioLogado // ENVIA O USUÁRIO PARA A API
                };

                using var client = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(dados), Encoding.UTF8, "application/json");
                await client.PostAsync($"{_apiBase}?action=excluir", content);

                await CarregarEstacoes();
            }
            catch (Exception ex) { Console.WriteLine($"Erro: {ex.Message}"); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}