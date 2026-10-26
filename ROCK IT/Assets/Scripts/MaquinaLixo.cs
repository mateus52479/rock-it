using UnityEngine;

public class MaquinaLixo : MonoBehaviour, IMaquinaRecebedora
{
    public SpriteRenderer spriteRenderer;

    [Header("Sprites")]
    [Tooltip("Lata de lixo comum. Se ficar vazio, carrega de Resources/Sprites/Maquinas.")]
    public Sprite spriteLixoNormal;

    [Tooltip("Lata de lixo de ouro (upgrade). Se ficar vazio, carrega de Resources/Sprites/Maquinas.")]
    public Sprite spriteLixoLuxo;

    [Header("Cores de reserva")]
    [Tooltip("Usadas só se o sprite correspondente não for encontrado.")]
    public Color corLixoNormal = Color.gray;
    public Color corLixoLuxo = Color.yellow;

    private const string CaminhoLataNormal = "Sprites/Maquinas/LATA DE LIXO";
    private const string CaminhoLataLuxo = "Sprites/Maquinas/LATA DE LIXO DE OURO";

    private string idAtual = "";
    private bool ativo = false;

    void Awake()
    {
        GarantirSprite();
        CarregarSprites();
    }

    void GarantirSprite()
    {
        // O spriteRenderer pode vir vazio da cena; resolve pelo componente
        // do próprio objeto para não estourar NullReferenceException.
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void CarregarSprites()
    {
        if (spriteLixoNormal == null)
            spriteLixoNormal = CarregarSprite(CaminhoLataNormal);

        if (spriteLixoLuxo == null)
            spriteLixoLuxo = CarregarSprite(CaminhoLataLuxo);
    }

    static Sprite CarregarSprite(string caminho)
    {
        Sprite sprite = Resources.Load<Sprite>(caminho);

        if (sprite != null)
            return sprite;

        // O PNG está no modo "Multiple" (sprite sheet), então o Load simples
        // pode não devolver nada. Nesse caso pega o primeiro recorte.
        Sprite[] recortes = Resources.LoadAll<Sprite>(caminho);

        return recortes != null && recortes.Length > 0 ? recortes[0] : null;
    }

    public void Ativar(string id)
    {
        GarantirSprite();
        CarregarSprites();

        idAtual = id;
        ativo = true;
        gameObject.SetActive(true);

        AplicarVisual();
    }

    void AplicarVisual()
    {
        if (spriteRenderer == null) return;

        bool luxo = idAtual == "lixo_luxo";
        Sprite alvo = luxo ? spriteLixoLuxo : spriteLixoNormal;

        if (alvo == null)
        {
            // Sem o sprite, mantém o visual antigo (quadrado tingido).
            spriteRenderer.color = luxo ? corLixoLuxo : corLixoNormal;
            return;
        }

        spriteRenderer.sprite = alvo;
        spriteRenderer.color = Color.white;

        AjustarCollider(alvo);
    }

    /// <summary>
    /// O collider acompanha o desenho da lata. A escala é a do transform na
    /// cena (posicionada e redimensionada no editor), então ela não é tocada.
    /// </summary>
    void AjustarCollider(Sprite sprite)
    {
        BoxCollider2D colisor = GetComponent<BoxCollider2D>();
        if (colisor == null) return;

        colisor.size = sprite.bounds.size;
        colisor.offset = sprite.bounds.center;
    }

    public bool PodeReceber(PedraManager pedra)
    {
        return ativo;
    }

    // Chamado pelo PedraManager quando a pedra é solta em cima do lixo.
    public void ReceberPedra(PedraManager pedra)
    {
        if (!ativo || pedra == null) return;

        if (idAtual == "lixo_luxo")
        {
            // Recebe 25% do valor de compra da pedra.
            float recompensa = pedra.GetPrecoCompra() * 0.25f;
            GameManager.Instance.AdicionarDinheiro(recompensa);
            Debug.Log("Lixo de Luxo: +" + recompensa);
        }

        Destroy(pedra.gameObject);
    }
}
