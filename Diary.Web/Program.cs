using Diary.Core;
using Diary.Implementation;
using Diary.Implementation.ExportStrategies;
using Diary.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.IO;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDiaryService, DiaryService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IExporterFactory, ExporterFactory>();

// App Configs
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(nameof(AppConfigs)));

// DB setup
var connectionStringBuilder = new SqliteConnectionStringBuilder
{
    DataSource = builder.Configuration["AppConfigs:SqlitePath"] ?? Path.Combine(AppContext.BaseDirectory, "diary.sqlite"), // Use BaseDirectory for web app
    Cache = SqliteCacheMode.Shared,
    Mode = SqliteOpenMode.ReadWriteCreate
};
var connectionString = connectionStringBuilder.ToString();
builder.Services.AddDbContext<DiaryDbContext>(options => options.UseSqlite(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();
