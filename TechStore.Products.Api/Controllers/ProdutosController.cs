using Microsoft.AspNetCore.Mvc;
using TechStore.Products.Api.Dtos;
using TechStore.Products.Api.Services.Interfaces;

namespace TechStore.Products.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _service;

    public ProdutosController(IProdutoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProdutoResponseDto>>> Listar(
        CancellationToken cancellationToken,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        [FromQuery] string? nome = null)
    {
        var produtos = await _service.ListarAsync(
            pagina,
            tamanhoPagina,
            nome,
            cancellationToken);

        return Ok(produtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoResponseDto>> BuscarPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var produto = await _service.BuscarPorIdAsync(
            id,
            cancellationToken);

        if (produto is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Produto não encontrado"
            });
        }

        return Ok(produto);
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoResponseDto>> Criar(
        [FromBody] ProdutoRequestDto dto,
        CancellationToken cancellationToken)
    {
        var produto = await _service.CriarAsync(
            dto,
            cancellationToken);

        return CreatedAtAction(
            nameof(BuscarPorId),
            new { id = produto.Id },
            produto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProdutoResponseDto>> Atualizar(
        int id,
        [FromBody] ProdutoRequestDto dto,
        CancellationToken cancellationToken)
    {
        var produto = await _service.AtualizarAsync(
            id,
            dto,
            cancellationToken);

        if (produto is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Produto não encontrado"
            });
        }

        return Ok(produto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(
        int id,
        CancellationToken cancellationToken)
    {
        var excluido = await _service.ExcluirAsync(
            id,
            cancellationToken);

        if (!excluido)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Produto não encontrado"
            });
        }

        return NoContent();
    }
}