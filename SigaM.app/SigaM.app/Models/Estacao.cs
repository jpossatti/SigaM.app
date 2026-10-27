using Newtonsoft.Json;

namespace SigaM.app.Models
{
    public class Estacao
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("localidade_id")]
        public int LocalidadeId { get; set; }

        [JsonProperty("localidade")]
        public string? Localidade { get; set; }

        [JsonProperty("cidade")]
        public string? Cidade { get; set; }

        [JsonProperty("estado")]
        public string? Estado { get; set; }

        [JsonProperty("ip_gerenciamento")]
        public string? IpGerenciamento { get; set; }

        [JsonProperty("ip_local_atual")]
        public string? IpLocalAtual { get; set; }

        [JsonProperty("versao_firmware")]
        public string? VersaoFirmware { get; set; }

        [JsonProperty("ultima_conexao")]
        public string? UltimaConexao { get; set; }

        [JsonProperty("observacao")]
        public string? Observacao { get; set; }

        // ==========================================
        // PROPRIEDADES CALCULADAS PARA A TELA
        // ==========================================

        public string CidadeEstado => $"{Cidade} - {Estado}";

        public string TextoIp => string.IsNullOrEmpty(IpLocalAtual) ? "Offline" : $"Conectado ({IpLocalAtual})";

        public string TextoVersao => string.IsNullOrEmpty(VersaoFirmware) ? "Sem versão" : $"Versão: {VersaoFirmware}";

        public string TextoData => string.IsNullOrEmpty(UltimaConexao) ? "Nunca conectado" : $"Últ. Contato: {UltimaConexao}";

        // REGRA DE NEGÓCIO: Só permite editar se o IP estiver preenchido (ESP32 online)
        public bool PodeEditarEstacao => !string.IsNullOrEmpty(IpLocalAtual);
    }
}