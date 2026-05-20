using System.Text.Json.Serialization;

namespace JobPortal_API.DTOs
{
    public class AplicacaoTrabalhoExibirDTO
    {
        public int IdAplicacao { get; set; }
        public int IdOferta { get; set; }  
        public DateTime DataAplicacao { get; set; } = DateTime.Now;
 
    }
}
