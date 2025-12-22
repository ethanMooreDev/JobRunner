using JobRunner.Domain.Interfaces;
using JobRunner.Infrastructure.Data;
using JobRunner.Infrastructure.Queues;
using JobRunner.Workers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddScoped<IJobQueue, EfJobQueue>();
builder.Services.AddHostedService<JobRunnerWorker>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var cs = builder.Configuration.GetConnectionString("Default");
app.Logger.LogInformation("Connection String: {connectionString}", cs);

app.Run();


