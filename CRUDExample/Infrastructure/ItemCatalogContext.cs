using CRUDExample.Model;

namespace CRUDExample.Infrastructure
{
    public class ItemCatalogContext : DbContext
    {
        public ItemCatalogContext(DbContextOptions<ItemCatalogContext> options)
            : base(options)
        { 
        }

        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<ItemGroup> ItemGroup { get; set; } = null!;
    }
}
