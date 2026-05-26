using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal_API.Models
{
    public class FileCV
    {
        [Key]
        public int  IdFile { get; set; }
        public byte[] File { get; set; }

        [ForeignKey("IdCandidatoFile")]
        public int IdCandidatoFile { get; set; }
        
        public virtual Candidato Candidato { get; set; }
    }
}
