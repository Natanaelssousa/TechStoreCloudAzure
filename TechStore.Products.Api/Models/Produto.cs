namespace TechStore.Products.Api.Models;

public class Produto
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public decimal Preco { get; set; }

    public string Categoria { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;

    public DateTime? AtualizadoEmUtc { get; set; }
}