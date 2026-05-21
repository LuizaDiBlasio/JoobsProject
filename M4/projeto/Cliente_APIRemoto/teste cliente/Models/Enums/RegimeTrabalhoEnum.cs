using System.ComponentModel.DataAnnotations;

namespace teste_cliente.Models.Enums
{
    public enum RegimeTrabalhoEnum
    {
        [Display(Name = "Não especificado")]
        Nenhum = 0,

        Presencial = 1,
        Remoto = 2,

        [Display(Name = "Híbrido")]
        Hibrido = 3
    }
}
