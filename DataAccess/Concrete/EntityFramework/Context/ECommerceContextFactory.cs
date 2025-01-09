using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework.Context
{
    public class ECommerceContextFactory : IDesignTimeDbContextFactory<ECommerceContext>
    {
        public ECommerceContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ECommerceContext>();

            optionsBuilder.UseSqlServer("Server=KAAN;Database=DbECommerceApp;Integrated Security=True;TrustServerCertificate=True");

            return new ECommerceContext(optionsBuilder.Options);
        }
    }
}
