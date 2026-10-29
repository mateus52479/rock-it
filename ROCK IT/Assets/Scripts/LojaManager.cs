
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LojaManager : MonoBehaviour
{
    public static LojaManager Instance;

    /// <summary>Disparado quando uma pedra é comprada com sucesso.</summary>
    public static event System.Action<PedraDados> OnPedraComprada;

    /// <summary>Disparado quando a loja abre ou fecha (usado pelo tutorial).</summary>
    public static event System.Action OnLojaAberta;
    public static event System.Action OnLojaFechada;

    public static bool LojaAberta { get; private set; }

    [Header("Loja UI")]
    public GameObject painelLoja;
    public Transform containerItens;
    public GameObject prefabItemLoja;

    [Header("Spawn de Pedras")]
    public Transform containerPedras;
    public GameObject prefabPedra;

    void Awake()
    {
        Instance = this;

        LojaAberta = false;

        if (painelLoja != null)
            painelLoja.SetActive(false);
    }

    void Start()
    {
        PopularLoja();
    }

    public void AbrirLoja()
    {
        if (painelLoja != null)
            painelLoja.SetActive(true);

        LojaAberta = true;

        if (OnLojaAberta != null)
            OnLojaAberta();
    }

    public void FecharLoja()
    {
        if (painelLoja != null)
            painelLoja.SetActive(false);

        LojaAberta = false;

        if (OnLojaFechada != null)
            OnLojaFechada();
    }

    void PopularLoja()
    {
        if (PedraDatabase.Instance == null)
        {
            Debug.LogError("LojaManager: PedraDatabase não encontrado.");
            return;
        }

        if (containerItens == null || prefabItemLoja == null)
        {
            Debug.LogError("LojaManager: referências dos itens da loja não configuradas.");
            return;
        }

        foreach (Transform filho in containerItens)
            Destroy(filho.gameObject);

        foreach (PedraDados pedra in PedraDatabase.Instance.todasAsPedras)
        {
            GameObject item = Instantiate(prefabItemLoja, containerItens);

            ItemLoja itemScript = item.GetComponent<ItemLoja>();

            if (itemScript != null)
                itemScript.Configurar(pedra);
            else
                Debug.LogError("O prefab de item não possui ItemLoja.");
        }
    }

    public void ComprarPedra(PedraDados pedra, bool gratisForcado = false)
    {
        if (pedra == null)
        {
            Debug.LogError("LojaManager: pedra inválida.");
            return;
        }

        if (SaveManager.Instance == null ||
            GameManager.Instance == null ||
            StatusManager.Instance == null)
        {
            Debug.LogError("LojaManager: um dos managers necessários não existe.");
            return;
        }

        if (prefabPedra == null)
        {
            Debug.LogError("LojaManager: prefabPedra não foi configurado.");
            return;
        }

        // Verifica se esta é a primeira compra da Pedra Comum.
        bool primeiraComum =
            pedra.nome == "Pedra Comum" &&
            !SaveManager.Instance.PrimeiraComumComprada();

        // Pedra de socorro: o jogador está sem dinheiro e sem pedras na
        // mesa. Em vez de o jogo gerar uma pedra do nada, a pedra mais
        // barata fica grátis na loja e o próprio jogador compra.
        bool socorro =
            !primeiraComum &&
            pedra == PedraMaisBarata() &&
            CalcularSemSaida();

        // A primeira Pedra Comum é gratuita, e a de socorro também.
        int preco = (primeiraComum || socorro) ? 0 : pedra.precoCompra;

        // Desconto Universitário: reduz o preço em 25%.
        if (!primeiraComum && !socorro &&
            StatusManager.Instance.TemHabilidade("desconto_universitario"))
        {
            preco = Mathf.RoundToInt(preco * 0.75f);
        }

        // Cashback: 50% de chance de tornar a compra gratuita.
        bool gratis = gratisForcado || socorro;

        if (!primeiraComum && !socorro &&
            StatusManager.Instance.TemHabilidade("cashback"))
        {
            if (Random.value < 0.5f)
                gratis = true;
        }

        // Cobra somente quando a compra não é gratuita.
        if (!gratis && preco > 0)
        {
            if (!GameManager.Instance.GastarDinheiro(preco))
            {
                Debug.Log("Sem dinheiro suficiente!");
                return;
            }
        }

        // Cria a pedra antes de registrar a primeira compra.
        float x = Random.Range(-3f, 3f);
        float y = Random.Range(-2f, 1f);

        Vector3 posSpawn = new Vector3(x, y, 0f);

        GameObject novaPedra = Instantiate(
            prefabPedra,
            posSpawn,
            Quaternion.identity
        );

        PedraManager pm = novaPedra.GetComponent<PedraManager>();

        if (pm == null)
        {
            Debug.LogError(
                "O prefab da pedra não possui PedraManager."
            );

            Destroy(novaPedra);
            return;
        }

        // O segundo argumento informa se a recompensa máxima
        // deve ser garantida nesta pedra.
        pm.Inicializar(pedra, primeiraComum);

        // Marca a primeira compra somente após criar a pedra.
        if (primeiraComum)
            SaveManager.Instance.MarcarPrimeiraComum();

        AudioManager.Compra();

        // A loja continua aberta depois da compra: o jogador pode comprar
        // várias pedras seguidas sem precisar reabrir o painel. O tutorial
        // fecha a loja no passo final dele, com "fecharLoja".
        if (OnPedraComprada != null)
            OnPedraComprada(pedra);
    }

    /// <summary>
    /// Verdadeiro quando o jogador não tem dinheiro nem pedras na mesa.
    /// Nessa situação a pedra mais barata fica grátis na loja, em vez de o
    /// jogo gerar uma pedra do nada.
    /// </summary>
    public static bool EstaSemSaida { get; private set; }

    private float proximaVerificacaoResgate = 0f;

    void Update()
    {
        if (Time.unscaledTime < proximaVerificacaoResgate) return;
        proximaVerificacaoResgate = Time.unscaledTime + 1f;

        EstaSemSaida = CalcularSemSaida();
    }

    static bool CalcularSemSaida()
    {
        if (GameManager.Instance == null || PedraDatabase.Instance == null)
            return false;

        PedraDados maisBarata = PedraMaisBarata();

        if (maisBarata != null &&
            GameManager.Instance.dinheiro >= maisBarata.precoCompra)
            return false;

        // Ainda tem pedra na mesa? Então o jogador consegue dinheiro sozinho.
        PedraManager[] pedrasNaMesa =
            FindObjectsByType<PedraManager>(FindObjectsSortMode.None);

        return pedrasNaMesa.Length == 0;
    }

    static PedraDados PedraMaisBarata()
    {
        if (PedraDatabase.Instance == null) return null;

        PedraDados[] todas = PedraDatabase.Instance.todasAsPedras;

        if (todas == null || todas.Length == 0) return null;

        PedraDados maisBarata = todas[0];

        foreach (PedraDados p in todas)
            if (p != null && p.precoCompra < maisBarata.precoCompra)
                maisBarata = p;

        return maisBarata;
    }

    /// <summary>
    /// A pedra mais barata fica grátis enquanto o jogador está sem saída.
    /// Usado pela loja para mostrar "Grátis" no item.
    /// </summary>
    public static bool EhPedraDeSocorro(PedraDados pedra)
    {
        if (pedra == null || !EstaSemSaida) return false;

        return pedra == PedraMaisBarata();
    }
}
