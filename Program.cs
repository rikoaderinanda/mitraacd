using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.OpenApi.Models;
using mitraacd.Models;
using mitraacd.Services;
using mitraacd.Hubs;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
    });

builder.Services.AddEndpointsApiExplorer();
// Tambahkan Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mitra ACD API",
        Version = "v1"
    });
});
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new Npgsql.NpgsqlConnection(connectionString);
});
builder.Services.AddScoped<IBidRepository, BidRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ICloudinaryRepository, CloudinaryRepository>();
builder.Services.AddScoped<IPerangkatPelangganRepository, PerangkatPelangganRepository>();
builder.Services.AddSignalR();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mitra ACD API V1");
    c.RoutePrefix = "swagger"; // akses Swagger di /swagger
});

//app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name == "manifest.json" || ctx.File.Name == "sw.js")
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache");
        }
    }
});

app.UseRouting();
app.UseAuthorization();
app.MapHub<NotifikasiHub>("/notifikasiHub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
