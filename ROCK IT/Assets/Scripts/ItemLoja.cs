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
    private string ultimoPrecoExibido;

    private Color corPodeComprar = new Color(0.2f, 0.9f, 0.2f);
    private Color corSemDinheiro = new Color(0.9f, 0.2f, 0.2f);

    void Update()
    {
        AtualizarPreco();
    }

    public void Configurar(PedraDados pedra)
    {
        dados = pedra;
        // Recorta a margem transparente do PNG para o desenho preencher o slot da loja.
        imagemPedra.sprite = SpriteUtil.RecortarParaVisivel(pedra.sprite100);
        imagemPedra.color = Color.white;
        imagemPedra.preserveAspect = true;
        textoNome.text = pedra.nome;
        textoDescricao.text = pedra.descricao;
        botaoComprar.onClick.AddListener(() => LojaManager.Instance.ComprarPedra(dados));
        AtualizarPreco();
    }

    void AtualizarPreco()
    {
        if (dados == null) return;

        // A primeira Pedra Comum é grátis e garantida com o prêmio máximo.
        bool primeiraGratis =
            dados.nome == "Pedra Comum" &&
            SaveManager.Instance != null &&
            !SaveManager.Instance.PrimeiraComumComprada();

        int preco = dados.precoCompra;

        if (!primeiraGratis &&
            StatusManager.Instance.TemHabilidade("desconto_universitario"))
        {
            preco = Mathf.RoundToInt(preco * 0.75f);
        }

        string texto = primeiraGratis
            ? "Grátis"
            : "R$ " + NumeroUtil.Formatar(preco, 0);

        // Só reescreve quando muda, para não gerar lixo a cada frame.
        if (texto != ultimoPrecoExibido)
        {
            ultimoPrecoExibido = texto;
            textoPreco.text = texto;
        }

        textoPreco.color = (primeiraGratis || preco == 0 || GameManager.Instance.dinheiro >= preco)
            ? corPodeComprar
            : corSemDinheiro;
    }
}