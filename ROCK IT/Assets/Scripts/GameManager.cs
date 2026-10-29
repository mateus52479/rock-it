using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float dinheiro = 0f;
    public TextMeshProUGUI textoDinheiro;
    public TextMeshProUGUI textoDiferenca; // texto do +X ou -X ao lado

    [Header("Animação do contador")]
    [Tooltip("Duração da animação para valores pequenos (em segundos).")]
    public float duracaoMinima = 0.25f;

    [Tooltip("Duração máxima da animação, mesmo para valores muito altos (em segundos).")]
    public float duracaoMaxima = 1.2f;

    [Header("HUD do dinheiro")]
    [Tooltip("Tamanho da fonte do valor. Se o número for muito comprido, ele diminui sozinho.")]
    public float tamanhoFonteDinheiro = 42f;

    [Tooltip("Tamanho da fonte do +X/-X que aparece ao ganhar ou gastar.")]
    public float tamanhoFonteDiferenca = 26f;

    [Tooltip("Cor do \"R$\".")]
    public Color corMoeda = new Color(1f, 0.79f, 0.29f);

    [Tooltip("Quanto o valor cresce no pulinho ao ganhar ou gastar (0 desliga o pulo).")]
    [Range(0f, 0.4f)]
    public float escalaDoPulso = 0.12f;

    private float dinheiroExibido = 0f;
    private float alvoAnimacao = 0f;
    private float inicioAnimacao = 0f;
    private float tempoAnimacao = 0f;
    private float duracaoAnimacao = 0f;
    private Coroutine corotinaDiferenca;
    private Coroutine corotinaPulso;
    private string corMoedaHtml = "#FFC94A";
    private Vector2 posicaoBaseDiferenca = new Vector2(0f, 46f);

    /// <summary>Teto do dinheiro. Evita que o saldo vire infinito.</summary>
    public const float DinheiroMaximo = 1e30f;

    void Awake()
    {
        Instance = this;
        TutorialBootstrap.Instalar();
        LimiteMesa.Instalar();
        ConfigurarHUD();

        // O "R$" é fixo, mas o sufixo (mil/mi ou K/M) muda com o idioma.
        Traducao.OnIdiomaMudou += AtualizarHUD;
    }

    void OnDestroy()
    {
        Traducao.OnIdiomaMudou -= AtualizarHUD;
    }

    void Start()
    {
        AtualizarHUD();
    }

    /// <summary>
    /// Deixa o dinheiro legível: o texto original tinha só 140 px de largura e
    /// quebrava a linha em qualquer valor maior que "R$ 999,99". Aqui o layout
    /// é refeito em runtime, sem mexer na cena: sem quebra de linha, fonte que
    /// se adapta, separador de milhar, "R$" dourado e um pulinho ao ganhar.
    /// </summary>
    void ConfigurarHUD()
    {
        if (textoDinheiro == null) return;

        Transform container = textoDinheiro.transform.parent;

        if (container != null)
        {
            // O HorizontalLayoutGroup espremia o texto em 140 px e brigava
            // com o posicionamento manual que fazemos abaixo.
            HorizontalLayoutGroup layout = container.GetComponent<HorizontalLayoutGroup>();
            if (layout != null)
            {
                layout.enabled = false;
                Destroy(layout);
            }

            ContentSizeFitter fitter = container.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                fitter.enabled = false;
                Destroy(fitter);
            }

            RectTransform rtContainer = container as RectTransform;
            if (rtContainer != null)
            {
                // Fica no topo, centralizado. Antes era "centro do canvas +
                // 200", então no fullscreen o texto escorregava para o meio.
                rtContainer.anchorMin = new Vector2(0.5f, 1f);
                rtContainer.anchorMax = new Vector2(0.5f, 1f);
                rtContainer.pivot = new Vector2(0.5f, 1f);
                rtContainer.anchoredPosition = new Vector2(0f, -52f);
                rtContainer.sizeDelta = new Vector2(640f, 96f);
            }
        }

        corMoedaHtml = "#" + ColorUtility.ToHtmlStringRGB(corMoeda);

        PrepararTextoDinheiro();
        PrepararTextoDiferenca();
    }

    void PrepararTextoDinheiro()
    {
        RectTransform rt = textoDinheiro.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 0f);
        rt.sizeDelta = new Vector2(620f, 64f);

        textoDinheiro.textWrappingMode = TextWrappingModes.NoWrap;
        textoDinheiro.overflowMode = TextOverflowModes.Overflow;
        textoDinheiro.enableAutoSizing = true;
        textoDinheiro.fontSizeMin = 20f;
        textoDinheiro.fontSizeMax = tamanhoFonteDinheiro;
        textoDinheiro.fontSize = tamanhoFonteDinheiro;
        textoDinheiro.alignment = TextAlignmentOptions.Center;
        textoDinheiro.fontStyle = FontStyles.Bold;
        textoDinheiro.color = Color.white;
        textoDinheiro.raycastTarget = false;

        if (textoDinheiro.GetComponent<Shadow>() == null)
        {
            Shadow sombra = textoDinheiro.gameObject.AddComponent<Shadow>();
            sombra.effectColor = new Color(0f, 0f, 0f, 0.7f);
            sombra.effectDistance = new Vector2(0f, -3f);
        }
    }

    void PrepararTextoDiferenca()
    {
        if (textoDiferenca == null) return;

        RectTransform rt = textoDiferenca.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = posicaoBaseDiferenca;
        rt.sizeDelta = new Vector2(620f, 32f);

        textoDiferenca.textWrappingMode = TextWrappingModes.NoWrap;
        textoDiferenca.overflowMode = TextOverflowModes.Overflow;
        textoDiferenca.enableAutoSizing = false;
        textoDiferenca.fontSize = tamanhoFonteDiferenca;
        textoDiferenca.alignment = TextAlignmentOptions.Center;
        textoDiferenca.fontStyle = FontStyles.Bold;
        textoDiferenca.raycastTarget = false;
    }

    void Update()
    {
        // O valor mudou (recompensa, compra, carregamento)? Recomeça a
        // animação a partir do número que está na tela.
        if (dinheiro != alvoAnimacao)
        {
            alvoAnimacao = dinheiro;
            inicioAnimacao = dinheiroExibido;
            tempoAnimacao = 0f;

            float distancia = Mathf.Abs(alvoAnimacao - inicioAnimacao);

            // Sem distância não há o que animar.
            if (distancia <= 0.001f)
            {
                dinheiroExibido = alvoAnimacao;
                AtualizarHUD();
            }
            else
            {
                duracaoAnimacao = CalcularDuracao(distancia);
                AudioManager.ModificandoMoney();
            }
        }

        if (dinheiroExibido == alvoAnimacao)
            return;

        tempoAnimacao += Time.deltaTime;

        float t = duracaoAnimacao > 0f
            ? Mathf.Clamp01(tempoAnimacao / duracaoAnimacao)
            : 1f;

        // Desacelera perto do final, mas termina exatamente no alvo.
        float suave = 1f - Mathf.Pow(1f - t, 3f);
        dinheiroExibido = Mathf.Lerp(inicioAnimacao, alvoAnimacao, suave);

        if (t >= 1f)
            dinheiroExibido = alvoAnimacao;

        AtualizarHUD();
    }

    /// <summary>
    /// Valores pequenos terminam rápido e valores altos levam um pouco mais,
    /// mas sempre têm um fim (nada de aproximação infinita).
    /// </summary>
    float CalcularDuracao(float distancia)
    {
        float duracao = 0.25f + 0.35f * Mathf.Log10(1f + distancia);

        // Garante limites válidos mesmo se os campos do Inspector forem
        // configurados ao contrário.
        float minimo = Mathf.Max(0.01f, Mathf.Min(duracaoMinima, duracaoMaxima));
        float maximo = Mathf.Max(minimo, duracaoMaxima);

        return Mathf.Clamp(duracao, minimo, maximo);
    }

    public void AdicionarDinheiro(float valor)
    {
        valor = SanearValor(valor);

        // Recompensa negativa não pode deixar o saldo negativo: se isso
        // acontecesse o jogador não conseguiria comprar nem a Pedra Comum.
        // O teto evita que uma recompensa gigante vire infinito.
        dinheiro = Mathf.Clamp(
            dinheiro + valor,
            0f,
            DinheiroMaximo
        );

        MostrarDiferenca(valor);
    }

    public bool GastarDinheiro(float valor)
    {
        valor = SanearValor(valor);

        // Custo negativo nunca deve pagar o jogador (era o que acontecia
        // quando o preço de um upgrade estourava em níveis altos).
        if (valor < 0f) return false;

        if (dinheiro < valor) return false;

        dinheiro -= valor;
        MostrarDiferenca(-valor);
        return true;
    }

    public void CarregarDinheiro(float valor)
    {
        dinheiro = Mathf.Clamp(SanearValor(valor), 0f, DinheiroMaximo);
        dinheiroExibido = dinheiro;
        alvoAnimacao = dinheiro;
        tempoAnimacao = 0f;
        AtualizarHUD();
    }

    /// <summary>
    /// Deixa o valor dentro de limites seguros: nada de NaN nem infinito
    /// chegando no HUD ou no save.
    /// </summary>
    static float SanearValor(float valor)
    {
        if (float.IsNaN(valor))
            return 0f;

        if (float.IsInfinity(valor))
            return valor > 0f ? DinheiroMaximo : -DinheiroMaximo;

        return Mathf.Clamp(valor, -DinheiroMaximo, DinheiroMaximo);
    }

    void AtualizarHUD()
    {
        if (textoDinheiro == null) return;

        textoDinheiro.text =
            "<color=" + corMoedaHtml + ">R$</color> " + FormatarValor(dinheiroExibido);
    }

    /// <summary>
    /// Abaixo de um milhão usa separador de milhar (1.234,56); acima disso,
    /// o sufixo curto (1,2 mi) para não estourar a tela.
    /// </summary>
    static string FormatarValor(float valor)
    {
        float absoluto = Mathf.Abs(valor);

        return absoluto < 1e6f
            ? ComSeparadorDeMilhar(absoluto)
            : NumeroUtil.Formatar(absoluto);
    }

    /// <summary>1234.56 -> 1.234,56 (sem depender da cultura do sistema).</summary>
    static string ComSeparadorDeMilhar(float valor)
    {
        string texto = Mathf.Abs(valor).ToString(
            "F2",
            System.Globalization.CultureInfo.InvariantCulture
        );

        string[] partes = texto.Split('.');
        string inteiro = partes[0];
        string saida = "";

        for (int i = 0; i < inteiro.Length; i++)
        {
            if (i > 0 && (inteiro.Length - i) % 3 == 0)
                saida += ".";

            saida += inteiro[i];
        }

        return partes.Length > 1 ? saida + "," + partes[1] : saida;
    }

    void MostrarDiferenca(float valor)
    {
        PulsarValor();

        if (textoDiferenca == null) return;

        if (corotinaDiferenca != null)
            StopCoroutine(corotinaDiferenca);

        corotinaDiferenca = StartCoroutine(AnimarDiferenca(valor));
    }

    /// <summary>Pulinho rápido no valor quando ele muda.</summary>
    void PulsarValor()
    {
        if (textoDinheiro == null || escalaDoPulso <= 0f) return;

        if (corotinaPulso != null)
            StopCoroutine(corotinaPulso);

        textoDinheiro.transform.localScale = Vector3.one;
        corotinaPulso = StartCoroutine(AnimarPulso());
    }

    IEnumerator AnimarPulso()
    {
        Transform alvo = textoDinheiro.transform;
        float duracao = 0.22f;
        float tempo = 0f;

        while (tempo < duracao && alvo != null)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracao);

            alvo.localScale = Vector3.one * (1f + escalaDoPulso * Mathf.Sin(t * Mathf.PI));
            yield return null;
        }

        if (alvo != null)
            alvo.localScale = Vector3.one;

        corotinaPulso = null;
    }

    IEnumerator AnimarDiferenca(float valor)
    {
        textoDiferenca.gameObject.SetActive(true);

        RectTransform rt = textoDiferenca.rectTransform;
        rt.anchoredPosition = posicaoBaseDiferenca;

        if (valor >= 0)
        {
            textoDiferenca.text = "+" + FormatarDiferenca(valor);
            textoDiferenca.color = new Color(0.475f, 0.878f, 0.416f, 1f);
        }
        else
        {
            textoDiferenca.text = "-" + FormatarDiferenca(valor);
            textoDiferenca.color = new Color(0.886f, 0.424f, 0.376f, 1f);
        }

        // Começa embaixo do valor e sobe devagar enquanto fica visível.
        float tempo = 0f;
        float duracao = 1.5f;
        float subida = 18f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracao);

            rt.anchoredPosition = posicaoBaseDiferenca + new Vector2(0f, subida * t);
            yield return null;
        }

        // Some gradualmente
        tempo = 0f;
        duracao = 0.5f;
        Color corInicial = textoDiferenca.color;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, tempo / duracao);
            textoDiferenca.color = new Color(corInicial.r, corInicial.g, corInicial.b, alpha);
            yield return null;
        }

        textoDiferenca.gameObject.SetActive(false);
        rt.anchoredPosition = posicaoBaseDiferenca;
    }

    /// <summary>Valores altos usam sufixo para o texto não estourar a tela.</summary>
    static string FormatarDiferenca(float valor)
    {
        float absoluto = Mathf.Abs(valor);

        return absoluto < 1e6f
            ? ComSeparadorDeMilhar(absoluto)
            : NumeroUtil.Formatar(absoluto);
    }
}