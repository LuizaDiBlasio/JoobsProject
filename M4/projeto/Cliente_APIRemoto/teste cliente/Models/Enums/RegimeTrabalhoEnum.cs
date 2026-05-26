using System.ComponentModel.DataAnnotations;

namespace teste_cliente.Models.Enums
{
    public enum RegimeTrabalhoEnum
    {

        Presencial = 1,

        Remoto = 2,

        [Display(Name = "Híbrido")]
        Hibrido = 3
    }
}
