using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.OpenApi.Models;
using mitraacd.Models;
using mitraacd.Services;
using mitraacd.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using Swashbuckle.AspNetCore.Annotations;


var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
Console.WriteLine($"Current Environment: {env.EnvironmentName}");

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
    c.SwaggerDoc("account", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API Account",
        Version = "v1"
    });
    c.SwaggerDoc("whatsapp", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API Whatsapp",
        Version = "v1"
    });
    c.SwaggerDoc("cloudinary", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API Cloudinary",
        Version = "v1"
    });

    
    // Filter supaya dokumen "Account" hanya memuat controller dengan GroupName = "transaksi"
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (string.IsNullOrWhiteSpace(apiDesc.GroupName))
        {
            // Kalau endpoint tidak punya GroupName, taruh di "v1"
            return docName == "v1";
        }

        return string.Equals(apiDesc.GroupName, docName, StringComparison.OrdinalIgnoreCase);
    });
    c.EnableAnnotations();
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
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IWhatsappRepo, WhatsappRepo>();


builder.Services.AddHttpClient();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

builder.Services.AddSignalR();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,

        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    // ✅ Tambahan untuk SignalR: biar bisa ambil token dari query string saat WebSocket connect
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // kalau request menuju Hub notifikasi, pakai token dari query string
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/notifikasiHub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "https://localhost:7298",
            "https://localhost:7245",
            "https://mitraacd.onrender.com",
            "https://mitra.dikariapp.com",
            "https://customer.dikariapp.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
});

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
    c.SwaggerEndpoint("/swagger/account/swagger.json", "API account");
    c.SwaggerEndpoint("/swagger/whatsapp/swagger.json", "API Whatsapp");
    c.SwaggerEndpoint("/swagger/cloudinary/swagger.json", "API Cloudinary");
    
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
