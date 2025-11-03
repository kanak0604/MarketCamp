//connects frontend to the backend 
//Fontend(user clicks button) → Controller → Service → Database → Response back to user.
using MarketCampaignProject.DTOs;
using MarketCampaignProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;//gives my class [apicontroller],[httppost]
using System;
using static System.Net.WebRequestMethods;

namespace MarketCampaignProject.Controllers
{
    [ApiController]  // tells .NET this class handles API requests
    [Route("api/[controller]")] // sets URL route: api/Auth
    //[ApiController] → tells ASP.NET Core this is a web API controller that can receive HTTP requests (like GET, POST, etc.).
   //[Route("api/[controller]")] → sets the API endpoint URL.
   //[controller] means the name of the class (AuthController → Auth).
   //So, this controller will respond to:https://localhost:7039/api/Auth/register and https://localhost:7039/api/Auth/login
    public class AuthController:ControllerBase
    //we have defined a controller name authcontroller which will extract all the properties from controllerBase
    //Controller base contain basic features to handle http request and response.
    {
        private readonly AuthService _authService;
        //The main use of the underscore is to immediately signal to anyone reading the code that the
        //variable is a private field belonging to the class.
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        //IActionResult is a standard API response form 
        {
            var result = await _authService.RegisterAsync(registerDto);
            //return message back to the frontend
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            return Ok(result);
        }
    }
}
