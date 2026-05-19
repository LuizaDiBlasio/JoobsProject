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
    [Route("api/LookUps")]
    [ApiController]
    public class LookUpsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LookUpsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("Concelhos")]
        public async Task<IEnumerable<ConcelhoDTO>> GetAllConcelhos()
        {
            return await  _context.Concelho
                    .OrderBy(c => c.NomeConcelho)
                    .ProjectTo<ConcelhoDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync();
        }

        [HttpGet("TiposContratos")]
        public async Task<IEnumerable<TipoContratoDTO>> GetAllTiposContratos()
        {
            return await _context.TipoContrato
                    .ProjectTo<TipoContratoDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync();
        }


    }
}
