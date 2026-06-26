using Basket.API.Basket;
using BuildingBlocks.Exceptions.Handler;
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

//Cross-Cutting Services
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapBasketEndpoints();

app.UseExceptionHandler(options => { });

app.Run();
