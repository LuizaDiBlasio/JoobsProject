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
    [Authorize]
    [Route("api/cv")]
    [ApiController]
    public class CVController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CVController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET api/cv
        [Authorize(Roles = "Admin")] // ALTERAÇÃO: Apenas Admin deve listar todos os CVs do sistema
        [HttpGet]
        public async Task<IEnumerable<CVDTO>> GetAll()
        {
            return await _context.CV
                .ProjectTo<CVDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        // GET api/cv/idCandidato?idCandidato=123
        [HttpGet("idCandidato")]
        public async Task<ActionResult<CVDTO>> GetByCandidato([FromQuery] int idCandidato)
        {
            // add 2/6
            // Se for Administrador ou Empresa, ignoram totalmente a validação de propriedade do ID e avançam
            if (User.IsInRole("Admin") || User.IsInRole("Empresa"))
            {
                // Salta direto para a busca do banco
                return await BuscarCVNoBanco(idCandidato);
            }
            //-----------------------------------


            // ALTERAÇÃO: Validação de Segurança: Candidato só vê o próprio CV (Admin ignora o bloqueio)
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var idCandidatoClaim = identity?.FindFirst("IdCandidato")?.Value;

            if (string.IsNullOrEmpty(idCandidatoClaim)) return Unauthorized();

            int idCandidatoLogado = int.Parse(idCandidatoClaim);

            if (idCandidatoLogado != idCandidato)
            {
                return Forbid();
            }
            // Fim da ALTERAÇÃO

            //var cv = await _context.CV
            //    .Where(c => c.IdCandidatoCv == idCandidato)
            //    .ProjectTo<CVDTO>(_mapper.ConfigurationProvider)
            //    .FirstOrDefaultAsync();

            //if (cv == null) return NotFound();

            //return Ok(cv);  

            // add 2/6
            return await BuscarCVNoBanco(idCandidato);

        }


        // add 02/06
        // Função auxiliar para evitar código repetido na busca
        private async Task<ActionResult<CVDTO>> BuscarCVNoBanco(int idCandidato)
        {
            var cv = await _context.CV
                .Where(c => c.IdCandidatoCv == idCandidato)
                .ProjectTo<CVDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (cv == null) return NotFound();

            return Ok(cv);
        }


        // POST api/cv
        [Authorize(Roles = "Admin, Candidato")]
        [HttpPost]
        public async Task<ActionResult<CVDTO>> PostCv([FromBody] CVDTO cvDTO)
        {
            // ALTERAÇÃO: Validação de Segurança: Candidato só pode criar o próprio CV (Admin ignora o bloqueio)
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var idCandidatoClaim = identity?.FindFirst("IdCandidato")?.Value;

            if (string.IsNullOrEmpty(idCandidatoClaim))
            {
                return Unauthorized();
            }

            // 🛡️ CORREÇÃO CIRÚRGICA: Isola o primeiro valor caso o Token venha duplicado ("18,18")
            var primeiroId = idCandidatoClaim.Split(',')[0];

            if (!int.TryParse(primeiroId, out int idCandidatoLogado))
            {
                return BadRequest();
            }

            // Garante que o CV criado pertence ao Candidato Logado
            cvDTO.IdCandidatoCv = idCandidatoLogado;

            // Verifica se o candidato já possui um CV cadastrado (regra de 1 por conta, se aplicável)
            var jaExiste = await _context.CV.AnyAsync(c => c.IdCandidatoCv == idCandidatoLogado);
            if (jaExiste)
            {
                return BadRequest();
            }

            var cv = _mapper.Map<CV>(cvDTO);
            _context.CV.Add(cv);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<CVDTO>(cv);
            return CreatedAtAction(nameof(GetByCandidato),
                                   new { idCandidato = cv.IdCandidatoCv },
                                   resultDto);
        }

        // PUT api/cv/5
        [Authorize(Roles = "Admin, Candidato")]
        [HttpPut("{id}")]
        public async Task<ActionResult<CVDTO>> PutCv(int id, [FromBody] CVDTO cvDTO)
        {
            if (id != cvDTO.IdCV) return BadRequest();

            // 1. Busca o currículo alvo no banco
            var cvNoBanco = await _context.CV.FindAsync(id);
            if (cvNoBanco == null) return NotFound();

            // 2. Captura a identidade do Token
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var idCandidatoClaim = identity?.FindFirst("IdCandidato")?.Value;

            if (string.IsNullOrEmpty(idCandidatoClaim)) return Unauthorized();
            int idCandidatoLogado = int.Parse(idCandidatoClaim);

            // 3. SEGURANÇA CHAVE: O currículo pertence ao candidato logado?
            if (cvNoBanco.IdCandidatoCv != idCandidatoLogado && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Garante consistência de IDs antes do mapeamento
            cvDTO.IdCV = id;
            cvDTO.IdCandidatoCv = cvNoBanco.IdCandidatoCv ?? idCandidatoLogado; 
           
            _mapper.Map(cvDTO, cvNoBanco);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<CVDTO>(cvNoBanco));
        }
    }
}
