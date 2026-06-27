using Basket.API.Basket;
using BuildingBlocks.Exceptions.Handler;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

//using Microsoft.Extensions.Caching.Distributed;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
    opts.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
// Add caching to the BasketRepository using Scrutor's Decorate method
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();

// Code below is used to add multiple dependancy injection caching to the BasketRepository in manually.
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

//Cross-Cutting Services
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapBasketEndpoints();

app.UseExceptionHandler(options => { });

app.UseHealthChecks("/health", 
    new HealthCheckOptions { 
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();
