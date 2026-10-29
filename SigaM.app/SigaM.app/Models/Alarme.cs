using System.Text.Json.Serialization;
using Microsoft.Maui.Graphics;

namespace SigaM.app.Models
{
    public class Alarme
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("data_criacao")]
        public string DataCriacaoStr { get; set; } = string.Empty;

        [JsonPropertyName("localidade")]
        public string Localidade { get; set; } = string.Empty;

        [JsonPropertyName("tipo_alerta")]
        public string TipoAlerta { get; set; } = string.Empty;

        [JsonPropertyName("mensagem")]
        public string Mensagem { get; set; } = string.Empty;

        [JsonPropertyName("prioridade")]
        public string Prioridade { get; set; } = string.Empty;

        // Propriedades calculadas para a Interface
        [JsonIgnore]
        public Color CorPrioridade => Prioridade.ToLower() switch
        {
            "critica" => Color.FromArgb("#dc3545"),
            "alta" => Color.FromArgb("#fd7e14"),
            "media" => Color.FromArgb("#ffc107"),
            _ => Color.FromArgb("#28a745")
        };

        [JsonIgnore]
        public string TextoPrioridade => Prioridade.ToUpper();

        [JsonIgnore]
        public string DataFormatada
        {
            get
            {
                if (DateTime.TryParse(DataCriacaoStr, out DateTime dt))
                    return dt.ToString("dd/MM/yyyy HH:mm");
                return DataCriacaoStr;
            }
        }
    }
}