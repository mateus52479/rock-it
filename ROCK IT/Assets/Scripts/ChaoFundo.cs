using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Monta o chão de tijolos como fundo da cena do Jogo. O desenho é repetido
/// (draw mode Tiled) e redimensionado para cobrir a câmera em qualquer
/// resolução, então a mesa, as máquinas e as pedras ficam por cima dele.
/// Fica atrás de tudo por causa do sortingOrder bem negativo.
/// </summary>
public class ChaoFundo : MonoBehaviour
{
    private const string CaminhoChao = "Sprites/Chao";
    private const string CenaDoJogo = "Jogo";
    private const int OrdemDesenho = -100;

    [Tooltip("Margem extra de tiles para o fundo nunca terminar dentro da tela.")]
    public float margemTiles = 1f;

    private static ChaoFundo hook;

    private SpriteRenderer desenho;
    private Camera cameraJogo;

    private int larguraTela = -1;
    private int alturaTela = -1;
    private bool tentouMontar;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CriarHook()
    {
        if (hook != null)
            return;

        GameObject go = new GameObject("ChaoFundoHook");
        Object.DontDestroyOnLoad(go);
        hook = go.AddComponent<ChaoFundo>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
    }

    void Start()
    {
        // A primeira cena também dispara sceneLoaded, mas caso o componente
        // entre depois dela, garante o fundo aqui.
        if (SceneManager.GetActiveScene().name == CenaDoJogo)
            Montar();
    }

    void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        if (cena.name == CenaDoJogo)
            Montar();
    }

    void Montar()
    {
        tentouMontar = true;

        Camera camera = Camera.main;

        if (camera == null)
        {
            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Exclude);

            foreach (Camera c in cameras)
            {
                if (c.isActiveAndEnabled)
                {
                    camera = c;
                    break;
                }
            }
        }

        if (camera == null)
            return;

        // A cena pode ser recarregada; nesse caso o desenho antigo foi
        // destruído junto e um novo precisa ser montado.
        if (desenho == null)
        {
            Sprite sprite = CarregarSprite();

            if (sprite == null)
            {
                Debug.LogWarning("ChaoFundo: sprite do chão não encontrado em Resources/" + CaminhoChao + ".");
                return;
            }

            GameObject go = new GameObject("ChaoFundo");
            desenho = go.AddComponent<SpriteRenderer>();
            desenho.sprite = sprite;
            desenho.drawMode = SpriteDrawMode.Tiled;
            desenho.tileMode = SpriteTileMode.Continuous;
            desenho.sortingOrder = OrdemDesenho;
            desenho.color = Color.white;
        }

        cameraJogo = camera;
        Ajustar();
    }

    static Sprite CarregarSprite()
    {
        Sprite sprite = Resources.Load<Sprite>(CaminhoChao);

        if (sprite != null)
            return sprite;

        // O PNG está no modo "Multiple" (sprite sheet), então o Load simples
        // pode não devolver nada. Nesse caso pega o primeiro recorte.
        Sprite[] recortes = Resources.LoadAll<Sprite>(CaminhoChao);

        return recortes != null && recortes.Length > 0 ? recortes[0] : null;
    }

    void LateUpdate()
    {
        if (desenho == null)
        {
            // A cena carregou direto no Play sem passar pelo sceneLoaded.
            if (!tentouMontar && SceneManager.GetActiveScene().name == CenaDoJogo)
                Montar();

            return;
        }

        if (Screen.width != larguraTela || Screen.height != alturaTela)
            Ajustar();
    }

    void Ajustar()
    {
        if (desenho == null)
            return;

        if (cameraJogo == null)
            cameraJogo = Camera.main;

        if (cameraJogo == null)
            return;

        larguraTela = Screen.width;
        alturaTela = Screen.height;

        float altura = cameraJogo.orthographicSize * 2f;
        float largura = altura * cameraJogo.aspect;

        Vector2 tile = desenho.sprite != null ? (Vector2)desenho.sprite.bounds.size : new Vector2(5.31f, 4.32f);

        // Uma margem de um tile em cada lado cobre a tela mesmo quando a
        // resolução muda ou a câmera se move um pouco.
        desenho.size = new Vector2(
            largura + tile.x * Mathf.Max(0f, margemTiles),
            altura + tile.y * Mathf.Max(0f, margemTiles)
        );

        Vector3 posicao = cameraJogo.transform.position;
        desenho.transform.position = new Vector3(posicao.x, posicao.y, 0f);
    }
}
