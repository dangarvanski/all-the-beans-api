using all_the_beans_application.Interfaces;
using all_the_beans_application.Queries;
using all_the_beans_application.Services;
using all_the_breans_infrastructure.Interfaces;
using all_the_breans_infrastructure.Repositories;
using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<BeansDbRepository>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IBeansDbRepository, BeansDbRepository>();
builder.Services.AddScoped<IBeansService, BeansService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly(), typeof(GetRecordByIndexQuery).Assembly));
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BeansDbRepository>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseIpRateLimiting();

app.UseAuthorization();

app.MapControllers();

app.Run();
