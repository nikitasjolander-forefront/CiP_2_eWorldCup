using CiPeWorldCup.Application.Services;
using CiPeWorldCup.Core.Interfaces;
using CiPeWorldCup.Infrastructure.Data;
using CiPeWorldCup.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<CiPeWorldCupDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();

// Application Services
builder.Services.AddScoped<ParticipantService>();
builder.Services.AddScoped<IMatchService, MatchService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();