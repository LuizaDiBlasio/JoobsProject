using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobPortal_API.Data;
using JobPortal_API.DTOs;
using JobPortal_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Notifications = JobPortal_API.Models.Notifications;

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
        [Authorize(Roles = "Admin, Empresa")]  // ??? O BuscarTodas deve ser exclusivo para Admin. Deixar Empresa ali é uma brecha de segurança.???
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

        // ===================================================================================
        // 1. BUSCA DIRETAMENTE POR ID DA CANDIDATURA
        // ===================================================================================
        // Neste método, o id do parâmetro é o ID da própria Candidatura (Busca Direta).
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
            // Garante que a aplicação não trave se o banco sumir ou se o ID não existir.
            if (aplicacao == null)
            {
                return NotFound(new {mensagem = $"(ID {id}): A aplicação não foi encontrada no sistema." });
            }

            return Ok(aplicacao) ;
        }

        // ===================================================================================
        // 2. BUSCA AS CANDIDATURAS RECEBIDAS POR UMA EMPRESA
        // ===================================================================================        [Authorize(Roles = "Admin, Empresa")]
        // O idEmpresa é passado por parâmetro para filtrar as vagas que pertencem a esta empresa.
        [HttpGet("BuscarPorIdEmpresa")]
        public async Task<ActionResult<AplicacaoTrabalhoDTO>> GetAplicacaoEmpresa(int idEmpresa)
        {
            // Verifica se a "tabela" (o DbSet) no banco de dados está acessível.
            if (_context.AplicacaoTrabalho == null)
            {
                return NotFound(new { mensagem = $"(ID {idEmpresa}): Sem conexão com banco de dados." });
            }

            // Realiza um JOIN explícito entre as candidaturas e as ofertas para descobrir as vagas da empresa
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

       
        [HttpGet("BuscarPorIdCandidato/{id}")]
        public async Task<ActionResult<IEnumerable<AplicacaoTrabalhoDTO>>> GetAplicacaoCandidato(int id)
        {
            // Pega o valor do claim primeiro
            // 1. O Claim de NameIdentifier no Identity é uma STRING (GUID), não um int.
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { mensagem = "Candidato não autenticado." });
            }

            // Busca o Id do candidato relacionado a esse userId
            // Busca o candidato logado para saber QUEM ele é no banco de dados
            var candidatoLogado = await _context.Candidato.FirstOrDefaultAsync(c => c.UserId == userIdString);

            if (candidatoLogado == null)
                return NotFound(new { mensagem = "Perfil do candidato logado não encontrado." });

            // 2. VALIDAÇÃO DE SEGURANÇA (O ponto chave)
            // Se o ID da URL for diferente do ID do candidato no banco, e não for Admin, barramos.
            if (candidatoLogado.IdCandidato != id && !User.IsInRole("Admin"))
            {
                // 403 Forbidden: "Eu sei quem você é, mas você não tem permissão para ver as aplicações do ID {id}"
                return StatusCode(403, new { mensagem = "Acesso negado: Você só pode ver suas próprias candidaturas." });
            }

            // 3. BUSCA REAL
            // Busca usando o DTO de Exibir para o Entity Framework trazer o IdAplicacao do banco
            var candidaturas = await _context.AplicacaoTrabalho
                .Where(a => a.IdCandidato == id)
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
        //ALTERAÇÃO:
        //Trata o erro de FK capturando o ID do candidato logado direto do Token JWT.
        //Evita que o candidato precise enviar o próprio ID no JSON.
        [Authorize(Roles = "Admin,Candidato")]
        [HttpPost("CriarAplicacao")]
        public async Task<ActionResult> PostAplicacaoTrabalho(AplicacaoTrabalhoDTO aplicacaoDTO)
        {
            // 1. Guardar a aplicação na base de dados (o teu código original)
            var aplicacao = _mapper.Map<AplicacaoTrabalho>(aplicacaoDTO);


            if (aplicacao.DataAplicacao == default)
            {
                aplicacao.DataAplicacao = DateTime.UtcNow;
            }

            _context.Add(aplicacao);
            await _context.SaveChangesAsync();

            // SISTEMA DE NOTIFICAÇÕES
            try
            {
                var oferta = await _context.OfertaEmprego
                    .Include(o => o.Empresa)
                    .FirstOrDefaultAsync(o => o.IdOferta == aplicacaoDTO.IdOferta);

                var candidato = await _context.Candidato
                    .FirstOrDefaultAsync(c => c.IdCandidato == aplicacaoDTO.IdCandidato);

                if (oferta != null && oferta.Empresa != null && candidato != null)
                {
                    var notificacao = new Notifications
                    {
                        UserId = oferta.Empresa.UserId,
                        Notification = $"O candidato {candidato.Nome} submeteu uma candidatura à sua oferta: {oferta.Titulo}.",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };

                    _context.Notifications.Add(notificacao);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            { 
                Console.WriteLine($"Erro ao gerar notificação: {ex.Message}");
            }

            return Ok();
        }

        //Editar aplicação
        //ALTERAÇÃO: Garante que o ID do candidato da URL/Token seja mantido e atualizado corretamente.
        [Authorize(Roles = "Admin,Candidato")]
        [HttpPut("EditarAplicacao/{id:int}")]
        public async Task<ActionResult> PutAplicacaoTrabalho(AplicacaoTrabalhoDTO aplicacaoDTO, int id)
        {
            var aplicacao = await _context.AplicacaoTrabalho.FirstOrDefaultAsync(c => c.IdAplicacao == id);
            if (aplicacao == null)
            {
                return NotFound(new { mensagem = "Candidatura não encontrada." });
            }

            // ALTERAÇÃO: Garante que o ID do candidato logado se mantém no mapeamento
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var idCandidatoClaim = identity.FindFirst("IdCandidato")?.Value;
                if (!string.IsNullOrEmpty(idCandidatoClaim))
                {
                    aplicacaoDTO.IdCandidato = int.Parse(idCandidatoClaim);
                }
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
                                NomeConcelho = o.Concelho,
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
