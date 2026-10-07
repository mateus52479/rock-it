using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class ArvoreHabilidadesUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Referências")]
    public RectTransform areaArvore;        // objeto que move e escala
    public GameObject prefabNoHabilidade;
    public GameObject painelTooltip;        // painel do tooltip
    public TextMeshProUGUI textoTooltipNome;
    public TextMeshProUGUI textoTooltipDescricao;
    public TextMeshProUGUI textoTooltipCusto;

    [Header("Configurações")]
    public float velocidadeZoom = 0.1f;
    public float zoomMinimo = 0.5f;
    public float zoomMaximo = 2f;

    private Dictionary<string, RectTransform> nosInstanciados = new Dictionary<string, RectTransform>();
    private bool arrastando = false;
    private Vector2 ultimaPosicaoMouse;
    private bool mouseNaArea = false;

    // Posições de cada habilidade na teia
    private Dictionary<string, Vector2> posicoes = new Dictionary<string, Vector2>
    {
        { "hab_inicio",   new Vector2(  0,    0) },
        { "hab_forca1",   new Vector2(-200,  150) },
        { "hab_sorte1",   new Vector2( 200,  150) },
        { "hab_fortuna1", new Vector2(  0,   250) },
        { "hab_geral1",   new Vector2(-200, -150) },
        { "hab_forca2",   new Vector2(-350,  300) },
    };

    void OnEnable()
    {
        ConstruirArvore();
        if (painelTooltip != null)
            painelTooltip.SetActive(false);
    }

    void OnDisable()
    {
        LimparArvore();
    }

    void Update()
    {
        if (!mouseNaArea) return;

        if (Input.GetMouseButtonDown(0))
        {
            arrastando = true;
            ultimaPosicaoMouse = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
            arrastando = false;

        if (arrastando)
        {
            Vector2 delta = (Vector2)Input.mousePosition - ultimaPosicaoMouse;
            areaArvore.anchoredPosition += delta;
            ultimaPosicaoMouse = Input.mousePosition;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float novoZoom = areaArvore.localScale.x + scroll * velocidadeZoom * 10f;
            novoZoom = Mathf.Clamp(novoZoom, zoomMinimo, zoomMaximo);
            areaArvore.localScale = Vector3.one * novoZoom;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) { mouseNaArea = true; }
    public void OnPointerExit(PointerEventData eventData)
    {
        mouseNaArea = false;
        arrastando = false;
    }

    void ConstruirArvore()
    {
        LimparArvore();
        nosInstanciados.Clear();

        // Primeiro cria os nós
        foreach (HabilidadeDados hab in HabilidadeDatabase.Instance.todasHabilidades)
        {
            if (!posicoes.ContainsKey(hab.id)) continue;

            GameObject no = Instantiate(prefabNoHabilidade, areaArvore);
            RectTransform rt = no.GetComponent<RectTransform>();
            rt.anchoredPosition = posicoes[hab.id];

            NoHabilidadeUI noUI = no.GetComponent<NoHabilidadeUI>();
            noUI.Configurar(hab, this);

            nosInstanciados[hab.id] = rt;
        }

        // Depois desenha as linhas por baixo dos nós
        foreach (HabilidadeDados hab in HabilidadeDatabase.Instance.todasHabilidades)
        {
            foreach (string reqId in hab.requisitoIds)
            {
                if (nosInstanciados.ContainsKey(hab.id) && nosInstanciados.ContainsKey(reqId))
                    DesenharLinha(nosInstanciados[reqId], nosInstanciados[hab.id]);
            }
        }
    }

    void DesenharLinha(RectTransform de, RectTransform para)
    {
        GameObject linhaObj = new GameObject("Linha");
        RectTransform linhaRT = linhaObj.AddComponent<RectTransform>();
        linhaObj.transform.SetParent(areaArvore, false);
        linhaObj.transform.SetAsFirstSibling();

        Image img = linhaObj.AddComponent<Image>();
        img.color = new Color(0.8f, 0.7f, 0.3f, 1f);

        Vector2 posA = de.anchoredPosition;
        Vector2 posB = para.anchoredPosition;

        Vector2 direcao = posB - posA;
        float distancia = direcao.magnitude;
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;

        linhaRT.sizeDelta = new Vector2(distancia, 4f);
        linhaRT.anchoredPosition = posA + direcao * 0.5f;
        linhaRT.localRotation = Quaternion.Euler(0, 0, angulo);
        linhaRT.pivot = new Vector2(0.5f, 0.5f);
    }

    void LimparArvore()
    {
        foreach (Transform filho in areaArvore)
            Destroy(filho.gameObject);
        nosInstanciados.Clear();
    }

    public void AtualizarNos()
    {
        foreach (var par in nosInstanciados)
        {
            NoHabilidadeUI noUI = par.Value.GetComponent<NoHabilidadeUI>();
            if (noUI != null) noUI.AtualizarVisual();
        }
    }

    public void MostrarTooltip(HabilidadeDados dados, Vector2 posicaoMouse)
    {
        if (painelTooltip == null) return;
        painelTooltip.SetActive(true);

        textoTooltipNome.text = dados.nome;
        textoTooltipDescricao.text = dados.descricao;

        bool desbloqueado = StatusManager.Instance.TemHabilidade(dados.id);
        textoTooltipCusto.text = desbloqueado ? "✓ Comprado" : "R$ " + dados.custo;

        RectTransform tooltipRT = painelTooltip.GetComponent<RectTransform>();
        tooltipRT.position = posicaoMouse + new Vector2(10, 10);
    }

    public void EsconderTooltip()
    {
        if (painelTooltip != null)
            painelTooltip.SetActive(false);
    }
}