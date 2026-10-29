using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Aplica o tema em todos os painéis que já existem nas cenas, em runtime.
/// As cenas continuam guardando só os textos e os botões; o visual é montado
/// aqui com os materiais do jogo: mesa de madeira, latas de lixo, pedras e o
/// cinza-azulado dos painéis.
/// </summary>
public static class EstiloUI
{
    /// <summary>
    /// Liga o fundo de madeira, as latas e as pedras da tela de início.
    /// Desligado por enquanto: o menu fica só com os botões, sobre o azul
    /// da câmera. Basta trocar para true que a cena decorada volta.
    /// </summary>
    private static readonly bool MenuDecorado = false;

    /// <summary>
    /// RuntimeInitializeOnLoadMethod só roda na primeira cena; para vestir
    /// também o Jogo quando ele é carregado depois do menu, um objeto
    /// persistente escuta sceneLoaded.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CriarHook()
    {
        GameObject hook = new GameObject("EstiloUIHook");
        Object.DontDestroyOnLoad(hook);
        hook.AddComponent<EstiloUIHook>();
    }

    internal static void Aplicar()
    {
        Canvas canvas = CanvasPrincipal();
        if (canvas == null)
            return;

        // A cena vinha em "Constant Pixel Size", então a UI não acompanhava a
        // resolução: no fullscreen o dinheiro escorregava para o meio da tela.
        UIUtil.PadronizarEscala(canvas);

        string cena = SceneManager.GetActiveScene().name;

        if (cena == "MenuPrincipal")
            EstilizarMenu(canvas);
        else if (cena == "Jogo")
            EstilizarJogo(canvas);

        // Depois do tema, todos os textos da cena passam pelo tradutor. Os
        // rótulos montados aqui em cima também são traduzidos.
        Traducao.AplicarEmTudo();
    }

    /// <summary>
    /// A cena do Jogo tem mais de um Canvas: o principal e o do tooltip, que
    /// é um objeto raiz separado. Escolher o primeiro que aparecer deixaria os
    /// painéis sem tema, então procura o Canvas que realmente tem os painéis.
    /// </summary>
    private static Canvas CanvasPrincipal()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);

        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                continue;

            if (TemPainel(canvas.transform))
                return canvas;
        }

        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay &&
                canvas.isActiveAndEnabled)
            {
                return canvas;
            }
        }

        return canvases.Length > 0 ? canvases[0] : null;
    }

    private static bool TemPainel(Transform canvas)
    {
        return canvas.Find("PainelMenu") != null ||
               canvas.Find("PainelLoja") != null ||
               canvas.Find("PainelPause") != null ||
               canvas.Find("PainelConfiguracoes") != null;
    }

    /// <summary>Procura um Canvas raiz pelo nome do objeto (inclusive inativo).</summary>
    private static GameObject BuscarCanvas(string nome)
    {
        foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
        {
            if (canvas.gameObject.name == nome)
                return canvas.gameObject;
        }

        return null;
    }

    // --- Menu principal -------------------------------------------------

    private static void EstilizarMenu(Canvas canvas)
    {
        if (MenuDecorado)
            CriarFundoMenu(canvas);

        GameObject painelMenu = Filho(canvas.transform, "PainelMenu");
        if (painelMenu != null)
            EstilizarPainelMenu(painelMenu);

        GameObject painelConfig = Filho(canvas.transform, "PainelConfiguracoes");
        if (painelConfig != null)
            EstilizarPainelConfiguracoes(painelConfig);
    }

    /// <summary>
    /// O fundo do menu é a própria mesa do jogo: tábuas da cor de mesa.png,
    /// com as latas nas beiradas e pedras espalhadas, como no gameplay.
    /// </summary>
    private static void CriarFundoMenu(Canvas canvas)
    {
        if (canvas.transform.Find("__FundoMenu") != null)
            return;

        Image fundo = UIUtil.CriarImagem(
            canvas.transform,
            "__FundoMenu",
            TemaUI.Madeira,
            Vector2.zero,
            Vector2.zero
        );

        UIUtil.Esticar(fundo.rectTransform);
        TemaUI.AplicarSprite(fundo, TemaUI.SpriteMadeira(), Color.white);
        fundo.raycastTarget = false;
        fundo.transform.SetAsFirstSibling();

        Decorar(fundo.transform, "LataEsquerda",
            "Sprites/Maquinas/LATA DE LIXO",
            new Vector2(0f, 0f), new Vector2(96f, 430f), 300f);

        Decorar(fundo.transform, "LataDireita",
            "Sprites/Maquinas/LATA DE LIXO DE OURO",
            new Vector2(1f, 0f), new Vector2(-96f, 430f), 300f);

        Decorar(fundo.transform, "PedraAmetista",
            "Sprites/Pedras/100/100 AMETISTA",
            new Vector2(0f, 0f), new Vector2(304f, 380f), 196f);

        Decorar(fundo.transform, "PedraSafira",
            "Sprites/Pedras/100/100 SAFIRA",
            new Vector2(0f, 0f), new Vector2(440f, 172f), 168f);

        Decorar(fundo.transform, "PedraBonita",
            "Sprites/Pedras/100/100 PEDRA BONITA",
            new Vector2(1f, 0f), new Vector2(-308f, 356f), 206f);

        Decorar(fundo.transform, "PedraComum",
            "Sprites/Pedras/100/100 PEDRA COMUM",
            new Vector2(1f, 0f), new Vector2(-146f, 152f), 168f);
    }

    /// <summary>Coloca um sprite do jogo como enfeite, sem captar clique.</summary>
    private static void Decorar(
        Transform pai,
        string nome,
        string recurso,
        Vector2 ancoras,
        Vector2 posicao,
        float largura)
    {
        if (pai.Find(nome) != null)
            return;

        Sprite sprite = Resources.Load<Sprite>(recurso);
        if (sprite == null)
            return;

        Image imagem = UIUtil.CriarImagem(
            pai,
            nome,
            Color.white,
            new Vector2(largura, largura),
            posicao
        );

        RectTransform rt = imagem.rectTransform;
        rt.anchorMin = ancoras;
        rt.anchorMax = ancoras;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = posicao;

        imagem.sprite = SpriteUtil.RecortarParaVisivel(sprite);
        imagem.preserveAspect = true;
        imagem.raycastTarget = false;
    }

    private static void EstilizarPainelMenu(GameObject painel)
    {
        if (painel.transform.Find("CartaoMenu") != null)
            return;

        Image imagem = painel.GetComponent<Image>();
        if (imagem != null)
        {
            imagem.color = new Color(0f, 0f, 0f, 0f);
            imagem.sprite = null;
            imagem.raycastTarget = false;
        }

        DesativarLayouts(painel.transform);

        List<Transform> filhos = ColetarFilhos(painel.transform);
        TextMeshProUGUI titulo = null;

        foreach (Transform filho in filhos)
        {
            if (titulo == null && filho.GetComponent<TextMeshProUGUI>() != null)
                titulo = filho.GetComponent<TextMeshProUGUI>();
        }

        // Tela de início limpa: só o título e os botões. O resto do texto
        // que vinha na cena fica escondido.
        foreach (Transform filho in filhos)
        {
            TextMeshProUGUI texto = filho.GetComponent<TextMeshProUGUI>();
            if (texto != null && texto != titulo)
                texto.gameObject.SetActive(false);
        }

        // O cartão é só uma coluna invisível: sem fundo, sem moldura.
        RectTransform cartao = CriarCartao(
            painel.transform,
            "CartaoMenu",
            MenuDecorado ? new Vector2(720f, 920f) : new Vector2(540f, 690f),
            MenuDecorado
                ? new RectOffset(70, 70, 190, 30)
                : new RectOffset(0, 0, 0, 0),
            22f,
            false
        );

        VerticalLayoutGroup coluna = cartao.GetComponent<VerticalLayoutGroup>();
        if (coluna != null && !MenuDecorado)
            coluna.childAlignment = TextAnchor.MiddleCenter;

        if (MenuDecorado)
            CriarPedraMenu(cartao, 920f);

        if (titulo != null)
        {
            titulo.transform.SetParent(cartao, false);
            titulo.text = "ROCK IT";

            TemaUI.EstilizarTexto(
                titulo,
                96f,
                TemaUI.TextoForte,
                FontStyles.Normal,
                TextAlignmentOptions.Center
            );

            TemaUI.Sombra(titulo, 6f, 0.75f);
            TemaUI.AjustarAltura(titulo, 112f);
            TemaUI.AjustarLargura(titulo, 540f, 0f);
        }

        if (MenuDecorado)
        {
            TextMeshProUGUI subtitulo = UIUtil.CriarTexto(
                cartao,
                "__Subtitulo",
                "PEDRA  ·  SORTE  ·  LUCRO",
                22f,
                new Vector2(540f, 26f),
                Vector2.zero,
                TextAlignmentOptions.Center
            );

            TemaUI.EstilizarTexto(
                subtitulo,
                22f,
                TemaUI.Acento,
                FontStyles.Normal,
                TextAlignmentOptions.Center,
                6f
            );

            TemaUI.Sombra(subtitulo, 2f, 0.75f);
            TemaUI.AjustarAltura(subtitulo, 26f);
            TemaUI.AjustarLargura(subtitulo, 540f, 0f);
        }

        AdicionarBotaoMenu(cartao, filhos, "BotaoNovoJogo", "Novo jogo",
            TemaUI.EstiloBotao.Secundario, 84f);
        AdicionarBotaoMenu(cartao, filhos, "BotaoContinuar", "Continuar",
            TemaUI.EstiloBotao.Primario, 84f);
        AdicionarBotaoMenu(cartao, filhos, "BotaoConfiguracoes", "Configurações",
            TemaUI.EstiloBotao.Secundario, 84f);
        AdicionarBotaoMenu(cartao, filhos, "BotaoCreditos", "Créditos",
            TemaUI.EstiloBotao.Secundario, 84f);
        AdicionarBotaoMenu(cartao, filhos, "BotaoSair", "Sair",
            TemaUI.EstiloBotao.Perigo, 84f);
    }

    private static void CriarPedraMenu(RectTransform cartao, float alturaCartao)
    {
        if (cartao.Find("__Pedra") != null)
            return;

        Sprite sprite = Resources.Load<Sprite>("Sprites/Pedras/100/100 OURO");
        if (sprite == null)
            sprite = Resources.Load<Sprite>("Sprites/Pedras/100/100 PEDRA COMUM");
        if (sprite == null)
            return;

        Image pedra = UIUtil.CriarImagem(
            cartao,
            "__Pedra",
            Color.white,
            new Vector2(200f, 160f),
            new Vector2(0f, alturaCartao * 0.5f - 118f)
        );

        pedra.sprite = SpriteUtil.RecortarParaVisivel(sprite);
        pedra.preserveAspect = true;
        pedra.raycastTarget = false;

        LayoutElement layout = pedra.gameObject.AddComponent<LayoutElement>();
        layout.ignoreLayout = true;

        TemaUI.Sombra(pedra, 7f, 0.55f);
    }

    private static void AdicionarBotaoMenu(
        RectTransform cartao,
        List<Transform> filhos,
        string nomeObjeto,
        string rotulo,
        TemaUI.EstiloBotao estilo,
        float altura)
    {
        Transform filho = Buscar(filhos, nomeObjeto);
        if (filho == null)
            return;

        Button botao = filho.GetComponent<Button>();
        if (botao == null)
            return;

        filho.SetParent(cartao, false);
        TemaUI.EstilizarBotao(botao, estilo, 30f);
        TemaUI.AjustarAltura(botao, altura);
        TemaUI.AjustarLargura(botao, 540f, 0f);

        TextMeshProUGUI texto = botao.GetComponentInChildren<TextMeshProUGUI>(true);
        if (texto != null && !string.IsNullOrEmpty(rotulo))
            texto.text = rotulo;
    }

    // --- Configurações --------------------------------------------------

    private static void EstilizarPainelConfiguracoes(GameObject painel)
    {
        if (painel.transform.Find("CartaoConfiguracoes") != null)
            return;

        Image imagem = painel.GetComponent<Image>();
        if (imagem != null)
        {
            imagem.sprite = null;
            imagem.type = Image.Type.Simple;
            imagem.color = new Color(0.07f, 0.06f, 0.10f, 0.70f);
            imagem.raycastTarget = true;
        }

        DesativarLayouts(painel.transform);

        List<Transform> filhos = ColetarFilhos(painel.transform);

        RectTransform cartao = CriarCartao(
            painel.transform,
            "CartaoConfiguracoes",
            new Vector2(940f, 770f),
            new RectOffset(56, 56, 40, 36),
            8f
        );

        TextMeshProUGUI titulo = BuscarTexto(filhos, "Configurações");
        if (titulo != null)
        {
            titulo.transform.SetParent(cartao, false);
            titulo.text = "CONFIGURAÇÕES";

            TemaUI.EstilizarTexto(
                titulo,
                44f,
                TemaUI.TextoForte,
                FontStyles.Normal,
                TextAlignmentOptions.Center,
                3f
            );

            TemaUI.Sombra(titulo, 3f, 0.65f);
            TemaUI.AjustarAltura(titulo, 58f);
        }

        CriarSecao(cartao, "ÁUDIO");
        AdicionarRotulo(cartao, filhos, "Volume Geral");
        AdicionarSlider(cartao, filhos, "SliderVolumeGeral");
        AdicionarRotulo(cartao, filhos, "Volume Música");
        AdicionarSlider(cartao, filhos, "SliderVolumeMusica");
        AdicionarRotulo(cartao, filhos, "Volume Efeitos");
        AdicionarSlider(cartao, filhos, "SliderVolumeEfeitos");

        CriarSecao(cartao, "VÍDEO");
        AdicionarRotulo(cartao, filhos, "Resolução");
        AdicionarDropdown(cartao, filhos, "DropdownResolucao");
        AdicionarToggle(cartao, filhos, "ToggleFullscreen");

        CriarSecao(cartao, "IDIOMA");
        AdicionarRotulo(cartao, filhos, "Idioma");
        AdicionarDropdown(cartao, filhos, "DropdownIdioma");

        Transform fechar = Buscar(filhos, "BotaoFecharConfiguracoes");
        if (fechar != null)
        {
            fechar.SetParent(cartao, false);

            Button botao = fechar.GetComponent<Button>();
            if (botao != null)
            {
                TemaUI.EstilizarBotao(botao, TemaUI.EstiloBotao.Perigo, 20f);

                TextMeshProUGUI texto =
                    botao.GetComponentInChildren<TextMeshProUGUI>(true);
                if (texto != null)
                    texto.text = "X";
            }

            LayoutElement layout = fechar.GetComponent<LayoutElement>();
            if (layout == null)
                layout = fechar.gameObject.AddComponent<LayoutElement>();

            layout.ignoreLayout = true;

            RectTransform rt = fechar as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(-34f, -32f);
                rt.sizeDelta = new Vector2(42f, 42f);
            }

            fechar.SetAsLastSibling();
        }
    }

    private static void AdicionarRotulo(
        RectTransform cartao,
        List<Transform> filhos,
        string nome)
    {
        TextMeshProUGUI texto = BuscarTexto(filhos, nome);
        if (texto == null)
            return;

        texto.transform.SetParent(cartao, false);
        texto.text = LimparRotulo(texto.text);

        TemaUI.EstilizarTexto(
            texto,
            21f,
            TemaUI.Texto,
            FontStyles.Normal,
            TextAlignmentOptions.Left
        );

        texto.margin = new Vector4(4f, 0f, 0f, 0f);
        TemaUI.AjustarAltura(texto, 26f);
    }

    private static void AdicionarSlider(
        RectTransform cartao,
        List<Transform> filhos,
        string nome)
    {
        Transform filho = Buscar(filhos, nome);
        if (filho == null)
            return;

        filho.SetParent(cartao, false);
        TemaUI.EstilizarSlider(filho.GetComponent<Slider>());
        TemaUI.AjustarAltura(filho.GetComponent<Slider>(), 30f);
    }

    private static void AdicionarDropdown(
        RectTransform cartao,
        List<Transform> filhos,
        string nome)
    {
        Transform filho = Buscar(filhos, nome);
        if (filho == null)
            return;

        filho.SetParent(cartao, false);
        TemaUI.EstilizarDropdown(filho.GetComponent<TMP_Dropdown>());
        TemaUI.AjustarAltura(filho.GetComponent<TMP_Dropdown>(), 50f);
    }

    private static void AdicionarToggle(
        RectTransform cartao,
        List<Transform> filhos,
        string nome)
    {
        Transform filho = Buscar(filhos, nome);
        if (filho == null)
            return;

        filho.SetParent(cartao, false);
        TemaUI.EstilizarToggle(filho.GetComponent<Toggle>());
        TemaUI.AjustarAltura(filho.GetComponent<Toggle>(), 44f);
    }

    private static void CriarSecao(RectTransform cartao, string titulo)
    {
        string nome = "__Secao_" + titulo.Replace(" ", "");
        if (cartao.Find(nome) != null)
            return;

        RectTransform secao = UIUtil.CriarRetangulo(cartao, nome);

        LayoutElement layout = secao.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = 40f;
        layout.preferredHeight = 40f;
        layout.flexibleHeight = 0f;

        Image faixa = UIUtil.CriarImagem(
            secao,
            "Faixa",
            new Color(1f, 1f, 1f, 0f),
            Vector2.zero,
            Vector2.zero
        );

        UIUtil.Esticar(faixa.rectTransform);
        faixa.raycastTarget = false;

        Image linha = UIUtil.CriarImagem(
            secao,
            "Linha",
            new Color32(72, 71, 82, 255),
            Vector2.zero,
            Vector2.zero
        );

        linha.sprite = null;
        linha.raycastTarget = false;

        RectTransform linhaRT = linha.rectTransform;
        linhaRT.anchorMin = new Vector2(0f, 0f);
        linhaRT.anchorMax = new Vector2(1f, 0f);
        linhaRT.pivot = new Vector2(0.5f, 0f);
        linhaRT.anchoredPosition = new Vector2(0f, 2f);
        linhaRT.sizeDelta = new Vector2(0f, 2f);

        TextMeshProUGUI rotulo = UIUtil.CriarTexto(
            secao,
            "Rotulo",
            titulo,
            16f,
            new Vector2(300f, 30f),
            Vector2.zero,
            TextAlignmentOptions.Left
        );

        RectTransform rotuloRT = rotulo.rectTransform;
        rotuloRT.anchorMin = new Vector2(0f, 0f);
        rotuloRT.anchorMax = new Vector2(1f, 1f);
        rotuloRT.offsetMin = new Vector2(2f, 4f);
        rotuloRT.offsetMax = new Vector2(-2f, 0f);

        TemaUI.EstilizarTexto(
            rotulo,
            16f,
            TemaUI.Acento,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            6f
        );
    }

    private static string LimparRotulo(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;

        return texto.Trim().Trim('"');
    }

    // --- Jogo -----------------------------------------------------------

    private static void EstilizarJogo(Canvas canvas)
    {
        GameObject loja = Filho(canvas.transform, "PainelLoja");
        if (loja != null)
            EstilizarPainelLoja(loja);

        GameObject pause = Filho(canvas.transform, "PainelPause");
        if (pause != null)
            EstilizarPainelPause(pause);

        GameObject config = Filho(canvas.transform, "PainelConfiguracoes");
        if (config != null)
            EstilizarPainelConfiguracoes(config);

        GameObject tooltip = Filho(canvas.transform, "PainelTooltip");
        if (tooltip == null)
            tooltip = BuscarCanvas("PainelTooltip");

        if (tooltip != null)
            EstilizarPainelTooltip(tooltip);
    }

    private static void EstilizarPainelPause(GameObject painel)
    {
        if (painel.transform.Find("CartaoPause") != null)
            return;

        Image imagem = painel.GetComponent<Image>();
        if (imagem != null)
        {
            imagem.sprite = null;
            imagem.type = Image.Type.Simple;
            imagem.color = new Color(0.07f, 0.06f, 0.10f, 0.70f);
            imagem.raycastTarget = true;
        }

        DesativarLayouts(painel.transform);

        List<Transform> filhos = ColetarFilhos(painel.transform);

        RectTransform cartao = CriarCartao(
            painel.transform,
            "CartaoPause",
            new Vector2(470f, 450f),
            new RectOffset(42, 42, 38, 34),
            14f
        );

        TextMeshProUGUI titulo = null;
        foreach (Transform filho in filhos)
        {
            TextMeshProUGUI texto = filho.GetComponent<TextMeshProUGUI>();
            if (texto != null)
            {
                titulo = texto;
                break;
            }
        }

        if (titulo != null)
        {
            titulo.transform.SetParent(cartao, false);
            titulo.text = "PAUSADO";

            TemaUI.EstilizarTexto(
                titulo,
                46f,
                TemaUI.TextoForte,
                FontStyles.Normal,
                TextAlignmentOptions.Center,
                3f
            );

            TemaUI.Sombra(titulo, 3f, 0.65f);
            TemaUI.AjustarAltura(titulo, 62f);
        }

        AdicionarBotaoPause(cartao, filhos, "BotaoRetomar", "Retomar",
            TemaUI.EstiloBotao.Primario);
        AdicionarBotaoPause(cartao, filhos, "BotaoConfiguracoesPause", "Configurações",
            TemaUI.EstiloBotao.Secundario);
        AdicionarBotaoPause(cartao, filhos, "BotaoSalvar", "Salvar",
            TemaUI.EstiloBotao.Secundario);
        AdicionarBotaoPause(cartao, filhos, "BotaoSairPause", "Sair",
            TemaUI.EstiloBotao.Perigo);
    }

    private static void AdicionarBotaoPause(
        RectTransform cartao,
        List<Transform> filhos,
        string nomeObjeto,
        string rotulo,
        TemaUI.EstiloBotao estilo)
    {
        Transform filho = Buscar(filhos, nomeObjeto);
        if (filho == null)
            return;

        Button botao = filho.GetComponent<Button>();
        if (botao == null)
            return;

        filho.SetParent(cartao, false);
        TemaUI.EstilizarBotao(botao, estilo, 24f);
        TemaUI.AjustarAltura(botao, 54f);

        TextMeshProUGUI texto = botao.GetComponentInChildren<TextMeshProUGUI>(true);
        if (texto != null)
            texto.text = rotulo;
    }

    private static void EstilizarPainelLoja(GameObject painel)
    {
        if (painel.transform.Find("__LojaEstilizada") != null)
            return;

        GameObject marcador = new GameObject("__LojaEstilizada", typeof(RectTransform));
        marcador.transform.SetParent(painel.transform, false);
        marcador.transform.SetAsLastSibling();

        Image fundo = painel.GetComponent<Image>();
        if (fundo != null)
        {
            TemaUI.AplicarPlaca(fundo, TemaUI.SpritePainel(), Color.white);
            TemaUI.Sombra(fundo, 14f, 0.75f);
        }

        Transform barra = painel.transform.Find("BarraNavegacao");
        if (barra != null)
            EstilizarBarraLoja(barra);

        Transform botaoFechar = painel.transform.Find("BotaoFechar");
        if (botaoFechar != null)
        {
            Button botao = botaoFechar.GetComponent<Button>();
            if (botao != null)
            {
                TemaUI.EstilizarBotao(botao, TemaUI.EstiloBotao.Perigo, 20f);

                TextMeshProUGUI rotulo =
                    botao.GetComponentInChildren<TextMeshProUGUI>(true);
                if (rotulo != null)
                    rotulo.text = "X";
            }

            RectTransform rt = botaoFechar as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(-36f, -34f);
                rt.sizeDelta = new Vector2(42f, 42f);
            }

            botaoFechar.SetAsLastSibling();
        }

        Transform area = painel.transform.Find("AreaConteudo");
        if (area == null)
            return;

        Image imagemArea = area.GetComponent<Image>();
        if (imagemArea != null)
        {
            imagemArea.sprite = null;
            imagemArea.color = new Color(0f, 0f, 0f, 0f);
            imagemArea.raycastTarget = false;

            RectTransform areaRT = imagemArea.rectTransform;
            areaRT.offsetMin = new Vector2(22f, 22f);
            areaRT.offsetMax = new Vector2(-22f, -70f);
        }

        foreach (Transform aba in area)
            EstilizarAbaLoja(aba);
    }

    private static void EstilizarBarraLoja(Transform barra)
    {
        Image imagemBarra = barra.GetComponent<Image>();
        if (imagemBarra != null)
        {
            imagemBarra.sprite = null;
            imagemBarra.color = new Color(0f, 0f, 0f, 0f);
            imagemBarra.raycastTarget = false;
        }

        RectTransform barraRT = barra as RectTransform;
        if (barraRT != null)
        {
            barraRT.anchorMin = new Vector2(0f, 1f);
            barraRT.anchorMax = new Vector2(1f, 1f);
            barraRT.pivot = new Vector2(0.5f, 0.5f);
            barraRT.anchoredPosition = new Vector2(0f, -40f);
            barraRT.sizeDelta = new Vector2(-36f, 48f);
        }

        HorizontalLayoutGroup layoutBarra = barra.GetComponent<HorizontalLayoutGroup>();
        if (layoutBarra != null)
        {
            layoutBarra.padding = new RectOffset(24, 118, 4, 6);
            layoutBarra.spacing = 6f;
            layoutBarra.childControlWidth = true;
            layoutBarra.childControlHeight = true;
            layoutBarra.childForceExpandWidth = true;
            layoutBarra.childForceExpandHeight = true;
            layoutBarra.childAlignment = TextAnchor.MiddleCenter;
        }

        if (barra.Find("__LinhaBarra") == null)
        {
            Image linha = UIUtil.CriarImagem(
                barra,
                "__LinhaBarra",
                new Color32(72, 71, 82, 255),
                Vector2.zero,
                Vector2.zero
            );

            linha.sprite = null;
            linha.raycastTarget = false;

            RectTransform rt = linha.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 0f);
            rt.sizeDelta = new Vector2(-28f, 2f);

            LayoutElement layout = linha.gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;
        }

        Button[] abas = new Button[4];
        TextMeshProUGUI[] rotulos = new TextMeshProUGUI[4];
        Image[] linhas = new Image[4];

        string[] nomes = { "BotaoPedras", "BotaoStatus", "BotaoHabilidades", "BotaoMaquinas" };

        for (int i = 0; i < nomes.Length; i++)
        {
            Transform filho = barra.Find(nomes[i]);
            if (filho == null)
                continue;

            Button botao = filho.GetComponent<Button>();
            if (botao == null)
                continue;

            LayoutElement layoutAntigo = botao.GetComponent<LayoutElement>();
            if (layoutAntigo != null)
                Object.Destroy(layoutAntigo);

            Image fundoAba = botao.GetComponent<Image>();
            if (fundoAba != null)
            {
                fundoAba.sprite = null;
                fundoAba.color = new Color(0f, 0f, 0f, 0f);
                fundoAba.raycastTarget = true;
            }

            ColorBlock cores = botao.colors;
            cores.normalColor = Color.white;
            cores.highlightedColor = Color.white;
            cores.pressedColor = new Color(0.78f, 0.78f, 0.80f, 1f);
            cores.selectedColor = Color.white;
            cores.disabledColor = Color.white;
            botao.colors = cores;

            TextMeshProUGUI rotulo = botao.GetComponentInChildren<TextMeshProUGUI>(true);
            if (rotulo != null)
            {
                rotulo.text = rotulo.text.ToUpperInvariant();
                TemaUI.EstilizarTexto(
                    rotulo,
                    21f,
                    TemaUI.TextoFraco,
                    FontStyles.Normal,
                    TextAlignmentOptions.Center,
                    2f
                );

                rotulo.textWrappingMode = TextWrappingModes.NoWrap;
                rotulo.raycastTarget = false;
            }

            Image linhaAba = null;
            Transform linhaExistente = filho.Find("__Sublinhado");

            if (linhaExistente != null)
            {
                linhaAba = linhaExistente.GetComponent<Image>();
            }
            else
            {
                linhaAba = UIUtil.CriarImagem(
                    filho,
                    "__Sublinhado",
                    TemaUI.Acento,
                    new Vector2(0f, 3f),
                    Vector2.zero
                );

                linhaAba.sprite = TemaUI.SpriteQuadrado();
                linhaAba.type = Image.Type.Simple;
                linhaAba.raycastTarget = false;

                RectTransform rt = linhaAba.rectTransform;
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0f, 4f);
                rt.sizeDelta = new Vector2(-42f, 3f);
            }

            abas[i] = botao;
            rotulos[i] = rotulo;
            linhas[i] = linhaAba;
        }

        AbasLojaUI abasUI = barra.GetComponent<AbasLojaUI>();
        if (abasUI == null)
            abasUI = barra.gameObject.AddComponent<AbasLojaUI>();

        abasUI.abas = abas;
        abasUI.rotulos = rotulos;
        abasUI.linhas = linhas;
        abasUI.AtualizarAgora();
    }

    private static void EstilizarAbaLoja(Transform aba)
    {
        Image fundoAba = aba.GetComponent<Image>();
        bool temMascara = aba.GetComponent<Mask>() != null;

        if (fundoAba != null)
        {
            TemaUI.AplicarPlaca(fundoAba, TemaUI.SpriteRebaixado(), Color.white);

            // Na aba de habilidades a Image é o recorte do Mask e precisa
            // continuar recebendo o ponteiro para arrastar/zoom a árvore.
            fundoAba.raycastTarget = temMascara;
        }

        if (temMascara && aba.Find("__FundoAba") == null)
        {
            Image fundoInterno = UIUtil.CriarImagem(
                aba,
                "__FundoAba",
                Color.white,
                Vector2.zero,
                Vector2.zero
            );

            UIUtil.Esticar(fundoInterno.rectTransform);
            TemaUI.AplicarPlaca(fundoInterno, TemaUI.SpriteRebaixado(), Color.white);
            fundoInterno.raycastTarget = false;
            fundoInterno.transform.SetAsFirstSibling();
        }

        foreach (Image imagem in aba.GetComponentsInChildren<Image>(true))
        {
            string nome = imagem.gameObject.name;

            // "Viewport" fica de fora de propósito: a Image dele é o
            // recorte (Mask) do ScrollRect e precisa continuar opaca.
            if (nome == "Scroll View" || nome == "ScrollView" ||
                nome == "Viewport Mask")
            {
                imagem.sprite = null;
                imagem.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        ScrollRect scroll = aba.GetComponentInChildren<ScrollRect>(true);
        if (scroll != null)
        {
            scroll.horizontal = false;

            // O padrão (1) anda quase nada por clique da roda, o que deixa a
            // lista de pedras lenta demais. 36 dá cerca de um terço de item
            // por clique, sem ficar difícil de controlar.
            scroll.scrollSensitivity = TemaUI.SensibilidadeScroll;

            if (scroll.horizontalScrollbar != null)
                scroll.horizontalScrollbar.gameObject.SetActive(false);
            else
            {
                foreach (Scrollbar barra in
                    aba.GetComponentsInChildren<Scrollbar>(true))
                {
                    if (barra.direction == Scrollbar.Direction.LeftToRight ||
                        barra.direction == Scrollbar.Direction.RightToLeft)
                    {
                        barra.gameObject.SetActive(false);
                    }
                }
            }

            if (scroll.content != null)
            {
                VerticalLayoutGroup layout =
                    scroll.content.GetComponent<VerticalLayoutGroup>();

                if (layout != null)
                {
                    layout.padding = new RectOffset(8, 8, 8, 8);
                    layout.spacing = 10f;
                }
            }
        }

        foreach (Scrollbar scrollbar in aba.GetComponentsInChildren<Scrollbar>(true))
            TemaUI.EstilizarScrollbar(scrollbar);
    }

    private static void EstilizarPainelTooltip(GameObject painel)
    {
        Image imagem = painel.GetComponent<Image>();
        if (imagem != null)
        {
            TemaUI.AplicarPlaca(imagem, TemaUI.SpritePainelFino(), Color.white);
            TemaUI.Sombra(imagem, 6f, 0.60f);
            imagem.raycastTarget = false;
        }

        RectTransform rt = painel.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(340f, 214f);
        }

        VerticalLayoutGroup layout = painel.GetComponent<VerticalLayoutGroup>();
        if (layout != null)
        {
            layout.padding = new RectOffset(18, 18, 15, 13);
            layout.spacing = 5f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        foreach (Transform filho in painel.transform)
        {
            TextMeshProUGUI texto = filho.GetComponent<TextMeshProUGUI>();
            if (texto == null)
                continue;

            texto.raycastTarget = false;
            texto.alignment = TextAlignmentOptions.TopLeft;

            if (filho.name == "TooltipNome")
            {
                TemaUI.EstilizarTexto(
                    texto,
                    22f,
                    TemaUI.TextoForte,
                    FontStyles.Bold,
                    TextAlignmentOptions.Left
                );

                TemaUI.AjustarAltura(texto, 30f);
            }
            else if (filho.name == "TooltipCusto")
            {
                TemaUI.EstilizarTexto(
                    texto,
                    18f,
                    TemaUI.Acento,
                    FontStyles.Bold,
                    TextAlignmentOptions.Left
                );

                TemaUI.AjustarAltura(texto, 24f);
            }
            else
            {
                TemaUI.EstilizarTexto(
                    texto,
                    17f,
                    TemaUI.TextoSuave,
                    FontStyles.Normal,
                    TextAlignmentOptions.TopLeft
                );

                TemaUI.AjustarAltura(texto, 110f);
            }
        }
    }

    // --- Peças reutilizáveis --------------------------------------------

    private static RectTransform CriarCartao(
        Transform pai,
        string nome,
        Vector2 tamanho,
        RectOffset padding,
        float espacamento,
        bool placa = true)
    {
        Image fundo = UIUtil.CriarImagem(
            pai,
            nome,
            Color.white,
            tamanho,
            Vector2.zero
        );

        RectTransform rt = fundo.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        if (placa)
        {
            TemaUI.AplicarPlaca(fundo, TemaUI.SpritePainel(), Color.white);
            TemaUI.Sombra(fundo, 12f, 0.78f);
        }
        else
        {
            fundo.sprite = null;
            fundo.color = new Color(0f, 0f, 0f, 0f);
            fundo.raycastTarget = false;
        }

        VerticalLayoutGroup layout = fundo.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = padding;
        layout.spacing = espacamento;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = placa;
        layout.childForceExpandHeight = false;

        return rt;
    }

    private static void DesativarLayouts(Transform painel)
    {
        foreach (LayoutGroup layout in painel.GetComponents<LayoutGroup>())
            layout.enabled = false;

        foreach (ContentSizeFitter fitter in painel.GetComponents<ContentSizeFitter>())
            fitter.enabled = false;
    }

    private static List<Transform> ColetarFilhos(Transform pai)
    {
        List<Transform> filhos = new List<Transform>();

        foreach (Transform filho in pai)
            filhos.Add(filho);

        return filhos;
    }

    private static Transform Buscar(List<Transform> filhos, string nome)
    {
        foreach (Transform filho in filhos)
        {
            if (NomeLimpo(filho.name) == nome)
                return filho;
        }

        return null;
    }

    private static TextMeshProUGUI BuscarTexto(
        List<Transform> filhos,
        string nome)
    {
        foreach (Transform filho in filhos)
        {
            if (NomeLimpo(filho.name) != nome)
                continue;

            return filho.GetComponent<TextMeshProUGUI>();
        }

        return null;
    }

    private static string NomeLimpo(string nome)
    {
        return (nome ?? "").Trim().Trim('"');
    }

    private static GameObject Filho(Transform pai, string nome)
    {
        Transform filho = pai.Find(nome);
        return filho != null ? filho.gameObject : null;
    }
}

/// <summary>
/// Mantém o sublinhado e a cor da aba ativa na barra da loja. As abas em si
/// continuam sendo os botões da cena; este componente só troca o visual.
/// </summary>
public class AbasLojaUI : MonoBehaviour
{
    public Button[] abas;
    public TextMeshProUGUI[] rotulos;
    public Image[] linhas;

    public Color corAtiva = new Color32(255, 255, 255, 255);
    public Color corInativa = new Color32(138, 135, 148, 255);

    private int atual = -1;
    private NavegacaoLoja navegacao;

    void OnEnable()
    {
        NavegacaoLoja.OnPainelTrocado += Trocar;
        AtualizarAgora();
    }

    void OnDisable()
    {
        NavegacaoLoja.OnPainelTrocado -= Trocar;
    }

    void Start()
    {
        AtualizarAgora();
    }

    /// <summary>Relê o painel atual e repinta as abas.</summary>
    public void AtualizarAgora()
    {
        if (navegacao == null)
            navegacao = GetComponentInParent<NavegacaoLoja>();

        int indice = navegacao != null ? navegacao.PainelAtual : 0;
        Aplicar(indice);
    }

    void Trocar(int indice)
    {
        Aplicar(indice);
    }

    void Aplicar(int indice)
    {
        if (rotulos == null || linhas == null)
            return;

        atual = indice;

        for (int i = 0; i < rotulos.Length; i++)
        {
            bool ativa = i == indice;

            if (rotulos[i] != null)
                rotulos[i].color = ativa ? corAtiva : corInativa;

            if (linhas[i] != null)
            {
                linhas[i].color = TemaUI.Acento;
                linhas[i].enabled = ativa;
            }
        }
    }
}
