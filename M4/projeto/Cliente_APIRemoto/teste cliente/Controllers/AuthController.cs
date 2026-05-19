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

        public AuthController(IAuthService authService, HttpClient httpClient, IConfiguration configuration, IFlashMessage flashMessage)
        {
            _authService = authService;
            _httpClient = httpClient;
            _configuration = configuration;
            _flashMessage = flashMessage;

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

            Console.WriteLine("=========== DEBUG APIResponse ===========");
            Console.WriteLine($"response: {JsonConvert.SerializeObject(response)}");
            Console.WriteLine($"response.Result: {response?.Result}");
            Console.WriteLine("=========================================");

            if (response != null && response.IsSuccess)
            {
                var json0 = Convert.ToString(response.Result);

                LoginResponseDTO model = (response.Result as JObject)?.ToObject<LoginResponseDTO>();

                Console.WriteLine(model);


                ///////////////
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, (model.User.UserName).Trim()));
                identity.AddClaim(new Claim(ClaimTypes.Role, model.User.Role));

                if (model.User.Role == SD.Role_Candidato)
                {
                    // lê o IdCandidato que já veio dentro do token JWT
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(model.Token);
                    var idCandidato = jwt.Claims.First(c => c.Type == "IdCandidato").Value;
                    identity.AddClaim(new Claim("IdCandidato", idCandidato));
                }

                if (model.User.Role == SD.Role_Empresa)
                {
                    // lê o IdEmpresa que já veio dentro do JWT
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(model.Token);
                    var idEmpresa = jwt.Claims.First(c => c.Type == "IdEmpresa").Value;
                    identity.AddClaim(new Claim("IdEmpresa", idEmpresa));
                }
                /////////////////

                // adiciona o claim com o token
                identity.AddClaim(new Claim("JWToken", model.Token));

                var principal = new ClaimsPrincipal(identity);
                var props = new AuthenticationProperties
                {
                    IsPersistent = true,                           // persiste além da sessão atual
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3)
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                // se a API devolveu 401 (credenciais inválidas)
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError(string.Empty, "Utilizador ou password inválidos. Tente novamente.");
                }
                else
                {
                    // fallback para qualquer outra mensagem de erro
                    var msg = response.ErrorMessages.FirstOrDefault()
                              ?? "Ocorreu um erro inesperado. Tente novamente.";
                    ModelState.AddModelError(string.Empty, msg);
                }
                return View(obj);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            //LoginRequestDTO obj = new();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterationRequestDTO obj)
        {
            using (var httpClient = new HttpClient())
            {
                // Serializa o objeto de registro em JSON
                StringContent content = new StringContent(
                    JsonConvert.SerializeObject(obj),
                    Encoding.UTF8,
                    "application/json"
                );

                // Chamada para o endpoint centralizado da API
                using (var response = await httpClient.PostAsync("https://localhost:7211/api/Auth/register", content))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Resposta da API: {apiResponse}"); // Log para depuração

                    if (response.IsSuccessStatusCode)
                    {
                        string role = obj.Role?.Trim(); // Usa a role enviada no formulário como fallback

                        try
                        {
                            // Tenta desserializar como APIResponse
                            var responseData = JsonConvert.DeserializeObject<APIResponse>(apiResponse);
                            var result = responseData?.Result as JObject;

                            // Extrai a role do JSON, se disponível
                            if (result != null)
                            {
                                // Tenta direto no result (ex.: { "role": "Admin" })
                                role = result["role"]?.ToString()?.Trim();
                                // Tenta em um objeto user aninhado (ex.: { "user": { "role": "Admin" } })
                                if (string.IsNullOrEmpty(role))
                                {
                                    role = result["user"]?["role"]?.ToString()?.Trim();
                                }
                            }

                            Console.WriteLine($"Role extraída: {role}");
                        }
                        catch (Newtonsoft.Json.JsonException ex)
                        {
                            // Se a desserialização falhar, usa a role do formulário
                            Console.WriteLine($"Erro ao desserializar resposta: {ex.Message}\nResposta: {apiResponse}");
                            Console.WriteLine($"Usando role do formulário: {role}");
                        }

                        // Define a URL de redirecionamento com base na role
                        string redirectUrl = (role == SD.Role_Candidato || role == SD.Role_Empresa)
                            ? Url.Action("Login", "Auth")
                            : Url.Action("Index", "Home");

                        // Exibe o modal de sucesso
                        return View("Success", ("Conta criada com sucesso!!!", redirectUrl));
                    }
                    else
                    {
                        // Mostra o erro vindo da API
                        Console.WriteLine($"Erro na API: {apiResponse}");
                        ModelState.AddModelError(string.Empty, "Erro ao registrar usuário ");
                        return View();
                    }
                }
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Register(RegisterationRequestDTO obj)
        //{
        //    using (var httpClient = new HttpClient())
        //    {
        //        // Serializa o objeto de registro em JSON
        //        StringContent content = new StringContent(
        //            JsonConvert.SerializeObject(obj),
        //            Encoding.UTF8,
        //            "application/json"
        //        );

        //        // Chamada para o endpoint centralizado da API
        //        using (var response = await httpClient.PostAsync("https://localhost:7211/api/Auth/register", content))
        //        {
        //            string apiResponse = await response.Content.ReadAsStringAsync();
        //            Console.WriteLine($"Resposta da API: {apiResponse}"); // Log para depuração

        //            if (response.IsSuccessStatusCode)
        //            {
        //                string role = obj.Role?.Trim(); // Usa a role enviada no formulário como fallback

        //                try
        //                {
        //                    // Tenta desserializar como APIResponse
        //                    var responseData = JsonConvert.DeserializeObject<APIResponse>(apiResponse);
        //                    var result = responseData?.Result as JObject;

        //                    // Extrai a role do JSON, se disponível
        //                    if (result != null)
        //                    {
        //                        // Tenta direto no result (ex.: { "role": "Admin" })
        //                        role = result["role"]?.ToString()?.Trim();
        //                        // Tenta em um objeto user aninhado (ex.: { "user": { "role": "Admin" } })
        //                        if (string.IsNullOrEmpty(role))
        //                        {
        //                            role = result["user"]?["role"]?.ToString()?.Trim();
        //                        }
        //                    }

        //                    Console.WriteLine($"Role extraída: {role}");
        //                }
        //                catch (JsonException ex)
        //                {
        //                    // Se a desserialização falhar (ex.: resposta é "User created successfully"), usa a role do formulário
        //                    Console.WriteLine($"Erro ao desserializar resposta: {ex.Message}\nResposta: {apiResponse}");
        //                    Console.WriteLine($"Usando role do formulário: {role}");
        //                }

        //                // Verifica a role e redireciona
        //                if (!string.IsNullOrEmpty(role))
        //                {
        //                    if (role == SD.Role_Candidato || role == SD.Role_Empresa)
        //                    {
        //                        return RedirectToAction("Login");
        //                    }
        //                    else if (role == SD.Role_Admin)
        //                    {
        //                        return RedirectToAction("Index", "Home");
        //                    }
        //                }

        //                // Fallback para sucesso genérico
        //                Console.WriteLine("Role não encontrada, usando fallback para Login");
        //                return RedirectToAction("Login");
        //            }
        //            else
        //            {
        //                // Mostra o erro vindo da API
        //                Console.WriteLine($"Erro na API: {apiResponse}");
        //                ModelState.AddModelError(string.Empty, "Erro ao registrar usuário: " + apiResponse);
        //                return View(obj);
        //            }
        //        }
        //    }
        //}

        public async Task<IActionResult> Logout()
        {

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
            //await HttpContext.SignOutAsync();
            //HttpContext.Session.SetString(SD.SessionToken, "");
            //return RedirectToAction("Index", "OfertaEmprego");
        }

        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }

        //_____ADIÇÃO DE CÓDIGO____
        /// <summary>
        /// Displays ForgotPassword View
        /// </summary>
        /// <returns>IActionResult of the view</returns>
        //Get da _ForgotPasswordPartial
        public IActionResult ForgotPassword()
        {
            return View();
        }

        //_____ADIÇÃO DE CÓDIGO____
        /// <summary>
        /// Call API to send a retrieve password link to user
        /// </summary>
        /// <param name="model"></param>
        /// <returns>A json containing the API call outcome</returns>
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPassword model)
        {
            var jsonContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(model, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                Encoding.UTF8,
                "application/json");

            try
            {
                var apiCall = await _httpClient.PostAsync("https://localhost:7211/api/Auth/GenerateForgotPasswordTokenAndEmail", jsonContent);

                if (apiCall.IsSuccessStatusCode)
                {
                    _flashMessage.Confirmation("A retrieve password link has been sent to your email");
                    return View(model);
                }

                _flashMessage.Danger("Unable to send link, please contact admin.");
                return View(model);
            }
            catch (Exception)
            {
                return View("Error500");
            }

        }


        //_______ADIÇÃO DE CODIGO______
        /// <summary>
        /// Displays the view for recovering the user's password after email confirmation.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="token">The email confirmation token.</param>
        /// <returns>The password recover view or a "NotAuthorized" view if parameters are invalid.</returns>
        //Get do RecoverPassword
        public IActionResult RecoverPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token)) //verificar parâmetros
            {
                return View("AccessDenied");
            }

            var model = new RecoverPassword()
            {
                UserId = userId,
                Token = token,
                Password = string.Empty  //ainda não foi colocada a senha
            };

            return View(model);
        }


        //_______ADIÇÃO DE CODIGO______
        /// <summary>
        /// Processes the user's password recover request.
        /// </summary>
        /// <param name="model">The model containing the username, reset token, and new password.</param>
        /// <returns>The password recover view with a success or error message.</returns>
        [HttpPost]
        public async Task<IActionResult> RequestResetPassword(RecoverPassword model) //recebo modelo preechido com dados para recover da password
        {
            if (string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.Token)) //verificar parâmetros (se o token for null, quer dizer que processo falhou e não autoriza)
            {
                return View("AccessDenied"); ;
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
                var apiCall = await _httpClient.PostAsync("https://localhost:7211/api/Auth/ResetPassword", jsonContent);


                var response = await apiCall.Content.ReadFromJsonAsync<APIResponse>(options);

                if (apiCall.IsSuccessStatusCode)
                {
                    _flashMessage.Confirmation(response.Message);

                    return View("RecoverPassword", new RecoverPassword());
                }

                _flashMessage.Danger(response.Message);

                return View("RecoverPassword", new RecoverPassword());
            }
            catch (Exception e)
            {
                _flashMessage.Danger($"Unable to reset password, please contact admin");

                return View("RecoverPassword", new RecoverPassword());
            }
        }

        public async Task<IActionResult> GoogleLogin([FromBody] string credential)
        {
            if (string.IsNullOrEmpty(credential))
                return Json(new { isSuccess = false, message = "Token inválido." });

            // 1. Envia o token do Google para a Backend API
            APIResponse response = await _authService.GoogleLoginAsync<APIResponse>(credential);

            if (response != null && response.IsSuccess)
            {
                LoginResponseDTO model = (response.Result as JObject)?.ToObject<LoginResponseDTO>();

                // 2. Cria a sessão local com os claims (igual ao seu Login normal)
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

                // 3. Retorna sucesso para o JavaScript redirecionar
                return Json(new { isSuccess = true, redirectUrl = Url.Action("Index", "Home") });
            }

            return Json(new { isSuccess = false, message = "Falha ao autenticar com o Google." });
        }
        private string TraduzirErroIdentity(string code, string fallbackDescription)
        {
            return code switch
            {
                "DuplicateUserName" => "Este nome de utilizador já se encontra em uso. Por favor, escolha outro.",
                "DuplicateEmail" => "Este endereço de email já está registado na nossa plataforma.",
                "InvalidUserName" => "O nome de utilizador é inválido (só pode conter letras ou números).",
                "InvalidEmail" => "O email introduzido não é válido.",
                "PasswordTooShort" => "A palavra-passe é demasiado curta.",
                "PasswordRequiresNonAlphanumeric" => "A palavra-passe tem de conter pelo menos um caractere especial.",
                "PasswordRequiresDigit" => "A palavra-passe tem de conter pelo menos um número.",
                "PasswordRequiresUpper" => "A palavra-passe tem de conter pelo menos uma letra maiúscula.",
                "PasswordRequiresLower" => "A palavra-passe tem de conter pelo menos uma letra minúscula.",
                // Adiciona mais casos aqui, se necessário
                _ => fallbackDescription // Retorna a mensagem original se não houver tradução
            };
        }

    }
}
