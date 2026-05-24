using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace teste_cliente.Helpers
{
    public static class EnumHelper
    {
        public static List<SelectListItem> ObterSelectListDoEnum<T>() where T : Enum // transforma o Enum numa lista do tipo SelectList
        {
            return Enum.GetValues(typeof(T)) 
                .Cast<T>()
                .Select(e => new SelectListItem
                {
                    Value = Convert.ToInt32(e).ToString(),
                    
                    Text = e.GetType()
                            .GetMember(e.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()?.Name ?? e.ToString()
                })
                .ToList();  
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            if (enumValue == null) return string.Empty;

          
            var membro = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();

            if (membro == null) return enumValue.ToString();

          
            return membro.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? enumValue.ToString();
        }
    }
}
