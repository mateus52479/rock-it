using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Monta o chão de pedra como fundo da cena do Jogo. O desenho é esticado
/// proporcionalmente (cover) para cobrir a câmera em qualquer resolução,
/// então a mesa, as máquinas e as pedras ficam por cima dele.
/// Fica atrás de tudo por causa do sortingOrder bem negativo.
/// </summary>
public class ChaoFundo : MonoBehaviour
{
    private const string CaminhoChao = "Sprites/Chao";
    private const string CenaDoJogo = "Jogo";
    private const int OrdemDesenho = -100;

    [Tooltip("Margem extra (em fração) para o fundo nunca terminar dentro da tela.")]
    public float margemTela = 0.04f;

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
            desenho.drawMode = SpriteDrawMode.Simple;
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

        Vector2 tamanhoSprite = desenho.sprite != null
            ? (Vector2)desenho.sprite.bounds.size
            : new Vector2(8.25f, 5.67f);

        if (tamanhoSprite.x <= 0f || tamanhoSprite.y <= 0f)
            return;

        // Cover: aumenta proporcionalmente até cobrir a tela inteira nos dois
        // eixos, sem repetir o desenho e sem deixar sobras nas bordas.
        float escala = Mathf.Max(largura / tamanhoSprite.x, altura / tamanhoSprite.y);
        escala *= 1f + Mathf.Max(0f, margemTela);

        desenho.transform.localScale = Vector3.one * escala;

        Vector3 posicao = cameraJogo.transform.position;
        desenho.transform.position = new Vector3(posicao.x, posicao.y, 0f);
    }
}
