namespace PetCare360.Domain.Pagination
{
    
    public class PagedResult<T>
    {
        public IEnumerable<T> Itens { get; init; } = new List<T>();
        public int Pagina { get; init; }
        public int TamanhoPagina { get; init; }
        public int TotalItens { get; init; }

        
        public int TotalPaginas => TamanhoPagina <= 0 ? 0 : (int)Math.Ceiling(TotalItens / (double)TamanhoPagina);
        public bool TemPaginaAnterior => Pagina > 1;
        public bool TemProximaPagina => Pagina < TotalPaginas;
    }
}