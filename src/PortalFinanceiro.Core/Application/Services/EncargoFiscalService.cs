using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;
using PortalFinanceiro.Core.Domain.Services;

namespace PortalFinanceiro.Core.Application.Services;

public interface IEncargoFiscalService
{
    Task<Result<Unit>> GerarDasAsync(Guid idUsuario, Receita receita, decimal percentual);
    Task<Result<Unit>> SincronizarDasAsync(Guid idUsuario, Receita receita, bool geraDas, decimal percentual);
    Task<Result<Unit>> RemoverDasAsync(Guid idUsuario, Receita receita);
    Task<Result<Unit>> GerarInssAsync(Guid idUsuario, ProLabore proLabore, decimal percentual);
    Task<Result<Unit>> RemoverInssAsync(Guid idUsuario, ProLabore proLabore);
}

public class EncargoFiscalService : IEncargoFiscalService
{
    private const string DescricaoDas = "DAS";
    private const string DescricaoInss = "INSS";

    private readonly IDespesaRepository _despesaRepository;
    private readonly ICategoriaDespesaRepository _categoriaDespesaRepository;

    public EncargoFiscalService(IDespesaRepository despesaRepository, ICategoriaDespesaRepository categoriaDespesaRepository)
    {
        _despesaRepository = despesaRepository;
        _categoriaDespesaRepository = categoriaDespesaRepository;
    }

    public async Task<Result<Unit>> GerarDasAsync(Guid idUsuario, Receita receita, decimal percentual)
    {
        var categoria = await ResolverCategoriaFiscalAsync(EncargoFiscal.CategoriaDas);
        if (categoria is null)
            return Erro.Negocio("CATEGORIA_DAS_NAO_CONFIGURADA", $"A subcategoria \"{EncargoFiscal.CategoriaDas}\" (em \"{EncargoFiscal.CategoriaCnpj}\") não foi encontrada. Cadastre-a para gerar o DAS.");

        var valor = EncargoFiscal.Calcular(receita.Valor, percentual);
        if (valor <= 0)
            return Erro.Validacao("DAS_VALOR_INVALIDO", "O percentual informado não gera um valor válido para o DAS.");

        var result = Despesa.Criar(idUsuario, $"{DescricaoDas} - {receita.Descricao}", valor, receita.Data, receita.IdConta, categoria.Id, null, idReceitaOrigem: receita.Id);
        if (!result.EhSucesso)
            return result.Erro!;

        await _despesaRepository.InserirAsync(result.Dado!);
        return Resultado.Sucesso();
    }

    public async Task<Result<Unit>> SincronizarDasAsync(Guid idUsuario, Receita receita, bool geraDas, decimal percentual)
    {
        var existentes = await _despesaRepository.ListarPorReceitaOrigemAsync(receita.Id);

        if (!geraDas)
        {
            foreach (var das in existentes)
            {
                das.Desativar();
                await _despesaRepository.AtualizarAsync(das);
            }
            return Resultado.Sucesso();
        }

        var categoria = await ResolverCategoriaFiscalAsync(EncargoFiscal.CategoriaDas);
        if (categoria is null)
            return Erro.Negocio("CATEGORIA_DAS_NAO_CONFIGURADA", $"A subcategoria \"{EncargoFiscal.CategoriaDas}\" (em \"{EncargoFiscal.CategoriaCnpj}\") não foi encontrada. Cadastre-a para gerar o DAS.");

        var valor = EncargoFiscal.Calcular(receita.Valor, percentual);
        if (valor <= 0)
            return Erro.Validacao("DAS_VALOR_INVALIDO", "O percentual informado não gera um valor válido para o DAS.");

        var dasExistente = existentes.FirstOrDefault();
        if (dasExistente is null)
        {
            var result = Despesa.Criar(idUsuario, $"{DescricaoDas} - {receita.Descricao}", valor, receita.Data, receita.IdConta, categoria.Id, null, idReceitaOrigem: receita.Id);
            if (!result.EhSucesso)
                return result.Erro!;

            await _despesaRepository.InserirAsync(result.Dado!);
            return Resultado.Sucesso();
        }

        var update = dasExistente.Atualizar($"{DescricaoDas} - {receita.Descricao}", valor, receita.Data, receita.IdConta, categoria.Id, null);
        if (!update.EhSucesso)
            return update.Erro!;

        await _despesaRepository.AtualizarAsync(dasExistente);
        return Resultado.Sucesso();
    }

    public async Task<Result<Unit>> RemoverDasAsync(Guid idUsuario, Receita receita)
    {
        var existentes = await _despesaRepository.ListarPorReceitaOrigemAsync(receita.Id);
        foreach (var das in existentes)
        {
            das.Desativar();
            await _despesaRepository.AtualizarAsync(das);
        }
        return Resultado.Sucesso();
    }

    public async Task<Result<Unit>> GerarInssAsync(Guid idUsuario, ProLabore proLabore, decimal percentual)
    {
        var categoria = await ResolverCategoriaFiscalAsync(EncargoFiscal.CategoriaInss);
        if (categoria is null)
            return Erro.Negocio("CATEGORIA_INSS_NAO_CONFIGURADA", $"A subcategoria \"{EncargoFiscal.CategoriaInss}\" (em \"{EncargoFiscal.CategoriaCnpj}\") não foi encontrada. Cadastre-a para gerar o INSS.");

        var valor = EncargoFiscal.Calcular(proLabore.Valor, percentual);
        if (valor <= 0)
            return Erro.Validacao("INSS_VALOR_INVALIDO", "O percentual informado não gera um valor válido para o INSS.");

        var data = new DateTime(proLabore.Ano, proLabore.Mes, DateTime.DaysInMonth(proLabore.Ano, proLabore.Mes));
        var result = Despesa.Criar(idUsuario, $"{DescricaoInss} - {proLabore.Mes:D2}/{proLabore.Ano}", valor, data, proLabore.IdConta, categoria.Id, null, idProLaboreOrigem: proLabore.Id);
        if (!result.EhSucesso)
            return result.Erro!;

        await _despesaRepository.InserirAsync(result.Dado!);
        return Resultado.Sucesso();
    }

    public async Task<Result<Unit>> RemoverInssAsync(Guid idUsuario, ProLabore proLabore)
    {
        var existentes = await _despesaRepository.ListarPorProLaboreOrigemAsync(proLabore.Id);
        foreach (var inss in existentes)
        {
            inss.Desativar();
            await _despesaRepository.AtualizarAsync(inss);
        }
        return Resultado.Sucesso();
    }

    private async Task<CategoriaDespesa?> ResolverCategoriaFiscalAsync(string subnome)
    {
        var categorias = await _categoriaDespesaRepository.ListarAsync();
        var cnpj = categorias.FirstOrDefault(c => c.Nome == EncargoFiscal.CategoriaCnpj && c.CategoriaPaiId is null);
        return cnpj is null
            ? null
            : categorias.FirstOrDefault(c => c.Nome == subnome && c.CategoriaPaiId == cnpj.Id);
    }
}