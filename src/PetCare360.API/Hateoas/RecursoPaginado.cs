namespace PetCare360.API.Hateoas
{
    public class RecursoPaginado<T>
    {
        public List<Recurso<T>> Itens { get; set; } = new();
        public int Pagina { get; set; }
        public int TamanhoPagina { get; set; }
        public int TotalItens { get; set; }
        public int TotalPaginas { get; set; }
        public bool TemPaginaAnterior { get; set; }
        public bool TemProximaPagina { get; set; }
        public List<Link> Links { get; set; } = new();
    }
}