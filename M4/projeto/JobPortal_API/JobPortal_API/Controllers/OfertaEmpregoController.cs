
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
        public async Task<List<OfertaEmpregoDTO>> GetOfertaEmprego(string? search, string? regimeTrabalho, string? concelho)
        {
            var oferta = _context.OfertaEmprego
                         .Include(o => o.Concelho)
                         .Include(o=> o.TipoContrato)
                         .Where(o => o.VagaDisponivel == true)
                         .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                //oferta = oferta.Where(b =>
                //    b.Titulo.Contains(search) ||
                //    b.IsFullTime == true && "Full time".Contains(search) ||
                //    b.IsFullTime == false && "Part time".Contains(search) ||
                //    b.IsFullTime == null && "Flexível".Contains(search) ||
                //    b.Concelho.NomeConcelho.Contains(search) ||
                //    b.IsPresencial == true && "Presencial".Contains(search) ||
                //    b.IsPresencial == false && "Remoto".Contains(search) ||
                //    b.IsPresencial == null && "Hbrido".Contains(search) ||
                //    b.TipoContrato.Tipo.Contains(search) ||
                //    b.Requisitos.Contains(search));

                oferta = oferta.Where(b =>
                    b.Titulo.Contains(search) ||
                    (b.IsFullTime == true && "Full time".Contains(search)) ||
                    (b.IsFullTime == false && "Part time".Contains(search)) ||
                    (b.IsFullTime == null && "Flexível".Contains(search)) ||
                    b.Concelho.NomeConcelho.Contains(search) ||
                    (b.IsPresencial == true && "Presencial".Contains(search)) ||
                    (b.IsPresencial == false && "Remoto".Contains(search)) ||
                    (b.IsPresencial == null && "Hibrido".Contains(search)) || // Dica: considere usar "Híbrido".Contains(search) se usar acento
                    b.TipoContrato.Tipo.Contains(search) ||
                    b.Requisitos.Contains(search));
            }

            if (!string.IsNullOrEmpty(regimeTrabalho))
            {
                oferta = oferta.Where(b => b.RegimeTrabalho.Contains(regimeTrabalho));
            }

            if (!string.IsNullOrEmpty(concelho))
            {
                oferta = oferta.Where(b => b.Concelho.NomeConcelho.Contains(concelho));
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
            var oferta = await _context.OfertaEmprego.FirstOrDefaultAsync(c => c.IdOferta == id);
            if (oferta == null)
            {
                return NotFound();
            }
            oferta = _mapper.Map(ofertaDTO, oferta);

            await _context.SaveChangesAsync();
            return Ok();
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
