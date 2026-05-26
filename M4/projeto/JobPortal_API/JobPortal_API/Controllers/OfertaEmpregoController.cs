
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobPortal_API.Data;
using JobPortal_API.DTOs;
using JobPortal_API.Filters;
using JobPortal_API.Models;
using JobPortal_API.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Linq;
using System.Security.Claims;

namespace JobPortal_API.Controllers
{
    //[Authorize] Não faz sentido implementar o auhtorize pra tudo se as ofertas vão estar disponíveis pra todos
    [ApiController]
    [Route("api/oferta")]
    public class OfertaEmpregoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public OfertaEmpregoController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;   
        }

        //todos os registros
        ////////[HttpGet]
        ////////public async Task<IEnumerable<OfertaEmpregoDTO>> GetOfertaEmprego()
        ////////{
        ////////    return await _context.OfertaEmprego.ProjectTo<OfertaEmpregoDTO>(_mapper.ConfigurationProvider).ToListAsync();
        ////////}

        //Buscas todas as ofertas
        [HttpGet("TodasOfertas")]
        public async Task<List<OfertaEmpregoDTO>> GetOfertaEmprego([FromQuery] string? search,
                                                                   [FromQuery] int? jornada,
                                                                   [FromQuery] int? concelho,
                                                                   [FromQuery] int? regimeTrabalho)
        {
            var query = _context.OfertaEmprego
                           .Where(o => o.VagaDisponivel == true)
                           .AsQueryable();

            // filtro de pesquisa textual/enum
            if (!string.IsNullOrEmpty(search))
            {
                // Tenta converter o termo de pesquisa para cada um dos enums
                bool isJornada = Enum.TryParse<JornadaEnum>(search, ignoreCase: true, out var jornadaEnum);
                bool isRegime = Enum.TryParse<RegimeTrabalhoEnum>(search, ignoreCase: true, out var regimeEnum);
                bool isContrato = Enum.TryParse<TipoContratoEnum>(search, ignoreCase: true, out var contratoEnum);
                bool isConcelho = Enum.TryParse<ConcelhoEnum>(search, ignoreCase: true, out var concelhoEnum);

                // Para evitar erros de tradução no EF Core: avaliação local das flags (ternários)
                query = query.Where(b =>
                    b.Titulo.Contains(search) ||
                    b.Requisitos.Contains(search) ||
                    (b.Descricao != null && b.Descricao.Contains(search)) ||
                    (isJornada ? b.Jornada == jornadaEnum : false) ||
                    (isRegime ? b.RegimeTrabalho == regimeEnum : false) ||
                    (isContrato ? b.TipoContrato == contratoEnum : false) ||
                    (isConcelho ? b.Concelho == concelhoEnum : false)
                );
            }

            // Filtro Específico por Regime de Trabalho (Combobox envia o ID do Enum)
            if (regimeTrabalho.HasValue && regimeTrabalho.Value > 0)
            {
                var regimeEnumSelect = (RegimeTrabalhoEnum)regimeTrabalho.Value;
                query = query.Where(b => b.RegimeTrabalho == regimeEnumSelect);
            }

            // Filtro Específico por Concelho (Combobox envia o ID)
            if (concelho.HasValue && concelho.Value > 0)
            {
                var concelhoEnumSelect = (ConcelhoEnum)concelho.Value;
                query = query.Where(b => b.Concelho == concelhoEnumSelect);
            }

            // Filtro Específico por Jornada (Combobox envia o ID)
            if (jornada.HasValue && jornada.Value > 0)
            {
                var jornadaEnumSelect = (JornadaEnum)jornada.Value;
                query = query.Where(b => b.Jornada == jornadaEnumSelect);
            }

            //  Projeta diretamente para o DTO (Lembra-te de limpar os .ToString() do AutoMapperProfile!)
            return await query
                .ProjectTo<OfertaEmpregoDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

        }


        //Buscar oferta por ID 
        [HttpGet("BuscarPorId/{id:int}")]
        public async Task<ActionResult<OfertaEmpregoDTO>> GetOfertaEmprego(int id)
        {
            if (_context.OfertaEmprego == null)
            {
                return NotFound();
            }
            var oferta = _context.OfertaEmprego.ProjectTo<OfertaEmpregoDTO>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(m => m.IdOferta == id);
            if (oferta == null)
            {
                return NotFound();
            }
            return await oferta;
        }



        //Buscar oferta por ID Empresa *carregar na home*      
        [HttpGet("BucarPorIdEmpresa")]
        public async Task<ActionResult<OfertaEmpregoDTO>> GetOfertaEmpresa(int idEmpresa)
        {
            if (_context.OfertaEmprego == null)
            {
                return NotFound();
            }
            List<OfertaEmpregoDTO> Listanova = (from a in _context.OfertaEmprego
                                                where a.IdEmpresa == idEmpresa
                                                select new OfertaEmpregoDTO
                                                {
                                                    IdOferta = a.IdOferta,
                                                    IdEmpresa = a.IdEmpresa,

                                                }).ToList();

             return Ok(Listanova);
        }

        [Authorize(Roles = "Admin,Empresa")]
        [HttpGet("historicoEmpresa")]
        public async Task<ActionResult<IEnumerable<OfertaEmpregoDTO>>> GetHistoricoEmpresa(int idEmpresa)
        {
            if (_context.OfertaEmprego == null)
            {
                return NotFound();
            }

            var listaOfertas = await _context.OfertaEmprego
                .Where(a => a.IdEmpresa == idEmpresa)
                .ProjectTo<OfertaEmpregoDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(listaOfertas);
        }
        
        //Criar oferta
        [Authorize(Roles = "Admin,Empresa")]
        [HttpPost("CriarOferta")]
        public async Task<ActionResult> PostOfertaEmprego(OfertaEmpregoDTO ofertaDTO)
        {
            // Pega o ID da empresa direto do Token de quem está logado.
            // ALTERAÇÃO: Captura o ID da empresa logada diretamente das Claims do Token JWT.
            // Isso evita erros de Foreign Key (FK) caso o ID venha zerado ou incorreto do Swagger/Client,
            // garantindo que a oferta seja sempre vinculada à empresa autenticada.
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var idEmpresaClaim = identity.FindFirst("IdEmpresa")?.Value;
                if (!string.IsNullOrEmpty(idEmpresaClaim))
                {
                    ofertaDTO.IdEmpresa = int.Parse(idEmpresaClaim);
                }
            }

            var oferta = _mapper.Map<OfertaEmprego>(ofertaDTO);

            _context.Add(oferta);
            await _context.SaveChangesAsync();
            return Ok();
        }

        //Edit/Update
        [Authorize(Roles = "Admin,Empresa")]
        [ServiceFilter(typeof(VerificaOfertaDeEmpresaFilter))]
        [HttpPut("EditarOferta/{id:int}")]
        public async Task<ActionResult> PutOfertaEmprego(OfertaEmpregoDTO ofertaDTO, int id)
        {
            // ALTERAÇÃO: Força o ID do objeto a ser o mesmo da URL (ignora o que veio no JSON)
            ofertaDTO.IdOferta = id;

            // ALTERAÇÃO: Garante que a vaga continua vinculada à empresa dona que está logada
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var idEmpresaClaim = identity.FindFirst("IdEmpresa")?.Value;
                if (!string.IsNullOrEmpty(idEmpresaClaim))
                {
                    ofertaDTO.IdEmpresa = int.Parse(idEmpresaClaim);
                }
            }

            // 1) Carrega a oferta existente para rastreamento (tracking) do EF
            var ofertaNoBanco = await _context.OfertaEmprego.FirstOrDefaultAsync(c => c.IdOferta == id);
            if (ofertaNoBanco == null)
            {
                return NotFound();
            }

            // 2) Mapeia as alterações do DTO por cima do objeto rastreado
            _mapper.Map(ofertaDTO, ofertaNoBanco);
            await _context.SaveChangesAsync();

            return Ok(); // Retorno limpo padrão da main
        }

        [Authorize(Roles = "Admin,Empresa")]
        [HttpGet("EditarOferta/{id:int}")]
        public async Task<ActionResult<OfertaEmpregoDTO>> GetOfertaParaEditar(int id)
        {
            var oferta = await _context.OfertaEmprego.FindAsync(id);
            if (oferta == null)
            {
                return NotFound();
            }
            var ofertaDTO = _mapper.Map<OfertaEmpregoDTO>(oferta);
            return Ok(ofertaDTO);
        }


        //Deletar oferta
        [Authorize(Roles = "Admin,Empresa")]
        [ServiceFilter(typeof(VerificaOfertaDeEmpresaFilter))]
        [HttpDelete("DeletarOferta/{id:int}")]
        public async Task<ActionResult> DeleteOfertaEmprego(int id)
        {
            var oferta = await _context.OfertaEmprego.FirstOrDefaultAsync(c => c.IdOferta == id);
            if (oferta == null)
            {
                return NotFound();
            }
            _context.OfertaEmprego.Remove(oferta);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPatch("{id:int}/incrementarContagem")]
        public async Task<ActionResult> IncrementarContagem(int id)
        {
            var oferta = await _context.OfertaEmprego.FirstOrDefaultAsync(c => c.IdOferta == id);
            if (oferta == null)
            {
                return NotFound();
            }

            oferta.Contagem += 1;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
