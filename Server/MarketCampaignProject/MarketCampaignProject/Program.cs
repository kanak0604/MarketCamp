using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MarketCampaignProject.Data;
using MarketCampaignProject.Services;

//this line create the main web builder 1.step - creation 
var builder = WebApplication.CreateBuilder(args);

//it reads the database connection string from appsetting.json 2. connection with the database 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// if connection string is not null 
if (connectionString != null)
{
    //if found = connect database otherwise it will give error 
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30)))
    );
}
else
{
    Console.WriteLine("Database connection string is not found");
}

//when controller need the service it will provide them
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtTokenService>();

//reads jwt token from the appsetting.json
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (jwtKey != null && jwtIssuer != null && jwtAudience != null)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
}
else
{
    Console.WriteLine("JWT configuration missing in appsettings.json");
}

builder.Services.AddAuthorization();

//This allows us to use the controller and open the swagger for testing
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//for CORS ERROR - CROSS ORIGIN RESOURCE SHARING 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200") 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Added below block to show "Authorize" button in Swagger UI 
// It lets you enter JWT tokens and test [Authorize] endpoints
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token as: Bearer {your token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

//this line finalize the services and all set up and build the app
var app = builder.Build();

// if used locally open server , if used in production hide it
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    Console.WriteLine("Swagger disabled in production mode.");
}

app.UseHttpsRedirection(); // keeps app secure (still simple to keep)

//if jwt key is found enable authentication 
if (jwtKey != null)
{
    app.UseAuthentication(); //looks for JWT token in headers.
    app.UseAuthorization();  //checks if that token is valid and user is allowed.
}
else
{
    Console.WriteLine("Authentication not enabled due to missing JWT key!");
}

//Apply the CORS policy we defined above
app.UseCors("AllowAngularApp");

//if app is build connect it to the api routes like api/auth/login
app.MapControllers();

//running and starting the server 
try
{
    app.Run();
    Console.WriteLine("Application started successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Application failed to start: {ex.Message}");
}
