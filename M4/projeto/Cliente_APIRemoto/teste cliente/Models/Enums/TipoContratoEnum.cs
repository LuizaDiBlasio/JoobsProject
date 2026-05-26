using System.ComponentModel.DataAnnotations;

namespace teste_cliente.Models.Enums
{
    public enum TipoContratoEnum
    {
        [Display(Name ="Sem Termo")]
        SemTermo = 1,

        [Display(Name = "A Termo")]
        ATermo = 2,

        [Display(Name = "Prestação de serviço")]
        PrestacaoServico = 3,

        [Display(Name = "Tempo Parcial")]
        TempoParcial = 4,

        [Display(Name = "Curta Duração")]
        CurtaDuracao = 5,

        [Display(Name = "Ato Único")]
        AtoUnico = 6
    }
}
