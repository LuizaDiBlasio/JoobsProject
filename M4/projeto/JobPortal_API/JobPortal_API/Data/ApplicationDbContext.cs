using JobPortal_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobPortal_API.Data
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //__________MODIFICAÇÃO AO CÓDIGO_____________(Roles e SysAdmin criados no SeedDB)

            //modelBuilder.Entity<IdentityRole>().HasData(
            //    new IdentityRole
            //    {
            //        Id = "1", // cuidado: Identity usa string como ID
            //        Name = "Admin",
            //        NormalizedName = "ADMIN"
            //    },
            //    new IdentityRole
            //    {
            //        Id = "2",
            //        Name = "Candidato",
            //        NormalizedName = "CANDIDATO"
            //    },
            //    new IdentityRole
            //    {
            //        Id = "3",
            //        Name = "Empresa",
            //        NormalizedName = "EMPRESA"
            //    }
            //);

            //// Criar o user admin
            //var hasher = new PasswordHasher<ApplicationUser>();

            //var admin = new ApplicationUser
            //{
            //    Id = "10", 
            //    UserName = "admin@admin.com",
            //    NormalizedUserName = "ADMIN@ADMIN.COM",
            //    Email = "admin@admin.com",
            //    NormalizedEmail = "ADMIN@ADMIN.COM",
            //    EmailConfirmed = true,
            //    SecurityStamp = Guid.NewGuid().ToString("D")
            //};

            //// Setar a senha
            //admin.PasswordHash = hasher.HashPassword(admin, "Admin123!"); 

            //modelBuilder.Entity<ApplicationUser>().HasData(admin);

            //// Dar a role de Admin pro user
            //modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            //{
            //    RoleId = "1", // o id da role Admin
            //    UserId = "10" // o id do user admin
            //});

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Empresa) // Uma Review pertence a uma Empresa
                .WithMany() // Uma Empresa pode ter muitas Reviews
                .HasForeignKey(r => r.IdEmpresa); // Chave estrangeira

            modelBuilder.Entity<OfertaEmprego>()
                .Property(o => o.TipoContrato)
                .HasConversion<string>();

            modelBuilder.Entity<CV>()
                .Property(cv => cv.Concelho)
                .HasConversion<string>();

            modelBuilder.Entity<CV>()
                .Property(cv => cv.Escolaridade)
                .HasConversion<string>();

            modelBuilder.Entity<Empresa>()
                .Property(e => e.Concelho)
                .HasConversion<string>();

            modelBuilder.Entity<OfertaEmprego>()
                .Property(o => o.RegimeTrabalho)
                .HasConversion<string>(); // O EF converte o enum para string ao salvar

            modelBuilder.Entity<OfertaEmprego>()
                .Property(o => o.Jornada)
                .HasConversion<string>();

            modelBuilder.Entity<OfertaEmprego>()
               .Property(o => o.Concelho)
               .HasConversion<string>();

            //ALTERAÇÃO ERIKA
            // Mapeamento explícito da FK do Logo da Empresa
            modelBuilder.Entity<LogoEmpresa>()
                .HasOne<Empresa>()  // relação de 1 para muitos**
                .WithMany()
                .HasForeignKey(l => l.IdEmpresaFoto) // Garante que o EF usa esta propriedade como a FK real
                .OnDelete(DeleteBehavior.Restrict);

            // Fazer o mesmo para a Foto do Candidato (ajusta os nomes se na tua classe Candidato for diferente)
            modelBuilder.Entity<Foto>()
                .HasOne<Candidato>()
                .WithMany()
                .HasForeignKey(f => f.IdCandidatoFoto) // Ou o nome da propriedade ID que usas na classe Foto
                .OnDelete(DeleteBehavior.Restrict);
        }

        public virtual DbSet<AplicacaoTrabalho> AplicacaoTrabalho { get; set; }
        public virtual DbSet<Candidato> Candidato { get; set; }
        public virtual DbSet<CV> CV { get; set; }
        public virtual DbSet<Empresa> Empresa { get; set; }
        public virtual DbSet<Foto> Foto { get; set; }
        public virtual DbSet<LogoEmpresa> LogoEmpresa { get; set; }
        public virtual DbSet<OfertaEmprego> OfertaEmprego { get; set; }
        public virtual DbSet<FileCV> FileCV { get; set; }
        public virtual DbSet<Review> Review { get; set; }
        public virtual DbSet<Notifications> Notifications { get; set; }
 

    }
}
