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

    private static readonly Color corFundo = new Color(0.06f, 0.06f, 0.09f, 0.88f);
    private static readonly Color corPainel = new Color(0.12f, 0.12f, 0.16f, 0.99f);
    private static readonly Color corLinha = new Color(0.18f, 0.18f, 0.24f, 1f);
    private static readonly Color corBotao = new Color(0.22f, 0.55f, 0.30f, 1f);
    private static readonly Color corBotaoApagar = new Color(0.55f, 0.20f, 0.20f, 1f);

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
            raiz, "Painel", corPainel, new Vector2(1020f, 660f), Vector2.zero);

        UIUtil.CriarTexto(
            painel.transform, "Titulo",
            modo == Modo.NovoJogo ? "Novo jogo" : "Continuar",
            38f, new Vector2(600f, 54f), new Vector2(0f, 280f))
            .fontStyle = FontStyles.Bold;

        UIUtil.CriarTexto(
            painel.transform, "Subtitulo",
            modo == Modo.NovoJogo
                ? "Escolha o slot onde a nova partida vai ser salva."
                : "Escolha o slot que você quer carregar.",
            20f, new Vector2(820f, 34f), new Vector2(0f, 240f))
            .color = new Color(0.75f, 0.75f, 0.80f);

        UIUtil.CriarBotao(
            painel.transform, "BotaoFechar", "X",
            new Vector2(46f, 46f), new Vector2(470f, 292f),
            new Color(0.30f, 0.30f, 0.38f), Fechar);

        GameObject container = new GameObject("Linhas", typeof(RectTransform));
        container.transform.SetParent(painel.transform, false);

        containerLinhas = container.GetComponent<RectTransform>();
        containerLinhas.sizeDelta = new Vector2(940f, 440f);
        containerLinhas.anchoredPosition = new Vector2(0f, -30f);

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

        UIUtil.CriarTexto(
            linha.transform, "TituloSlot", "Slot " + slot,
            28f, new Vector2(240f, 40f), new Vector2(-330f, 28f),
            TextAlignmentOptions.Left)
            .fontStyle = FontStyles.Bold;

        string textoResumo = resumo.existe
            ? NumeroUtil.FormatarDinheiro(resumo.dinheiro)
              + "   ·   Força " + resumo.nivelForca
              + "   ·   " + resumo.data.ToString("dd/MM/yyyy HH:mm")
            : "Vazio";

        TextMeshProUGUI resumoTexto = UIUtil.CriarTexto(
            linha.transform, "Resumo", textoResumo,
            20f, new Vector2(580f, 40f), new Vector2(-160f, -26f),
            TextAlignmentOptions.Left);

        resumoTexto.color = resumo.existe
            ? new Color(0.85f, 0.85f, 0.90f)
            : new Color(0.55f, 0.55f, 0.60f);

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
            new Vector2(170f, 56f), new Vector2(250f, 0f),
            corBotao, () => EscolherSlot(slot));

        principal.interactable = habilitado;

        Button apagar = UIUtil.CriarBotao(
            linha.transform, "BotaoApagar", "Apagar",
            new Vector2(120f, 56f), new Vector2(400f, 0f),
            corBotaoApagar, () => ConfirmarApagar(slot));

        apagar.interactable = resumo.existe;
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
        Transform painel = transform.Find("Painel");

        if (painel == null)
        {
            aoConfirmar();
            return;
        }

        Image fundo = UIUtil.CriarImagem(
            painel, "Confirmacao", new Color(0f, 0f, 0f, 0.82f),
            new Vector2(1020f, 660f), Vector2.zero);

        GameObject confirmacao = fundo.gameObject;

        UIUtil.CriarTexto(
            confirmacao.transform, "Titulo", titulo,
            32f, new Vector2(860f, 50f), new Vector2(0f, 90f))
            .fontStyle = FontStyles.Bold;

        UIUtil.CriarTexto(
            confirmacao.transform, "Detalhe", detalhe,
            22f, new Vector2(820f, 40f), new Vector2(0f, 40f))
            .color = new Color(0.80f, 0.80f, 0.85f);

        UIUtil.CriarBotao(
            confirmacao.transform, "Sim", "Sim",
            new Vector2(190f, 58f), new Vector2(-110f, -60f),
            corBotaoApagar,
            () =>
            {
                Destroy(confirmacao);
                aoConfirmar();
            });

        UIUtil.CriarBotao(
            confirmacao.transform, "Nao", "Não",
            new Vector2(190f, 58f), new Vector2(110f, -60f),
            new Color(0.30f, 0.30f, 0.38f),
            () => Destroy(confirmacao));
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
