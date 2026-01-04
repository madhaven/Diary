using Diary.Data;
using Diary.Implementation;
using Diary.Web;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

Utils.EnsureConfigExists(builder.Environment.ContentRootPath); // Config setup

builder.ConfigureDiaryServices(); // Dependency Injection / Inversion of Control

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Auto-migrate DB
using var scope = app.Services.CreateScope();
var ctx = scope.ServiceProvider.GetRequiredService<DiaryDbContext>();
await ctx.Database.MigrateAsync();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

app.UseAuthorization();

// Static files
app.UseStaticFiles();

app.MapControllers();

app.MapFallbackToFile("index.html");

await app.RunAsync();