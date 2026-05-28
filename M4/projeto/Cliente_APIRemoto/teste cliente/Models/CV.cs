using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using teste_cliente.Models.Enums;

namespace teste_cliente.Models
{
    public class CV
    {
        [Key]
        public int IdCV { get; set; }
        public string Nome { get; set; }
        public ConcelhoEnum Concelho { get; set; }
        public EscolaridadeEnum Escolaridade { get; set; }
        public string? ExpProfissional { get; set; }
        public string? Competencias { get; set; }
        public string? Interesses { get; set; }
        public int IdCandidatoCv { get; set; }

        [ForeignKey("IdCandidatoCv")]
        public virtual Candidato? IdCandidato { get; set; }
        public byte[]? FotoPerfil { get; set; }
    }
}
