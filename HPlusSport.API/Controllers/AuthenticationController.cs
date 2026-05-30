using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HPlusSport.API.Models;
using HPlusSport.API.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HPlusSport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.EmailAddress);
            if (existingUser != null)
            {
                return BadRequest("Email address is already registered.");
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.EmailAddress,
                FirstName = model.FirstName,
                LastName = model.LastName,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("User registered successfully.");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.EmailAddress);

            if (existingUser != null && await _userManager.CheckPasswordAsync(existingUser, model.Password))
            {
                // Generate JWT token
                var token = GenerateJwtToken(existingUser);
                return Ok(token);
            }

            return BadRequest("Invalid email or password.");
        }

        private AuthResultVM GenerateJwtToken(ApplicationUser user)
        {
            var email = user.Email ?? user.UserName ?? string.Empty;
            var userName = user.UserName ?? user.Email ?? user.Id;
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var jwtSecretKey = GetRequiredJwtSetting("SecretKey");
            var jwtIssuer = GetRequiredJwtSetting("Issuer");
            var jwtAudience = GetRequiredJwtSetting("Audience");
            var expirationInMinutes = _configuration.GetValue<double?>("JwtSettings:ExpirationInMinutes")
                ?? throw new InvalidOperationException("JWT expiration time is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationInMinutes),
                signingCredentials: creds
            );

            return new AuthResultVM
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = token.ValidTo   
            };
        }

        private string GetRequiredJwtSetting(string key)
        {
            return _configuration[$"JwtSettings:{key}"]
                ?? throw new InvalidOperationException($"JWT setting '{key}' is not configured.");
        }
    }

}
