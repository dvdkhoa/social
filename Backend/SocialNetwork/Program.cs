using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SocialNetwork.Api.Helpers;
using SocialNetwork.Api.Hubs;
using SocialNetwork.DTO.Extensions;
using SocialNetwork.Ioc;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();

// Add services to the container
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

DependencyInjection dependencyInjection = new DependencyInjection(builder.Configuration);
dependencyInjection.InjectDependencies(builder.Services);

builder.Services.AddControllers();
builder.Services.AddAuthorization();

builder.Services.AddSignalR( e => e.MaximumReceiveMessageSize = 102400000);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var secretKey = builder.Configuration.GetSection("AppSettings:SecretKey").Value;
var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//                .AddJwtBearer(options =>
//                {
//                    options.TokenValidationParameters = new TokenValidationParameters
//                    {
//                        // Tu cap token
//                        ValidateIssuer = false,
//                        ValidateAudience = false,

//                        // ky vao Token
//                        ValidateIssuerSigningKey = true,
//                        IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),

//                        ClockSkew = TimeSpan.Zero
//                    };
//                });
builder.Services.AddAuthentication(auth =>
{
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidAudience = "http://ahmadmozaffar.net",
        ValidIssuer = "http://ahmadmozaffar.net",
        RequireExpirationTime = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// global cors policy
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin
    .AllowCredentials()); // allow credentials

//app.UseAuthentication();
//app.UseAuthorization();
app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

app.MapHub<NotifyHub>("/notifyHub");

app.Run();
