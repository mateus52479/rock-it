using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Ajuda a montar UI simples em runtime. Usada pela tela de slots e pelo
/// sistema de diálogos, para não depender de editar as cenas.
/// </summary>
public static class UIUtil
{
    public static GameObject CriarCanvas(string nome, int sortingOrder)
    {
        GameObject go = new GameObject(
            nome,
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GarantirEventSystem();

        return go;
    }

    public static void GarantirEventSystem()
    {
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
            return;

        GameObject es = new GameObject(
            "EventSystem",
            typeof(UnityEngine.EventSystems.EventSystem),
            typeof(UnityEngine.EventSystems.StandaloneInputModule)
        );
    }

    public static RectTransform CriarRetangulo(Transform pai, string nome)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        return rt;
    }

    public static RectTransform Esticar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    public static Image CriarImagem(
        Transform pai,
        string nome,
        Color cor,
        Vector2 tamanho,
        Vector2 posicao)
    {
        RectTransform rt = CriarRetangulo(pai, nome);
        rt.sizeDelta = tamanho;
        rt.anchoredPosition = posicao;

        Image img = rt.gameObject.AddComponent<Image>();
        img.color = cor;

        return img;
    }

    public static TextMeshProUGUI CriarTexto(
        Transform pai,
        string nome,
        string texto,
        float tamanhoFonte,
        Vector2 tamanho,
        Vector2 posicao,
        TextAlignmentOptions alinhamento = TextAlignmentOptions.Center)
    {
        RectTransform rt = CriarRetangulo(pai, nome);
        rt.sizeDelta = tamanho;
        rt.anchoredPosition = posicao;

        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = tamanhoFonte;
        tmp.alignment = alinhamento;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;

        return tmp;
    }

    public static Button CriarBotao(
        Transform pai,
        string nome,
        string texto,
        Vector2 tamanho,
        Vector2 posicao,
        Color cor,
        System.Action aoClicar)
    {
        Image img = CriarImagem(pai, nome, cor, tamanho, posicao);

        Button btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock cores = btn.colors;
        cores.normalColor = Color.white;
        cores.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
        cores.pressedColor = new Color(0.72f, 0.72f, 0.72f);
        cores.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);
        cores.colorMultiplier = 1f;
        btn.colors = cores;

        TextMeshProUGUI label = CriarTexto(
            img.transform,
            "Texto",
            texto,
            24f,
            new Vector2(tamanho.x - 12f, tamanho.y - 6f),
            Vector2.zero
        );

        label.fontStyle = FontStyles.Bold;

        if (aoClicar != null)
            btn.onClick.AddListener(() => aoClicar());

        return btn;
    }
}
