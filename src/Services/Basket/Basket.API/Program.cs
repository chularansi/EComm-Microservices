using Basket.API.Basket;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Security;
using BuildingBlocks.Settings;
using Confluent.Kafka;
using Discount.Grpc;
using HealthChecks.UI.Client;
using Messaging.Kafka;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Application Services
ServiceSettings serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>()!;

var assembly = typeof(Program).Assembly;

builder.Services.AddDispatcher(Assembly.GetExecutingAssembly());
builder.Services.AddPipelineBehavior(typeof(LoggingBehaviour<,>));
builder.Services.AddPipelineBehavior(typeof(ValidationBehaviour<,>));
//builder.Services.AddPipelineBehavior(typeof(CachingBehaviour<,>));
//builder.Services.AddPipelineBehavior(typeof(TransactionBehaviour<,>));

builder.Services.AddValidatorsFromAssembly(assembly);

// Data Services
builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
    opts.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
// Add caching to the BasketRepository using Scrutor's Decorate method
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();

var kafkaProducerConfig = builder.Configuration
    .GetSection("Kafka:Producer")
    .Get<ProducerConfig>() ?? new ProducerConfig();

builder.Services.AddSingleton(kafkaProducerConfig!);
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

// Code below is used to add multiple dependancy injection caching to the BasketRepository in manually.
// Without using Scrutor's Decorate method, you can manually register the CachedBasketRepository as a decorator for the BasketRepository.
// However, this approach is less clean and more error-prone than using Scrutor's Decorate method.
//builder.Services.AddScoped<IBasketRepository>(sp => 
//    new CachedBasketRepository(
//        sp.GetRequiredService<BasketRepository>(), 
//        sp.GetRequiredService<IDistributedCache>()
//    )
//);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis")!;
    //options.InstanceName = "Basket";
});

//Grpc Services
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);
    //options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"] ?? "https://discount.grpc:8081");
});
//.ConfigurePrimaryHttpMessageHandler(() =>
//{
//    var handler = new HttpClientHandler
//    {
//        ServerCertificateCustomValidationCallback =
//        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//    };

//    return handler;
//}); // Use to bypass the ssl/tls connection in local development

//Cross-Cutting Services
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

// Register custom building block security
//builder.Services.AddSharedAuthentication(builder.Configuration, builder.Environment, isPublicService: false, "basket-api.all");

var app = builder.Build();

// Configure the HTTP request pipeline.
//app.UseAuthentication();
//app.UseAuthorization();

app.UseHttpsRedirection();
app.MapBasketEndpoints();
app.UseExceptionHandler(options => { });

app.UseHealthChecks("/health", 
    new HealthCheckOptions { 
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