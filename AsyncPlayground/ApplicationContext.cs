using AsyncPlayground.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncPlayground
{
    internal class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
    {

        public DbSet<Employee> Employees { get; set; }
    }
}
