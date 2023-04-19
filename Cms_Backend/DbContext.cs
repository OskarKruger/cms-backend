using Cms_Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace Cms_Backend
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Users { get; set; }

        public DbSet<Cms_Backend.Model.Customer>? Customer { get; set; }

    }
}
