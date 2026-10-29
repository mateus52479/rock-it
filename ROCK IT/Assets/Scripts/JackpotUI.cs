using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Celebração do prêmio máximo: um "JACKPOT" com aparência 3D pula no centro
/// da tela e pequenos "$" amarelos caem atrás dele. A duração acompanha o som
/// do jackpot e todo o efeito congela junto com o jogo quando ele é pausado.
/// </summary>
public class JackpotUI : MonoBehaviour
{
    /// <summary>Acima do tutorial (300), mas abaixo dos menus de pause/configurações.</summary>
    private const int OrdemAcimaDoTutorial = 350;

    /// <summary>Quando um menu abre, o efeito desce para trás dele.</summary>
    private const int OrdemAtrasDoMenu = -1;

    private const float DuracaoReserva = 2.8f;
    private const float DuracaoEntrada = 0.45f;
    private const float DuracaoSaida = 0.25f;
    private const float GravidadeDolar = 420f;

    private static JackpotUI instancia;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform canvasRect;
    private RectTransform grupoTitulo;
    private RectTransform containerDolares;

    private readonly List<Dolar> dolares = new List<Dolar>();
    private Coroutine rotina;

    private class Dolar
    {
        public RectTransform rect;
        public float velocidadeX;
        public float velocidadeY;
        public float rotacao;
        public float angulo;
    }

    /// <summary>Mostra a celebração usando a duração real do áudio do jackpot.</summary>
    public static void Mostrar()
    {
        if (instancia == null)
            instancia = Criar();

        instancia.Executar(AudioManager.DuracaoJackpot);
    }

    static JackpotUI Criar()
    {
        GameObject canvasObj = UIUtil.CriarCanvas("JackpotUI", OrdemAcimaDoTutorial);

        JackpotUI ui = canvasObj.AddComponent<JackpotUI>();
        ui.Construir();

        return ui;
    }

    void Construir()
    {
        canvas = GetComponent<Canvas>();
        canvasRect = (RectTransform)transform;

        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // Os dólares ficam atrás do título.
        containerDolares = CriarRetangulo("Dolares", transform);
        Esticar(containerDolares);

        grupoTitulo = CriarRetangulo("Titulo", transform);
        grupoTitulo.anchorMin = new Vector2(0.5f, 0.5f);
        grupoTitulo.anchorMax = new Vector2(0.5f, 0.5f);
        grupoTitulo.pivot = new Vector2(0.5f, 0.5f);
        grupoTitulo.sizeDelta = new Vector2(1700f, 320f);
        grupoTitulo.anchoredPosition = Vector2.zero;

        // Cópia escura deslocada: dá a impressão de profundidade/3D.
        CriarTextoTitulo(
            "TituloProfundidade",
            new Color32(96, 46, 8, 255),
            new Vector2(14f, -16f));

        TextMeshProUGUI frente = CriarTextoTitulo(
            "TituloFrente",
            TemaUI.Dourado,
            Vector2.zero);

        Outline contorno = frente.gameObject.AddComponent<Outline>();
        contorno.effectColor = new Color(0.16f, 0.07f, 0.01f, 1f);
        contorno.effectDistance = new Vector2(5f, -5f);
        contorno.useGraphicAlpha = true;
    }

    RectTransform CriarRetangulo(string nome, Transform pai)
    {
        GameObject obj = new GameObject(nome, typeof(RectTransform));
        RectTransform rt = (RectTransform)obj.transform;
        rt.SetParent(pai, false);
        return rt;
    }

    static void Esticar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    TextMeshProUGUI CriarTextoTitulo(string nome, Color cor, Vector2 posicao)
    {
        RectTransform rt = CriarRetangulo(nome, grupoTitulo);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1700f, 320f);
        rt.anchoredPosition = posicao;

        TextMeshProUGUI texto = rt.gameObject.AddComponent<TextMeshProUGUI>();
        texto.font = TMP_Settings.defaultFontAsset;
        texto.text = "JACKPOT";
        texto.fontSize = 190f;
        texto.fontStyle = FontStyles.Bold;
        texto.alignment = TextAlignmentOptions.Center;
        texto.color = cor;
        texto.raycastTarget = false;
        texto.textWrappingMode = TextWrappingModes.NoWrap;
        texto.overflowMode = TextOverflowModes.Overflow;
        texto.enableAutoSizing = false;

        return texto;
    }

    void Executar(float duracao)
    {
        if (rotina != null)
            StopCoroutine(rotina);

        LimparDolares();

        canvasGroup.alpha = 1f;
        grupoTitulo.localScale = Vector3.one * 0.05f;
        grupoTitulo.anchoredPosition = new Vector2(0f, -180f);
        grupoTitulo.localRotation = Quaternion.identity;

        float duracaoFinal = duracao > 0.1f ? duracao : DuracaoReserva;
        rotina = StartCoroutine(Rotina(Mathf.Max(0.7f, duracaoFinal)));
    }

    IEnumerator Rotina(float duracao)
    {
        float tempo = 0f;

        // Entrada: sobe e cresce com um leve overshoot.
        while (tempo < DuracaoEntrada)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / DuracaoEntrada);

            grupoTitulo.localScale = Vector3.one * Mathf.Max(0.05f, EscalaEntrada(t));
            grupoTitulo.anchoredPosition = new Vector2(0f, Mathf.Lerp(-180f, 0f, t));
            grupoTitulo.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-9f, 0f, t));

            yield return null;
        }

        grupoTitulo.localScale = Vector3.one;
        grupoTitulo.anchoredPosition = Vector2.zero;
        grupoTitulo.localRotation = Quaternion.identity;

        float fim = Mathf.Max(DuracaoEntrada + 0.05f, duracao - DuracaoSaida);
        float proximoDolar = 0f;

        // Mantém o título pulsando e os dólares caindo até a saída.
        while (tempo < fim)
        {
            float dt = Time.deltaTime;
            tempo += dt;

            AtualizarDolares(dt);

            proximoDolar -= dt;
            if (proximoDolar <= 0f)
            {
                SpawnarDolar();

                if (Random.value < 0.5f)
                    SpawnarDolar();

                proximoDolar = Random.Range(0.06f, 0.12f);
            }

            float pulso = 1f + 0.035f * Mathf.Sin(tempo * 9f);
            grupoTitulo.localScale = Vector3.one * pulso;
            grupoTitulo.localRotation = Quaternion.Euler(
                0f, 0f, Mathf.Sin(tempo * 6f) * 1.5f);

            yield return null;
        }

        // Sai junto com o fim do áudio.
        float saida = 0f;

        while (saida < DuracaoSaida)
        {
            saida += Time.deltaTime;
            float t = Mathf.Clamp01(saida / DuracaoSaida);

            canvasGroup.alpha = 1f - t;
            grupoTitulo.localScale = Vector3.one * Mathf.Lerp(1f, 0.82f, t);
            AtualizarDolares(Time.deltaTime);

            yield return null;
        }

        Fechar();
    }

    /// <summary>Curva de entrada com overshoot (ease-out back).</summary>
    static float EscalaEntrada(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float u = t - 1f;

        return 1f + c3 * u * u * u + c1 * u * u;
    }

    void SpawnarDolar()
    {
        float metadeLargura = Mathf.Max(200f, canvasRect.rect.width * 0.5f);
        float metadeAltura = Mathf.Max(200f, canvasRect.rect.height * 0.5f);

        RectTransform rt = CriarRetangulo("Dolar", containerDolares);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(140f, 140f);
        rt.anchoredPosition = new Vector2(
            Random.Range(-metadeLargura, metadeLargura),
            metadeAltura + Random.Range(60f, 180f));

        TextMeshProUGUI texto = rt.gameObject.AddComponent<TextMeshProUGUI>();
        texto.font = TMP_Settings.defaultFontAsset;
        texto.text = "$";
        texto.fontSize = Random.Range(30f, 68f);
        texto.fontStyle = FontStyles.Bold;
        texto.alignment = TextAlignmentOptions.Center;
        texto.color = new Color(1f, 0.82f, 0.22f, Random.Range(0.72f, 1f));
        texto.raycastTarget = false;
        texto.textWrappingMode = TextWrappingModes.NoWrap;
        texto.overflowMode = TextOverflowModes.Overflow;
        texto.enableAutoSizing = false;

        Outline contorno = rt.gameObject.AddComponent<Outline>();
        contorno.effectColor = new Color(0.30f, 0.17f, 0.02f, 0.9f);
        contorno.effectDistance = new Vector2(2f, -2f);
        contorno.useGraphicAlpha = true;

        Dolar dolar = new Dolar();
        dolar.rect = rt;
        dolar.velocidadeX = Random.Range(-45f, 45f);
        dolar.velocidadeY = Random.Range(-120f, -260f);
        dolar.rotacao = Random.Range(-90f, 90f);
        dolar.angulo = Random.Range(0f, 360f);

        rt.localRotation = Quaternion.Euler(0f, 0f, dolar.angulo);

        dolares.Add(dolar);
    }

    void AtualizarDolares(float dt)
    {
        if (dt <= 0f)
            return;

        float limite = -Mathf.Max(200f, canvasRect.rect.height * 0.5f) - 160f;

        for (int i = dolares.Count - 1; i >= 0; i--)
        {
            Dolar dolar = dolares[i];

            if (dolar.rect == null)
            {
                dolares.RemoveAt(i);
                continue;
            }

            dolar.velocidadeY -= GravidadeDolar * dt;

            Vector2 posicao = dolar.rect.anchoredPosition;
            posicao.x += dolar.velocidadeX * dt;
            posicao.y += dolar.velocidadeY * dt;
            dolar.rect.anchoredPosition = posicao;

            dolar.angulo += dolar.rotacao * dt;
            dolar.rect.localRotation = Quaternion.Euler(0f, 0f, dolar.angulo);

            if (posicao.y < limite)
            {
                Destroy(dolar.rect.gameObject);
                dolares.RemoveAt(i);
            }
        }
    }

    void LimparDolares()
    {
        foreach (Dolar dolar in dolares)
        {
            if (dolar.rect != null)
                Destroy(dolar.rect.gameObject);
        }

        dolares.Clear();
    }

    void Update()
    {
        if (canvas == null)
            return;

        bool menuNaFrente =
            (PauseManager.Instance != null && PauseManager.Instance.EstaPausado) ||
            (ConfiguracoesManager.Instance != null && ConfiguracoesManager.Instance.ConfiguracoesAbertas);

        int ordem = menuNaFrente ? OrdemAtrasDoMenu : OrdemAcimaDoTutorial;

        if (canvas.sortingOrder != ordem)
            canvas.sortingOrder = ordem;
    }

    void Fechar()
    {
        rotina = null;

        if (instancia == this)
            instancia = null;

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (instancia == this)
            instancia = null;
    }
}
