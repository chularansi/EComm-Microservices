using BuildingBlocks.CQRS.Behaviours;
using Ordering.API;
using Ordering.Application;
using Ordering.Application.Orders.CreateOrder;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Data;
using Ordering.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//var assembly = typeof(Program).Assembly;

//builder.Services.AddDispatcher(Assembly.GetExecutingAssembly());
//builder.Services.AddPipelineBehavior(typeof(LoggingBehaviour<,>));
//builder.Services.AddPipelineBehavior(typeof(ValidationBehaviour<,>));
//builder.Services.AddPipelineBehavior(typeof(CachingBehaviour<,>));
//builder.Services.AddPipelineBehavior(typeof(TransactionBehaviour<,>));

//builder.Services.AddValidatorsFromAssembly(assembly);

//// Gather the assemblies from both layers
//var applicationAssembly = typeof(CreateOrderCommand).Assembly;
//var infrastructureAssembly = typeof(ApplicationDbContext).Assembly;

//// Call your custom registration extension method for BOTH layers
//builder.Services.AddDispatcher(applicationAssembly);
//builder.Services.AddDispatcher(infrastructureAssembly);

builder.Services.AddDispatcher(
    typeof(CreateOrderCommand).Assembly,       // Scans Application layer
    typeof(ApplicationDbContext).Assembly      // Scans Infrastructure layer (captures handler!)
);
builder.Services.AddPipelineBehavior(typeof(LoggingBehaviour<,>));
builder.Services.AddPipelineBehavior(typeof(ValidationBehaviour<,>));

//await builder.Services
//    .AddApiServices(builder.Configuration)
//    .AddApplicationServices(builder.Configuration)
//    .AddInfrastructureServices(builder.Configuration);

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseApiServices();

if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}

app.Run();
