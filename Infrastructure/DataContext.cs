using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
    
    public DbSet<UserAccount> UserAccounts { get; set; }
}