using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<MedicalFile> MedicalFiles { get; set; }

    }
}
