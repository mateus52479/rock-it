using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Escurece o fundo enquanto uma pedra está ampliada para ser quebrada e
/// mostra um botão X para sair desse modo. O escurecimento fica atrás da
/// interface do jogo (tabela de prêmios e HUD continuam visíveis) e o botão
/// desce para trás dos menus quando pause ou configurações abrem.
/// </summary>
public class PedraFocoUI : MonoBehaviour
{
    /// <summary>Atrás da tabela de prêmios (-1) e de toda a interface do jogo (0).</summary>
    private const int OrdemFundo = -2;

    /// <summary>Acima do HUD, mas abaixo do tutorial (300) e do jackpot (350).</summary>
    private const int OrdemBotao = 250;

    /// <summary>Quando um menu abre, o botão desce para trás dele.</summary>
    private const int OrdemAtrasDoMenu = -1;

    private const float AlphaFundo = 0.5f;
    private const float DuracaoFade = 0.25f;

    private static PedraFocoUI instancia;

    private Canvas canvasFundo;
    private Canvas canvasBotao;
    private Image fundo;
    private CanvasGroup grupoBotao;
    private Coroutine rotinaFade;
    private PedraManager pedraAtual;

    /// <summary>Pedra que está em foco agora, ou null quando ninguém está ampliado.</summary>
    public static PedraManager PedraAtual
    {
        get { return instancia != null ? instancia.pedraAtual : null; }
    }

    /// <summary>Escurece o fundo e mostra o X para a pedra que acabou de ser ampliada.</summary>
    public static void Mostrar(PedraManager pedra)
    {
        if (pedra == null)
            return;

        if (instancia == null)
            instancia = Criar();

        instancia.Apresentar(pedra);
    }

    /// <summary>
    /// Clareia o fundo e esconde o X. Se outra pedra já assumiu o foco, não
    /// fecha o efeito dela.
    /// </summary>
    public static void Esconder(PedraManager pedra)
    {
        if (instancia == null)
            return;

        if (pedra != null && instancia.pedraAtual != pedra)
            return;

        instancia.pedraAtual = null;
        instancia.ExecutarFade(0f);
    }

    static PedraFocoUI Criar()
    {
        GameObject raiz = new GameObject("PedraFocoUI");
        PedraFocoUI ui = raiz.AddComponent<PedraFocoUI>();
        ui.Construir();
        return ui;
    }

    void Construir()
    {
        canvasFundo = UIUtil.CriarCanvas("PedraFocoFundo", OrdemFundo)
            .GetComponent<Canvas>();

        fundo = UIUtil.CriarImagem(
            canvasFundo.transform,
            "Escurecimento",
            new Color(0f, 0f, 0f, 0f),
            Vector2.zero,
            Vector2.zero);

        UIUtil.Esticar((RectTransform)fundo.transform);

        // O escurecimento não pode bloquear o clique na pedra.
        fundo.raycastTarget = false;

        canvasBotao = UIUtil.CriarCanvas("PedraFocoBotoes", OrdemBotao)
            .GetComponent<Canvas>();

        grupoBotao = canvasBotao.gameObject.AddComponent<CanvasGroup>();
        grupoBotao.alpha = 0f;
        grupoBotao.blocksRaycasts = false;
        grupoBotao.interactable = false;

        Button botao = UIUtil.CriarBotao(
            canvasBotao.transform,
            "BotaoSair",
            "X",
            new Vector2(64f, 64f),
            Vector2.zero,
            TemaUI.Vermelho,
            Sair);

        RectTransform rt = (RectTransform)botao.transform;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-42f, -42f);

        TextMeshProUGUI rotulo = botao.GetComponentInChildren<TextMeshProUGUI>();

        if (rotulo != null)
        {
            rotulo.fontSize = 34f;
            rotulo.fontStyle = FontStyles.Bold;
            rotulo.color = Color.white;
        }

        canvasBotao.gameObject.SetActive(false);
    }

    void Apresentar(PedraManager pedra)
    {
        pedraAtual = pedra;

        canvasBotao.gameObject.SetActive(true);
        grupoBotao.blocksRaycasts = true;
        grupoBotao.interactable = true;

        ExecutarFade(AlphaFundo);
    }

    void ExecutarFade(float alphaAlvo)
    {
        if (rotinaFade != null)
            StopCoroutine(rotinaFade);

        rotinaFade = StartCoroutine(Fade(alphaAlvo));
    }

    IEnumerator Fade(float alphaAlvo)
    {
        float alphaInicial = fundo.color.a;
        float alphaBotaoInicial = grupoBotao.alpha;
        float alphaBotaoAlvo = alphaAlvo > 0f ? 1f : 0f;

        float tempo = 0f;

        while (tempo < DuracaoFade)
        {
            tempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tempo / DuracaoFade);

            Color cor = fundo.color;
            cor.a = Mathf.Lerp(alphaInicial, alphaAlvo, t);
            fundo.color = cor;

            grupoBotao.alpha = Mathf.Lerp(alphaBotaoInicial, alphaBotaoAlvo, t);

            yield return null;
        }

        Color corFinal = fundo.color;
        corFinal.a = alphaAlvo;
        fundo.color = corFinal;
        grupoBotao.alpha = alphaBotaoAlvo;

        rotinaFade = null;

        if (alphaAlvo <= 0f)
        {
            grupoBotao.blocksRaycasts = false;
            grupoBotao.interactable = false;
            canvasBotao.gameObject.SetActive(false);
        }
    }

    void Sair()
    {
        PedraManager pedra = pedraAtual;

        if (pedra != null)
            pedra.SairDoFoco();
    }

    void Update()
    {
        if (canvasBotao == null)
            return;

        bool menuNaFrente =
            (PauseManager.Instance != null && PauseManager.Instance.EstaPausado) ||
            (ConfiguracoesManager.Instance != null &&
                ConfiguracoesManager.Instance.ConfiguracoesAbertas) ||
            LojaManager.LojaAberta;

        int ordem = menuNaFrente ? OrdemAtrasDoMenu : OrdemBotao;

        if (canvasBotao.sortingOrder != ordem)
            canvasBotao.sortingOrder = ordem;
    }

    void OnDestroy()
    {
        if (instancia == this)
            instancia = null;

        if (canvasFundo != null)
            Destroy(canvasFundo.gameObject);

        if (canvasBotao != null)
            Destroy(canvasBotao.gameObject);
    }
}
