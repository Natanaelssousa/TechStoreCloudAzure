using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TechStore.Products.Api.Data;
using TechStore.Products.Api.Dtos;
using TechStore.Products.Api.Models;
using TechStore.Products.Api.Services.Exceptions;
using TechStore.Products.Api.Services.Interfaces;

namespace TechStore.Products.Api.Services;

public class ProdutoService : IProdutoService
{
    private readonly AppDbContext _context;

    public ProdutoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProdutoResponseDto>> ListarAsync(
        int pagina,
        int tamanhoPagina,
        string? nome,
        CancellationToken cancellationToken)
    {
        if (pagina < 1 || tamanhoPagina < 1 || tamanhoPagina > 100)
        {
            throw new ProdutoValidationException(
                "A página deve ser maior que zero e o tamanho deve estar entre 1 e 100.");
        }

        var deslocamento = ((long)pagina - 1) * tamanhoPagina;

        if (deslocamento > int.MaxValue)
        {
            throw new ProdutoValidationException(
                "A página solicitada ultrapassa o limite permitido.");
        }

        var query = _context.Produtos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            var filtro = nome.Trim();
            query = query.Where(p => p.Nome.Contains(filtro));
        }

        var produtos = await query
            .OrderBy(p => p.Id)
            .Skip((int)deslocamento)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        return produtos.Select(Mapear).ToList();
    }

    public async Task<ProdutoResponseDto?> BuscarPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var produto = await _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return produto is null ? null : Mapear(produto);
    }

    public async Task<ProdutoResponseDto> CriarAsync(
        ProdutoRequestDto dto,
        CancellationToken cancellationToken)
    {
        Validar(dto);

        var codigo = dto.Codigo.Trim();

        await VerificarCodigoAsync(codigo, null, cancellationToken);

        var produto = new Produto
        {
            CriadoEmUtc = DateTime.UtcNow
        };

        AplicarDados(produto, dto);

        _context.Produtos.Add(produto);

        await SalvarAsync(cancellationToken);

        return Mapear(produto);
    }

    public async Task<ProdutoResponseDto?> AtualizarAsync(
        int id,
        ProdutoRequestDto dto,
        CancellationToken cancellationToken)
    {
        var produto = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (produto is null)
        {
            return null;
        }

        Validar(dto);

        var codigo = dto.Codigo.Trim();

        await VerificarCodigoAsync(codigo, id, cancellationToken);

        AplicarDados(produto, dto);
        produto.AtualizadoEmUtc = DateTime.UtcNow;

        await SalvarAsync(cancellationToken);

        return Mapear(produto);
    }

    public async Task<bool> ExcluirAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var produto = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (produto is null)
        {
            return false;
        }

        _context.Produtos.Remove(produto);

        await SalvarAsync(cancellationToken);

        return true;
    }

    private static void Validar(ProdutoRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Codigo) ||
            string.IsNullOrWhiteSpace(dto.Nome) ||
            string.IsNullOrWhiteSpace(dto.Categoria))
        {
            throw new ProdutoValidationException(
                "Código, nome e categoria são obrigatórios.");
        }

        if (dto.Codigo.Length > 50 ||
            dto.Nome.Length > 150 ||
            dto.Categoria.Length > 100 ||
            dto.Descricao?.Length > 1000)
        {
            throw new ProdutoValidationException(
                "Um dos campos ultrapassa o tamanho permitido.");
        }

        if (dto.Preco <= 0 ||
            dto.Preco > 9999999999999999.99m)
        {
            throw new ProdutoValidationException(
                "O preço está fora da faixa permitida.");
        }

        if (decimal.Round(dto.Preco, 2) != dto.Preco)
        {
            throw new ProdutoValidationException(
                "O preço deve ter no máximo duas casas decimais.");
        }
    }

    private async Task VerificarCodigoAsync(
        string codigo,
        int? idIgnorado,
        CancellationToken cancellationToken)
    {
        var existe = await _context.Produtos.AnyAsync(
            p => p.Codigo == codigo &&
                 (!idIgnorado.HasValue || p.Id != idIgnorado.Value),
            cancellationToken);

        if (existe)
        {
            throw new ProdutoConflictException(
                "Já existe um produto com esse código.");
        }
    }

    private async Task SalvarAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlException &&
                  (sqlException.Number == 2601 ||
                   sqlException.Number == 2627))
        {
            throw new ProdutoConflictException(
                "Já existe um produto com esse código.");
        }
    }

    private static void AplicarDados(
        Produto produto,
        ProdutoRequestDto dto)
    {
        produto.Codigo = dto.Codigo.Trim();
        produto.Nome = dto.Nome.Trim();

        produto.Descricao = string.IsNullOrWhiteSpace(dto.Descricao)
            ? null
            : dto.Descricao.Trim();

        produto.Preco = dto.Preco;
        produto.Categoria = dto.Categoria.Trim();
        produto.Ativo = dto.Ativo;
    }

    private static ProdutoResponseDto Mapear(Produto produto)
    {
        return new ProdutoResponseDto
        {
            Id = produto.Id,
            Codigo = produto.Codigo,
            Nome = produto.Nome,
            Descricao = produto.Descricao,
            Preco = produto.Preco,
            Categoria = produto.Categoria,
            Ativo = produto.Ativo,
            CriadoEmUtc = produto.CriadoEmUtc,
            AtualizadoEmUtc = produto.AtualizadoEmUtc
        };
    }
}