using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models.DTO;
using RoyalVilla_API.Services;

namespace RoyalVilla_API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Register(RegisterationRequestDTO registerationRequestDTO)
        {
            try
            {
                if (registerationRequestDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registeration data is required"));
                }

                if (await _authService.IsEmailExistsAsync(registerationRequestDTO.Email))
                {
                    return Conflict(ApiResponse<object>.Conflict($"User with email '{registerationRequestDTO.Email}' already exists"));
                }

                var user = await _authService.RegisterAsync(registerationRequestDTO);

                if (user == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registration failed"));
                }

                //auth service 
                var response = ApiResponse<UserDTO>.CreatedAt(user, "User registered successfully");
                return CreatedAtAction(nameof(Register), response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred during registration", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
    }
}
