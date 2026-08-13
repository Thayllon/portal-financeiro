using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.Core.Application.Validations;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Configurations;

public static class ConfigureControllers
{
    public static IServiceCollection AddAppControllers(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<DespesaRequestValidator>();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var primeiraMensagem = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));

                var erro = Erro.Validacao("VALIDACAO", primeiraMensagem ?? "Requisição inválida.");
                return new BadRequestObjectResult(erro);
            };
        });

        return services;
    }
}