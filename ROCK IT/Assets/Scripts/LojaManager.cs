using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LojaManager : MonoBehaviour
{
    public static LojaManager Instance;

    [Header("Loja UI")]
    public GameObject painelLoja;           // painel inteiro da loja
    public Transform containerItens;        // ScrollView > Viewport > Content
    public GameObject prefabItemLoja;       // prefab de uma linha da loja

    [Header("Spawn de Pedras")]
    public Transform containerPedras;       // onde as pedras spawnadas ficam na cena
    public GameObject prefabPedra;          // prefab da PedraUI

    void Awake()
    {
        Instance = this;
        painelLoja.SetActive(false);
    }

    void Start()
    {
        PopularLoja();
    }

    public void AbrirLoja()
    {
        painelLoja.SetActive(true);
    }

    public void FecharLoja()
    {
        painelLoja.SetActive(false);
    }

    void PopularLoja()
    {
        foreach (PedraDados pedra in PedraDatabase.Instance.todasAsPedras)
        {
            GameObject item = Instantiate(prefabItemLoja, containerItens);
            ItemLoja itemScript = item.GetComponent<ItemLoja>();
            itemScript.Configurar(pedra);
        }
    }

    public void ComprarPedra(PedraDados pedra)
    {
        int preco = pedra.precoCompra;

        // Desconto Universitário — 25% de desconto
        if (StatusManager.Instance.TemHabilidade("desconto_universitario"))
            preco = Mathf.RoundToInt(preco * 0.75f);

        // Cashback — 50% de chance de não gastar nada
        bool gratis = false;
        if (StatusManager.Instance.TemHabilidade("cashback"))
            if (Random.value < 0.5f)
                gratis = true;

        if (!gratis)
        {
            if (!GameManager.Instance.GastarDinheiro(preco))
            {
                Debug.Log("Sem dinheiro suficiente!");
                return;
            }
        }
        else
        {
            Debug.Log("Cashback! Pedra grátis.");
        }

        float x = Random.Range(-3f, 3f);
        float y = Random.Range(-2f, 1f);
        Vector3 posSpawn = new Vector3(x, y, 0f);

        GameObject novaPedra = Instantiate(prefabPedra, posSpawn, Quaternion.identity);
        PedraManager pm = novaPedra.GetComponent<PedraManager>();
        pm.Inicializar(pedra);

        FecharLoja();
    }
}