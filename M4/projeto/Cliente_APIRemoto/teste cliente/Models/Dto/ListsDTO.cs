using Microsoft.AspNetCore.Mvc.Rendering;

namespace teste_cliente.Models.Dto
{
    public class ListsDTO
    {
        public List<SelectListItem> SelectListConcelhos { get; set; }

        public List<SelectListItem> SelectListJornada { get; set; }

        public List<SelectListItem> SelectListRegimeTrabalho { get; set; }

        public List<SelectListItem> SelectListContratos { get; set; }
    }
}
