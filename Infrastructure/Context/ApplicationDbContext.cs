using AzureBlobDemo.Domain.Common;
using AzureBlobDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AzureBlobDemo.Infrastructure.Context
{
	public class ApplicationDbContext : DbContext
	{
        public ApplicationDbContext(DbContextOptions opt) : base(opt)
        {
            
        }

        public DbSet<Item> Items { get; set; }

        public virtual async Task<int> SaveChangesAsync()
        {
            foreach(var entity in base.ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
				entity.Entity.UpdatedDate = DateTime.Now;
				if (entity.State == EntityState.Added)
				{
					entity.Entity.CreatedDate = DateTime.Now;
				}
			}
            return await base.SaveChangesAsync();
        }
    }
}
