using LogCollector.Context;
using LogCollectorAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(a =>
{
    a.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
    {
        Title = "Swagger Docs",
        Version = "v1",
    });
    a.IncludeXmlComments("SwaggerDocs.xml");
});
builder.Services.AddSingleton<IDapperContext, DapperContext>();
builder.Services.AddLogging(l => l.AddConsole()); ;


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.AddLogCollectorApi();

app.Run();

// For testing purposes only
public partial class Program { }