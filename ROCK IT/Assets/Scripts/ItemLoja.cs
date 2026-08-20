using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemLoja : MonoBehaviour
{
    public Image imagemPedra;
    public TextMeshProUGUI textoNome;
    public TextMeshProUGUI textoDescricao;
    public TextMeshProUGUI textoPreco;
    public Button botaoComprar;

    private PedraDados dados;

    public void Configurar(PedraDados pedra)
    {
        dados = pedra;
        imagemPedra.color = pedra.cor;
        textoNome.text = pedra.nome;
        textoDescricao.text = pedra.descricao;
        textoPreco.text = pedra.precoCompra == 0 ? "Grátis" : "R$ " + pedra.precoCompra;
        botaoComprar.onClick.AddListener(() => LojaManager.Instance.ComprarPedra(dados));
    }
}