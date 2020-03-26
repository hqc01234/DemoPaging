using DemoPaging.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemoPaging.DBContext
{
    public class DemoDbContext : DbContext
    {
        public DbSet<Item> Items { get; set; }

        public DemoDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}