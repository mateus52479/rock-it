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
        if (!GameManager.Instance.GastarDinheiro(pedra.precoCompra))
        {
            Debug.Log("Sem dinheiro suficiente!");
            return;
        }

        // Posição aleatória na cena dentro de uma área de mesa
        float x = Random.Range(-3f, 3f);
        float y = Random.Range(-2f, 1f);
        Vector3 posSpawn = new Vector3(x, y, 0f);

        GameObject novaPedra = Instantiate(prefabPedra, posSpawn, Quaternion.identity);
        PedraManager pm = novaPedra.GetComponent<PedraManager>();
        pm.Inicializar(pedra);

        FecharLoja();
    }
}