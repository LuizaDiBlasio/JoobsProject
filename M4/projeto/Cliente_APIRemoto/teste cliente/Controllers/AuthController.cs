using JobPortal_API.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using teste_cliente.DTOs;
using teste_cliente.Models;
using teste_cliente.Models.Dto;
using teste_cliente.Services.IServices;
using Vereyon.Web;

namespace teste_cliente.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IFlashMessage _flashMessage;
        private readonly string _baseUrl;

        public AuthController(IAuthService authService, HttpClient httpClient, IConfiguration configuration, IFlashMessage flashMessage)
        {
            _authService = authService;
            _httpClient = httpClient;
            _configuration = configuration;
            _flashMessage = flashMessage;
            _baseUrl = _configuration["ApiSettings:BaseUrl"];
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDTO obj = new();
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDTO obj)
        {
            APIResponse response = await _authService.LoginAsync<APIResponse>(obj);

            if (response != null && response.IsSuccess)
            {
                var json0 = Convert.ToString(response.Result);
                LoginResponseDTO model = (response.Result as JObject)?.ToObject<LoginResponseDTO>();

                Console.WriteLine(model);

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, (model.User.UserName).Trim()));
                identity.AddClaim(new Claim(ClaimTypes.Role, model.User.Role));

                if (model.User.Role == SD.Role_Candidato)
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(model.Token);
                    var idCandidato = jwt.Claims.First(c => c.Type == "IdCandidato").Value;
                    identity.AddClaim(new Claim("IdCandidato", idCandidato));
                }

                if (model.User.Role == SD.Role_Empresa)
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(model.Token);
                    var idEmpresa = jwt.Claims.First(c => c.Type == "IdEmpresa").Value;
                    identity.AddClaim(new Claim("IdEmpresa", idEmpresa));
                }

                identity.AddClaim(new Claim("JWToken", model.Token));

                var principal = new ClaimsPrincipal(identity);
                var props = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3)
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError(string.Empty, "Utilizador ou password inválidos. Tente novamente.");
                }
                else
                {
                    var msg = response.ErrorMessages.FirstOrDefault() ?? "Ocorreu um erro inesperado. Tente novamente.";
                    ModelState.AddModelError(string.Empty, msg);
                }
                return View(obj);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterationRequestDTO obj)
        {
            using (var httpClient = new HttpClient())
            {
                StringContent content = new StringContent(
                    JsonConvert.SerializeObject(obj),
                    Encoding.UTF8,
                    "application/json"
                );

                // CORREÇÃO: Adicionado o prefixo "api/" ao endpoint de registo centralizado
                using (var response = await httpClient.PostAsync(_baseUrl + "api/Auth/register", content))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Resposta da API: {apiResponse}");

                    if (response.IsSuccessStatusCode)
                    {
                        string role = obj.Role?.Trim();
                        try
                        {
                            var responseData = JsonConvert.DeserializeObject<APIResponse>(apiResponse);
                            var result = responseData?.Result as JObject;
                            if (result != null)
                            {
                                role = result["role"]?.ToString()?.Trim() ?? result["user"]?["role"]?.ToString()?.Trim();
                            }
                        }
                        catch { }

                        string redirectUrl = (role == SD.Role_Candidato || role == SD.Role_Empresa)
                            ? Url.Action("Login", "Auth")
                            : Url.Action("Index", "Home");

                        return View("Success", ("Conta criada com sucesso!!!", redirectUrl));
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Erro da API: {errorContent}");

                        try
                        {
                            var jsonArray = JArray.Parse(errorContent);
                            foreach (var item in jsonArray)
                            {
                                var errorDesc = item["description"]?.ToString() ?? item["Description"]?.ToString();
                                if (!string.IsNullOrEmpty(errorDesc))
                                {
                                    ModelState.AddModelError(string.Empty, errorDesc);
                                }
                            }

                            if (ModelState.ErrorCount == 0)
                            {
                                ModelState.AddModelError(string.Empty, "MISTÉRIO 1: " + errorContent);
                            }
                        }
                        catch (Exception)
                        {
                            ModelState.AddModelError(string.Empty, "MISTÉRIO 2: " + errorContent);
                        }

                        return View(obj);
                    }
                }
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPassword model)
        {
            var jsonContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(model, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                Encoding.UTF8,
                "application/json");

            try
            {
                // CORREÇÃO: Adicionado o prefixo "api/" para o endpoint de esquecimento de password
                var apiCall = await _httpClient.PostAsync(_baseUrl + "api/Auth/GenerateForgotPasswordTokenAndEmail", jsonContent);

                if (apiCall.IsSuccessStatusCode)
                {
                    _flashMessage.Confirmation("Foi enviado para o seu email um link de recuperação de password");
                    return View(model);
                }

                _flashMessage.Danger("Não foi possível recuperar senha, favor contactar admin");
                return View(model);
            }
            catch (Exception)
            {
                return View("Error500");
            }
        }

        [HttpGet]
        public IActionResult RecoverPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return View("AccessDenied");
            }

            var model = new RecoverPassword()
            {
                UserId = userId,
                Token = token,
                Password = string.Empty
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RequestResetPassword(RecoverPassword model)
        {
            if (string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.Token))
            {
                return View("AccessDenied");
            }

            var dto = new ResetPasswordDTO
            {
                Token = model.Token,
                UserId = model.UserId,
                Password = model.Password
            };

            var jsonContent = new StringContent(
               System.Text.Json.JsonSerializer.Serialize(dto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
               Encoding.UTF8,
               "application/json");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.Preserve
            };

            try
            {
                var apiCall = await _httpClient.PostAsync(_baseUrl + "api/Auth/ResetPassword", jsonContent);
                var response = await apiCall.Content.ReadFromJsonAsync<APIResponse>(options);

                if (apiCall.IsSuccessStatusCode)
                {
                    _flashMessage.Confirmation(response.Message);
                    return View("RecoverPassword", new RecoverPassword());
                }

                _flashMessage.Danger(response.Message);
                return View("RecoverPassword", new RecoverPassword());
            }
            catch (Exception)
            {
                _flashMessage.Danger($"Unable to reset password, please contact admin");
                return View("RecoverPassword", new RecoverPassword());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestModel request)
        {
            if (request == null || string.IsNullOrEmpty(request.Credential))
                return Json(new { isSuccess = false, message = "Token inválido." });

            var googleDto = new GoogleLoginDTO
            {
                IdToken = request.Credential
            };

            // Nota: O _authService internamente já foi corrigido para usar "api/Auth/google-login"
            APIResponse response = await _authService.GoogleLoginAsync<APIResponse>(googleDto);

            if (response != null && response.IsSuccess)
            {
                LoginResponseDTO model = (response.Result as JObject)?.ToObject<LoginResponseDTO>();

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, (model.User.UserName).Trim()));
                identity.AddClaim(new Claim(ClaimTypes.Role, model.User.Role));

                if (model.User.Role == SD.Role_Candidato)
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(model.Token);
                    var idCandidato = jwt.Claims.First(c => c.Type == "IdCandidato").Value;
                    identity.AddClaim(new Claim("IdCandidato", idCandidato));
                }

                identity.AddClaim(new Claim("JWToken", model.Token));

                var principal = new ClaimsPrincipal(identity);
                var props = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

                return Json(new { isSuccess = true, redirectUrl = Url.Action("Index", "Home") });
            }

            return Json(new { isSuccess = false, message = "Falha ao autenticar com o Google." });
        }

        public class GoogleLoginRequestModel
        {
            public string Credential { get; set; }
        }
    }
}