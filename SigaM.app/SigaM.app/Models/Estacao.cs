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

        [JsonProperty("observacao")]
        public string? Observacao { get; set; }

        public string CidadeEstado => $"{Cidade} - {Estado}";
    }
}