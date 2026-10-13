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

    private Color corPodeComprar = new Color(0.2f, 0.9f, 0.2f);
    private Color corSemDinheiro = new Color(0.9f, 0.2f, 0.2f);

    void Update()
    {
        AtualizarCorPreco();
    }

    public void Configurar(PedraDados pedra)
    {
        dados = pedra;
        imagemPedra.color = pedra.cor;
        textoNome.text = pedra.nome;
        textoDescricao.text = pedra.descricao;
        textoPreco.text = pedra.precoCompra == 0 ? "Grátis" : "R$ " + pedra.precoCompra;
        botaoComprar.onClick.AddListener(() => LojaManager.Instance.ComprarPedra(dados));
        AtualizarCorPreco();
    }

    void AtualizarCorPreco()
    {
        if (dados == null) return;

        int preco = dados.precoCompra;

        if (StatusManager.Instance.TemHabilidade("desconto_universitario"))
            preco = Mathf.RoundToInt(preco * 0.75f);

        if (preco == 0)
        {
            textoPreco.color = corPodeComprar;
            return;
        }

        textoPreco.color = GameManager.Instance.dinheiro >= preco
            ? corPodeComprar
            : corSemDinheiro;
    }
}