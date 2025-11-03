using Microsoft.EntityFrameworkCore;
using MarketCampaignProject.Models;
using MarketCampaignProject.Data;
using MarketCampaignProject.DTOs;
using BCrypt.Net;



namespace MarketCampaignProject.Services
{
    public class AuthService
    { //Dependency injection 1. _context gives access to the database and _jwt generates the secure jwt token
        private readonly ApplicationDbContext _context;
        private readonly JwtTokenService _jwt;
        //constructor
        public AuthService(ApplicationDbContext context, JwtTokenService jwt)
        {
            _context = context;
            _jwt = jwt;
        }
        // till this dependency injection 

        public async Task<ResponseDto> RegisterAsync(RegisterDto dto)
        {
            // it will check if the email already exist or not
            var existinguser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existinguser != null)
            {
                return new ResponseDto(false, "User already registered");
            }
            else
            {
                var newUser = new User
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.Password)
                };
                //save to DB
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return new ResponseDto(true, "user register successfully");
            }

        }
        public async Task<ResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                return new ResponseDto(false, "Invalid username or password");
            }
            else
            {
                bool isPasswordCorrect = BCrypt.Net.BCrypt.EnhancedVerify(dto.Password, user.PasswordHash);

                if (isPasswordCorrect)
                {
                    //generate jwt token 
                    var token = _jwt.GenerateToken(user);

                    return new ResponseDto(true,"login successful", new { token });
                }
                else 
                {
                    //password is incorrect 
                    return new ResponseDto(false, "Invalid password",null);
                }
            }
        }
    }
}
