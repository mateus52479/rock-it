using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tela de créditos em tela cheia: o texto sobe devagar, como o letreiro do
/// Star Wars, com as linhas diminuindo conforme se afastam no topo. O conteúdo
/// vem de CreditosDados.
/// </summary>
public class CreditosUI : MonoBehaviour
{
    private const float VelocidadeRolagem = 60f;   // pixels por segundo
    private const float EsperaInicial = 0.8f;
    private const float LarguraTexto = 960f;
    private const float EscalaTopo = 0.62f;
    private const float EscalaBase = 1.02f;

    private static CreditosUI instancia;

    private RectTransform viewport;
    private RectTransform conteudo;
    private ScrollRect scroll;
    private float alturaConteudo;
    private float espera;
    private bool rolando = true;
    private TextMeshProUGUI linhaFinal;
    private Coroutine finalizacao;

    private readonly List<RectTransform> linhas = new List<RectTransform>();
    private readonly List<RectTransform> poeiras = new List<RectTransform>();
    private readonly List<float> velocidadesPoeira = new List<float>();

    public static void Abrir()
    {
        if (instancia != null)
            return;

        GameObject canvas = UIUtil.CriarCanvas("CreditosUI", 200);
        instancia = canvas.AddComponent<CreditosUI>();
        instancia.Construir();

        AudioManager.AbrirCreditos();
    }

    void OnDestroy()
    {
        if (instancia == this)
        {
            instancia = null;
            AudioManager.FecharCreditos();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Fechar();
            return;
        }

        AtualizarPoeira();

        if (!rolando)
            return;

        if (espera > 0f)
        {
            espera -= Time.unscaledDeltaTime;
            return;
        }

        alturaConteudo = conteudo.rect.height;
        float excedente = Mathf.Max(1f, alturaConteudo - viewport.rect.height);
        float passo = (VelocidadeRolagem / excedente) * Time.unscaledDeltaTime;

        scroll.verticalNormalizedPosition = Mathf.Max(
            0f, scroll.verticalNormalizedPosition - passo);

        if (scroll.verticalNormalizedPosition <= 0f)
        {
            rolando = false;

            if (finalizacao == null)
                finalizacao = StartCoroutine(FinalizarCreditos());
        }

        AtualizarPerspectiva();
    }

    void Construir()
    {
        RectTransform raiz = (RectTransform)transform;

        Image fundo = UIUtil.CriarImagem(
            raiz, "Fundo", new Color32(14, 13, 18, 255),
            Vector2.zero, Vector2.zero);

        UIUtil.Esticar(fundo.rectTransform);
        fundo.raycastTarget = true;

        CriarPoeira(fundo.transform);

        GameObject viewportGO = new GameObject(
            "Viewport", typeof(RectTransform), typeof(RectMask2D));

        viewportGO.transform.SetParent(fundo.transform, false);

        viewport = viewportGO.GetComponent<RectTransform>();
        UIUtil.Esticar(viewport);

        GameObject conteudoGO = new GameObject("Conteudo", typeof(RectTransform));
        conteudoGO.transform.SetParent(viewport, false);

        conteudo = conteudoGO.GetComponent<RectTransform>();
        conteudo.anchorMin = new Vector2(0.5f, 1f);
        conteudo.anchorMax = new Vector2(0.5f, 1f);
        conteudo.pivot = new Vector2(0.5f, 1f);
        conteudo.sizeDelta = new Vector2(LarguraTexto, 0f);
        conteudo.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup layout = conteudoGO.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter ajuste = conteudoGO.AddComponent<ContentSizeFitter>();
        ajuste.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll = fundo.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = conteudo;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.inertia = false;
        scroll.scrollSensitivity = TemaUI.SensibilidadeScroll;

        MontarConteudo();

        Button voltar = UIUtil.CriarBotao(
            raiz, "BotaoVoltar", "Voltar",
            new Vector2(120f, 40f), Vector2.zero,
            Color.white, Fechar);

        TemaUI.EstilizarBotao(voltar, TemaUI.EstiloBotao.Fantasma, 16f);

        RectTransform voltarRT = (RectTransform)voltar.transform;
        voltarRT.anchorMin = new Vector2(1f, 0f);
        voltarRT.anchorMax = new Vector2(1f, 0f);
        voltarRT.pivot = new Vector2(1f, 0f);
        voltarRT.anchoredPosition = new Vector2(-36f, 30f);

        LayoutRebuilder.ForceRebuildLayoutImmediate(conteudo);
        Canvas.ForceUpdateCanvases();

        alturaConteudo = conteudo.rect.height;
        scroll.verticalNormalizedPosition = 1f;
        espera = EsperaInicial;

        AtualizarPerspectiva();
    }

    void MontarConteudo()
    {
        AdicionarEspaco(1280f);

        CriarLinha(
            "ROCK IT", 100f, TemaUI.Acento,
            FontStyles.Bold, 120f, 8f);

        CriarLinha(
            "CRÉDITOS", 28f, TemaUI.TextoForte,
            FontStyles.Normal, 44f, 10f);

        AdicionarEspaco(80f);

        bool primeira = true;

        foreach (CreditosDados.Secao secao in CreditosDados.secoes)
        {
            if (secao == null || secao.nomes == null || secao.nomes.Length == 0)
                continue;

            if (!primeira)
                AdicionarEspaco(22f);

            primeira = false;

            CriarLinha(
                secao.titulo, 21f, TemaUI.Acento,
                FontStyles.Bold, 36f, 4f);

            foreach (string nome in secao.nomes)
            {
                if (string.IsNullOrEmpty(nome))
                    continue;

                CriarLinha(
                    nome, 30f, TemaUI.TextoForte,
                    FontStyles.Normal, 43f, 0f);
            }
        }

        if (CreditosDados.testadores != null &&
            CreditosDados.testadores.Length > 0)
        {
            AdicionarEspaco(22f);

            CriarLinha(
                "TESTES", 21f, TemaUI.Acento,
                FontStyles.Bold, 36f, 4f);

            foreach (string nome in CreditosDados.testadores)
            {
                if (string.IsNullOrEmpty(nome))
                    continue;

                CriarLinha(
                    nome, 30f, TemaUI.TextoForte,
                    FontStyles.Normal, 43f, 0f);
            }
        }

        if (CreditosDados.musicasTerceiros != null &&
            CreditosDados.musicasTerceiros.Length > 0)
        {
            AdicionarEspaco(30f);

            CriarLinha(
                "MÚSICAS DE TERCEIROS", 21f, TemaUI.Acento,
                FontStyles.Bold, 36f, 4f);

            foreach (CreditosDados.Musica musica in CreditosDados.musicasTerceiros)
            {
                CriarLinha(
                    "\"" + musica.titulo + "\" — " + musica.autor,
                    25f, TemaUI.TextoForte, FontStyles.Normal, 36f, 0f);
            }

            CriarLinha(
                CreditosDados.OrigemMusicas, 18f, TemaUI.TextoSuave,
                FontStyles.Normal, 28f, 0f);

            CriarLinha(
                CreditosDados.LinksMusicas, 17f, TemaUI.TextoFraco,
                FontStyles.Normal, 26f, 0f);
        }

        AdicionarEspaco(80f);

        linhaFinal = CriarLinha(
            "Obrigado por jogar.", 28f, TemaUI.Acento,
            FontStyles.Normal, 42f, 2f);

        AdicionarEspaco(320f);
    }

    /// <summary>
    /// Fim do letreiro: apaga as demais linhas e deixa só o "Obrigado por
    /// jogar." centralizado na tela.
    /// </summary>
    IEnumerator FinalizarCreditos()
    {
        float duracao = 0.6f;
        float tempo = 0f;
        RectTransform rtFinal = linhaFinal != null ? linhaFinal.rectTransform : null;

        while (tempo < duracao)
        {
            tempo += Time.unscaledDeltaTime;
            float alpha = 1f - Mathf.Clamp01(tempo / duracao);

            foreach (RectTransform linha in linhas)
            {
                if (linha == null || linha == rtFinal)
                    continue;

                TextMeshProUGUI texto = linha.GetComponent<TextMeshProUGUI>();

                if (texto != null)
                    texto.alpha = alpha;
            }

            yield return null;
        }

        // Some com todo o conteúdo do letreiro e deixa só a despedida.
        foreach (Transform filho in conteudo)
        {
            if (linhaFinal != null && filho == linhaFinal.transform)
                continue;

            filho.gameObject.SetActive(false);
        }

        linhas.Clear();

        if (linhaFinal != null)
        {
            RectTransform rt = linhaFinal.rectTransform;

            rt.SetParent(viewport, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(LarguraTexto, 70f);
            rt.localScale = Vector3.one;

            linhaFinal.alpha = 1f;
        }

        if (scroll != null)
            scroll.enabled = false;

        finalizacao = null;
    }

    void CriarPoeira(Transform pai)
    {
        System.Random sorteio = new System.Random(2026);
        int total = 56;

        for (int i = 0; i < total; i++)
        {
            float tamanho = 2f + (float)sorteio.NextDouble() * 3f;

            Image ponto = UIUtil.CriarImagem(
                pai, "Poeira", Color.white,
                new Vector2(tamanho, tamanho),
                new Vector2(
                    (float)(sorteio.NextDouble() * 1920.0 - 960.0),
                    (float)(sorteio.NextDouble() * 1080.0 - 540.0)));

            ponto.raycastTarget = false;

            // Poeira bem apagada: o texto continua sendo o foco.
            float brilho = (float)sorteio.NextDouble();

            if (brilho > 0.88f)
                ponto.color = new Color(TemaUI.Acento.r, TemaUI.Acento.g, TemaUI.Acento.b, 0.45f);
            else if (brilho > 0.55f)
                ponto.color = new Color(0.55f, 0.53f, 0.60f, 0.30f);
            else
                ponto.color = new Color(0.40f, 0.38f, 0.46f, 0.22f);

            poeiras.Add(ponto.rectTransform);
            velocidadesPoeira.Add(4f + (float)sorteio.NextDouble() * 10f);
        }
    }

    void AtualizarPoeira()
    {
        float dt = Time.unscaledDeltaTime;

        for (int i = 0; i < poeiras.Count; i++)
        {
            RectTransform ponto = poeiras[i];
            Vector2 posicao = ponto.anchoredPosition;

            posicao.y -= velocidadesPoeira[i] * dt;

            if (posicao.y < -560f)
            {
                posicao.y = 560f;
                posicao.x = Random.Range(-960f, 960f);
            }

            ponto.anchoredPosition = posicao;
        }
    }

    void AtualizarPerspectiva()
    {
        if (viewport == null || linhas.Count == 0)
            return;

        float altura = viewport.rect.height;
        if (altura <= 0f)
            return;

        float baseTela = viewport.rect.yMin;
        float topoTela = viewport.rect.yMax;

        for (int i = 0; i < linhas.Count; i++)
        {
            RectTransform linha = linhas[i];

            if (linha == null)
                continue;

            float y = viewport.InverseTransformPoint(linha.position).y;
            float t = Mathf.InverseLerp(topoTela, baseTela, y);
            float escala = Mathf.Lerp(EscalaTopo, EscalaBase, t);

            linha.localScale = new Vector3(escala, escala, 1f);
        }
    }

    void AdicionarEspaco(float altura)
    {
        RectTransform espaco = UIUtil.CriarRetangulo(conteudo, "Espaco");
        espaco.sizeDelta = new Vector2(0f, altura);

        LayoutElement elemento = espaco.gameObject.AddComponent<LayoutElement>();
        elemento.minHeight = altura;
        elemento.preferredHeight = altura;
        elemento.flexibleHeight = 0f;
    }

    TextMeshProUGUI CriarLinha(
        string texto, float tamanho, Color cor,
        FontStyles estilo, float altura, float espacamento)
    {
        TextMeshProUGUI tmp = UIUtil.CriarTexto(
            conteudo, "Linha", texto,
            tamanho, new Vector2(0f, altura), Vector2.zero,
            TextAlignmentOptions.Center);

        TemaUI.EstilizarTexto(
            tmp, tamanho, cor, estilo,
            TextAlignmentOptions.Center, espacamento);

        tmp.textWrappingMode = TextWrappingModes.Normal;

        LayoutElement elemento = tmp.gameObject.AddComponent<LayoutElement>();
        elemento.minHeight = altura;
        elemento.preferredHeight = altura;

        linhas.Add(tmp.rectTransform);

        return tmp;
    }

    void Fechar()
    {
        AudioManager.FecharCreditos();
        instancia = null;
        Destroy(gameObject);
    }
}
