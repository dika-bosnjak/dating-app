using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    //DataContext
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options) { }

        public DbSet<AppUser> Users { get; set; }
    }
}