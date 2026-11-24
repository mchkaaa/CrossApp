using Api.Contracts;
using Api.Endpoints;
using Api.Infrastructure;
using Api.Validation;
using Core.Abstractions;
using Core.Storage;
using FluentValidation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Реєстрація обробника доменних винятків (DomainException -> 409 Conflict)
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

// Реєстрація валідатора FluentValidation
builder.Services.AddScoped<IValidator<SaveOrderRequest>, SaveOrderRequestValidator>();

// Реєстрація сховища в DI як Singleton
builder.Services.AddSingleton<IOrderStore>(_ => new InMemoryOrderStore());

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapOrders();

app.Run();

// Потрібно для інтеграційних тестів WebApplicationFactory<Program>
public partial class Program { }