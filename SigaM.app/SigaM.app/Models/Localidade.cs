using System.Text.Json.Serialization;

namespace SigaM.app.Models
{
    public class Localidade
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nome")]
        public string? Nome { get; set; }

        [JsonPropertyName("cidade")]
        public string? Cidade { get; set; }
    }
}