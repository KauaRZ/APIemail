using Microsoft.EntityFrameworkCore;

public class ProdutoRepository
{
    private readonly AppDbContext _db;

    public ProdutoRepository(
        AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Produto>>
        Listar()
    {
        return await _db.Produtos.ToListAsync();
    }

    public async Task Criar(
        Produto produto)
    {
        _db.Produtos.Add(produto);

        await _db.SaveChangesAsync();
    }

    public async Task<Produto?>
        BuscarPorId(int id)
    {
        return await _db.Produtos
            .FirstOrDefaultAsync(
                x => x.Id == id);
    }

    public async Task Atualizar()
    {
        await _db.SaveChangesAsync();
    }

    public async Task Excluir(
        Produto produto)
    {
        _db.Produtos.Remove(produto);

        await _db.SaveChangesAsync();
    }
}