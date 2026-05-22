using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobPortal_API.Data;
using JobPortal_API.DTOs;
using JobPortal_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobPortal_API.Controllers
{
    [Authorize] // <- Adicionado para proteger o controller por padrão
    [ApiController]
    [Route("api/foto")]
    public class FotoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public FotoController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //todos os registros
        [Authorize(Roles = "Admin")]
        [HttpGet("TodasFotos")]
        public async Task<IEnumerable<FotoDTO>> GetFoto()
        {
            return await _context.Foto.ProjectTo<FotoDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }

        //busca por ID do candidato
        [AllowAnonymous]
        [HttpGet("FotoPorId{id}")]
        public async Task<ActionResult<FotoDTO>> GetFoto(int idCandidato)
        {
            if (_context.Foto == null)
            {
                return NotFound();
            }

            var foto = await _context.Foto.ProjectTo<FotoDTO>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(m => m.IdCandidatoFoto == idCandidato);
            
            if (foto == null)
            {
                return NotFound();
            }

            return Ok(foto);
            //return await foto;
        }

        [AllowAnonymous]
        [HttpGet("BuscarFotoPorIdCandidato/{idCandidato}")]
        public async Task<IActionResult> GetFotoPorCandidato(int idCandidato)
        {
            var foto = await _context.Foto
                .FirstOrDefaultAsync(m => m.IdCandidatoFoto == idCandidato);

            if (foto == null || foto.FotoPerfil == null)
            {
                return NotFound();
            }

            return File(foto.FotoPerfil, "image/jpeg");
        }


        //Criar candidato
        [Authorize(Roles = "Candidato,Admin")]
        [HttpPost("CriarFoto")]
        public async Task<ActionResult> PostFoto(FotoDTO fotoDTO)
        {
            // SEGURANÇA: Se for candidato, força o ID do Token para evitar fraudes
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleLogada = identity?.FindFirst(ClaimTypes.Role)?.Value;
            var idLogadoClaim = identity?.FindFirst("IdCandidato")?.Value;

            if (roleLogada == "Candidato" && !string.IsNullOrEmpty(idLogadoClaim))
            {
                // 🛡️ VACINA DO TOKEN DUPLICADO: Isola o "18" antes de converter
                var primeiroId = idLogadoClaim.Split(',')[0];

                if (int.TryParse(primeiroId, out int idCandidatoLogado))
                {
                    fotoDTO.IdCandidatoFoto = idCandidatoLogado;
                }
                else
                {
                    return BadRequest();
                }
            }

            // VALIDAÇÃO DE DUPLICADO: Evita criar duas fotos para o mesmo candidato
            var jaTemFoto = await _context.Foto.AnyAsync(f => f.IdCandidatoFoto == fotoDTO.IdCandidatoFoto);
            if (jaTemFoto)
            {
                return BadRequest();
            }
            var foto = _mapper.Map<Foto>(fotoDTO);
            _context.Add(foto);
            await _context.SaveChangesAsync();
            return Ok();
        }

        //editar candidato
        [Authorize(Roles = "Candidato,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult> PutFoto(int id, FotoDTO fotoDTO)
        {
            var foto = await _context.Foto.FirstOrDefaultAsync(c => c.Id == id);
            if (foto == null)
            {
                return NotFound();
            }

            // SEGURANÇA INTERNA EM LINHA: Tranca para que o candidato só mexa na sua própria foto
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleLogada = identity?.FindFirst(ClaimTypes.Role)?.Value;

            if (roleLogada == "Candidato")
            {
                var idsLogados = identity?.FindAll("IdCandidato").Select(c => c.Value) ?? Enumerable.Empty<string>();
                if (!idsLogados.Contains(foto.IdCandidatoFoto.ToString()))
                {
                    return Forbid(); // 403 Bloqueado se tentar alterar a foto de outro
                }
            }

            // Verificar se o IdCandidatoFoto corresponde
            if (foto.IdCandidatoFoto != fotoDTO.IdCandidatoFoto)
            {
                return BadRequest();
            }

            // 🎯 O TRUQUE CIRÚRGICO: Força o ID da URL de volta no DTO
            // Isto impede o AutoMapper de tentar zerar o ID e quebrar o Entity Framework!
            fotoDTO.Id = id;

            foto = _mapper.Map(fotoDTO, foto);
            await _context.SaveChangesAsync();

            return Ok();
        }

        //delete
        [Authorize(Roles = "Candidato,Admin")]
        [HttpDelete("DeletarFoto/{id:int}")]
        public async Task<ActionResult> DeleteFoto(int id)
        {
            var foto = await _context.Foto.FirstOrDefaultAsync(c => c.IdCandidatoFoto == id);
            if (foto == null)
            {
                return NotFound();
            }

            // SEGURANÇA INTERNA EM LINHA: Impede o Candidato X de apagar a foto do Candidato Y
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleLogada = identity?.FindFirst(ClaimTypes.Role)?.Value;

            if (roleLogada == "Candidato")
            {
                var idsLogados = identity?.FindAll("IdCandidato").Select(c => c.Value) ?? Enumerable.Empty<string>();
                if (!idsLogados.Contains(id.ToString()))
                {
                    return Forbid(); // 403 Forbidden
                }
            }

            _context.Foto.Remove(foto);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [AllowAnonymous] // <- Mantido para garantir que a rota continua pública como no teu original
        [HttpGet("ByCandidato/{idCandidato}")]
        public async Task<ActionResult<FotoDTO>> GetFotoJson(int idCandidato)
        {
            if (_context.Foto == null)
            {
                return NotFound();
            }

            var foto = await _context.Foto
                .ProjectTo<FotoDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(m => m.IdCandidatoFoto == idCandidato);

            if (foto == null)
            {
                return NotFound();
            }

            return Ok(foto);
        }
    }
}
