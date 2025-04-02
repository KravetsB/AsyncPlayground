using AsyncPlayground.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsyncPlayground
{
	internal class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
    {
        public DbSet<Employee> Employees { get; set; }
    }
}
