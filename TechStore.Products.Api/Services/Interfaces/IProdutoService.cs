using TechStore.Products.Api.Dtos;

namespace TechStore.Products.Api.Services.Interfaces;

public interface IProdutoService
{
    Task<List<ProdutoResponseDto>> ListarAsync(
        int pagina,
        int tamanhoPagina,
        string? nome,
        CancellationToken cancellationToken);

    Task<ProdutoResponseDto?> BuscarPorIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<ProdutoResponseDto> CriarAsync(
        ProdutoRequestDto dto,
        CancellationToken cancellationToken);

    Task<ProdutoResponseDto?> AtualizarAsync(
        int id,
        ProdutoRequestDto dto,
        CancellationToken cancellationToken);

    Task<bool> ExcluirAsync(
        int id,
        CancellationToken cancellationToken);
}