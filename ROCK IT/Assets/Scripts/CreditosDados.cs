/// <summary>
/// Conteúdo da tela de créditos. Para acrescentar alguém, edite só as listas
/// aqui: a tela monta as seções sozinha, na ordem em que aparecem.
/// </summary>
public static class CreditosDados
{
    public class Secao
    {
        public string titulo;
        public string[] nomes;

        public Secao(string titulo, params string[] nomes)
        {
            this.titulo = titulo;
            this.nomes = nomes;
        }
    }

    public class Musica
    {
        public string titulo;
        public string autor;

        public Musica(string titulo, string autor)
        {
            this.titulo = titulo;
            this.autor = autor;
        }
    }

    public static readonly Secao[] secoes =
    {
        new Secao("IDEALIZAÇÃO E DIREÇÃO", "Mateus De Boit"),
        new Secao("PROGRAMAÇÃO", "Mateus De Boit"),
        new Secao("DESIGN E BALANCEAMENTO", "Mateus De Boit"),
        new Secao("ARTE — PEDRAS", "Renata Nogueira Arangurem"),
        new Secao("ARTE — CENÁRIO E MÁQUINAS", "Renata Nogueira Arangurem"),
        new Secao("INTERFACE", "Renata Nogueira Arangurem"),
        new Secao("TRILHA SONORA", "Mateus De Boit"),
        new Secao("EFEITOS SONOROS", "Mateus De Boit"),
        new Secao("ROTEIRO E TEXTOS", "Mateus De Boit"),
        new Secao("TRADUÇÃO", "Mateus De Boit")
    };

    /// <summary>
    /// Ainda não há testadores. Quando houver, basta preencher esta lista que
    /// a seção "TESTES" aparece na tela.
    /// </summary>
    public static readonly string[] testadores = new string[0];

    public static readonly Musica[] musicasTerceiros =
    {
        new Musica("Introducing Pikuniku, Trailer 1", "Calum Bowen (Bo En)"),
        new Musica("The Forest", "Calum Bowen (Bo En)")
    };

    public const string OrigemMusicas =
        "Música original de Pikuniku — Sectordub / Devolver Digital";

    public const string LinksMusicas =
        "pikuniku.net  ·  calumbowen.bandcamp.com";
}
