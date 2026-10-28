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

    private HabilidadeDados dados;
    private ArvoreHabilidadesUI arvore;

    void Awake()
    {
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
            fundoNo.color = corDesbloqueado;
            textoCusto.text = "OK";
            textoCusto.color = TemaUI.TextoForte;
        }
        else if (disponivel)
        {
            fundoNo.color = corDisponivel;
            textoCusto.text = "R$ " + dados.custo;
            textoCusto.color = GameManager.Instance.dinheiro >= dados.custo
                ? TemaUI.Acento
                : TemaUI.Vermelho;
        }
        else
        {
            fundoNo.color = corBloqueado;
            textoCusto.text = "X";
            textoCusto.color = TemaUI.TextoFraco;
        }
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