public class User
{
    public int Id { get; set; } // chave primária (ID)
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}