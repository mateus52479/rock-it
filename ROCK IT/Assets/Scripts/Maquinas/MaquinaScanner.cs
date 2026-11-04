using UnityEngine;

public class MaquinaScanner : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    [Header("Sprite")]
    [Tooltip("Scanner. Se ficar vazio, carrega de Resources/Sprites/Maquinas.")]
    public Sprite spriteScanner;

    [Tooltip("O Scanner é um upgrade passivo, não um aparelho na mesa: por isso não aparece no mapa. Ligue para exibir o desenho na cena.")]
    public bool aparecerNoMapa = false;

    [Tooltip("Ajuste de tamanho. O desenho é bem largo; a 0.5 fica com a largura da lixeira.")]
    public float escalaVisual = 0.5f;

    private const string CaminhoScanner = "Sprites/Maquinas/Scanner";

    private bool ativo = false;

    void Awake()
    {
        GarantirVisual();
    }

    public bool EstaAtivo() { return ativo; }

    public void Ativar()
    {
        GarantirVisual();

        ativo = true;
        gameObject.SetActive(true);
        AtualizarTodosContornos();
    }

    void GarantirVisual()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            return;

        if (!aparecerNoMapa)
        {
            // Sem isso, o quadrado de reserva salvo na cena apareceria ao
            // comprar o Scanner.
            spriteRenderer.enabled = false;
            return;
        }

        if (spriteScanner == null)
            spriteScanner = SpriteUtil.CarregarDeResources(CaminhoScanner);

        if (spriteScanner == null)
            return;

        spriteRenderer.enabled = true;
        spriteRenderer.sprite = spriteScanner;
        spriteRenderer.color = Color.white;
        transform.localScale = Vector3.one * escalaVisual;
    }

    /// <summary>
    /// Recalcula a cor do contorno de todas as pedras na mesa.
    /// Chamado ao comprar o Scanner e quando Sorte/Fortuna mudam (as duas
    /// mudam o valor médio da pedra, então a cor antiga ficava desatualizada).
    /// </summary>
    public static void AtualizarTodosContornos()
    {
        MaquinaManager manager = MaquinaManager.Instance;

        if (manager == null || !manager.TemMaquina("scanner"))
            return;

        PedraManager[] pedras = FindObjectsByType<PedraManager>(FindObjectsSortMode.None);

        foreach (PedraManager p in pedras)
        {
            if (p != null)
                p.AtualizarContornoScanner(true);
        }
    }

    // Chama isso quando uma nova pedra spawna.
    public static void NotificarNovaPedra(PedraManager pedra)
    {
        if (pedra == null) return;

        MaquinaManager manager = MaquinaManager.Instance;
        if (manager != null && manager.TemMaquina("scanner"))
            pedra.AtualizarContornoScanner(true);
    }
}
