using Azure.Core;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
    
    public DbSet<UserAccount> UserAccounts { get; set; }
    
    public DbSet<Device> Devices { get; set; }
    
    public DbSet<AccessLevel> AccessLevels { get; set; }
    
    public DbSet<Card> Cards { get; set; }
    
    public DbSet<FingerPrint> FingerPrints { get; set; }
    
    public DbSet<IdentifiedAssignDevice> IdentifiedAssignDevices { get; set; }
    
    public DbSet<IdentifiedAssignCard> IdentifiedAssignCards { get; set; }
    
    public DbSet<PersonImage> PeopleImages { get; set; }
    
    public DbSet<Person> Persons { get; set; }
}