using System.ComponentModel.DataAnnotations;

namespace TechStore.Products.Api.Dtos;

public class ProdutoRequestDto
{
    [Required(ErrorMessage = "O código é obrigatório.")]
    [StringLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Descricao { get; set; }

    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ErrorMessage = "O preço deve estar entre 0,01 e 9999999999999999,99.",
        ParseLimitsInInvariantCulture = true)]
    public decimal Preco { get; set; }

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    [StringLength(100)]
    public string Categoria { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
}