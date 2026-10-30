using System.Text.Json.Serialization;
using Microsoft.Maui.Graphics;

namespace SigaM.app.Models
{
    public class Leitura
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("estacao_id")]
        public int EstacaoId { get; set; }

        // A MÁGICA ACONTECE AQUI: Dizemos para ler mesmo se vier como string
        [JsonPropertyName("tensao_rede")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public double TensaoRede { get; set; }

        [JsonPropertyName("tensao_gae")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public double TensaoGae { get; set; }

        [JsonPropertyName("tensao_bateria")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public double TensaoBateria { get; set; }

        [JsonPropertyName("porta_principal_status")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int PortaPrincipalStatus { get; set; }

        [JsonPropertyName("porta_gerador_status")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int PortaGeradorStatus { get; set; }

        [JsonPropertyName("porta_sala_baterias_status")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int PortaSalaBateriasStatus { get; set; }

        [JsonPropertyName("porta_banheiro_status")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int PortaBanheiroStatus { get; set; }

        [JsonPropertyName("sistema_armado")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int SistemaArmado { get; set; }

        [JsonPropertyName("alarme_ativo")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int AlarmeAtivo { get; set; }

        [JsonPropertyName("data_hora_leitura")]
        public string? DataHoraLeitura { get; set; }

        // --- PROPRIEDADES CALCULADAS PARA A TELA ---
        [JsonIgnore]
        public Color CorRede => (TensaoRede >= 110 && TensaoRede <= 135) ? Colors.Green : Colors.Red;

        [JsonIgnore]
        public Color CorBateria => (TensaoBateria >= 11.5) ? Colors.Green : Colors.Red;

        [JsonIgnore]
        public string TextoPrincipal => PortaPrincipalStatus == 1 ? "Aberta" : "Fechada";

        [JsonIgnore]
        public string TextoGerador => PortaGeradorStatus == 1 ? "Aberta" : "Fechada";

        [JsonIgnore]
        public string TextoSalaBaterias => PortaSalaBateriasStatus == 1 ? "Aberta" : "Fechada";

        [JsonIgnore]
        public string TextoBanheiro => PortaBanheiroStatus == 1 ? "Aberta" : "Fechada";

        [JsonIgnore]
        public Color CorPrincipal => PortaPrincipalStatus == 1 ? Colors.Red : Colors.Green;

        [JsonIgnore]
        public Color CorGerador => PortaGeradorStatus == 1 ? Colors.Red : Colors.Green;

        [JsonIgnore]
        public Color CorSalaBaterias => PortaSalaBateriasStatus == 1 ? Colors.Red : Colors.Green;

        [JsonIgnore]
        public Color CorBanheiro => PortaBanheiroStatus == 1 ? Colors.Red : Colors.Green;

        [JsonIgnore]
        public string TextoArmado => SistemaArmado == 1 ? "ARMADO" : "DESARMADO";

        [JsonIgnore]
        public Color CorArmado => SistemaArmado == 1 ? Colors.Green : Colors.Gray;
        [JsonIgnore]
        public bool AlarmeViolado => AlarmeAtivo == 1;

        [JsonIgnore]
        public string TextoStatusSeguranca
        {
            get
            {
                if (AlarmeViolado) return "🚨 ALARME DISPARADO";
                return SistemaArmado == 1 ? "🔒 SISTEMA ARMADO" : "🔓 SISTEMA DESARMADO";
            }
        }

        [JsonIgnore]
        public Color CorStatusSeguranca
        {
            get
            {
                if (AlarmeViolado) return Colors.Red; // Vermelho forte
                return SistemaArmado == 1 ? Colors.Green : Colors.Gray;
            }
        }
    }
}