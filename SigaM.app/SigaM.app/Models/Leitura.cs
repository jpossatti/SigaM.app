using Newtonsoft.Json;

namespace SigaM.app.Models
{
    public class Leitura
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("estacao_id")]
        public int EstacaoId { get; set; }

        [JsonProperty("tensao_rede")]
        public double TensaoRede { get; set; }

        [JsonProperty("tensao_gae")]
        public double TensaoGae { get; set; }

        [JsonProperty("tensao_bateria")]
        public double TensaoBateria { get; set; }

        [JsonProperty("porta_principal_status")]
        public int PortaPrincipalStatus { get; set; }

        [JsonProperty("porta_gerador_status")]
        public int PortaGeradorStatus { get; set; }

        [JsonProperty("porta_sala_baterias_status")]
        public int PortaSalaBateriasStatus { get; set; }

        [JsonProperty("porta_banheiro_status")]
        public int PortaBanheiroStatus { get; set; }

        [JsonProperty("sistema_armado")]
        public int SistemaArmado { get; set; }

        [JsonProperty("alarme_ativo")]
        public int AlarmeAtivo { get; set; }

        [JsonProperty("data_hora_leitura")]
        public string? DataHoraLeitura { get; set; }

        // --- PROPRIEDADES CALCULADAS PARA A TELA ---
        public Color CorRede => (TensaoRede >= 110 && TensaoRede <= 135) ? Colors.Green : Colors.Red;
        public Color CorBateria => (TensaoBateria >= 11.5) ? Colors.Green : Colors.Red;

        public string TextoPrincipal => PortaPrincipalStatus == 1 ? "Aberta" : "Fechada";
        public string TextoGerador => PortaGeradorStatus == 1 ? "Aberta" : "Fechada";
        public string TextoSalaBaterias => PortaSalaBateriasStatus == 1 ? "Aberta" : "Fechada";
        public string TextoBanheiro => PortaBanheiroStatus == 1 ? "Aberta" : "Fechada";

        public Color CorPrincipal => PortaPrincipalStatus == 1 ? Colors.Red : Colors.Green;
        public Color CorGerador => PortaGeradorStatus == 1 ? Colors.Red : Colors.Green;
        public Color CorSalaBaterias => PortaSalaBateriasStatus == 1 ? Colors.Red : Colors.Green;
        public Color CorBanheiro => PortaBanheiroStatus == 1 ? Colors.Red : Colors.Green;

        public string TextoArmado => SistemaArmado == 1 ? "ARMADO" : "DESARMADO";
        public Color CorArmado => SistemaArmado == 1 ? Colors.Green : Colors.Gray;
    }
}