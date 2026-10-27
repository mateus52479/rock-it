using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tela de seleção de slots, montada em runtime. Aparece quando o jogador
/// clica em "Novo Jogo" ou "Continuar" no menu principal.
/// </summary>
public class SlotSaveUI : MonoBehaviour
{
    public enum Modo
    {
        NovoJogo,
        Continuar
    }

    private static SlotSaveUI instancia;

    private Modo modo;
    private RectTransform containerLinhas;

    private static readonly Color corFundo = new Color(0f, 0f, 0f, 0.82f);
    private static readonly Color corPainel = Color.white;
    private static readonly Color corLinha = Color.white;
    private static readonly Color corBotao = TemaUI.PainelClaro;

    public static void Abrir(Modo modo)
    {
        if (instancia != null)
            Destroy(instancia.gameObject);

        GameObject canvas = UIUtil.CriarCanvas("SlotSaveUI", 200);
        instancia = canvas.AddComponent<SlotSaveUI>();
        instancia.modo = modo;
        instancia.Construir();
    }

    void OnDestroy()
    {
        if (instancia == this)
            instancia = null;
    }

    void Construir()
    {
        RectTransform raiz = (RectTransform)transform;

        Image fundo = UIUtil.CriarImagem(
            raiz, "Fundo", corFundo, Vector2.zero, Vector2.zero);
        UIUtil.Esticar(fundo.rectTransform);
        fundo.raycastTarget = true;

        Image painel = UIUtil.CriarImagem(
            raiz, "Painel", corPainel, new Vector2(1020f, 680f), Vector2.zero);

        TemaUI.AplicarPlaca(painel, TemaUI.SpritePainel(), Color.white);
        TemaUI.Sombra(painel, 14f, 0.75f);

        TextMeshProUGUI titulo = UIUtil.CriarTexto(
            painel.transform, "Titulo",
            modo == Modo.NovoJogo ? "NOVO JOGO" : "CONTINUAR",
            38f, new Vector2(600f, 52f), new Vector2(0f, 286f));

        TemaUI.EstilizarTexto(
            titulo, 38f, TemaUI.TextoForte, FontStyles.Normal,
            TextAlignmentOptions.Center, 3f);

        TemaUI.Sombra(titulo, 3f, 0.55f);

        TextMeshProUGUI subtitulo = UIUtil.CriarTexto(
            painel.transform, "Subtitulo",
            modo == Modo.NovoJogo
                ? "Escolha o slot onde a nova partida vai ser salva."
                : "Escolha o slot que você quer carregar.",
            19f, new Vector2(820f, 30f), new Vector2(0f, 244f));

        subtitulo.color = TemaUI.TextoSuave;

        Button fechar = UIUtil.CriarBotao(
            painel.transform, "BotaoFechar", "X",
            new Vector2(42f, 42f), Vector2.zero,
            Color.white, Fechar);

        TemaUI.EstilizarBotao(fechar, TemaUI.EstiloBotao.Perigo, 19f);

        RectTransform fecharRT = (RectTransform)fechar.transform;
        fecharRT.anchorMin = new Vector2(1f, 1f);
        fecharRT.anchorMax = new Vector2(1f, 1f);
        fecharRT.pivot = new Vector2(0.5f, 0.5f);
        fecharRT.anchoredPosition = new Vector2(-34f, -32f);

        GameObject container = new GameObject("Linhas", typeof(RectTransform));
        container.transform.SetParent(painel.transform, false);

        containerLinhas = container.GetComponent<RectTransform>();
        containerLinhas.sizeDelta = new Vector2(940f, 440f);
        containerLinhas.anchoredPosition = new Vector2(0f, -34f);

        AtualizarLinhas();
    }

    void AtualizarLinhas()
    {
        if (containerLinhas == null)
            return;

        foreach (Transform filho in containerLinhas)
        {
            filho.gameObject.SetActive(false);
            Destroy(filho.gameObject);
        }

        for (int i = 1; i <= SaveManager.TotalSlots; i++)
            CriarLinha(i);
    }

    void CriarLinha(int slot)
    {
        ResumoSlot resumo = SaveManager.Instance.LerResumo(slot);

        Image linha = UIUtil.CriarImagem(
            containerLinhas, "Linha" + slot, corLinha,
            new Vector2(940f, 130f),
            new Vector2(0f, 150f - (slot - 1) * 145f));

        TemaUI.AplicarPlaca(linha, TemaUI.SpriteLinhaItem(), Color.white);

        TextMeshProUGUI tituloSlot = UIUtil.CriarTexto(
            linha.transform, "TituloSlot", "Slot " + slot,
            26f, new Vector2(240f, 38f), new Vector2(-330f, 28f),
            TextAlignmentOptions.Left);

        TemaUI.EstilizarTexto(
            tituloSlot, 26f, TemaUI.TextoForte, FontStyles.Bold,
            TextAlignmentOptions.Left, 1f);

        string textoResumo = resumo.existe
            ? NumeroUtil.FormatarDinheiro(resumo.dinheiro)
              + "   ·   Força " + resumo.nivelForca
              + "   ·   " + resumo.data.ToString("dd/MM/yyyy HH:mm")
            : "Vazio";

        TextMeshProUGUI resumoTexto = UIUtil.CriarTexto(
            linha.transform, "Resumo", textoResumo,
            18f, new Vector2(580f, 32f), new Vector2(-160f, -28f),
            TextAlignmentOptions.Left);

        resumoTexto.color = resumo.existe
            ? TemaUI.TextoSuave
            : TemaUI.TextoFraco;

        string textoPrincipal;
        bool habilitado;

        if (modo == Modo.Continuar)
        {
            textoPrincipal = "Continuar";
            habilitado = resumo.existe;
        }
        else
        {
            textoPrincipal = resumo.existe ? "Sobrescrever" : "Começar";
            habilitado = true;
        }

        Button principal = UIUtil.CriarBotao(
            linha.transform, "BotaoPrincipal", textoPrincipal,
            new Vector2(170f, 50f), new Vector2(250f, 0f),
            corBotao, () => EscolherSlot(slot));

        principal.interactable = habilitado;
        TemaUI.EstilizarBotao(principal, TemaUI.EstiloBotao.Verde, 18f);

        Button apagar = UIUtil.CriarBotao(
            linha.transform, "BotaoApagar", "Apagar",
            new Vector2(120f, 50f), new Vector2(404f, 0f),
            TemaUI.Vermelho, () => ConfirmarApagar(slot));

        apagar.interactable = resumo.existe;
        TemaUI.EstilizarBotao(apagar, TemaUI.EstiloBotao.Perigo, 16f);
    }

    void EscolherSlot(int slot)
    {
        ResumoSlot resumo = SaveManager.Instance.LerResumo(slot);

        if (modo == Modo.NovoJogo && resumo.existe)
        {
            Confirmar(
                "Sobrescrever o Slot " + slot + "?",
                "O progresso salvo nele será perdido.",
                () => CarregarSlot(slot));
            return;
        }

        CarregarSlot(slot);
    }

    void CarregarSlot(int slot)
    {
        if (modo == Modo.NovoJogo)
        {
            SaveManager.Instance.IniciarNovoJogo(slot);
            GameLoader.deveCarregar = false;
        }
        else
        {
            SaveManager.Instance.IniciarContinuacao(slot);
            GameLoader.deveCarregar = true;
        }

        instancia = null;
        Destroy(gameObject);
        SceneManager.LoadScene("Jogo");
    }

    void ConfirmarApagar(int slot)
    {
        Confirmar(
            "Apagar o Slot " + slot + "?",
            "Não dá para desfazer.",
            () =>
            {
                SaveManager.Instance.ApagarSlot(slot);
                AtualizarLinhas();
            });
    }

    void Confirmar(string titulo, string detalhe, System.Action aoConfirmar)
    {
        Image fundo = UIUtil.CriarImagem(
            transform, "Confirmacao", new Color(0f, 0f, 0f, 0.78f),
            Vector2.zero, Vector2.zero);

        UIUtil.Esticar(fundo.rectTransform);
        fundo.raycastTarget = true;

        GameObject confirmacao = fundo.gameObject;

        Image caixa = UIUtil.CriarImagem(
            fundo.transform, "Caixa", Color.white,
            new Vector2(620f, 280f), Vector2.zero);

        TemaUI.AplicarPlaca(caixa, TemaUI.SpritePainel(), Color.white);
        TemaUI.Sombra(caixa, 10f, 0.70f);

        TextMeshProUGUI tituloConfirmacao = UIUtil.CriarTexto(
            caixa.transform, "Titulo", titulo,
            30f, new Vector2(540f, 46f), new Vector2(0f, 80f));

        TemaUI.EstilizarTexto(
            tituloConfirmacao, 30f, TemaUI.TextoForte, FontStyles.Normal,
            TextAlignmentOptions.Center);

        TextMeshProUGUI detalheConfirmacao = UIUtil.CriarTexto(
            caixa.transform, "Detalhe", detalhe,
            19f, new Vector2(520f, 36f), new Vector2(0f, 26f));

        detalheConfirmacao.color = TemaUI.TextoSuave;

        Button sim = UIUtil.CriarBotao(
            caixa.transform, "Sim", "Sim",
            new Vector2(190f, 54f), new Vector2(-110f, -72f),
            TemaUI.Vermelho,
            () =>
            {
                Destroy(confirmacao);
                aoConfirmar();
            });

        TemaUI.EstilizarBotao(sim, TemaUI.EstiloBotao.Perigo, 20f);

        Button nao = UIUtil.CriarBotao(
            caixa.transform, "Nao", "Não",
            new Vector2(190f, 54f), new Vector2(110f, -72f),
            Color.white,
            () => Destroy(confirmacao));

        TemaUI.EstilizarBotao(nao, TemaUI.EstiloBotao.Secundario, 20f);
    }

    void Fechar()
    {
        MenuPrincipal menu = FindAnyObjectByType<MenuPrincipal>();

        if (menu != null)
            menu.AtualizarBotaoContinuar();

        instancia = null;
        Destroy(gameObject);
    }
}
