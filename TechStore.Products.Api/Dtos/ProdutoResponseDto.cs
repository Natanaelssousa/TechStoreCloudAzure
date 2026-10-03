namespace TechStore.Products.Api.Dtos;

public class ProdutoResponseDto
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public decimal Preco { get; set; }

    public string Categoria { get; set; } = string.Empty;

    public bool Ativo { get; set; }

    public DateTime CriadoEmUtc { get; set; }

    public DateTime? AtualizadoEmUtc { get; set; }
}