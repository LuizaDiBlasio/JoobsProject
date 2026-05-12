using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teste_cliente.Models
{
    public class Candidato
    {
        [Key]
        public int IdCandidato { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        public string Email { get; set; }

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "O telefone deve conter 9 números.")]
        public int? Telefone { get; set; }

        [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
        [Column(TypeName = "Date")]
        public DateTime? DataNasc { get; set; }

        public string? Morada { get; set; }
        public string? LinkedIn { get; set; }
        public string? Facebook { get; set; }

        public virtual ICollection<AplicacaoTrabalho>? AplicacaoTrabalho { get; set; }
        public virtual ICollection<CV>? CV { get; set; }
        public virtual ICollection<Foto>? Foto { get; set; }
    }
}
