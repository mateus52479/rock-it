using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Realce discreto dos botões: sobe 1 px no hover e afunda 1 px no clique.
/// Não usa escala, para manter o desenho pixelado sem o efeito de "vidro".
/// </summary>
public class BotaoAnimado : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    private RectTransform rt;
    private Vector2 posicaoBase;
    private bool baseGuardada;

    void Awake()
    {
        rt = transform as RectTransform;
    }

    void OnDisable()
    {
        if (rt != null && baseGuardada)
            rt.anchoredPosition = posicaoBase;
    }

    void GuardarBase()
    {
        if (baseGuardada || rt == null)
            return;

        posicaoBase = rt.anchoredPosition;
        baseGuardada = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GuardarBase();
        if (rt != null)
            rt.anchoredPosition = posicaoBase + new Vector2(0f, 1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (rt != null && baseGuardada)
            rt.anchoredPosition = posicaoBase;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GuardarBase();
        if (rt != null)
            rt.anchoredPosition = posicaoBase + new Vector2(0f, -1f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (rt != null && baseGuardada)
            rt.anchoredPosition = posicaoBase + new Vector2(0f, 1f);
    }
}
