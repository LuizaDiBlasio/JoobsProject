using Microsoft.AspNetCore.Mvc.Rendering;
using teste_cliente.Models.Enums;

namespace teste_cliente.Models.ViewModels
{
    public class OfertaEmpregoViewModel
    {
        public int IdOferta { get; set; }

        public int IdEmpresa { get; set; }

        public string Titulo { get; set; }

        public float? Salario { get; set; }

        public ConcelhoEnum Concelho { get; set; }

        public TipoContratoEnum TipoContrato { get; set; }

        public string? Requisitos { get; set; }

        public bool? VagaDisponivel { get; set; }

        public string? Descricao { get; set; }

        public int Contagem { get; set; } = 0;

        public string? LogoEmpresaBase64 { get; set; }

        public JornadaEnum Jornada { get; set; }

        public RegimeTrabalhoEnum RegimeTrabalho { get; set; }

        public List<SelectListItem> SelectListConcelhos { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> SelectListTiposContratos { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> SelectListJornada { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> SelectListRegimeTrabalho { get; set; } = new List<SelectListItem>();
    }
}
