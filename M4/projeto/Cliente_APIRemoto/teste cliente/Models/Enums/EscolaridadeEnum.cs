using System.ComponentModel.DataAnnotations;

namespace teste_cliente.Models.Enums
{
    public enum EscolaridadeEnum
    {
        [Display(Name = "Não especificado")]
        NaoEspecificado = 0,

        [Display(Name = "Ensino básico")]
        EnsinoBasico = 1,

        [Display(Name = "Ensino secundário")]
        EnsinoSecundario = 2,

        Licenciatura = 3,

        Mestrado = 4,

        Doutoramento = 5,

        [Display(Name = "Pós-Doutoramento")]
        PosDoutoramento = 6
    }
}
