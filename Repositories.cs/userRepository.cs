using Microsoft.EntityFrameworkCore;

public class UserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> BuscarPorEmail(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<bool> EmailExiste(string email)
    {
        return await _context.Users
            .AnyAsync(x => x.Email == email);
    }

    public async Task Criar(User user)
    {
        _context.Users.Add(user);

        await _context.SaveChangesAsync();
    }

    public async Task Deletar(User user)
    {
        _context.Users.Remove(user);

        await _context.SaveChangesAsync();
    }
}