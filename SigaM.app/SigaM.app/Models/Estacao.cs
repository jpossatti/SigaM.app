using System.Text.Json.Serialization;

namespace SigaM.app.Models
{
    public class Estacao
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("localidade_id")]
        public int LocalidadeId { get; set; }

        [JsonPropertyName("localidade")]
        public string Localidade { get; set; } = string.Empty;

        [JsonPropertyName("cidade")]
        public string Cidade { get; set; } = string.Empty;

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = string.Empty;

        [JsonPropertyName("ip_gerenciamento")]
        public string? IpGerenciamento { get; set; }

        [JsonPropertyName("ip_local_atual")]
        public string? IpLocalAtual { get; set; }

        [JsonPropertyName("versao_firmware")]
        public string? VersaoFirmware { get; set; }

        [JsonPropertyName("ultima_conexao")]
        public string? UltimaConexao { get; set; }

        [JsonPropertyName("observacao")]
        public string? Observacao { get; set; }

        [JsonIgnore]
        public string CidadeEstado => $"{Cidade} - {Estado}";

        [JsonIgnore]
        public string TextoIp => string.IsNullOrEmpty(IpLocalAtual) ? "📡 Offline (Aguardando ESP32)" : $"📡 Conectado: {IpLocalAtual}";

        [JsonIgnore]
        public string TextoVersao => string.IsNullOrEmpty(VersaoFirmware) ? "" : $"Versão Firmware: {VersaoFirmware}";

        [JsonIgnore]
        public string TextoData
        {
            get
            {
                if (string.IsNullOrEmpty(UltimaConexao)) return "Nunca conectado";
                if (DateTime.TryParse(UltimaConexao, out DateTime dt))
                    return $"Visto por último: {dt:dd/MM/yyyy HH:mm}";
                return UltimaConexao;
            }
        }

        [JsonIgnore]
        public bool PodeEditarEstacao => true;
    }
}