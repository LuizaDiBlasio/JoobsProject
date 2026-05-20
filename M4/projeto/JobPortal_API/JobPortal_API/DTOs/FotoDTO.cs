using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;

namespace JobPortal_API.DTOs
{
    public class FotoDTO
    {
        [JsonIgnore] // <- Faz o campo SUMIR do Swagger no POST e PUT (o JSON fica limpo!)
        [BindNever]  // <- Garante que o .NET ignora este campo se alguém tentar forçar no JSON
        public int Id { get; set; }
        public int IdCandidatoFoto { get; set; }
        public byte[] FotoPerfil { get; set; }
    }
}
