namespace PetCare360.API.Hateoas
{
    public class Recurso<T>
    {
        public T Dados { get; set; } = default!;
        public List<Link> Links { get; set; } = new();
    }
}