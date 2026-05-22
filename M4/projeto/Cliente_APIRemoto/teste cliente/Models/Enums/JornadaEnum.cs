using System.ComponentModel.DataAnnotations;

namespace teste_cliente.Models.Enums
{
    public enum JornadaEnum
    {

        [Display(Name = "Full time")]
        FullTime = 1,

        [Display(Name = "Part time")]
        PartTime = 2,

        [Display(Name = "Flexível")]
        Flexivel = 3
    }
}
