using Diary.Data;
using Diary.Implementation;
using Diary.Web;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// base dir setup // TODO: can we improve this logic using dotnet builtins?
var executableDir = Path.GetDirectoryName(AppContext.BaseDirectory)!;
Utils.EnsureConfigExists(executableDir);

builder.ConfigureDiaryServices();

var app = builder.Build();

// DB setup
using var scope = app.Services.CreateScope();
var ctx = scope.ServiceProvider.GetRequiredService<DiaryDbContext>();
await ctx.Database.MigrateAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

// to serve static htmls
app.UseDefaultFiles();
app.UseStaticFiles();

await app.RunAsync();
