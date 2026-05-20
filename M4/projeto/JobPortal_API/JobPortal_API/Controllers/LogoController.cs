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
    //[Authorize] // <- Adicionado para proteger os endpoints por padrão
    [ApiController]
    [Route("api/logo")]
    public class LogoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public LogoController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //todos os registros
        [HttpGet]
        public async Task<IEnumerable<LogoEmpresaDTO>> GetLogo()
        {
            return await _context.LogoEmpresa.ProjectTo<LogoEmpresaDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }

        //busca por ID da Empresa
        [AllowAnonymous]
        [HttpGet("{idEmpresa}")]
        public async Task<ActionResult<LogoEmpresaDTO>> GetLogo(int idEmpresa)
        {
            if (_context.LogoEmpresa == null)
            {
                return NotFound();
            }
            var logo = await _context.LogoEmpresa.ProjectTo<LogoEmpresaDTO>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(m => m.IdEmpresaFoto == idEmpresa);
            if (logo == null)
            {
                return NotFound();
            }

            return Ok(logo);
        }
        
        //Criar logo
        [HttpPost]
        public async Task<ActionResult> PostLogo(LogoEmpresaDTO logoDTO)
        {
            // ALTERAÇÃO: SEGURANÇA: Injeta o ID da Empresa logada se for o perfil Empresa
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null && identity.FindFirst(ClaimTypes.Role)?.Value == "Empresa")
            {
                var idEmpresaClaim = identity.FindFirst("IdEmpresa")?.Value;
                if (!string.IsNullOrEmpty(idEmpresaClaim))
                {
                    logoDTO.IdEmpresaFoto = int.Parse(idEmpresaClaim);
                }
            }

            var logo = _mapper.Map<LogoEmpresa>(logoDTO);
            _context.Add(logo);
            await _context.SaveChangesAsync();
            return Ok();
        }

        //Edit/Update pelo id da empresa
        [HttpPut("{id:int}")]
        public async Task<ActionResult> PutLogo(LogoEmpresaDTO logoDTO, int id)
        {
            var logo = await _context.LogoEmpresa.FirstOrDefaultAsync(c => c.IdEmpresaFoto == id);
            if (logo == null)
            {
                return NotFound();
            }

            // ALTERAÇÃO: SEGURANÇA: Impede que uma empresa edite o logo de outra
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null && identity.FindFirst(ClaimTypes.Role)?.Value == "Empresa")
            {
                var idEmpresaClaim = identity.FindFirst("IdEmpresa")?.Value;
                if (!string.IsNullOrEmpty(idEmpresaClaim) && int.Parse(idEmpresaClaim) != logo.IdEmpresaFoto)
                {
                    return Forbid();
                }
            }

            _mapper.Map(logoDTO, logo);
            await _context.SaveChangesAsync();
            return Ok();
        }

        //delete
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteLogo(int id)
        {
            var logo = await _context.LogoEmpresa.FirstOrDefaultAsync(c => c.IdEmpresaFoto == id);
            if (logo == null)
            {
                return NotFound();
            }
            _context.LogoEmpresa.Remove(logo);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // Novo endpoint para criar ou atualizar o logo da empresa
        [HttpPost("Update")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> CreateOrUpdateLogo([FromForm] int IdEmpresaFoto, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest();
            }

            // ALTERAÇÃO:SEGURANÇA: Força o ID do token se for perfil Empresa
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null && identity.FindFirst(ClaimTypes.Role)?.Value == "Empresa")
            {
                var idEmpresaClaim = identity.FindFirst("IdEmpresa")?.Value;
                if (!string.IsNullOrEmpty(idEmpresaClaim))
                {
                    IdEmpresaFoto = int.Parse(idEmpresaClaim);
                }
            }

            byte[] logoBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                logoBytes = ms.ToArray();
            }

            // Tenta encontrar um logo já existente para a empresa com base no IdEmpresaFoto
            var existingLogo = await _context.LogoEmpresa.FirstOrDefaultAsync(l => l.IdEmpresaFoto == IdEmpresaFoto);
            if (existingLogo != null)
            {
                existingLogo.Logo = logoBytes;
                await _context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                var newLogo = new LogoEmpresa
                {
                    IdEmpresaFoto = IdEmpresaFoto,
                    Logo = logoBytes
                };
                _context.LogoEmpresa.Add(newLogo);
                await _context.SaveChangesAsync();
                return Ok();
            }
        }

        [HttpGet("empresa/{idEmpresa}")]
        public async Task<IActionResult> GetLogoFile(int idEmpresa)
        {
            // Procura o logo na base
            var logo = await _context.LogoEmpresa.FirstOrDefaultAsync(l => l.IdEmpresaFoto == idEmpresa);
            if (logo == null || logo.Logo == null)
            {
                return NotFound();
            }
            // Aqui assumimos que o logo é uma imagem JPEG.
            // Se for outro formato, ajusta o content-type conforme necessário.
            return File(logo.Logo, "image/jpeg");
        }
    }
}