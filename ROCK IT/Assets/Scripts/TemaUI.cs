using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Paleta e peças visuais dos menus, tiradas dos próprios materiais do jogo:
/// a madeira da mesa, o cinza-azulado das latas e o aro dourado.
/// Tudo é gerado em runtime com texturas pequenas e filtro Point: cantos em
/// degrau, sombra dura e nenhum gradiente suave ou brilho de vidro.
/// </summary>
public static class TemaUI
{
    // --- Paleta ---------------------------------------------------------

    // Fundo da câmera do jogo e madeira da mesa.
    public static readonly Color Fundo = new Color32(49, 80, 122, 255);
    public static readonly Color Madeira = new Color32(159, 118, 109, 255);
    public static readonly Color MadeiraClara = new Color32(186, 148, 139, 255);
    public static readonly Color MadeiraEscura = new Color32(122, 80, 72, 255);
    public static readonly Color MadeiraVeio = new Color32(138, 97, 86, 255);

    // Painéis e botões: o cinza-azulado das latas de lixo.
    public static readonly Color Painel = new Color32(59, 58, 68, 255);
    public static readonly Color PainelClaro = new Color32(72, 71, 82, 255);
    public static readonly Color PainelEscuro = new Color32(42, 41, 50, 255);
    public static readonly Color Contorno = new Color32(34, 32, 52, 255);

    // Nomes antigos mantidos para o resto do código continuar compilando.
    public static readonly Color Cartao = Painel;
    public static readonly Color CartaoClaro = PainelClaro;
    public static readonly Color CartaoEscuro = PainelEscuro;

    public static readonly Color TextoForte = new Color32(255, 255, 255, 255);
    public static readonly Color Texto = TextoForte;
    public static readonly Color TextoSuave = new Color32(198, 196, 206, 255);
    public static readonly Color TextoFraco = new Color32(138, 135, 148, 255);

    // Acento: o aro amarelo-verde da lata.
    public static readonly Color Acento = new Color32(209, 205, 106, 255);
    public static readonly Color AcentoEscuro = new Color32(150, 147, 77, 255);

    // Dourado do Lixo de Luxo. O PNG do upgrade é igual ao normal, então o
    // tom vem do tingimento do sprite.
    public static readonly Color Dourado = new Color32(255, 201, 92, 255);

    public static readonly Color Verde = new Color32(168, 208, 106, 255);
    public static readonly Color Vermelho = new Color32(226, 106, 96, 255);
    public static readonly Color Azul = new Color32(96, 146, 194, 255);

    private static readonly Dictionary<string, Sprite> cache =
        new Dictionary<string, Sprite>();

    // --- Texturas base --------------------------------------------------

    private static Texture2D NovaTextura(int largura, int altura, string nome)
    {
        Texture2D textura = new Texture2D(largura, altura, TextureFormat.RGBA32, false);
        textura.name = nome;
        textura.filterMode = FilterMode.Point;
        textura.wrapMode = TextureWrapMode.Clamp;
        textura.hideFlags = HideFlags.HideAndDontSave;
        return textura;
    }

    private static Sprite CriarSprite(
        Texture2D textura,
        Vector4 borda,
        string nome,
        bool repetir = false)
    {
        if (repetir)
            textura.wrapMode = TextureWrapMode.Repeat;

        Sprite sprite = Sprite.Create(
            textura,
            new Rect(0f, 0f, textura.width, textura.height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            borda
        );

        sprite.name = nome;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private static float Ruido(int x, int y, int semente)
    {
        int n = x * 374761393 + y * 668265263 + semente * 2147483647;
        n = (n ^ (n >> 13)) * 1274126177;
        n = n ^ (n >> 16);
        return (n & 1023) / 1023f;
    }

    // --- Molduras de 9 fatias -------------------------------------------

    /// <summary>
    /// Placa de 9 fatias com cantos cortados em degrau. <paramref name="corte"/>
    /// é quantos pixels cada canto perde na diagonal; zero deixa o canto reto.
    /// </summary>
    private static Sprite Moldura(
        string chave,
        int borda,
        Color centro,
        Color luz,
        Color sombra,
        Color contorno,
        int corte = 0,
        bool centroVazio = false)
    {
        if (cache.TryGetValue(chave, out Sprite emCache) && emCache != null)
            return emCache;

        int tamanho = Mathf.Max(Mathf.Max(borda * 2 + 8, corte * 2 + 8), 24);
        Texture2D textura = NovaTextura(tamanho, tamanho, "TemaUI_" + chave);
        Color32[] pixels = new Color32[tamanho * tamanho];

        for (int y = 0; y < tamanho; y++)
        {
            for (int x = 0; x < tamanho; x++)
            {
                int esq = x;
                int dir = tamanho - 1 - x;
                int baixo = y;
                int cima = tamanho - 1 - y;
                int d = Mathf.Min(Mathf.Min(esq, dir), Mathf.Min(baixo, cima));

                // Canto cortado na diagonal, sem arredondar nada.
                bool foraDoCanto =
                    esq + baixo < corte ||
                    dir + baixo < corte ||
                    esq + cima < corte ||
                    dir + cima < corte;

                Color cor;

                if (foraDoCanto)
                {
                    cor = new Color(0f, 0f, 0f, 0f);
                }
                else if (d == 0)
                {
                    cor = contorno;
                }
                else if (d < borda)
                {
                    bool iluminado = esq < borda || cima < borda;
                    cor = iluminado ? luz : sombra;

                    // Vinco interno: separa a borda do miolo.
                    if (d == borda - 1)
                        cor = Color.Lerp(cor, contorno, 0.45f);

                    float j = (Ruido(x, y, borda * 31) - 0.5f) * 0.04f;
                    cor += new Color(j, j, j, 0f);
                }
                else if (centroVazio)
                {
                    cor = new Color(0f, 0f, 0f, 0f);
                }
                else
                {
                    cor = centro;
                }

                cor.a = Mathf.Clamp01(cor.a);
                pixels[y * tamanho + x] = cor;
            }
        }

        textura.SetPixels32(pixels);
        textura.Apply(false, true);

        // A fatia precisa conter o canto inteiro, senão o corte diagonal é
        // esmagado na hora de esticar a placa.
        int fatia = Mathf.Max(borda, corte) + 1;

        Sprite sprite = CriarSprite(
            textura,
            new Vector4(fatia, fatia, fatia, fatia),
            "TemaUI_" + chave
        );
        cache[chave] = sprite;
        return sprite;
    }

    // --- Peças públicas -------------------------------------------------

    /// <summary>Placa principal: cinza-azulado da lata com canto cortado.</summary>
    public static Sprite SpritePainel()
    {
        return Moldura(
            "painel", 3,
            Painel,
            new Color32(82, 81, 94, 255),
            new Color32(43, 42, 52, 255),
            Contorno,
            10
        );
    }

    /// <summary>Versão fina da placa, para tooltips e balões.</summary>
    public static Sprite SpritePainelFino()
    {
        return Moldura(
            "painelFino", 3,
            new Color32(52, 51, 62, 255),
            new Color32(74, 73, 86, 255),
            new Color32(38, 37, 47, 255),
            Contorno,
            7
        );
    }

    /// <summary>Caixa rebaixada, para áreas de conteúdo e trilhos.</summary>
    public static Sprite SpriteRebaixado()
    {
        return Moldura(
            "rebaixado", 3,
            new Color32(42, 41, 50, 255),
            new Color32(32, 31, 40, 255),
            new Color32(76, 75, 88, 255),
            Contorno,
            7
        );
    }

    /// <summary>Linha de item: um degrau acima do fundo do painel.</summary>
    public static Sprite SpriteLinhaItem()
    {
        return Moldura(
            "linhaItem", 3,
            PainelClaro,
            new Color32(90, 89, 101, 255),
            new Color32(54, 53, 64, 255),
            new Color32(42, 41, 50, 255)
        );
    }

    /// <summary>Placa neutra em tons de cinza, tingida pelo estado do objeto.</summary>
    public static Sprite SpritePlacaNeutra()
    {
        return Moldura(
            "placaNeutra", 3,
            new Color32(230, 230, 230, 255),
            new Color32(255, 255, 255, 255),
            new Color32(168, 168, 168, 255),
            new Color32(46, 46, 46, 255),
            6
        );
    }

    /// <summary>Aro dourado vazado, usado por cima de botões e abas ativas.</summary>
    public static Sprite SpriteAro()
    {
        return Moldura(
            "aro", 2,
            Color.clear,
            Color.white,
            Color.white,
            Color.white,
            7,
            true
        );
    }

    public static Sprite SpriteBotao(EstiloBotao estilo)
    {
        switch (estilo)
        {
            case EstiloBotao.Primario:
                return Moldura(
                    "botaoPrimario", 3,
                    new Color32(88, 87, 99, 255),
                    new Color32(112, 111, 124, 255),
                    new Color32(58, 57, 68, 255),
                    Contorno,
                    7
                );

            case EstiloBotao.Perigo:
                return Moldura(
                    "botaoPerigo", 3,
                    new Color32(98, 52, 54, 255),
                    new Color32(126, 70, 72, 255),
                    new Color32(68, 34, 38, 255),
                    Contorno,
                    7
                );

            case EstiloBotao.Verde:
                return Moldura(
                    "botaoVerde", 3,
                    new Color32(88, 87, 99, 255),
                    new Color32(112, 111, 124, 255),
                    new Color32(58, 57, 68, 255),
                    Contorno,
                    7
                );

            case EstiloBotao.Fantasma:
                return Moldura(
                    "botaoFantasma", 2,
                    Color.clear,
                    new Color32(92, 90, 104, 255),
                    new Color32(56, 55, 66, 255),
                    new Color32(56, 55, 66, 255),
                    7,
                    true
                );

            default:
                return Moldura(
                    "botaoNormal", 3,
                    PainelClaro,
                    new Color32(94, 93, 105, 255),
                    new Color32(50, 49, 60, 255),
                    Contorno,
                    7
                );
        }
    }

    public static Sprite SpriteSliderFundo()
    {
        return Moldura(
            "sliderFundo", 2,
            new Color32(42, 41, 50, 255),
            new Color32(30, 29, 38, 255),
            new Color32(64, 63, 75, 255),
            Contorno
        );
    }

    public static Sprite SpriteSliderPreenchimento()
    {
        return Moldura(
            "sliderPreenchimento", 2,
            Acento,
            new Color32(232, 228, 132, 255),
            AcentoEscuro,
            new Color32(96, 94, 44, 255)
        );
    }

    public static Sprite SpriteAlca()
    {
        return Moldura(
            "alca", 2,
            new Color32(232, 229, 200, 255),
            new Color32(255, 253, 226, 255),
            new Color32(178, 174, 138, 255),
            Contorno,
            2
        );
    }

    /// <summary>Linha horizontal com as pontas apagadas.</summary>
    public static Sprite SpriteLinha()
    {
        string chave = "linha";

        if (cache.TryGetValue(chave, out Sprite emCache) && emCache != null)
            return emCache;

        const int largura = 64;
        const int altura = 3;
        Texture2D textura = NovaTextura(largura, altura, "TemaUI_Linha");
        Color32[] pixels = new Color32[largura * altura];

        for (int x = 0; x < largura; x++)
        {
            float t = (x + 0.5f) / largura;
            float borda = Mathf.Min(t / 0.16f, (1f - t) / 0.16f);
            byte alfa = (byte)(Mathf.Clamp01(borda) * 255f);

            for (int y = 0; y < altura; y++)
                pixels[y * largura + x] = new Color32(255, 255, 255, alfa);
        }

        textura.SetPixels32(pixels);
        textura.Apply(false, true);

        Sprite sprite = CriarSprite(
            textura, Vector4.zero, "TemaUI_Linha");
        cache[chave] = sprite;
        return sprite;
    }

    /// <summary>
    /// Tábuas verticais no tom da mesa do jogo. O grão é curto e quebrado,
    /// e cada tábua tem uma emenda horizontal em altura diferente.
    /// </summary>
    public static Sprite SpriteMadeira()
    {
        string chave = "madeira";

        if (cache.TryGetValue(chave, out Sprite emCache) && emCache != null)
            return emCache;

        const int largura = 640;
        const int altura = 360;
        const int tabua = 80;

        Texture2D textura = NovaTextura(largura, altura, "TemaUI_Madeira");
        Color32[] pixels = new Color32[largura * altura];

        Color32 baseCor = Madeira;
        Color32 veio = MadeiraVeio;
        Color32 veioClaro = MadeiraClara;
        Color32 emenda = new Color32(116, 74, 67, 255);
        Color32 emendaFundo = new Color32(84, 52, 47, 255);

        for (int t = 0; t < largura / tabua; t++)
        {
            int x0 = t * tabua;
            int variacao = Mathf.RoundToInt((Ruido(t, 7, 23) - 0.5f) * 7f);
            int emendaY = 40 + Mathf.RoundToInt(Ruido(t, 11, 41) * (altura - 80));

            for (int x = x0; x < x0 + tabua; x++)
            {
                for (int y = 0; y < altura; y++)
                {
                    Color32 cor = baseCor;
                    cor.r = (byte)Mathf.Clamp(cor.r + variacao, 0, 255);
                    cor.g = (byte)Mathf.Clamp(cor.g + variacao, 0, 255);
                    cor.b = (byte)Mathf.Clamp(cor.b + variacao, 0, 255);

                    // Emenda entre as tábuas.
                    if (x == x0)
                        cor = emendaFundo;
                    else if (x == x0 + 1)
                        cor = emenda;
                    else if (x == x0 + 2)
                        cor = veioClaro;

                    // Emenda horizontal, sempre dentro da tábua.
                    if (y == emendaY && x > x0 + 1)
                        cor = emenda;
                    else if (y == emendaY + 1 && x > x0 + 1)
                        cor = emendaFundo;

                    // Grão: riscos curtos e quebrados, alguns claros.
                    float n = Ruido(x, y / 3, 57 + t);
                    bool risco = n > 0.82f && (x > x0 + 4) && (x < x0 + tabua - 3);

                    if (risco)
                        cor = (Ruido(x, y / 3, 91 + t) > 0.55f) ? veio : veioClaro;

                    pixels[y * largura + x] = cor;
                }
            }
        }

        textura.SetPixels32(pixels);
        textura.Apply(false, true);

        Sprite sprite = CriarSprite(
            textura, Vector4.zero, "TemaUI_Madeira", true);
        cache[chave] = sprite;
        return sprite;
    }

    /// <summary>Seta pixelada para baixo.</summary>
    public static Sprite SpriteSeta()
    {
        string chave = "seta";

        if (cache.TryGetValue(chave, out Sprite emCache) && emCache != null)
            return emCache;

        const int largura = 16;
        const int altura = 12;
        Texture2D textura = NovaTextura(largura, altura, "TemaUI_Seta");
        Color32[] pixels = new Color32[largura * altura];

        for (int y = 0; y < altura; y++)
        {
            float t = (float)y / (altura - 1);
            int meia = Mathf.RoundToInt((1f - t) * (largura * 0.5f - 1f));
            int centro = largura / 2;

            for (int x = centro - meia; x <= centro + meia; x++)
            {
                if (x < 0 || x >= largura) continue;
                pixels[y * largura + x] =
                    new Color32(255, 255, 255, 255);
            }
        }

        textura.SetPixels32(pixels);
        textura.Apply(false, true);

        Sprite sprite = CriarSprite(
            textura, Vector4.zero, "TemaUI_Seta");
        cache[chave] = sprite;
        return sprite;
    }

    /// <summary>Marca de seleção pixelada.</summary>
    public static Sprite SpriteCheck()
    {
        string chave = "check";

        if (cache.TryGetValue(chave, out Sprite emCache) && emCache != null)
            return emCache;

        const int tamanho = 16;
        Texture2D textura = NovaTextura(tamanho, tamanho, "TemaUI_Check");
        Color32[] pixels = new Color32[tamanho * tamanho];

        for (int i = 0; i <= 10; i++)
        {
            float t = i / 10f;
            int x = Mathf.RoundToInt(3f + t * 4f);
            int y = Mathf.RoundToInt(8f - t * 4f);
            Pintar(pixels, tamanho, x, y, 2);
        }

        for (int i = 0; i <= 12; i++)
        {
            float t = i / 12f;
            int x = Mathf.RoundToInt(7f + t * 6f);
            int y = Mathf.RoundToInt(4f + t * 8f);
            Pintar(pixels, tamanho, x, y, 2);
        }

        textura.SetPixels32(pixels);
        textura.Apply(false, true);

        Sprite sprite = CriarSprite(
            textura, Vector4.zero, "TemaUI_Check");
        cache[chave] = sprite;
        return sprite;
    }

    /// <summary>Engrenagem pixelada usada como ícone padrão de máquina.</summary>
    public static Sprite SpriteEngrenagem()
    {
        string chave = "engrenagem";

        if (cache.TryGetValue(chave, out Sprite emCache) && emCache != null)
            return emCache;

        const int tamanho = 32;
        Texture2D textura = NovaTextura(tamanho, tamanho, "TemaUI_Engrenagem");
        Color32[] pixels = new Color32[tamanho * tamanho];
        float centro = tamanho * 0.5f;

        for (int y = 0; y < tamanho; y++)
        {
            for (int x = 0; x < tamanho; x++)
            {
                float dx = x + 0.5f - centro;
                float dy = y + 0.5f - centro;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float angulo = Mathf.Atan2(dy, dx);

                float setor = Mathf.Repeat(
                    angulo + Mathf.PI / 8f, Mathf.PI / 4f) - Mathf.PI / 8f;

                bool dente = Mathf.Abs(setor) < 0.22f && d < 14.5f;
                bool anel = d >= 7.5f && d <= 12f;

                if ((anel || dente) && d >= 4f)
                    pixels[y * tamanho + x] =
                        new Color32(255, 255, 255, 255);
            }
        }

        textura.SetPixels32(pixels);
        textura.Apply(false, true);

        Sprite sprite = CriarSprite(
            textura, Vector4.zero, "TemaUI_Engrenagem");
        cache[chave] = sprite;
        return sprite;
    }

    /// <summary>Quadrado branco simples, para linhas e realces.</summary>
    public static Sprite SpriteQuadrado()
    {
        string chave = "quadrado";

        if (cache.TryGetValue(chave, out Sprite emCache) && emCache != null)
            return emCache;

        Texture2D textura = NovaTextura(4, 4, "TemaUI_Quadrado");
        Color32[] pixels = new Color32[16];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(255, 255, 255, 255);

        textura.SetPixels32(pixels);
        textura.Apply(false, true);

        Sprite sprite = CriarSprite(
            textura, Vector4.zero, "TemaUI_Quadrado");
        cache[chave] = sprite;
        return sprite;
    }

    private static void Pintar(Color32[] pixels, int tamanho, int cx, int cy, int espessura)
    {
        for (int y = cy - espessura + 1; y <= cy + espessura - 1; y++)
        {
            for (int x = cx - espessura + 1; x <= cx + espessura - 1; x++)
            {
                if (x < 0 || y < 0 || x >= tamanho || y >= tamanho) continue;
                pixels[y * tamanho + x] = new Color32(255, 255, 255, 255);
            }
        }
    }

    // --- Aplicação ------------------------------------------------------

    public static Image AplicarPlaca(Image imagem, Sprite sprite, Color? cor = null)
    {
        if (imagem == null)
            return null;

        imagem.sprite = sprite;
        imagem.type = Image.Type.Sliced;
        imagem.pixelsPerUnitMultiplier = 1f;
        imagem.color = cor ?? Color.white;

        return imagem;
    }

    public static Image AplicarSprite(Image imagem, Sprite sprite, Color? cor = null)
    {
        if (imagem == null)
            return null;

        imagem.sprite = sprite;
        imagem.type = Image.Type.Simple;
        imagem.color = cor ?? Color.white;

        return imagem;
    }

    // --- Estilos --------------------------------------------------------

    public enum EstiloBotao
    {
        Primario,
        Secundario,
        Perigo,
        Verde,
        Fantasma
    }

    /// <summary>Sombra dura, deslocada. Nunca difusa.</summary>
    public static void Sombra(Graphic graphic, float distancia = 4f, float alpha = 0.55f)
    {
        if (graphic == null)
            return;

        Shadow sombra = graphic.GetComponent<Shadow>();
        if (sombra == null)
            sombra = graphic.gameObject.AddComponent<Shadow>();

        sombra.effectColor = new Color(0f, 0f, 0f, alpha);
        sombra.effectDistance = new Vector2(0f, -distancia);
        sombra.useGraphicAlpha = true;
    }

    public static void EstilizarTexto(
        TMP_Text texto,
        float tamanho,
        Color cor,
        FontStyles estilo = FontStyles.Normal,
        TextAlignmentOptions? alinhamento = null,
        float espacamento = 0f)
    {
        if (texto == null)
            return;

        texto.fontSize = tamanho;
        texto.color = cor;
        texto.fontStyle = estilo;

        if (espacamento > 0f)
            texto.characterSpacing = espacamento;

        if (alinhamento.HasValue)
            texto.alignment = alinhamento.Value;
    }

    public static void EstilizarBotao(
        Button botao,
        EstiloBotao estilo = EstiloBotao.Secundario,
        float tamanhoFonte = 24f)
    {
        if (botao == null)
            return;

        Image fundo = botao.targetGraphic as Image;
        if (fundo == null)
            fundo = botao.GetComponent<Image>();

        Color corTexto;

        switch (estilo)
        {
            case EstiloBotao.Perigo:
                corTexto = new Color32(255, 220, 214, 255);
                break;
            case EstiloBotao.Fantasma:
                corTexto = TextoSuave;
                break;
            default:
                corTexto = TextoForte;
                break;
        }

        AplicarPlaca(fundo, SpriteBotao(estilo), Color.white);
        Sombra(fundo, 4f, 0.55f);

        ColorBlock cores = botao.colors;
        cores.normalColor = Color.white;
        cores.highlightedColor = new Color(1.10f, 1.10f, 1.08f, 1f);
        cores.pressedColor = new Color(0.78f, 0.78f, 0.80f, 1f);
        cores.selectedColor = Color.white;
        cores.disabledColor = new Color(0.52f, 0.52f, 0.56f, 0.55f);
        cores.colorMultiplier = 1f;
        cores.fadeDuration = 0.04f;
        botao.colors = cores;

        // Botões sem aro dourado: fica só a placa, como os outros painéis.
        Transform aro = botao.transform.Find("__Aro");

        if (aro != null)
            aro.gameObject.SetActive(false);

        TextMeshProUGUI rotulo = botao.GetComponentInChildren<TextMeshProUGUI>(true);
        if (rotulo != null)
        {
            EstilizarTexto(
                rotulo,
                tamanhoFonte,
                corTexto,
                FontStyles.Normal,
                TextAlignmentOptions.Center,
                estilo == EstiloBotao.Fantasma ? 1f : 0f
            );

            rotulo.textWrappingMode = TextWrappingModes.NoWrap;
            rotulo.enableAutoSizing = false;

            if (estilo != EstiloBotao.Fantasma)
                Sombra(rotulo, 2f, 0.70f);
        }
    }

    public static void EstilizarSlider(Slider slider)
    {
        if (slider == null)
            return;

        // O Slider não tem Graphic próprio; sem isso só dá para acertar a
        // barrinha fina. Uma área invisível na linha inteira facilita o arrasto.
        if (slider.GetComponent<Image>() == null)
        {
            Image area = slider.gameObject.AddComponent<Image>();
            area.color = new Color(1f, 1f, 1f, 0f);
            area.raycastTarget = true;
        }

        Transform fundoTransform = slider.transform.Find("Background");

        if (fundoTransform != null)
        {
            Image fundo = fundoTransform.GetComponent<Image>();
            if (fundo != null)
            {
                AplicarPlaca(fundo, SpriteSliderFundo(), Color.white);

                RectTransform rt = fundo.rectTransform;
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(-20f, 10f);
            }
        }

        if (slider.fillRect != null)
        {
            Image preenchimento = slider.fillRect.GetComponent<Image>();
            if (preenchimento != null)
                AplicarPlaca(preenchimento, SpriteSliderPreenchimento(), Color.white);

            RectTransform area = slider.fillRect.parent as RectTransform;
            if (area != null)
            {
                area.anchorMin = new Vector2(0f, 0.5f);
                area.anchorMax = new Vector2(1f, 0.5f);
                area.pivot = new Vector2(0.5f, 0.5f);
                area.anchoredPosition = Vector2.zero;
                area.sizeDelta = new Vector2(-20f, 10f);
            }
        }

        if (slider.handleRect != null)
        {
            Image alca = slider.handleRect.GetComponent<Image>();
            if (alca != null)
                AplicarPlaca(alca, SpriteAlca(), Color.white);

            RectTransform rt = slider.handleRect;
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(14f, 22f);

            Sombra(alca, 2f, 0.55f);
        }

        Transform slideArea = slider.transform.Find("Handle Slide Area");
        if (slideArea != null)
        {
            RectTransform rt = slideArea as RectTransform;
            if (rt != null)
            {
                rt.offsetMin = new Vector2(7f, 0f);
                rt.offsetMax = new Vector2(-7f, 0f);
            }
        }

        LayoutElement layout = slider.GetComponent<LayoutElement>();
        if (layout == null)
            layout = slider.gameObject.AddComponent<LayoutElement>();

        layout.minHeight = 28f;
        layout.preferredHeight = 30f;
        layout.flexibleHeight = 0f;
    }

    public static void EstilizarDropdown(TMP_Dropdown dropdown)
    {
        if (dropdown == null)
            return;

        Image fundo = dropdown.GetComponent<Image>();
        if (fundo != null)
        {
            AplicarPlaca(fundo, SpriteBotao(EstiloBotao.Secundario), Color.white);
            Sombra(fundo, 4f, 0.50f);
        }

        if (dropdown.captionText != null)
        {
            EstilizarTexto(
                dropdown.captionText,
                21f,
                Texto,
                FontStyles.Normal,
                TextAlignmentOptions.Left
            );

            dropdown.captionText.margin = new Vector4(18f, 0f, 46f, 0f);
            dropdown.captionText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        if (dropdown.template != null)
        {
            Image templateFundo = dropdown.template.GetComponent<Image>();
            if (templateFundo != null)
                AplicarPlaca(templateFundo, SpriteRebaixado(), Color.white);

            if (templateFundo != null)
                Sombra(templateFundo, 6f, 0.60f);

            RectTransform templateRT = dropdown.template;
            templateRT.sizeDelta = new Vector2(templateRT.sizeDelta.x, 208f);

            Transform seta = dropdown.transform.Find("Arrow");
            if (seta != null)
            {
                Image imagemSeta = seta.GetComponent<Image>();
                if (imagemSeta != null)
                {
                    AplicarSprite(imagemSeta, SpriteSeta(), Acento);
                    imagemSeta.preserveAspect = true;
                    imagemSeta.rectTransform.sizeDelta = new Vector2(14f, 10f);
                    imagemSeta.rectTransform.anchoredPosition = new Vector2(-22f, 0f);
                }
            }

            if (dropdown.itemText != null)
            {
                EstilizarTexto(
                    dropdown.itemText,
                    20f,
                    Texto,
                    FontStyles.Normal,
                    TextAlignmentOptions.Left
                );

                dropdown.itemText.margin = new Vector4(10f, 0f, 10f, 0f);

                Transform item = dropdown.itemText.transform.parent;
                if (item != null)
                {
                    Transform fundoItem = item.Find("Item Background");
                    Image itemFundoImg = fundoItem != null
                        ? fundoItem.GetComponent<Image>()
                        : item.GetComponent<Image>();

                    if (itemFundoImg != null)
                    {
                        AplicarPlaca(itemFundoImg, SpriteLinhaItem(), Color.white);
                        itemFundoImg.raycastTarget = true;
                    }

                    RectTransform itemRT = item as RectTransform;
                    if (itemRT != null)
                        itemRT.sizeDelta = new Vector2(itemRT.sizeDelta.x, 38f);

                    Transform check = item.Find("Item Checkmark");
                    if (check != null)
                    {
                        Image checkImg = check.GetComponent<Image>();
                        if (checkImg != null)
                        {
                            AplicarSprite(checkImg, SpriteCheck(), Acento);
                            checkImg.rectTransform.sizeDelta = new Vector2(14f, 14f);
                        }
                    }

                    Button itemBotao = item.GetComponent<Button>();
                    if (itemBotao != null)
                    {
                        ColorBlock cores = itemBotao.colors;
                        cores.normalColor = Color.white;
                        cores.highlightedColor = new Color(1.14f, 1.14f, 1.10f, 1f);
                        cores.pressedColor = new Color(0.78f, 0.78f, 0.80f, 1f);
                        cores.selectedColor = Color.white;
                        itemBotao.colors = cores;
                    }
                }
            }

            foreach (Scrollbar scrollbar in
                dropdown.template.GetComponentsInChildren<Scrollbar>(true))
            {
                EstilizarScrollbar(scrollbar);
            }
        }
    }

    public static void EstilizarToggle(Toggle toggle)
    {
        if (toggle == null)
            return;

        if (toggle.targetGraphic != null)
        {
            Image fundo = toggle.targetGraphic as Image;
            if (fundo != null)
            {
                AplicarPlaca(fundo, SpriteRebaixado(), Color.white);

                RectTransform rt = fundo.rectTransform;
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.anchoredPosition = new Vector2(4f, 0f);
                rt.sizeDelta = new Vector2(26f, 26f);
            }
        }

        if (toggle.graphic != null)
        {
            Image marca = toggle.graphic as Image;
            if (marca != null)
            {
                AplicarSprite(marca, SpriteCheck(), Acento);

                RectTransform rt = marca.rectTransform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(18f, 18f);
            }
        }

        // A caixinha é pequena; uma área invisível no item inteiro deixa o
        // clique em qualquer ponto da linha marcar/desmarcar.
        if (toggle.GetComponent<Image>() == null)
        {
            Image area = toggle.gameObject.AddComponent<Image>();
            area.color = new Color(1f, 1f, 1f, 0f);
            area.raycastTarget = true;
        }

        // O rótulo antigo é um Text legado. Ele fica invisível e um TMP no
        // mesmo lugar assume, para combinar com o resto da interface.
        Text legado = toggle.GetComponentInChildren<Text>(true);
        string rotulo = "Tela cheia";

        if (legado != null)
        {
            if (!string.IsNullOrEmpty(legado.text))
                rotulo = legado.text;

            legado.text = "";
            legado.color = new Color(0f, 0f, 0f, 0f);
        }

        if (rotulo == "Fullscreen")
            rotulo = "Tela cheia";

        Transform rotuloExistente = toggle.transform.Find("RotuloTMP");
        TextMeshProUGUI tmp;

        if (rotuloExistente != null)
        {
            tmp = rotuloExistente.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            tmp = UIUtil.CriarTexto(
                toggle.transform,
                "RotuloTMP",
                rotulo,
                22f,
                new Vector2(300f, 30f),
                Vector2.zero,
                TextAlignmentOptions.Left
            );

            RectTransform rt = tmp.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(40f, 0f);
            rt.offsetMax = new Vector2(-8f, 0f);
        }

        tmp.text = rotulo;
        EstilizarTexto(tmp, 22f, Texto, FontStyles.Normal, TextAlignmentOptions.Left);
    }

    public static void EstilizarScrollbar(Scrollbar scrollbar)
    {
        if (scrollbar == null)
            return;

        Image trilho = scrollbar.GetComponent<Image>();
        if (trilho != null)
        {
            trilho.sprite = null;
            trilho.color = new Color(0.16f, 0.15f, 0.19f, 0.65f);
        }

        if (scrollbar.handleRect != null)
        {
            Image alca = scrollbar.handleRect.GetComponent<Image>();
            if (alca != null)
            {
                AplicarPlaca(alca, SpritePlacaNeutra(),
                    TextoFraco);

                RectTransform rt = alca.rectTransform;
                if (scrollbar.direction == Scrollbar.Direction.LeftToRight ||
                    scrollbar.direction == Scrollbar.Direction.RightToLeft)
                {
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, 8f);
                }
                else
                {
                    rt.sizeDelta = new Vector2(8f, rt.sizeDelta.y);
                }
            }
        }
    }

    public static void AjustarAltura(Component componente, float altura)
    {
        if (componente == null)
            return;

        LayoutElement layout = componente.GetComponent<LayoutElement>();
        if (layout == null)
            layout = componente.gameObject.AddComponent<LayoutElement>();

        layout.minHeight = altura;
        layout.preferredHeight = altura;
        layout.flexibleHeight = 0f;
        layout.minWidth = 0f;
    }

    public static void AjustarLargura(
        Component componente,
        float largura,
        float flexivel = 0f)
    {
        if (componente == null)
            return;

        LayoutElement layout = componente.GetComponent<LayoutElement>();
        if (layout == null)
            layout = componente.gameObject.AddComponent<LayoutElement>();

        layout.minWidth = largura;
        layout.preferredWidth = largura;
        layout.flexibleWidth = flexivel;
    }

    /// <summary>Configura a linha horizontal padrão dos itens de loja.</summary>
    public static HorizontalLayoutGroup PrepararLinha(Transform raiz, float altura)
    {
        HorizontalLayoutGroup layout = raiz.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
            layout = raiz.gameObject.AddComponent<HorizontalLayoutGroup>();

        layout.padding = new RectOffset(12, 14, 10, 10);
        layout.spacing = 14f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.reverseArrangement = false;

        LayoutElement elemento = raiz.GetComponent<LayoutElement>();
        if (elemento == null)
            elemento = raiz.gameObject.AddComponent<LayoutElement>();

        elemento.minHeight = altura;
        elemento.preferredHeight = altura;
        elemento.flexibleHeight = 0f;

        RectTransform rt = raiz as RectTransform;
        if (rt != null)
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, altura);

        return layout;
    }

    /// <summary>Cria a coluna empilhada de nome/descrição de um item.</summary>
    public static RectTransform ColunaTextos(Transform linha, float espacamento = 3f)
    {
        Transform existente = linha.Find("__Textos");
        if (existente != null)
            return existente as RectTransform;

        RectTransform rt = UIUtil.CriarRetangulo(linha, "__Textos");

        VerticalLayoutGroup layout = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = espacamento;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        LayoutElement elemento = rt.gameObject.AddComponent<LayoutElement>();
        elemento.minWidth = 160f;
        elemento.flexibleWidth = 1f;

        return rt;
    }
}
