using Newtonsoft.Json;

namespace SigaM.app.Models
{
    public class Localidade
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nome")]
        public string? Nome { get; set; }

        [JsonProperty("cidade")]
        public string? Cidade { get; set; }
    }
}
