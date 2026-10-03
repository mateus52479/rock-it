using UnityEngine;
using UnityEngine.UI;

public class NavegacaoLoja : MonoBehaviour
{
    public GameObject painelPedras;
    public GameObject painelStatus;
    public GameObject painelHabilidades;
    public GameObject painelMaquinas;

    public Button botaoPedras;
    public Button botaoStatus;
    public Button botaoHabilidades;
    public Button botaoMaquinas;

    void Start()
    {
        botaoPedras.onClick.AddListener(() => MostrarPainel(0));
        botaoStatus.onClick.AddListener(() => MostrarPainel(1));
        botaoHabilidades.onClick.AddListener(() => MostrarPainel(2));
        botaoMaquinas.onClick.AddListener(() => MostrarPainel(3));

        MostrarPainel(0);
    }

    void MostrarPainel(int indice)
    {
        painelPedras.SetActive(indice == 0);
        painelStatus.SetActive(indice == 1);
        painelHabilidades.SetActive(indice == 2);
        painelMaquinas.SetActive(indice == 3);
    }
}