using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace RoleBasedAuthenticationUsingIdentity.Models
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options)
        : base(options)
        {

        }

        public DbSet<User> User { get; set; } = default!;
        public DbSet<Product> Product { get; set; } = default!;
        public DbSet<ProductTables> ProductTables { get; set; } = default!;


    }
}
