using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tabela de prêmios que aparece à direita quando a pedra está aumentada.
/// Mostra a chance de cada resultado (já com a Sorte aplicada) e quanto ele
/// paga, usando as mesmas placas e cores dos outros painéis do jogo.
/// </summary>
public class TabelaPremiosUI : MonoBehaviour
{
    private const float Largura = 410f;
    private const float Margem = 20f;
    private const float AlturaTitulo = 36f;
    private const float AlturaColunas = 22f;
    private const float AlturaLinha = 44f;
    private const float EspacoLinha = 5f;

    private static TabelaPremiosUI instancia;

    private Canvas canvas;
    private RectTransform painel;
    private RectTransform conteudo;

    private PedraManager pedraAtual;
    private int nivelSorteExibido = int.MinValue;
    private int larguraTela = -1;
    private int alturaTela = -1;

    void Awake()
    {
        instancia = this;
        Construir();
        Traducao.OnIdiomaMudou += Redesenhar;
    }

    void OnDestroy()
    {
        Traducao.OnIdiomaMudou -= Redesenhar;

        if (instancia == this)
            instancia = null;
    }

    /// <summary>Abre a tabela para a pedra que acabou de ser ampliada.</summary>
    public static void Mostrar(PedraManager pedra)
    {
        if (pedra == null)
            return;

        if (instancia == null)
        {
            GameObject go = new GameObject("TabelaPremiosUI");
            instancia = go.AddComponent<TabelaPremiosUI>();
        }

        instancia.Apresentar(pedra);
    }

    /// <summary>
    /// Fecha a tabela. Se outra pedra estiver usando, ela continua aparecendo:
    /// só some quando quem pede para esconder é a pedra que está na tela.
    /// </summary>
    public static void Esconder(PedraManager pedra)
    {
        if (instancia == null)
            return;

        if (pedra != null && instancia.pedraAtual != pedra)
            return;

        instancia.pedraAtual = null;
        instancia.nivelSorteExibido = int.MinValue;

        if (instancia.canvas != null)
            instancia.canvas.gameObject.SetActive(false);
    }

    void Apresentar(PedraManager pedra)
    {
        pedraAtual = pedra;
        nivelSorteExibido = NivelSorte();

        if (canvas != null)
            canvas.gameObject.SetActive(true);

        PosicionarProximoDaPedra();
        Redesenhar();
    }

    void Construir()
    {
        // Atrás do canvas do jogo: a loja e o pause continuam cobrindo a tabela
        // quando abrem, em vez de ela ficar por cima do fundo escurecido.
        canvas = UIUtil.CriarCanvas("TabelaPremiosCanvas", -1).GetComponent<Canvas>();

        painel = UIUtil.CriarRetangulo(canvas.transform, "Painel");
        painel.anchorMin = new Vector2(1f, 0.5f);
        painel.anchorMax = new Vector2(1f, 0.5f);
        painel.pivot = new Vector2(1f, 0.5f);
        painel.anchoredPosition = new Vector2(-36f, 0f);

        Image fundo = painel.gameObject.AddComponent<Image>();
        TemaUI.AplicarPlaca(fundo, TemaUI.SpritePainel(), Color.white);
        fundo.raycastTarget = false;
        TemaUI.Sombra(fundo, 6f, 0.6f);

        conteudo = UIUtil.CriarRetangulo(painel, "Conteudo");
        UIUtil.Esticar(conteudo);

        canvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (pedraAtual == null || canvas == null)
            return;

        // Com o jogo pausado, o painel do pause precisa cobrir a tabela.
        bool pausado = PauseManager.Instance != null &&
                       PauseManager.Instance.painelPause != null &&
                       PauseManager.Instance.painelPause.activeSelf;

        if (canvas.enabled == pausado)
            canvas.enabled = !pausado;

        if (pausado)
            return;

        // Comprar Sorte com a pedra na mão muda as porcentagens.
        if (NivelSorte() != nivelSorteExibido)
            Redesenhar();

        // A posição depende do tamanho da tela: ao redimensionar a janela, a
        // tabela precisa continuar colada na pedra em vez de sair do lugar.
        if (Screen.width != larguraTela || Screen.height != alturaTela)
            PosicionarProximoDaPedra();
    }

    /// <summary>
    /// Encosta a tabela à direita da pedra ampliada. A pedra viaja para o
    /// centro da câmera, então a posição é calculada a partir da borda direita
    /// dela nesse ponto, e não da borda da tela.
    /// </summary>
    void PosicionarProximoDaPedra()
    {
        if (painel == null || canvas == null || Camera.main == null)
            return;

        larguraTela = Screen.width;
        alturaTela = Screen.height;

        float escala = canvas.scaleFactor;

        // Na primeira exibição o canvas acabou de nascer; se o CanvasScaler
        // ainda não calculou, força uma atualização antes de medir.
        if (escala <= 0f)
        {
            Canvas.ForceUpdateCanvases();
            escala = canvas.scaleFactor;
        }

        if (escala <= 0f)
            escala = 1f;

        Vector3 centroMundo = Camera.main.ViewportToWorldPoint(
            new Vector3(0.5f, 0.5f, 10f));

        float metadePedra = pedraAtual != null
            ? pedraAtual.MetadeLarguraAmpliada()
            : 0.9f;

        Vector3 bordaMundo = centroMundo + Vector3.right * metadePedra;
        Vector3 bordaTela = Camera.main.WorldToScreenPoint(bordaMundo);

        // O canvas é Screen Space - Overlay: o centro da tela é a origem e
        // cada unidade do canvas vale "escala" pixels.
        float larguraCanvas = Screen.width / escala;
        float xBorda = (bordaTela.x - Screen.width * 0.5f) / escala;
        float x = xBorda + 46f + Largura * 0.5f;

        // Se a tela for estreita, a tabela pode encostar na borda. Nesse caso
        // ela desliza para dentro, mesmo que fique um pouco sobre a pedra.
        float limiteDireita = larguraCanvas * 0.5f - Largura * 0.5f - 24f;
        float limiteEsquerda = -larguraCanvas * 0.5f + Largura * 0.5f + 24f;

        if (limiteEsquerda <= limiteDireita)
            x = Mathf.Clamp(x, limiteEsquerda, limiteDireita);
        else
            x = 0f;

        painel.anchorMin = new Vector2(0.5f, 0.5f);
        painel.anchorMax = new Vector2(0.5f, 0.5f);
        painel.pivot = new Vector2(0.5f, 0.5f);
        painel.anchoredPosition = new Vector2(x, 0f);
    }

    void Redesenhar()
    {
        if (pedraAtual == null || conteudo == null)
            return;

        PedraDados dados = pedraAtual.DadosDaPedra;

        if (dados == null || dados.tabelaValores == null ||
            dados.tabelaValores.Length == 0 || dados.tabelaValores[0] == null)
        {
            Esconder(pedraAtual);
            return;
        }

        nivelSorteExibido = NivelSorte();

        foreach (Transform filho in conteudo)
            Destroy(filho.gameObject);

        int total = dados.tabelaValores.Length;
        float[] chances = dados.ObterChancesAjustadas(nivelSorteExibido);

        // Da maior recompensa para a menor: o prêmio máximo fica no topo.
        int[] ordem = new int[total];

        for (int i = 0; i < total; i++)
            ordem[i] = i;

        for (int i = 0; i < total; i++)
        {
            for (int j = i + 1; j < total; j++)
            {
                if (dados.tabelaValores[ordem[j]].valor >
                    dados.tabelaValores[ordem[i]].valor)
                {
                    int troca = ordem[i];
                    ordem[i] = ordem[j];
                    ordem[j] = troca;
                }
            }
        }

        int maiorValor = dados.tabelaValores[ordem[0]].valor;

        // Sorte alta pode zerar resultados ruins. Linha com 0% só ocuparia
        // espaço e passaria a ideia de que aquele prêmio ainda é possível.
        int totalVisivel = 0;

        for (int i = 0; i < total; i++)
            if (chances[ordem[i]] > 0.001f)
                totalVisivel++;

        if (totalVisivel == 0)
        {
            Esconder(pedraAtual);
            return;
        }

        float altura = Margem + AlturaTitulo + 6f + 2f + 8f
            + AlturaColunas + 8f
            + totalVisivel * AlturaLinha
            + Mathf.Max(0, totalVisivel - 1) * EspacoLinha
            + Margem;

        painel.sizeDelta = new Vector2(Largura, altura);

        float y = -Margem;

        TextMeshProUGUI titulo = UIUtil.CriarTexto(
            conteudo,
            "Titulo",
            "PRÊMIOS",
            26f,
            new Vector2(Largura - 2f * Margem, AlturaTitulo),
            Vector2.zero,
            TextAlignmentOptions.Left
        );

        AncorarNoTopo(titulo.rectTransform, y, AlturaTitulo);
        TemaUI.EstilizarTexto(
            titulo, 26f, TemaUI.TextoForte, FontStyles.Bold,
            TextAlignmentOptions.Left, 1.5f);
        SemQuebra(titulo);

        y -= AlturaTitulo + 6f;

        Image linha = UIUtil.CriarImagem(
            conteudo,
            "Linha",
            TemaUI.Acento,
            new Vector2(Largura - 2f * Margem, 2f),
            Vector2.zero
        );

        linha.raycastTarget = false;
        AncorarNoTopo(linha.rectTransform, y, 2f);

        y -= 2f + 8f;

        TextMeshProUGUI rotuloChance = UIUtil.CriarTexto(
            conteudo,
            "RotuloChance",
            "CHANCE",
            16f,
            new Vector2(110f, AlturaColunas),
            Vector2.zero,
            TextAlignmentOptions.Left
        );

        rotuloChance.rectTransform.anchorMin = new Vector2(0f, 1f);
        rotuloChance.rectTransform.anchorMax = new Vector2(0f, 1f);
        rotuloChance.rectTransform.pivot = new Vector2(0f, 1f);
        rotuloChance.rectTransform.anchoredPosition = new Vector2(Margem, y);
        rotuloChance.rectTransform.sizeDelta = new Vector2(110f, AlturaColunas);
        TemaUI.EstilizarTexto(
            rotuloChance, 16f, TemaUI.TextoFraco, FontStyles.Normal,
            TextAlignmentOptions.Left, 1.5f);

        TextMeshProUGUI rotuloValor = UIUtil.CriarTexto(
            conteudo,
            "RotuloValor",
            "VALOR",
            16f,
            new Vector2(140f, AlturaColunas),
            Vector2.zero,
            TextAlignmentOptions.Right
        );

        rotuloValor.rectTransform.anchorMin = new Vector2(1f, 1f);
        rotuloValor.rectTransform.anchorMax = new Vector2(1f, 1f);
        rotuloValor.rectTransform.pivot = new Vector2(1f, 1f);
        rotuloValor.rectTransform.anchoredPosition = new Vector2(-Margem, y);
        rotuloValor.rectTransform.sizeDelta = new Vector2(140f, AlturaColunas);
        TemaUI.EstilizarTexto(
            rotuloValor, 16f, TemaUI.TextoFraco, FontStyles.Normal,
            TextAlignmentOptions.Right, 1.5f);

        y -= AlturaColunas + 8f;

        for (int i = 0; i < total; i++)
        {
            int indice = ordem[i];
            int valor = dados.tabelaValores[indice].valor;
            float chance = chances[indice];
            bool ehMaximo = valor > 0 && valor == maiorValor;

            if (chance <= 0.001f)
                continue;

            Image fundoLinha = UIUtil.CriarImagem(
                conteudo,
                "Linha" + i,
                Color.white,
                new Vector2(Largura - 2f * Margem, AlturaLinha),
                Vector2.zero
            );

            TemaUI.AplicarPlaca(fundoLinha, TemaUI.SpriteLinhaItem(), Color.white);
            fundoLinha.raycastTarget = false;
            AncorarNoTopo(fundoLinha.rectTransform, y, AlturaLinha);

            TextMeshProUGUI textoChance = UIUtil.CriarTexto(
                fundoLinha.transform,
                "Chance",
                FormatarChance(chance),
                22f,
                new Vector2(100f, 34f),
                Vector2.zero,
                TextAlignmentOptions.Left
            );

            RectTransform chanceRT = textoChance.rectTransform;
            chanceRT.anchorMin = new Vector2(0f, 0.5f);
            chanceRT.anchorMax = new Vector2(0f, 0.5f);
            chanceRT.pivot = new Vector2(0f, 0.5f);
            chanceRT.anchoredPosition = new Vector2(18f, 0f);
            chanceRT.sizeDelta = new Vector2(100f, 34f);

            TemaUI.EstilizarTexto(
                textoChance, 22f,
                ehMaximo ? TemaUI.TextoForte : TemaUI.TextoSuave,
                FontStyles.Normal, TextAlignmentOptions.Left);
            SemQuebra(textoChance);

            Color corValor = TemaUI.TextoForte;

            if (valor < 0)
                corValor = TemaUI.Vermelho;
            else if (valor == 0)
                corValor = TemaUI.TextoSuave;
            else if (ehMaximo)
                corValor = TemaUI.Acento;

            TextMeshProUGUI textoValor = UIUtil.CriarTexto(
                fundoLinha.transform,
                "Valor",
                FormatarValor(valor),
                22f,
                new Vector2(220f, 34f),
                Vector2.zero,
                TextAlignmentOptions.Right
            );

            RectTransform valorRT = textoValor.rectTransform;
            valorRT.anchorMin = new Vector2(1f, 0.5f);
            valorRT.anchorMax = new Vector2(1f, 0.5f);
            valorRT.pivot = new Vector2(1f, 0.5f);
            valorRT.anchoredPosition = new Vector2(-18f, 0f);
            valorRT.sizeDelta = new Vector2(220f, 34f);

            TemaUI.EstilizarTexto(
                textoValor, 22f, corValor,
                ehMaximo ? FontStyles.Bold : FontStyles.Normal,
                TextAlignmentOptions.Right);
            SemQuebra(textoValor);

            y -= AlturaLinha + EspacoLinha;
        }
    }

    static int NivelSorte()
    {
        return StatusManager.Instance != null
            ? StatusManager.Instance.nivelSorte
            : 1;
    }

    static string FormatarChance(float chance)
    {
        if (chance <= 0.001f)
            return "0%";

        // A Sorte anda de 1 em 1, mas arredondar evita "34,999%".
        float arredondada = Mathf.Round(chance * 10f) / 10f;
        return arredondada.ToString("0.#") + "%";
    }

    static string FormatarValor(int valor)
    {
        if (valor < 0)
            return "-" + NumeroUtil.FormatarDinheiro(-valor);

        return NumeroUtil.FormatarDinheiro(valor);
    }

    static void AncorarNoTopo(RectTransform rt, float y, float altura)
    {
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(Largura - 2f * Margem, altura);
    }

    static void SemQuebra(TextMeshProUGUI texto)
    {
        texto.textWrappingMode = TextWrappingModes.NoWrap;
        texto.overflowMode = TextOverflowModes.Overflow;
    }
}
