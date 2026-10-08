namespace PetCare360.Domain.Pagination
{
    
    public class QueryParameters
    {
        public const int TamanhoMaximoPagina = 50;
        private const int TamanhoPaginaPadrao = 10;

        private int _pagina = 1;
        private int _tamanhoPagina = TamanhoPaginaPadrao;

        
        public int Pagina
        {
            get => _pagina;
            set => _pagina = value < 1 ? 1 : value;
        }

        
        public int TamanhoPagina
        {
            get => _tamanhoPagina;
            set => _tamanhoPagina = value < 1 ? TamanhoPaginaPadrao : Math.Min(value, TamanhoMaximoPagina);
        }

        
        public string? OrdenarPor { get; set; }

        
        public bool Ascendente { get; set; } = true;
    }
}