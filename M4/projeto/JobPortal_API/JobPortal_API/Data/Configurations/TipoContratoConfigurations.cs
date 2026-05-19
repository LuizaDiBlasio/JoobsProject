using JobPortal_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPortal_API.Data.Configurations
{
    public class TipoContratoConfigurations : IEntityTypeConfiguration<TipoContrato>
    {
        public void Configure(EntityTypeBuilder<TipoContrato> builder)
        {
            builder.HasKey(tc => tc.IdTipoContrato);
            builder.Property(tc => tc.Tipo).IsRequired().HasMaxLength(150);

            builder.HasData(
               new TipoContrato { IdTipoContrato = 1, Tipo = "Sem Termo"},
               new TipoContrato { IdTipoContrato = 2, Tipo = "A Termo" },
               new TipoContrato { IdTipoContrato = 3, Tipo = "Prestação de Serviços" },
               new TipoContrato { IdTipoContrato = 4, Tipo = "Tempo Parcial" },
               new TipoContrato { IdTipoContrato = 5, Tipo = "Curta Duração" }
                );
        }
    }
}
