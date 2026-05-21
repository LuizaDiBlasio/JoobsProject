using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobPortal_API.Models.Enums;

namespace JobPortal_API.Models
{
    public class CV
    {
        [Key]
        public int IdCV { get; set; }

        public string Nome { get; set; }

        [ForeignKey("Concelho")]
        public int IdConcelho { get; set; }   

        public Concelho Concelho { get; set; }

        //public string Localizacao { get; set; } - Normalização da tabela

        // public string? Educacao { get; set; } - Enum

        public EscolaridadeEnum Escolaridade { get; set; }

        public string? ExpProfissional { get; set; }

        public string? Competencias { get; set; }

        public string? Interesses { get; set; }

        public int? IdCandidatoCv { get; set; }


        [ForeignKey("IdCandidatoCv")]
        public virtual Candidato IdCandidato { get; set; }

    }
}
