
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LojaManager : MonoBehaviour
{
    public static LojaManager Instance;

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
    }

    public void FecharLoja()
    {
        if (painelLoja != null)
            painelLoja.SetActive(false);
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

        // A primeira Pedra Comum é gratuita.
        int preco = primeiraComum ? 0 : pedra.precoCompra;

        // Desconto Universitário: reduz o preço em 25%.
        if (!primeiraComum &&
            StatusManager.Instance.TemHabilidade("desconto_universitario"))
        {
            preco = Mathf.RoundToInt(preco * 0.75f);
        }

        // Cashback: 50% de chance de tornar a compra gratuita.
        bool gratis = gratisForcado;

        if (!primeiraComum &&
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

        if (!gratisForcado)
            FecharLoja();
    }

    private float proximaVerificacaoResgate = 0f;

    void Update()
    {
        if (Time.unscaledTime < proximaVerificacaoResgate) return;
        proximaVerificacaoResgate = Time.unscaledTime + 1f;

        if (GameManager.Instance == null ||
            PedraDatabase.Instance == null ||
            prefabPedra == null)
            return;

        if (GameManager.Instance.dinheiro >= 1f)
            return;

        // Sem dinheiro e sem nenhuma pedra na mesa: o jogador ficaria
        // travado para sempre. Dá uma Pedra Comum de socorro.
        PedraManager[] pedrasNaMesa =
            FindObjectsByType<PedraManager>(FindObjectsSortMode.None);

        if (pedrasNaMesa.Length > 0)
            return;

        PedraDados comum = PedraDatabase.Instance.todasAsPedras[0];

        if (comum != null)
            ComprarPedra(comum, true);
    }
}
