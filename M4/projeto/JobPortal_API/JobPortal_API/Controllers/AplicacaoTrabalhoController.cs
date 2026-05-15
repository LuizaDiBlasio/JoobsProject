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
        public async Task<ActionResult<IEnumerable<AplicacaoTrabalhoDTO>>> GetAplicacaoTrabalho()
        {
            // Bloqueio de segurança: apenas Admin pode listar TUDO do sistema
            // Verifica manualmente se não é Admin
            if (!User.IsInRole("Admin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    mensagem = "Acesso negado."
                });
            }

            var resultado = await _context.AplicacaoTrabalho
                                .ProjectTo<AplicacaoTrabalhoDTO>(_mapper.ConfigurationProvider)
                                .ToListAsync();

            return Ok(resultado);
        }

        //Busca aplicação por ID
        [Authorize(Roles = "Admin, Empresa")]
        [HttpGet("BuscarPorID/{id}")]
        public async Task<ActionResult<AplicacaoTrabalhoDTO>> GetAplicacaoTrabalho(int id)
        {
            // Verifica se a "tabela" (o DbSet) no banco de dados está acessível.
            if ( _context.AplicacaoTrabalho == null)
            {
                return NotFound(new {mensagem = $"(ID {id}): Sem conexão com banco de dados." });
            }

            // Busca com AWAIT para obter o dado real.
            var aplicacao = await _context.AplicacaoTrabalho
                .ProjectTo<AplicacaoTrabalhoDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(m => m.IdAplicacao == id);

            // Verifica se a busca que você fez encontrou algum resultado. 
            // garante que a aplicação não trave se o banco sumir ou se o ID não existir.
            if (aplicacao == null)
            {
                return NotFound(new {mensagem = $"(ID {id}): A aplicação não foi encontrada no sistema." });
            }

            return Ok(aplicacao) ;
        }

        //Busca a aplicação por empresa
        [Authorize(Roles = "Admin, Empresa")]
        [HttpGet("BuscarPorIdEmpresa")]
        public async Task<ActionResult<AplicacaoTrabalhoDTO>> GetAplicacaoEmpresa(int idEmpresa)
        {
            // Verifica se a "tabela" (o DbSet) no banco de dados está acessível.
            if (_context.AplicacaoTrabalho == null)
            {
                return NotFound(new { mensagem = $"(ID {idEmpresa}): Sem conexão com banco de dados." });
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
        public async Task<ActionResult<IEnumerable<AplicacaoTrabalhoDTO>>> GetAplicacaoCandidato(int id)
        {
            // Pega o valor do claim primeiro
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verifica se ele existe e se é um número válido antes de dar Parse
            // O 'out int userId' já cria a variável e tenta colocar o valor nela
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { mensagem = "Acesso negado." });
            }     

            // Busca o Id do candidato relacionado a esse userId
            var candidato = await _context.Candidato.FirstOrDefaultAsync(c => c.IdCandidato == userId);

            if (candidato == null)
                return NotFound(new { mensagem = "Candidato não encontrado."});

            // Busca as candidaturas usando o ID que veio da URL (mantendo a compatibilidade com o Front)
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
                return NotFound(new { mensagem = "Acesso negado." });
            }
            aplicacao = _mapper.Map(aplicacaoDTO, aplicacao);

            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Dados da aplicação alterados com sucesso!" });
        }

        //Deletar aplicação
        [Authorize(Roles = "Admin,Candidato")]
        [HttpDelete("DeletarAplicacao/{id:int}")]
        public async Task<ActionResult> DeleteAplicacaoTrabalho(int id)
        {
            var aplicacao = await _context.AplicacaoTrabalho.FirstOrDefaultAsync(c => c.IdAplicacao == id);
            if (aplicacao == null)
            {
                return NotFound(new { mensagem = "Acesso negado."});
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
                            join e in _context.Empresa on o.IdEmpresa equals e.IdEmpresa
                            where a.IdCandidato == idCandidato
                            select new HistoricoCandidaturaDTO
                            {
                                IdAplicacao = a.IdAplicacao,
                                DataAplicacao = a.DataAplicacao,
                                AplicacaoAceite = a.aplicacaoAceite,

                                IdOferta = o.IdOferta,
                                Titulo = o.Titulo,
                                Localizacao = o.Localização,
                                RegimeTrabalho = o.RegimeTrabalho,
                                TipoContrato = o.TipoContrato,
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
