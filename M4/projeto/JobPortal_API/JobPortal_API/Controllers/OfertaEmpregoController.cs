
using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobPortal_API.Data;
using JobPortal_API.DTOs;
using JobPortal_API.Filters;
using JobPortal_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<List<OfertaEmpregoDTO>> GetOfertaEmprego(string? search, string? regimeTrabalho, string? localidade)
        {
            var oferta = _context.OfertaEmprego
                         .Where(o => o.VagaDisponivel == true)
                         .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                oferta = oferta.Where(b =>
                    b.Titulo.Contains(search) ||
                    b.Jornada.Contains(search) ||
                    b.Localização.Contains(search) ||
                    b.RegimeTrabalho.Contains(search) ||
                    b.TipoContrato.Contains(search) ||
                    b.Requisitos.Contains(search));
            }

            if (!string.IsNullOrEmpty(regimeTrabalho))
            {
                oferta = oferta.Where(b => b.RegimeTrabalho.Contains(regimeTrabalho));
            }

            if (!string.IsNullOrEmpty(localidade))
            {
                oferta = oferta.Where(b => b.Localização.Contains(localidade));
            }

            return await oferta
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

            // mapeamento e o Update ...
            var oferta = _mapper.Map<OfertaEmprego>(ofertaDTO);
            _context.Update(oferta);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Oferta alterada com sucesso!" });
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
