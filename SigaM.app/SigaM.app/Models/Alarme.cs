using Newtonsoft.Json;

namespace SigaM.app.Models
{
    public class Alarme
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("data_criacao")]
        public string? DataCriacao { get; set; }

        [JsonProperty("localidade")]
        public string? Localidade { get; set; }

        [JsonProperty("tipo_alerta")]
        public string? TipoAlerta { get; set; }

        [JsonProperty("mensagem")]
        public string? Mensagem { get; set; }

        [JsonProperty("prioridade")]
        public string? Prioridade { get; set; }

        // Cores para a interface
        public Color CorPrioridade => Prioridade?.ToLower() == "critica" ? Colors.Red :
                                     (Prioridade?.ToLower() == "alta" ? Colors.Orange : Colors.Gray);
    }
}