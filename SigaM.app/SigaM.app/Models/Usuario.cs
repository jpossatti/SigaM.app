using Newtonsoft.Json;

namespace SigaM.app.Models
{
    public class Usuario
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nome")]
        public string? Nome { get; set; }

        [JsonProperty("prontuario")]
        public string? Prontuario { get; set; }
    }
}
