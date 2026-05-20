using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JobPortal_API.DTOs
{
    public class CandidatoDTO
    {
        [JsonIgnore] // <--- Isto faz o ID desaparecer do JSON do Swagger!
        public int IdCandidato { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public int? Telefone { get; set; }
        public string? Morada { get; set; }
        [Column(TypeName = "Date")]
        public DateTime? DataNasc { get; set; }
        public string? LinkedIn { get; set; }
        public string? Facebook { get; set; }

        //public int LocalUserId { get; set; } = 0;
    }
}
