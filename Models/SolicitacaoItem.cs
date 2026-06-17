namespace API.Models
{
    public class SolicitacaoItem
    {
        public int Id { get; set; }

        public int SolicitacaoId { get; set; }

        public Solicitacao? Solicitacao { get; set; }

        public int ProdutoId { get; set; }
      
        public Produto? Produto { get; set; }

        public decimal Quantidade { get; set; }
    }
}