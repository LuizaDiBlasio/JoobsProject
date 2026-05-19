using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobPortal_API.Data;
using JobPortal_API.DTOs;
using JobPortal_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/aplicacao")]
    public class AplicacaoTrabalhoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public AplicacaoTrabalhoController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;   
        }

        //Busca todos as aplicações
        [Authorize(Roles = "Admin, Empresa")]
        [HttpGet("BuscarTodas")]
        public async Task<IEnumerable<AplicacaoTrabalhoDTO>> GetAplicacaoTrabalho()
        {
            return await _context.AplicacaoTrabalho.ProjectTo<AplicacaoTrabalhoDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }

        //Busca aplicação por ID
        [Authorize(Roles = "Admin, Empresa")]
        [HttpGet("BuscarPorID/{id}")]
        public async Task<ActionResult<AplicacaoTrabalhoDTO>> GetAplicacaoTrabalho(int id)
        {
            if ( _context.AplicacaoTrabalho == null)
            {
                return NotFound();
            }
            var aplicacao = _context.AplicacaoTrabalho.ProjectTo<AplicacaoTrabalhoDTO>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(m => m.IdAplicacao == id);
            if (aplicacao == null)
            {
                return NotFound();
            }

            return await aplicacao;
        }

        //Busca a aplicação por empresa
        [Authorize(Roles = "Admin, Empresa")]
        [HttpGet("BuscarPorIdEmpresa")]
        public async Task<ActionResult<AplicacaoTrabalhoDTO>> GetAplicacaoEmpresa(int idEmpresa)
        {
            if (_context.AplicacaoTrabalho == null)
            {
                return NotFound();
            }
            List<AplicacaoTrabalhoDTO> Listanova = (from a in _context.AplicacaoTrabalho
                                                    join b in _context.OfertaEmprego on a.IdOferta equals b.IdOferta
  
                                                    where b.IdEmpresa == idEmpresa 

                                                select new AplicacaoTrabalhoDTO
                                                {
                                                    IdAplicacao = a.IdAplicacao,
                                                    IdOferta = (int) a.IdOferta,
                                                    IdCandidato = (int)a.IdCandidato,

                                                }).ToList();

            return Ok(Listanova);
        }

        //Busca aplicação de um candidato
        [HttpGet("BuscarPorIdCandidato/{id}")]
        public async Task<ActionResult<IEnumerable<AplicacaoTrabalhoDTO>>> GetAplicacaoCandidato()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (userId == null)
                return Unauthorized();

            // Busca o Id do candidato relacionado a esse userId
            var candidato = await _context.Candidato.FirstOrDefaultAsync(c => c.IdCandidato == userId);

            if (candidato == null)
                return NotFound("Candidato não encontrado.");

            var candidaturas = await _context.AplicacaoTrabalho
                .Where(a => a.IdCandidato == candidato.IdCandidato)
                .ProjectTo<AplicacaoTrabalhoDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(candidaturas);
        }
        /*public async Task<ActionResult<AplicacaoTrabalhoDTO>> GetAplicacaoCandidato(int idCandidato)
        {
            if (_context.AplicacaoTrabalho == null)
            {
                return NotFound();
            }
            List<AplicacaoTrabalhoDTO> Listanova = (from a in _context.AplicacaoTrabalho
                                                    join b in _context.Candidato on a.IdCandidato equals b.IdCandidato

                                                    where a.IdCandidato == idCandidato

                                                    select new AplicacaoTrabalhoDTO
                                                    {

                                                        IdAplicacao = a.IdAplicacao,
                                                        IdOferta = (int)a.IdOferta,
                                                        IdCandidato = (int)a.IdCandidato,

                                                    }).ToList();

            return Ok(Listanova);
        }*/


        //Criar aplicacao
        [Authorize(Roles = "Admin,Candidato")]
        [HttpPost("CriarAplicacao")]
        public async Task<ActionResult> PostAplicacaoTrabalho(AplicacaoTrabalhoDTO aplicacaoDTO)
        {
            var aplicacao = _mapper.Map<AplicacaoTrabalho>(aplicacaoDTO);
            _context.Add(aplicacao);
            await _context.SaveChangesAsync();
            return Ok();
        }

        //Editar aplicação
        [Authorize(Roles = "Admin,Candidato")]
        [HttpPut("EditarAplicacao/{id:int}")]
        public async Task<ActionResult> PutAplicacaoTrabalho(AplicacaoTrabalhoDTO aplicacaoDTO, int id)
        {
            var aplicacao = await _context.AplicacaoTrabalho.FirstOrDefaultAsync(c => c.IdAplicacao == id);
            if (aplicacao == null)
            {
                return NotFound();
            }
            aplicacao = _mapper.Map(aplicacaoDTO, aplicacao);

            await _context.SaveChangesAsync();
            return Ok();
        }

        //Deletar aplicação
        [Authorize(Roles = "Admin,Candidato")]
        [HttpDelete("DeletarAplicacao/{id:int}")]
        public async Task<ActionResult> DeleteAplicacaoTrabalho(int id)
        {
            var aplicacao = await _context.AplicacaoTrabalho.FirstOrDefaultAsync(c => c.IdAplicacao == id);
            if (aplicacao == null)
            {
                return NotFound();
            }
            _context.AplicacaoTrabalho.Remove(aplicacao);
            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpGet("verificar")]
        public async Task<ActionResult<bool>> VerificarCandidatura(int idOferta, int idCandidato)
        {
            var existe = await _context.AplicacaoTrabalho
                .AnyAsync(a => a.IdOferta == idOferta && a.IdCandidato == idCandidato);

            return Ok(existe);
        }

        [Authorize(Roles = "Admin, Candidato, Empresa")]
        [HttpGet("historico-candidato")]
        public async Task<ActionResult<IEnumerable<HistoricoCandidaturaDTO>>> GetHistoricoCandidato(int idCandidato)
        {
            var resultado = from a in _context.AplicacaoTrabalho
                            join o in _context.OfertaEmprego on a.IdOferta equals o.IdOferta
                            join tc in _context.TipoContrato on o.IdTipoContrato equals tc.IdTipoContrato
                            join c in _context.Concelho on o.IdConcelho equals c.IdConcelho
                            join e in _context.Empresa on o.IdEmpresa equals e.IdEmpresa
                            where a.IdCandidato == idCandidato
                            select new HistoricoCandidaturaDTO
                            {
                                IdAplicacao = a.IdAplicacao,
                                DataAplicacao = a.DataAplicacao,
                                AplicacaoAceite = a.aplicacaoAceite,

                                IdOferta = o.IdOferta,
                                Titulo = o.Titulo,
                                NomeConcelho = c.NomeConcelho,
                                RegimeTrabalho = o.RegimeTrabalho,
                                TipoContrato = tc.Tipo,
                                Salario = o.Salario,
                                Jornada = o.Jornada,

                                NomeEmpresa = e.Nome
                            };

            return Ok(await resultado.ToListAsync());
        }

        [Authorize(Roles = "Admin, Empresa")]
        [HttpGet("idOferta")]
        public async Task<ActionResult<IEnumerable<AplicacaoTrabalho>>> GetAplicacaoPorOferta(int idOferta)
        {
            var aplicacoes = await _context.AplicacaoTrabalho
                .Include(a => a.Candidato) // Carrega os dados do candidato
                .Where(a => a.IdOferta == idOferta)
                .ToListAsync();

            return Ok(aplicacoes);
        }
    }
}
