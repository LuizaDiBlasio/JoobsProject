// JobPortal_API/Controllers/FileCVController.cs
using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobPortal_API.Data;
using JobPortal_API.DTOs;
using JobPortal_API.Filters;
using JobPortal_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace JobPortal_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/filecv")]
    public class FileCVController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public FileCVController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET api/filecv/por-candidato/123
        [Authorize(Roles = "Candidato, Admin, Empresa")]
        [HttpGet("por-candidato/{idCandidato}")]
        public async Task<ActionResult<FileCVDTO>> GetFileCv(int idCandidato)
        {
            // ---- ALTERAÇÃO:

            // 1. REGRA DE SEGURANÇA: Um candidato só pode ver o SEU próprio arquivo
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleLogada = identity?.FindFirst(ClaimTypes.Role)?.Value;
            var idLogadoClaim = identity?.FindFirst("IdCandidato")?.Value;

            if (roleLogada == "Candidato")
            {
                if (string.IsNullOrEmpty(idLogadoClaim) || int.Parse(idLogadoClaim) != idCandidato)
                {
                    // Se for um candidato a tentar ver o ID de outro, barrado!
                    // msg padrao genérica personalizada Forbid 403 em Program.cs.
                    return Forbid();
                }
            }

            // 2. BUSCA NO BANCO DE DADOS E MAPEAMENTO PARA O DTO ORIGINAL
            // (Voltamos a usar o teu código original com o _mapper para garantir que o Frontend recebe os dados no formato exato)
            var dto = await _context.FileCV
                .Where(f => f.IdCandidatoFile == idCandidato)
                .ProjectTo<FileCVDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            // 3. SAÍDA PERSONALIZADA PARA O 404
            if (dto == null)
            {
                return NotFound(new
                {
                    sucesso = false,
                    mensagem = $"404: Nenhum currículo foi encontrado para o candidato com o ID {idCandidato}."
                });
            }

            // 4. RETORNO COM SUCESSO (200 OK)
            return Ok(dto);

            // ---- fim da ALTERAÇÃO
        }


        // POST api/filecv
        [Authorize(Roles = "Candidato,Admin")]
        [HttpPost]
        [Consumes("multipart/form-data")] // <- ISTO LIMPA O GRÁFICO CONFUSO DO SWAGGER!
        public async Task<IActionResult> PostFileCv(
            [FromForm, Required(ErrorMessage = "Por favor, selecione um ficheiro de currículo válido (PDF ou Imagem) antes de enviar.")] IFormFile file,
            [FromForm] int idCandidatoFile) 
        {
            // Validação limpa de segurança (ajustada a mensagem para Bad Request padrão)
            // se o ficheiro vier corrompido ou vazio, a API avisa o utilizador de forma limpa em vez de mandar um erro genérico 500.
            if (file == null || file.Length == 0)
                return BadRequest("400: Ficheiro inválido.");

            // PEGAR INFORMAÇÃO DO TOKEN
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var idLogadoClaim = identity?.FindFirst("IdCandidato")?.Value;

            // Variável booleana para saber quem gravou
            bool foiViaToken = !string.IsNullOrEmpty(idLogadoClaim);

            // Definir qual é o ID do candidato que estamos a tentar processar
            int idCandidatoAlvo = foiViaToken ? int.Parse(idLogadoClaim) : idCandidatoFile;

            // ---- NOVA VALIDAÇÃO SIMPLES DE DUPLICADO ----
            // Verifica se já existe algum registo para este candidato na tabela
            var jaTemCv = await _context.FileCV.AnyAsync(f => f.IdCandidatoFile == idCandidatoAlvo);

            if (jaTemCv)
            {
                return BadRequest(new
                {
                    mensagem = $"400: O Candidato {idCandidatoAlvo} já possui um currículo registado. Não é possível adicionar um novo. Utilize o método PUT."
                });
            }            

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);           

            var ent = new FileCV
            {
                File = ms.ToArray(),

                // SE FOR CANDIDATO: Usa o ID do Token (Segurança total).
                // SE FOR ADMIN: A claim é nula, então usa o 'idCandidatoFile' que veio do FromForm!
                IdCandidatoFile = idCandidatoAlvo
            };

            _context.FileCV.Add(ent);
            await _context.SaveChangesAsync();

            // // RETORNO DINÂMICO CONFORME A REALIDADE: MELHORIA NA MENSAGEM DE RETORNO 
            if (foiViaToken)
            {
                return Ok(new { mensagem = $"Ficheiro guardado com sucesso via Token para o Candidato {ent.IdCandidatoFile}!" });
            }
            else
            {
                return Ok(new { mensagem = $"Ficheiro guardado com sucesso via Formulário (Admin) para o Candidato {idCandidatoFile}!" });
            }
        }


        // PUT api/filecv/123
        [Authorize(Roles = "Candidato,Admin")]
        //[ServiceFilter(typeof(VerificaCandidatoFilter))]
        [HttpPut("{idCandidato}")]
        [Consumes("multipart/form-data")] // <- ISTO LIMPA O GRÁFICO CONFUSO DO SWAGGER!
        public async Task<IActionResult> PutFileCv(
            int idCandidato,  // O PUT usa o ID na URL ({idCandidato}) — Parâmetro de Rota (Path): ... api/filecv/18
            [FromForm, Required(ErrorMessage = "Por favor, selecione um ficheiro de currículo válido (PDF ou Imagem) para atualizar.")] IFormFile file)  // <- MANTÉM O [FromForm] AQUI para o front não quebrar!        
        {
            // REGRA DE SEGURANÇA INTERNA (Simples e direta!)
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleLogada = identity?.FindFirst(ClaimTypes.Role)?.Value;

            if (roleLogada == "Candidato")
            {
                // Captura todos os valores de IdCandidato que estão no token (resolve o problema do ID duplicado!)
                var idsLogados = identity?.FindAll("IdCandidato").Select(c => c.Value) ?? Enumerable.Empty<string>();

                // Se o ID da URL (ex: 19) NÃO estiver na lista de IDs do token (ex: ["18", "18"]), bloqueia!
                if (!idsLogados.Contains(idCandidato.ToString()))
                {
                    return Forbid(); // Retorna o 403 Forbidden direto!
                }
            }

            // Validação de segurança do ficheiro: se o ficheiro vier corrompido ou vazio, a API avisa o utilizador de forma limpa em vez de mandar um erro genérico 500.
            if (file == null || file.Length == 0)
                return BadRequest("400: Ficheiro inválido ou vazio.");

            // Procura se o candidato já tem um arquivo salvo
            var ent = await _context.FileCV
                .FirstOrDefaultAsync(f => f.IdCandidatoFile == idCandidato);

            // Se o registo não existir na tabela, manda a mensagem de orientação amigável
            if (ent == null)
            {
                return NotFound(new
                {
                    mensagem = $"O Candidato {idCandidato} ainda não possui um currículo registado na base de dados. Não é possível atualizar um registo inexistente. Por favor, utilize o método POST para realizar o primeiro envio do ficheiro."
                });
            }

            // Processamento seguro do Stream
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            // Atualiza os bytes do ficheiro
            ent.File = ms.ToArray();

            // Persistência
            _context.FileCV.Update(ent);
            await _context.SaveChangesAsync();

            // Retorno amigável
            return Ok(new
            {
                mensagem = $"Ficheiro de currículo do Candidato {idCandidato} atualizado com sucesso!"
            });
        }


        // DELETE api/filecv/123
        [Authorize(Roles = "Candidato,Admin")]
        [ServiceFilter(typeof(VerificaCandidatoFilter))] // Protege também o DELETE contra manipulação de ID
        [HttpDelete("{idCandidato}")]
        public async Task<IActionResult> DeleteFileCv(int idCandidato)
        {
            var ent = await _context.FileCV
                .FirstOrDefaultAsync(f => f.IdCandidatoFile == idCandidato);

            if (ent == null)
            {
                return NotFound(new { mensagem = $"Nenhum currículo encontrado para o Candidato {idCandidato} para ser removido." });
            }
            _context.FileCV.Remove(ent);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = $"Ficheiro de currículo do Candidato {idCandidato} removido com sucesso!" });
        }
    }
}
