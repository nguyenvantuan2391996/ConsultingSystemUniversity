using ConsultingSystemUniversity.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingSystemUniversity.Data
{
    public class ConsultingSystemUniversityContext : DbContext
    {
        public ConsultingSystemUniversityContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Account>().ToTable("account");
            builder.Entity<JwtToken>().ToTable("token");
            builder.Entity<Language>().ToTable("language");
            builder.Entity<University>().ToTable("university");
            builder.Entity<MajorsGroup>().ToTable("majors_group");
            builder.Entity<Majors>().ToTable("majors");
            builder.Entity<Feedback>().ToTable("feedback");
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<JwtToken> JwtTokens { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<University> Universites { get; set; }
        public DbSet<MajorsGroup> MajorsGroups { get; set; }
        public DbSet<Majors> Majors { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
    }
}
