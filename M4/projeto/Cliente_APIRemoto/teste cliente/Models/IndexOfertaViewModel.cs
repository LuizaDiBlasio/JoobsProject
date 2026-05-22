using teste_cliente.Models.Dto;
using teste_cliente.Models.Enums;

namespace teste_cliente.Models
{
    public class IndexOfertaViewModel
    {
        public IEnumerable<OfertaEmpregoDTO> OfertaEmpregosList { get; set; }

        public JornadaEnum? Jornada { get; set; }

        public RegimeTrabalhoEnum? RegimeTrabalho { get; set; }

        public int? IdConcelho {get; set; }
    }
}
