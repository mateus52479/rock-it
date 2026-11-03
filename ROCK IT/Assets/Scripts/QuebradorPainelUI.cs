using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Telinha pequena ao lado do Quebrador Automático, aberta ao clicar nele.
/// Mostra a conclusão de cada pedra que está sendo quebrada ao mesmo tempo.
/// </summary>
public class QuebradorPainelUI : MonoBehaviour
{
    private const float Largura = 320f;
    private const float Margem = 16f;
    private const float AlturaTitulo = 30f;
    private const float AlturaLinha = 48f;
    private const float EspacoLinha = 6f;
    private const float AlturaBarra = 6f;

    private class Linha
    {
        public PedraManager pedra;
        public TextMeshProUGUI nome;
        public TextMeshProUGUI porcentagem;
        public Image preenchimento;
    }

    private static QuebradorPainelUI instancia;

    private Canvas canvas;
    private RectTransform painel;
    private RectTransform conteudo;

    private MaquinaQuebrador maquina;
    private readonly List<Linha> linhas = new List<Linha>();
    private readonly List<PedraManager> pedrasExibidas = new List<PedraManager>();

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

    public static void Mostrar(MaquinaQuebrador maquina)
    {
        if (maquina == null)
            return;

        if (instancia == null)
        {
            GameObject go = new GameObject("QuebradorPainelUI");
            instancia = go.AddComponent<QuebradorPainelUI>();
        }

        instancia.maquina = maquina;
        instancia.canvas.gameObject.SetActive(true);
        instancia.Redesenhar();
        instancia.Posicionar();
    }

    public static void Esconder(MaquinaQuebrador maquina)
    {
        if (instancia == null)
            return;

        if (maquina != null && instancia.maquina != maquina)
            return;

        instancia.maquina = null;

        if (instancia.canvas != null)
            instancia.canvas.gameObject.SetActive(false);
    }

    void Construir()
    {
        // Atrás dos menus, como a tabela de prêmios: pause e loja continuam
        // cobrindo a telinha quando abrem.
        canvas = UIUtil.CriarCanvas("QuebradorPainelCanvas", -1).GetComponent<Canvas>();

        painel = UIUtil.CriarRetangulo(canvas.transform, "Painel");
        painel.anchorMin = new Vector2(0.5f, 0.5f);
        painel.anchorMax = new Vector2(0.5f, 0.5f);
        painel.pivot = new Vector2(0.5f, 0.5f);

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
        if (maquina == null || canvas == null)
            return;

        bool menuNaFrente =
            (PauseManager.Instance != null && PauseManager.Instance.EstaPausado) ||
            (ConfiguracoesManager.Instance != null &&
                ConfiguracoesManager.Instance.ConfiguracoesAbertas) ||
            LojaManager.LojaAberta;

        if (canvas.enabled == menuNaFrente)
            canvas.enabled = !menuNaFrente;

        if (menuNaFrente)
            return;

        List<PedraManager> pedras = maquina.PedrasProcessando;

        if (ListaMudou(pedras))
            Redesenhar();
        else
            AtualizarValores();

        if (Screen.width != larguraTela || Screen.height != alturaTela)
            Posicionar();
    }

    bool ListaMudou(List<PedraManager> pedras)
    {
        if (pedras == null)
            return pedrasExibidas.Count > 0;

        if (pedras.Count != pedrasExibidas.Count)
            return true;

        for (int i = 0; i < pedras.Count; i++)
            if (pedras[i] != pedrasExibidas[i])
                return true;

        return false;
    }

    void Redesenhar()
    {
        if (conteudo == null)
            return;

        foreach (Transform filho in conteudo)
            Destroy(filho.gameObject);

        linhas.Clear();
        pedrasExibidas.Clear();

        List<PedraManager> pedras = maquina != null
            ? maquina.PedrasProcessando
            : null;

        int total = pedras != null ? pedras.Count : 0;

        if (pedras != null)
            pedrasExibidas.AddRange(pedras);

        float altura = Margem + AlturaTitulo + 8f
            + Mathf.Max(1, total) * AlturaLinha
            + Mathf.Max(0, Mathf.Max(1, total) - 1) * EspacoLinha
            + Margem;

        painel.sizeDelta = new Vector2(Largura, altura);

        float y = -Margem;

        TextMeshProUGUI titulo = UIUtil.CriarTexto(
            conteudo,
            "Titulo",
            "Quebrador Automático",
            21f,
            new Vector2(Largura - 2f * Margem, AlturaTitulo),
            Vector2.zero,
            TextAlignmentOptions.Left
        );

        AncorarNoTopo(titulo.rectTransform, y, AlturaTitulo);

        TemaUI.EstilizarTexto(
            titulo, 21f, TemaUI.TextoForte, FontStyles.Bold,
            TextAlignmentOptions.Left, 1f);

        titulo.textWrappingMode = TextWrappingModes.NoWrap;

        y -= AlturaTitulo + 8f;

        if (total == 0)
        {
            TextMeshProUGUI vazio = UIUtil.CriarTexto(
                conteudo,
                "Vazio",
                "Nenhuma pedra",
                16f,
                new Vector2(Largura - 2f * Margem, AlturaLinha),
                Vector2.zero,
                TextAlignmentOptions.Left
            );

            AncorarNoTopo(vazio.rectTransform, y, AlturaLinha);
            TemaUI.EstilizarTexto(
                vazio, 16f, TemaUI.TextoFraco, FontStyles.Normal,
                TextAlignmentOptions.Left);
            vazio.textWrappingMode = TextWrappingModes.NoWrap;
            return;
        }

        for (int i = 0; i < total; i++)
        {
            PedraManager pedra = pedras[i];
            float alturaFundo = AlturaLinha;

            Image fundoLinha = UIUtil.CriarImagem(
                conteudo,
                "Linha" + i,
                Color.white,
                new Vector2(Largura - 2f * Margem, alturaFundo),
                Vector2.zero
            );

            TemaUI.AplicarPlaca(fundoLinha, TemaUI.SpriteLinhaItem(), Color.white);
            fundoLinha.raycastTarget = false;
            AncorarNoTopo(fundoLinha.rectTransform, y, alturaFundo);

            TextMeshProUGUI nome = UIUtil.CriarTexto(
                fundoLinha.transform,
                "Nome",
                pedra != null ? pedra.GetNomePedra() : "",
                16f,
                new Vector2(180f, 22f),
                Vector2.zero,
                TextAlignmentOptions.Left
            );

            RectTransform nomeRT = nome.rectTransform;
            nomeRT.anchorMin = new Vector2(0f, 1f);
            nomeRT.anchorMax = new Vector2(0f, 1f);
            nomeRT.pivot = new Vector2(0f, 1f);
            nomeRT.anchoredPosition = new Vector2(14f, -6f);
            nomeRT.sizeDelta = new Vector2(180f, 22f);

            TemaUI.EstilizarTexto(
                nome, 16f, TemaUI.TextoForte, FontStyles.Bold,
                TextAlignmentOptions.Left);
            nome.textWrappingMode = TextWrappingModes.NoWrap;

            TextMeshProUGUI porcentagem = UIUtil.CriarTexto(
                fundoLinha.transform,
                "Porcentagem",
                "0%",
                16f,
                new Vector2(90f, 22f),
                Vector2.zero,
                TextAlignmentOptions.Right
            );

            RectTransform porcentagemRT = porcentagem.rectTransform;
            porcentagemRT.anchorMin = new Vector2(1f, 1f);
            porcentagemRT.anchorMax = new Vector2(1f, 1f);
            porcentagemRT.pivot = new Vector2(1f, 1f);
            porcentagemRT.anchoredPosition = new Vector2(-14f, -6f);
            porcentagemRT.sizeDelta = new Vector2(90f, 22f);

            TemaUI.EstilizarTexto(
                porcentagem, 16f, TemaUI.TextoSuave, FontStyles.Bold,
                TextAlignmentOptions.Right);
            porcentagem.textWrappingMode = TextWrappingModes.NoWrap;

            Image fundoBarra = UIUtil.CriarImagem(
                fundoLinha.transform,
                "BarraFundo",
                new Color(0.15f, 0.14f, 0.18f, 1f),
                new Vector2(Largura - 2f * Margem - 28f, AlturaBarra),
                Vector2.zero
            );

            TemaUI.AplicarPlaca(fundoBarra, TemaUI.SpriteRebaixado(), Color.white);
            fundoBarra.raycastTarget = false;

            RectTransform fundoBarraRT = fundoBarra.rectTransform;
            fundoBarraRT.anchorMin = new Vector2(0f, 0f);
            fundoBarraRT.anchorMax = new Vector2(1f, 0f);
            fundoBarraRT.pivot = new Vector2(0.5f, 0f);
            fundoBarraRT.offsetMin = new Vector2(14f, 8f);
            fundoBarraRT.offsetMax = new Vector2(-14f, 8f + AlturaBarra);

            Image preenchimento = UIUtil.CriarImagem(
                fundoBarra.transform,
                "BarraPreenchida",
                TemaUI.Acento,
                Vector2.zero,
                Vector2.zero
            );

            TemaUI.AplicarSprite(preenchimento, TemaUI.SpriteQuadrado(), TemaUI.Acento);
            preenchimento.type = Image.Type.Filled;
            preenchimento.fillMethod = Image.FillMethod.Horizontal;
            preenchimento.fillOrigin = (int)Image.OriginHorizontal.Left;
            preenchimento.fillAmount = 0f;
            preenchimento.raycastTarget = false;
            UIUtil.Esticar(preenchimento.rectTransform);

            Linha linha = new Linha
            {
                pedra = pedra,
                nome = nome,
                porcentagem = porcentagem,
                preenchimento = preenchimento
            };

            linhas.Add(linha);

            AtualizarLinha(linha);

            y -= alturaFundo + EspacoLinha;
        }

        Posicionar();
    }

    void AtualizarValores()
    {
        for (int i = 0; i < linhas.Count; i++)
            AtualizarLinha(linhas[i]);
    }

    void AtualizarLinha(Linha linha)
    {
        if (linha == null || linha.pedra == null)
            return;

        float progresso = linha.pedra.GetPercentualConcluido();

        if (linha.nome != null)
            linha.nome.text = Traducao.T(linha.pedra.GetNomePedra());

        if (linha.porcentagem != null)
        {
            linha.porcentagem.text = Mathf.RoundToInt(progresso * 100f) + "%";
            linha.porcentagem.color = progresso >= 1f
                ? TemaUI.Acento
                : TemaUI.TextoSuave;
        }

        if (linha.preenchimento != null)
            linha.preenchimento.fillAmount = progresso;
    }

    /// <summary>Encosta o painel ao lado do quebrador e mantém tudo na tela.</summary>
    void Posicionar()
    {
        if (painel == null || canvas == null || Camera.main == null || maquina == null)
            return;

        larguraTela = Screen.width;
        alturaTela = Screen.height;

        float escala = canvas.scaleFactor;

        if (escala <= 0f)
        {
            Canvas.ForceUpdateCanvases();
            escala = canvas.scaleFactor;
        }

        if (escala <= 0f)
            escala = 1f;

        Vector3 telaMaquina = Camera.main.WorldToScreenPoint(
            maquina.transform.position);

        float xMaquina = (telaMaquina.x - Screen.width * 0.5f) / escala;
        float yMaquina = (telaMaquina.y - Screen.height * 0.5f) / escala;

        float larguraCanvas = Screen.width / escala;
        float alturaCanvas = Screen.height / escala;

        float deslocamento = 80f;
        float x = xMaquina + deslocamento + Largura * 0.5f;

        float limiteDireita = larguraCanvas * 0.5f - Largura * 0.5f - 24f;
        float limiteEsquerda = -larguraCanvas * 0.5f + Largura * 0.5f + 24f;

        // Perto da borda direita, o painel troca para o lado esquerdo.
        if (limiteEsquerda <= limiteDireita)
        {
            if (x > limiteDireita)
                x = xMaquina - deslocamento - Largura * 0.5f;

            x = Mathf.Clamp(x, limiteEsquerda, limiteDireita);
        }
        else
        {
            x = 0f;
        }

        float metadeAltura = painel.sizeDelta.y * 0.5f;
        float limiteCima = alturaCanvas * 0.5f - metadeAltura - 24f;
        float limiteBaixo = -alturaCanvas * 0.5f + metadeAltura + 24f;

        float y = yMaquina;

        if (limiteBaixo <= limiteCima)
            y = Mathf.Clamp(y, limiteBaixo, limiteCima);
        else
            y = 0f;

        painel.anchoredPosition = new Vector2(x, y);
    }

    static void AncorarNoTopo(RectTransform rt, float y, float altura)
    {
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(Largura - 2f * Margem, altura);
    }
}
