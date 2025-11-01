using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IServices;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RoyalVillaWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly ITokenProvider _tokenProvider;

        public AuthController(IAuthService authService, IMapper mapper, ITokenProvider tokenProvider)
        {
            _mapper = mapper;
            _authService = authService;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDTO loginRequestDTO)
        {
            try
            {
                var response = await _authService.LoginAsync<ApiResponse<TokenDTO>>(loginRequestDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    var principal = _tokenProvider.CreatePrincipalFromJwtToken(response.Data.AccessToken);
                    if (principal != null)
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        _tokenProvider.SetToken(response.Data.AccessToken,response.Data.RefreshToken);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["error"] = "Invalid token received. Please try again.";
                    }
                }
                else
                {
                    TempData["error"] = response.Message;
                    
                }
                return View(loginRequestDTO);
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }

            return View();
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterationRequestDTO
            {
                Email = string.Empty,
                Name = string.Empty,
                Password = string.Empty,
                Role = "Customer"
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterationRequestDTO registerationRequestDTO)
        {
            try
            {
                ApiResponse<UserDTO> response = await _authService.RegisterAsync<ApiResponse<UserDTO>>(registerationRequestDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Registration successful! Please login with your credentials.";
                    return RedirectToAction("Login");
                }
                else
                {
                    TempData["error"]= response?.Message?? "Registration failed. Please try again.";
                    return View(registerationRequestDTO);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }

            return View(registerationRequestDTO);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }


    }
}

