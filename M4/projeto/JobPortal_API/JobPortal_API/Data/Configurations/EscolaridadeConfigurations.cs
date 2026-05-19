using JobPortal_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPortal_API.Data.Configurations
{
    public class EscolaridadeConfigurations : IEntityTypeConfiguration<Escolaridade>
    {
        public void Configure(EntityTypeBuilder<Escolaridade> builder)
        {
            builder.HasKey(e => e.IdEscolaridade);
            builder.Property(e => e.Tipo).IsRequired().HasMaxLength(150);

            builder.HasData(
                new Escolaridade { IdEscolaridade = 1, Tipo = "Ensino Básico"},
                new Escolaridade { IdEscolaridade = 2, Tipo = "Ensino Secundário" },
                new Escolaridade { IdEscolaridade = 3, Tipo = "Licenciatura" },
                new Escolaridade { IdEscolaridade = 4, Tipo = "Mestrado" },
                new Escolaridade { IdEscolaridade = 5, Tipo = "Doutoramento" }
                );
        }
    }
}
