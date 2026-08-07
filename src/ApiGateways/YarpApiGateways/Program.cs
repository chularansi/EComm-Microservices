//using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//var certificate = LoadCertificate("certs/ecomm-microservices-api-cert.cert", "certs/ecomm-microservices-api-cert.key");

var kestrel = builder.Configuration.GetSection("Kestrel:Endpoints:Https:Certificate");

// Configure Kestrel to use HTTPS with the loaded cert.
// Extract string values from IConfigurationSection using .Value property
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5054, listenOptions =>
    {
        listenOptions.UseHttps(
            kestrel.GetSection("Path").Value!,
            kestrel.GetSection("Password").Value
        );
    });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .ConfigureHttpClient((context, handler) =>
    {
        // DEV ONLY: Ignore backend SSL errors
        handler.SslOptions.RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true;
//#if DEBUG
//        handler.SslOptions.RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true;
//#endif
    });

builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.Window = TimeSpan.FromSeconds(10);
        options.PermitLimit = 5;
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("ShoppingWebReactApp", policy =>
    {
        policy.WithOrigins("https://localhost:3000") // Vite Dev URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
// Make sure app.UseCors is placed BEFORE app.UseRouting() and app.MapReverseProxy()
app.UseCors("ShoppingWebReactApp");
// Add authentication middleware before MapReverseProxy()
app.UseRateLimiter();
app.MapReverseProxy();

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