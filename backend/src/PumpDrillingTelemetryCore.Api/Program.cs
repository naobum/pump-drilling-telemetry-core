using FluentValidation;
using PumpDrillingTelemetryCore.Api.Validation;
using PumpDrillingTelemetryCore.Api.Requests.CreateWorkSession;
using PumpDrillingTelemetryCore.Application.Commands.CreateWorkSession;
using PumpDrillingTelemetryCore.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddScoped<IValidator<CreateWorkSessionRequest>, CreateWorkSessionRequestValidator>();
builder.Services.AddProblemDetails();
builder.Services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblyContaining<CreateWorkSessionCommandHandler>());

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length > 0)
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseExceptionHandler();
// Кастомный middleware для обработки исключений в приложении
app.UseMiddleware<ExceptionHandlingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Pump Drilling Telemetry API v1"));
}

app.UseHttpsRedirection();

app.UseCors("Frontend");
app.MapControllers();
app.Run();



