using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobPortal_API.Data;
using JobPortal_API.DTOs;
using JobPortal_API.Filters;
using JobPortal_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/candidato")]
    public class CandidatoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public CandidatoController(ApplicationDbContext context, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }

        //todos os registros
        [Authorize(Roles = "Admin")]
        [HttpGet("BuscarTodos")]
        public async Task<IEnumerable<CandidatoDTO>> GetCandidato()
        {
            return await _context.Candidato.ProjectTo<CandidatoDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }

        //busca por ID
        [Authorize(Roles = "Admin,Candidato")]
        [ServiceFilter(typeof(VerificaCandidatoFilter))]
        [HttpGet("BuscarPorId/{id}")]
        public async Task<ActionResult<CandidatoDTO>> GetCandidato(int id)
        {
            if ( _context.Candidato == null)
            {
                return NotFound();
            }
            var candidato = _context.Candidato.ProjectTo<CandidatoDTO>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(m => m.IdCandidato == id);
            if (candidato == null)
            {
                return NotFound();
            }

            return await candidato;
        }


        /// Retorna os dados de um Candidato pelo email.
        /// Usado pelo MVC para preencher o Nome ao criar reviews.
        [AllowAnonymous] 
        [HttpGet("email/{email}")]
        public async Task<ActionResult<CandidatoDTO>> GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email não pode ser vazio.");

            // procura no banco
            var cand = await _context.Candidato
                              .Where(c => c.Email == email)
                              .ProjectTo<CandidatoDTO>(_mapper.ConfigurationProvider)
                              .FirstOrDefaultAsync();
            if (cand == null)
                return NotFound($"Candidato com email '{email}' não encontrado.");

            return Ok(cand);
        }


        //editar candidato
        [Authorize(Roles = "Admin, Candidato")]
        [ServiceFilter(typeof(VerificaCandidatoFilter))]
        [HttpPut("EditarCandidato/{id:int}")]
        public async Task<ActionResult> PutCandidato(CandidatoDTO candidatoDTO, int id)
        {
            // 1) Carrega o candidato EXISTENTE, incluindo o UserId
            var candidato = await _context.Candidato
                .FirstOrDefaultAsync(c => c.IdCandidato == id);

            if (candidato == null)
                return NotFound();

            var userId = candidato.UserId;            
            var emailAntigo = candidato.Email;        

            // 2) Atualiza os dados do candidato
            _mapper.Map(candidatoDTO, candidato);
            await _context.SaveChangesAsync();        // persiste na tabela Candidato

            // 3) Agora carrega o usuário Identity pelo Id
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                bool changed = false;


                // se email mudou
                if (user.Email != candidato.Email)
                {
                    user.Email = candidato.Email;
                    user.NormalizedEmail = candidato.Email.ToUpperInvariant();
                    changed = true;
                }

                // telefone sempre tratado
                if (candidato.Telefone.HasValue &&
                    user.PhoneNumber != candidato.Telefone.Value.ToString())
                {
                    user.PhoneNumber = candidato.Telefone.Value.ToString();
                    changed = true;
                }

                if (changed)
                {
                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                        return BadRequest(result.Errors);
                }
            }

            return Ok();
        }

        //delete
        [Authorize(Roles = "Admin,Candidato")]
        [ServiceFilter(typeof(VerificaCandidatoFilter))]
        [HttpDelete("DeletarCandidato/{id:int}")]
        public async Task<IActionResult> DeleteCandidato(int id)
        {
            var candidato = await _context.Candidato.FindAsync(id);
            if (candidato == null)
                return NotFound();

            // Deleta dados relacionados
            var aplicacoes = _context.AplicacaoTrabalho.Where(a => a.IdCandidato == id);
            var cvs = _context.CV.Where(c => c.IdCandidatoCv == id);
            var fotos = _context.Foto.Where(f => f.IdCandidatoFoto == id);
            var files = _context.FileCV.Where(f => f.IdCandidatoFile == id);

            _context.RemoveRange(aplicacoes);
            _context.RemoveRange(cvs);
            _context.RemoveRange(fotos);
            _context.RemoveRange(files);

            _context.Candidato.Remove(candidato);

            // Deleta o usuário do Identity
            var user = await _userManager.FindByEmailAsync(candidato.Email);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            await _context.SaveChangesAsync();

            return Ok("Candidato e dados associados deletados com sucesso.");
        }

        [Authorize(Roles = "Candidato,Admin")]
        [ServiceFilter(typeof(VerificaCandidatoFilter))]
        [HttpPost("ChangePassword/{id:int}")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDTO dto)
        {
            if (id != dto.IdCandidato)
                return Forbid();

            // 1) Carrega o candidato para obter o UserId
            var candidato = await _context.Candidato
                                  .FirstOrDefaultAsync(c => c.IdCandidato == dto.IdCandidato);
            if (candidato == null) return NotFound("Candidato não encontrado");

            // 2) Carrega o usuário Identity
            var user = await _userManager.FindByIdAsync(candidato.UserId);
            if (user == null) return NotFound("Usuário Identity não encontrado");

            // 3) Tenta trocar a password
            var result = await _userManager.ChangePasswordAsync(
                             user,
                             dto.CurrentPassword,
                             dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password alterada com sucesso");
        }
    }
}
