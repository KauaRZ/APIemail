using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class SolicitacaoDTO
{
    [Required]
    public string Setor { get; set; } = string.Empty;

    [Required]
    public string Solicitante { get; set; } = string.Empty;

    [Required]
    public string CentroCusto { get; set; } = string.Empty;

    public string Prioridade { get; set; } = "Normal";

    public string? Observacao { get; set; }

    public List<SolicitacaoItemDTO> Itens { get; set; } = new();
}