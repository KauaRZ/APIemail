public class Produto
{
    public int Id { get; set; }

    public string Codigo { get; set; }

    public string Descricao { get; set; }

    public string Categoria { get; set; }

    public string Unidade { get; set; }

    public string Fornecedor { get; set; }

    public int EstoqueMinimo { get; set; }

    public decimal QuantidadeAtual { get; set; }

    public string Observacao { get; set; }
}