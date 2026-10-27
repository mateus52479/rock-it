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

    private Color corPodeComprar = TemaUI.Acento;
    private Color corSemDinheiro = TemaUI.Vermelho;

    void Awake()
    {
        OrganizarLinha();
        AplicarEstilo();
    }

    /// <summary>
    /// O prefab vinha com os filhos fora de ordem e com larguras fixas. Aqui
    /// a linha vira: pedra | nome + descrição | preço | comprar.
    /// </summary>
    void OrganizarLinha()
    {
        TemaUI.PrepararLinha(transform, 112f);

        Image fundo = GetComponent<Image>();
        if (fundo != null)
            TemaUI.AplicarPlaca(fundo, TemaUI.SpriteLinhaItem(), Color.white);

        RectTransform coluna = TemaUI.ColunaTextos(transform, 2f);

        if (textoNome != null)
        {
            textoNome.rectTransform.SetParent(coluna, false);
            TemaUI.AjustarAltura(textoNome, 26f);
            textoNome.textWrappingMode = TextWrappingModes.NoWrap;
            textoNome.overflowMode = TextOverflowModes.Overflow;
        }

        if (textoDescricao != null)
        {
            textoDescricao.rectTransform.SetParent(coluna, false);
            TemaUI.AjustarAltura(textoDescricao, 48f);
            textoDescricao.textWrappingMode = TextWrappingModes.Normal;
            textoDescricao.overflowMode = TextOverflowModes.Ellipsis;
        }

        if (imagemPedra != null)
        {
            imagemPedra.rectTransform.SetParent(transform, false);
            TemaUI.AjustarAltura(imagemPedra, 84f);
            TemaUI.AjustarLargura(imagemPedra, 84f);
            imagemPedra.preserveAspect = true;
        }

        if (textoPreco != null)
        {
            textoPreco.rectTransform.SetParent(transform, false);
            TemaUI.AjustarAltura(textoPreco, 40f);
            TemaUI.AjustarLargura(textoPreco, 170f);
            textoPreco.alignment = TextAlignmentOptions.Right;
            textoPreco.textWrappingMode = TextWrappingModes.NoWrap;
            textoPreco.overflowMode = TextOverflowModes.Overflow;
        }

        if (botaoComprar != null)
        {
            botaoComprar.transform.SetParent(transform, false);
            TemaUI.AjustarAltura(botaoComprar, 44f);
            TemaUI.AjustarLargura(botaoComprar, 132f);
        }

        // Define a ordem visual da esquerda para a direita.
        if (imagemPedra != null)
            imagemPedra.transform.SetAsLastSibling();

        coluna.SetAsLastSibling();

        if (textoPreco != null)
            textoPreco.transform.SetAsLastSibling();

        if (botaoComprar != null)
            botaoComprar.transform.SetAsLastSibling();
    }

    void AplicarEstilo()
    {
        TemaUI.EstilizarTexto(
            textoNome, 20f, TemaUI.TextoForte, FontStyles.Bold,
            TextAlignmentOptions.Left);

        TemaUI.EstilizarTexto(
            textoDescricao, 13f, TemaUI.TextoSuave, FontStyles.Normal,
            TextAlignmentOptions.Left);

        TemaUI.EstilizarTexto(
            textoPreco, 20f, TemaUI.Texto, FontStyles.Bold,
            TextAlignmentOptions.Right);

        if (botaoComprar != null)
            TemaUI.EstilizarBotao(botaoComprar, TemaUI.EstiloBotao.Verde, 15f);
    }

    void Update()
    {
        AtualizarPreco();
    }

    public void Configurar(PedraDados pedra)
    {
        dados = pedra;

        if (imagemPedra != null)
        {
            // Recorta a margem transparente do PNG para o desenho preencher
            // o slot da loja.
            imagemPedra.sprite = SpriteUtil.RecortarParaVisivel(pedra.sprite100);
            imagemPedra.color = Color.white;
            imagemPedra.preserveAspect = true;
        }

        if (textoNome != null)
            textoNome.text = pedra.nome;

        if (textoDescricao != null)
            textoDescricao.text = pedra.descricao;

        if (botaoComprar != null)
            botaoComprar.onClick.AddListener(() => LojaManager.Instance.ComprarPedra(dados));

        AtualizarPreco();
    }

    void AtualizarPreco()
    {
        if (dados == null || textoPreco == null) return;

        // A primeira Pedra Comum é grátis e garantida com o prêmio máximo.
        bool primeiraGratis =
            dados.nome == "Pedra Comum" &&
            SaveManager.Instance != null &&
            !SaveManager.Instance.PrimeiraComumComprada();

        // Sem dinheiro e sem pedras na mesa, a pedra mais barata fica grátis
        // (socorro), em vez de o jogo gerar uma pedra sozinho.
        bool gratis = primeiraGratis || LojaManager.EhPedraDeSocorro(dados);

        int preco = dados.precoCompra;

        if (!gratis &&
            StatusManager.Instance.TemHabilidade("desconto_universitario"))
        {
            preco = Mathf.RoundToInt(preco * 0.75f);
        }

        string texto = gratis
            ? "Grátis"
            : "R$ " + NumeroUtil.Formatar(preco, 0);

        // Só reescreve quando muda, para não gerar lixo a cada frame.
        if (texto != ultimoPrecoExibido)
        {
            ultimoPrecoExibido = texto;
            textoPreco.text = texto;
        }

        textoPreco.color = (gratis || preco == 0 || GameManager.Instance.dinheiro >= preco)
            ? corPodeComprar
            : corSemDinheiro;
    }
}
