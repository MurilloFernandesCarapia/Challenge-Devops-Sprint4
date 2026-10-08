using PetCare360.API.Hateoas;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Pagination;

namespace PetCare360.UnitTests.Hateoas
{
    public class HateoasBuilderTests
    {
        private static Recurso<Pet> CriarRecursoPet(Pet pet)
        {
            return HateoasBuilder.CriarRecurso(pet, "/api/Pets", pet.IdPet);
        }

        private static PagedResult<Pet> CriarResultado(int pagina, int tamanhoPagina, int totalItens)
        {
            return new PagedResult<Pet>
            {
                Itens = new List<Pet> { new Pet { IdPet = 1, NmPet = "Rex", Especie = "Cachorro" } },
                Pagina = pagina,
                TamanhoPagina = tamanhoPagina,
                TotalItens = totalItens
            };
        }

        [Fact]
        public void CriarRecurso_EntidadeComId_GeraLinksSelfUpdateEDelete()
        {
            var pet = new Pet { IdPet = 7, NmPet = "Thor", Especie = "Cachorro" };

            var recurso = HateoasBuilder.CriarRecurso(pet, "/api/Pets", pet.IdPet);

            Assert.Same(pet, recurso.Dados);
            Assert.Contains(recurso.Links, l => l.Rel == "self" && l.Href == "/api/Pets/7" && l.Method == "GET");
            Assert.Contains(recurso.Links, l => l.Rel == "update" && l.Method == "PUT");
            Assert.Contains(recurso.Links, l => l.Rel == "delete" && l.Method == "DELETE");
        }

        [Fact]
        public void CriarRecurso_ComLinksExtras_AdicionaLinksExtras()
        {
            var pet = new Pet { IdPet = 7, NmPet = "Thor", Especie = "Cachorro" };
            var linkHistorico = new Link("/api/Pets/7/historico", "historico", "GET");

            var recurso = HateoasBuilder.CriarRecurso(pet, "/api/Pets", pet.IdPet, linkHistorico);

            Assert.Equal(4, recurso.Links.Count);
            Assert.Contains(linkHistorico, recurso.Links);
        }

        [Fact]
        public void CriarRecursoPaginado_PaginaIntermediaria_GeraLinksPreviousENext()
        {
            var resultado = CriarResultado(pagina: 2, tamanhoPagina: 2, totalItens: 5);
            var parametros = new PetQueryParameters { Pagina = 2, TamanhoPagina = 2 };

            var recurso = HateoasBuilder.CriarRecursoPaginado(resultado, parametros, "/api/Pets", CriarRecursoPet);

            Assert.Contains(recurso.Links, l => l.Rel == "previous" && l.Href.Contains("pagina=1"));
            Assert.Contains(recurso.Links, l => l.Rel == "next" && l.Href.Contains("pagina=3"));
            Assert.Contains(recurso.Links, l => l.Rel == "last" && l.Href.Contains("pagina=3"));
            Assert.Contains(recurso.Links, l => l.Rel == "create" && l.Method == "POST");
        }

        [Fact]
        public void CriarRecursoPaginado_PrimeiraPagina_NaoGeraLinkPrevious()
        {
            var resultado = CriarResultado(pagina: 1, tamanhoPagina: 10, totalItens: 1);
            var parametros = new PetQueryParameters();

            var recurso = HateoasBuilder.CriarRecursoPaginado(resultado, parametros, "/api/Pets", CriarRecursoPet);

            Assert.DoesNotContain(recurso.Links, l => l.Rel == "previous");
            Assert.DoesNotContain(recurso.Links, l => l.Rel == "next");
            Assert.Single(recurso.Itens);
            Assert.NotEmpty(recurso.Itens[0].Links);
        }

        [Fact]
        public void CriarRecursoPaginado_ComFiltros_MantemFiltrosNosLinks()
        {
            var resultado = CriarResultado(pagina: 1, tamanhoPagina: 2, totalItens: 4);
            var parametros = new PetQueryParameters
            {
                TamanhoPagina = 2,
                Especie = "Gato",
                OrdenarPor = "nome",
                Ascendente = false
            };

            var recurso = HateoasBuilder.CriarRecursoPaginado(resultado, parametros, "/api/Pets", CriarRecursoPet);

            var linkNext = Assert.Single(recurso.Links, l => l.Rel == "next");
            Assert.Contains("pagina=2", linkNext.Href);
            Assert.Contains("especie=Gato", linkNext.Href);
            Assert.Contains("ordenarPor=nome", linkNext.Href);
            Assert.Contains("ascendente=false", linkNext.Href);
        }
    }
}