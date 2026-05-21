using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace teste_cliente.Helpers
{
    public static class SelectListConverter
    {
        public static List<SelectListItem> ObterSelectList<T>(
        IEnumerable<T> listaOriginal,
        Func<T, object> mapearValue,
        Func<T, object> mapearText)
        {
            if (listaOriginal == null)
                return new List<SelectListItem>();

            return listaOriginal.Select(item => new SelectListItem
            {
                Value = mapearValue(item)?.ToString(),

                Text = mapearText(item)?.ToString()
            })
            .ToList();
        }
    }
}
