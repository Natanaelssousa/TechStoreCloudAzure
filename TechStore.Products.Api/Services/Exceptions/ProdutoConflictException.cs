namespace TechStore.Products.Api.Services.Exceptions
{
    public class ProdutoConflictException : Exception
    {
        public ProdutoConflictException(string mensagem)
            : base(mensagem)
        {
        }
    }
}
