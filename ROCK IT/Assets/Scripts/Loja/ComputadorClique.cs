using UnityEngine;
using UnityEngine.EventSystems;

public class ComputadorClique : MonoBehaviour
{
    [Header("Animação do computador")]
    [Tooltip("SpriteRenderer do computador. Se ficar vazio, usa o do próprio objeto.")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("Desenho com a janela aberta. Se ficar vazio, carrega de Resources/Sprites/Computador.")]
    public Sprite spriteComQuadrado;

    [Tooltip("Desenho com a tela vazia. Se ficar vazio, carrega de Resources/Sprites/Computador.")]
    public Sprite spriteSemQuadrado;

    [Tooltip("Tempo entre os dois desenhos, em segundos.")]
    public float intervaloAnimacao = 1f;

    private const string CaminhoComQuadrado = "Sprites/Computador/pc com quadrado";
    private const string CaminhoSemQuadrado = "Sprites/Computador/pc sem quadrado";

    private float tempoAnimacao;
    private bool mostrarComQuadrado = true;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        CarregarSprites();
        AplicarSprite(spriteComQuadrado);
        AjustarTamanho();
    }

    void Update()
    {
        if (spriteComQuadrado == null || spriteSemQuadrado == null)
            return;

        float intervalo = Mathf.Max(0.05f, intervaloAnimacao);
        tempoAnimacao += Time.deltaTime;

        if (tempoAnimacao < intervalo)
            return;

        tempoAnimacao -= intervalo;
        mostrarComQuadrado = !mostrarComQuadrado;
        AplicarSprite(mostrarComQuadrado ? spriteComQuadrado : spriteSemQuadrado);
    }

    void CarregarSprites()
    {
        if (spriteComQuadrado == null)
            spriteComQuadrado = SpriteUtil.CarregarDeResources(CaminhoComQuadrado);

        if (spriteSemQuadrado == null)
            spriteSemQuadrado = SpriteUtil.CarregarDeResources(CaminhoSemQuadrado);
    }

    void AplicarSprite(Sprite alvo)
    {
        if (spriteRenderer == null || alvo == null)
            return;

        spriteRenderer.sprite = alvo;
        spriteRenderer.color = Color.white;
    }

    /// <summary>
    /// A escala do computador vem da cena; aqui apenas refaz o collider para
    /// que a area de clique acompanhe os limites visiveis do sprite novo.
    /// </summary>
    void AjustarTamanho()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        Bounds visivel = SpriteUtil.ObterBoundsVisiveis(spriteRenderer.sprite);

        BoxCollider2D colisor = GetComponent<BoxCollider2D>();

        if (colisor != null)
        {
            colisor.size = visivel.size;
            colisor.offset = visivel.center;
        }
    }

    void OnMouseDown()
    {
        // Sem esta guarda, clicar em qualquer botão da UI que fique sobre o
        // computador (fechar as Configurações, por exemplo) atravessa o
        // canvas e abre a loja sem o jogador querer.
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        if (LojaManager.Instance != null)
            LojaManager.Instance.AbrirLoja();
    }
}
