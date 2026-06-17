using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Solicitacao
    {
        public int Id { get; set; }

        public int Numero { get; set; }

        [Required]
        public string Setor { get; set; } = string.Empty;

        public string Prioridade { get; set; } = "Normal";

        [Required]
        public string Solicitante { get; set; } = string.Empty;

        [Required]
        public string CentroCusto { get; set; } = string.Empty;

        public string? Observacao { get; set; }

        public DateTime DataSolicitacao { get; set; }

        public string Status { get; set; } = "Pendente";

        public List<SolicitacaoItem> Itens { get; set; } = new();
    }
}