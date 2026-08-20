using Catalog.API.Products;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//var certificate = LoadCertificate("certs/ecomm-microservices-api-cert.cert", "certs/ecomm-microservices-api-cert.key");

//var kestrel = builder.Configuration.GetSection("Kestrel:Endpoints:Https:Certificate");

//// Configure Kestrel to use HTTPS with the loaded cert.
//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(5050, listenOptions =>
//    {
//        listenOptions.UseHttps(
//            kestrel.GetSection("Path").Value!,
//            kestrel.GetSection("Password").Value
//        );
//    });
//});

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowGateway", policy =>
//    {
//        policy.WithOrigins("https://localhost:5054")
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials();
//    });
//});

var assembly = typeof(Program).Assembly;

builder.Services.AddDispatcher(Assembly.GetExecutingAssembly());
builder.Services.AddPipelineBehavior(typeof(LoggingBehaviour<,>));
builder.Services.AddPipelineBehavior(typeof(ValidationBehaviour<,>));
//builder.Services.AddPipelineBehavior(typeof(CachingBehaviour<,>));
//builder.Services.AddPipelineBehavior(typeof(TransactionBehaviour<,>));

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
}).UseLightweightSessions();

if (builder.Environment.IsDevelopment())
    builder.Services.InitializeMartenWith<CatalogInitialData>();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
//app.UseCors("AllowGateway");
app.MapProductsEndpoints();
app.UseExceptionHandler(options => { });

app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();

//static X509Certificate2 LoadCertificate(string certPath, string keyPath)
//{
//    if (!File.Exists(certPath) || !File.Exists(keyPath))
//        throw new FileNotFoundException("Certificate or key file not found.");

//    var certPem = File.ReadAllText(certPath);
//    var keyPem = File.ReadAllText(keyPath);

//    // Create certificate from PEM + key
//    using var publicCert = X509Certificate2.CreateFromPem(certPem, keyPem);
//    // Export to PFX so Kestrel can use it
//    return new X509Certificate2(publicCert.Export(X509ContentType.Pfx));
//}