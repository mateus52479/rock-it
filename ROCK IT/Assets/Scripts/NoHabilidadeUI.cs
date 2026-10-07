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

    private Color corDesbloqueado = new Color(0.2f, 0.8f, 0.2f);
    private Color corDisponivel = new Color(0.9f, 0.8f, 0.2f);
    private Color corBloqueado = new Color(0.4f, 0.4f, 0.4f);

    public void Configurar(HabilidadeDados d, ArvoreHabilidadesUI a)
    {
        dados = d;
        arvore = a;
        AtualizarVisual();
    }

    public void AtualizarVisual()
    {
        if (dados == null) return;

        textoNome.text = dados.nome;

        bool desbloqueado = StatusManager.Instance.TemHabilidade(dados.id);
        bool disponivel = !desbloqueado && RequisitosAtendidos();

        if (desbloqueado)
        {
            fundoNo.color = corDesbloqueado;
            textoCusto.text = "✓";
        }
        else if (disponivel)
        {
            fundoNo.color = corDisponivel;
            textoCusto.text = "R$ " + dados.custo;
        }
        else
        {
            fundoNo.color = corBloqueado;
            textoCusto.text = "[X]";
        }
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