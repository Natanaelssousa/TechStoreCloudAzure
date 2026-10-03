namespace TechStore.Products.Api.Services.Exceptions
{
    public class ProdutoValidationException : Exception
    {
        public ProdutoValidationException(string mensagem)
            : base(mensagem)
        {
        }
    }
}
