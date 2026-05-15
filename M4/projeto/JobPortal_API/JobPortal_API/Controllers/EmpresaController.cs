using System.Linq;
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
    [Route("api/empresa")]
    public class EmpresaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public EmpresaController(ApplicationDbContext context, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }

        //Buscar todas as empresas
        [Authorize(Roles = "Admin, Empresa")] // Deixamos ambos entrarem no método. Nós "fingimos" que a Empresa pode entrar ([Authorize(Roles = "Admin, Empresa")]), mas logo na primeira linha do código nós a barramos com uma mensagem educada.
        [HttpGet("BuscarTodas")]        
        public async Task<ActionResult<IEnumerable<EmpresaDTO>>> GetEmpresa()
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

            // Se for Admin, o código continua normalmente
            var empresas = await _context.Empresa
                .ProjectTo<EmpresaDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(empresas);
        }

        //Buscar empresa por ID
        [Authorize(Roles = "Admin, Empresa")]
        [ServiceFilter(typeof(VerificaEmpresaFilter))]  // garante que se o ID não for o da empresa logada não será possível acessar os dados.
        [HttpGet("BuscarPorId/{id}")]
        public async Task<ActionResult<EmpresaDTO>> GetEmpresa(int id)
        {
            // Verifica se a "tabela" (o DbSet) no banco de dados está acessível.
            if ( _context.Empresa == null)
            {
                return NotFound(new {mensagem = $"ID: {id}. Sem conexão com banco de dados."});
            }

            var empresa = await _context.Empresa
                .ProjectTo<EmpresaDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(m => m.IdEmpresa == id);

            // Verifica se a busca que você fez encontrou algum resultado. 
            // garante que a aplicação não trave se o banco sumir ou se o ID não existir.
            if (empresa == null)
            {
                return NotFound(new {mensagem = $"(ID: {id}). Empresa não foi encontrada no sistema."});
            }

            return Ok(empresa);
        }


        [AllowAnonymous]
        [HttpGet("public/BuscarPorId/{id}")]
        public async Task<ActionResult<EmpresaDTO>> GetEmpresaPublic(int id)
        {
            var empresa = await _context.Empresa
                              .ProjectTo<EmpresaDTO>(_mapper.ConfigurationProvider)
                              .FirstOrDefaultAsync(e => e.IdEmpresa == id);

            if (empresa == null) return NotFound(new {mensagem = $"(ID: {id}). Empresa não foi encontrada no sistema." });

            return Ok(empresa);
        }

        /*criar empresa - NÃO É PRECISO MAIS, O REGISTER FAZ ISSO
        
        [HttpPost]
        public async Task<ActionResult> PostEmpresa(EmpresaDTO empresaDTO)
        {
            var empresa = _mapper.Map<Empresa>(empresaDTO);
            _context.Add(empresa);
            await _context.SaveChangesAsync();
            return Ok();
        }*/

        //Editar Empresa
        [Authorize(Roles = "Admin, Empresa")]
        [ServiceFilter(typeof(VerificaEmpresaFilter))]
        [HttpPut("EditarEmpresa/{id:int}")]
        public async Task<ActionResult> PutEmpresa(EmpresaDTO empresaDTO, int id)
        {
            // Se quiser apenas validar o DTO:
            if (empresaDTO.IdEmpresa != 0 && id != empresaDTO.IdEmpresa)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    mensagem = "Acesso negado."
                });
            }

            // Garante que o ID da URL seja usado no objeto no retorno
            empresaDTO.IdEmpresa = id;

            // 1) Busca a empresa no banco
            var empresa = await _context.Empresa
                .FirstOrDefaultAsync(e => e.IdEmpresa == id);

            if (empresa == null)
                return NotFound(new {mensagem = "Acesso negado."});

            // 2) Guarda o UserId para carregar o ApplicationUser
            var userId = empresa.UserId;

            // 3) Atualiza os campos da entidade Empresa
            _mapper.Map(empresaDTO, empresa);
            await _context.SaveChangesAsync();

            // 4) Agora atualiza o AspNetUsers
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                bool changed = false;

                // se email mudou
                if (user.Email != empresa.Email)
                {
                    user.Email = empresa.Email;
                    user.NormalizedEmail = empresa.Email.ToUpperInvariant();
                    changed = true;
                }

                // telefone sempre tratado
                if (empresa.Telefone.HasValue &&
                    user.PhoneNumber != empresa.Telefone.Value.ToString())
                {
                    user.PhoneNumber = empresa.Telefone.Value.ToString();
                    changed = true;
                }

                if (changed)
                {
                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                        return BadRequest(result.Errors);
                }
            }

            return Ok(new {mensagem = "Dados da empresa alterados com sucesso!"});
        }

        //Deletar Empresa
        [Authorize(Roles = "Admin, Empresa")]
        [ServiceFilter(typeof(VerificaEmpresaFilter))]
        [HttpDelete("DeletarEmpresa/{id:int}")]
        public async Task<ActionResult> DeleteEmpresa(int id)
        {
            // 1) Carrega a empresa
            var empresa = await _context.Empresa.FindAsync(id);
            if (empresa == null)
                return NotFound(new {mensagem = "Acesso negado."});

            // 2) Deleta tudo que depende da empresa, na ordem:
            //    a) aplicações de ofertas dessa empresa
            var ofertaIds = await _context.OfertaEmprego
                                 .Where(o => o.IdEmpresa == id)
                                 .Select(o => o.IdOferta)
                                 .ToListAsync();
            var aplicacoes = _context.AplicacaoTrabalho
                                .Where(a => ofertaIds.Contains((int)a.IdOferta));
            _context.AplicacaoTrabalho.RemoveRange(aplicacoes);

            //    b) reviews
            var reviews = _context.Review.Where(r => r.IdEmpresa == id);
            _context.Review.RemoveRange(reviews);

            //    c) ofertas de emprego
            var ofertas = _context.OfertaEmprego.Where(o => o.IdEmpresa == id);
            _context.OfertaEmprego.RemoveRange(ofertas);

            //    d) logo(s) da empresa
            var logos = _context.LogoEmpresa.Where(l => l.IdEmpresaFoto == id);
            _context.LogoEmpresa.RemoveRange(logos);

            // 3) Remove a própria empresa
            _context.Empresa.Remove(empresa);

            // 4) Remove o usuário do Identity
            var user = await _userManager.FindByIdAsync(empresa.UserId);
            if (user != null)
            {
                var identityResult = await _userManager.DeleteAsync(user);
                if (!identityResult.Succeeded)
                    return BadRequest(identityResult.Errors);
            }

            // 5) Persiste todas as deleções
            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Empresa e todos os dados relacionados foram deletados." });
        }

        //Mudar senha
        [Authorize(Roles = "Empresa,Admin")]
        [ServiceFilter(typeof(VerificaEmpresaFilter))]
        [HttpPost("ChangePassword/{id:int}")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordEmpresaDTO dto)
        {
            if (id != dto.IdEmpresa)
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    mensagem = "Acesso negado."
                });

            // 1) Carrega o candidato para obter o UserId
            var empresa = await _context.Empresa
                                  .FirstOrDefaultAsync(c => c.IdEmpresa == dto.IdEmpresa);
            if (empresa == null) return NotFound("Empresa não encontrada");

            // 2) Carrega o usuário Identity
            var user = await _userManager.FindByIdAsync(empresa.UserId);
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
