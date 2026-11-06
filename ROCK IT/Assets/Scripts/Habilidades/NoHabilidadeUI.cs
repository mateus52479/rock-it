using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class NoHabilidadeUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Referências")]
    public Image fundoNo;
    public TextMeshProUGUI textoNome;
    public TextMeshProUGUI textoCusto;

    [Header("Bolas de habilidade")]
    [Tooltip("Bola cinza: habilidade ainda bloqueada. Se ficar vazia, carrega de Resources/Sprites/Habilidades.")]
    public Sprite spriteBloqueado;

    [Tooltip("Bola amarela: habilidade já pode ser comprada.")]
    public Sprite spriteDisponivel;

    [Tooltip("Bola verde: habilidade já comprada.")]
    public Sprite spriteComprado;

    private const string CaminhoBloqueado = "Sprites/Habilidades/bola de habilidade";
    private const string CaminhoDisponivel = "Sprites/Habilidades/bola de habilidade comprar";
    private const string CaminhoComprado = "Sprites/Habilidades/bola de habilidade pago";

    private HabilidadeDados dados;
    private ArvoreHabilidadesUI arvore;

    void Awake()
    {
        CarregarBolas();

        if (fundoNo != null)
        {
            TemaUI.AplicarPlaca(
                fundoNo, TemaUI.SpritePlacaNeutra(), fundoNo.color);
        }

        if (textoNome != null)
        {
            TemaUI.EstilizarTexto(
                textoNome, 14f, TemaUI.Texto, FontStyles.Bold, TextAlignmentOptions.Center);
            textoNome.textWrappingMode = TextWrappingModes.Normal;
        }

        if (textoCusto != null)
        {
            TemaUI.EstilizarTexto(
                textoCusto, 13f, TemaUI.TextoForte, FontStyles.Bold,
                TextAlignmentOptions.Center);
        }
    }

    private Color corDesbloqueado = new Color(0.30f, 0.42f, 0.22f);
    private Color corDisponivel = new Color(0.56f, 0.52f, 0.20f);
    private Color corBloqueado = new Color(0.23f, 0.22f, 0.25f);

    public void Configurar(HabilidadeDados d, ArvoreHabilidadesUI a)
    {
        dados = d;
        arvore = a;
        AtualizarVisual();
    }

    public void AtualizarVisual()
    {
        if (dados == null) return;

        textoNome.text = Traducao.T(dados.nome);

        bool desbloqueado = StatusManager.Instance.TemHabilidade(dados.id);
        bool disponivel = !desbloqueado && RequisitosAtendidos();

        if (desbloqueado)
        {
            AplicarBola(spriteComprado, corDesbloqueado);
            textoCusto.text = "OK";
            textoCusto.color = TemaUI.TextoForte;
        }
        else if (disponivel)
        {
            AplicarBola(spriteDisponivel, corDisponivel);
            textoCusto.text = "R$ " + dados.custo;
            textoCusto.color = GameManager.Instance.dinheiro >= dados.custo
                ? TemaUI.Acento
                : TemaUI.Vermelho;
        }
        else
        {
            AplicarBola(spriteBloqueado, corBloqueado);
            textoCusto.text = "X";
            textoCusto.color = TemaUI.TextoFraco;
        }
    }

    void CarregarBolas()
    {
        if (spriteBloqueado == null)
            spriteBloqueado = SpriteUtil.CarregarDeResources(CaminhoBloqueado);

        if (spriteDisponivel == null)
            spriteDisponivel = SpriteUtil.CarregarDeResources(CaminhoDisponivel);

        if (spriteComprado == null)
            spriteComprado = SpriteUtil.CarregarDeResources(CaminhoComprado);
    }

    void AplicarBola(Sprite bola, Color corReserva)
    {
        if (fundoNo == null)
            return;

        if (bola == null)
        {
            // Sem a bola, mantém o visual antigo de placa tingida.
            fundoNo.color = corReserva;
            return;
        }

        fundoNo.sprite = bola;
        fundoNo.type = Image.Type.Simple;
        fundoNo.color = Color.white;
    }

    void Update()
    {
        AtualizarVisual();
    }

    bool RequisitosAtendidos()
    {
        foreach (string req in dados.requisitoIds)
            if (!StatusManager.Instance.TemHabilidade(req))
                return false;
        return true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (StatusManager.Instance.TemHabilidade(dados.id)) return;
        if (!RequisitosAtendidos()) return;

        bool sucesso = StatusManager.Instance.ComprarHabilidade(dados);
        if (sucesso)
        {
            arvore.AtualizarNos();
            arvore.EsconderTooltip();
        }
        else
        {
            Debug.Log("Dinheiro insuficiente.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        arvore.MostrarTooltip(dados, Input.mousePosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        arvore.EsconderTooltip();
    }
}