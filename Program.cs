using Microsoft.EntityFrameworkCore;
using StockAPI.Models;
using System;
using StockAPI.Interfaces;
using StockAPI.Repositories;
using StockAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<stockContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IStockAnalysisService, StockAnalysisService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
