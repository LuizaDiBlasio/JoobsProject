namespace JobPortal_API.Utilities;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

public class SwaggerFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var parameters = context.MethodInfo.GetParameters();

        // Verifica se o método usa IFormFile
        var hasFormFile = parameters.Any(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFile[]));
        if (!hasFormFile) return;

        // Inicializa o corpo da requisição se não existir
        operation.RequestBody ??= new OpenApiRequestBody();
        operation.RequestBody.Content["multipart/form-data"] = new OpenApiMediaType
        {
            Schema = new OpenApiSchema
            {
                Type = "object",
                Properties = parameters
                    .Where(p => p.GetCustomAttributes(typeof(FromFormAttribute), true).Any() || p.ParameterType == typeof(IFormFile))
                    .ToDictionary(
                        p => p.Name,
                        p => {
                            if (p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFile[]))
                            {
                                return new OpenApiSchema { Type = "string", Format = "binary" };
                            }

                            // GERAR SCHEMA NORMAL DO INTEIRO
                            var schema = context.SchemaGenerator.GenerateSchema(p.ParameterType, context.SchemaRepository);

                            // ---- ADICIONADO AQUI: Se for o campo do ID, injeta o 0 por padrão no Swagger ----
                            if (p.Name == "idCandidatoFile")
                            {
                                schema.Default = new Microsoft.OpenApi.Any.OpenApiInteger(0);
                                schema.Description = "ID do Candidato. Obrigatório para Admin. Candidatos logados podem manter o 0 (o sistema usará o Token).";
                            }

                            return schema;
                        }





                        //p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFile[])
                        //    ? new OpenApiSchema { Type = "string", Format = "binary" } // O ficheiro fica como upload limpo
                        //    : context.SchemaGenerator.GenerateSchema(p.ParameterType, context.SchemaRepository) // O ID aparece normal!
                    )
            }
        };
    }
}