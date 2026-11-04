using UnityEngine;
using UnityEngine.UI;

public class NavegacaoLoja : MonoBehaviour
{
    /// <summary>Disparado quando uma aba é aberta (0 pedras, 1 status, 2 habilidades, 3 máquinas).</summary>
    public static event System.Action<int> OnPainelTrocado;

    public GameObject painelPedras;
    public GameObject painelStatus;
    public GameObject painelHabilidades;
    public GameObject painelMaquinas;

    public Button botaoPedras;
    public Button botaoStatus;
    public Button botaoHabilidades;
    public Button botaoMaquinas;

    public int PainelAtual { get; private set; }

    void Awake()
    {
        ResolverReferencias();
    }

    void Start()
    {
        ResolverReferencias();
        OrganizarAbas();

        ConfigurarBotao(botaoPedras, 0);
        ConfigurarBotao(botaoStatus, 1);
        ConfigurarBotao(botaoHabilidades, 2);
        ConfigurarBotao(botaoMaquinas, 3);

        MostrarPainel(0);
    }

    /// <summary>
    /// A cena original não tinha os botões nem os painéis todos ligados ao
    /// script (e as 4 abas estavam empilhadas na mesma posição). Em vez de
    /// mexer nas cenas, procuramos os objetos pelo nome.
    /// </summary>
    void ResolverReferencias()
    {
        Transform barra = transform.Find("BarraNavegacao");
        Transform area = transform.Find("AreaConteudo");

        if (painelPedras == null)
            painelPedras = Buscar(area, "PainelPedras");

        if (painelStatus == null)
            painelStatus = Buscar(area, "PainelStatus");

        if (painelHabilidades == null)
            painelHabilidades = Buscar(area, "PainelHabilidades");

        if (painelMaquinas == null)
            painelMaquinas = Buscar(area, "PainelMaquinas");

        if (botaoPedras == null)
            botaoPedras = BuscarBotao(barra, "BotaoPedras");

        if (botaoStatus == null)
            botaoStatus = BuscarBotao(barra, "BotaoStatus");

        if (botaoHabilidades == null)
            botaoHabilidades = BuscarBotao(barra, "BotaoHabilidades");

        if (botaoMaquinas == null)
            botaoMaquinas = BuscarBotao(barra, "BotaoMaquinas");
    }

    static GameObject Buscar(Transform pai, string nome)
    {
        if (pai == null) return null;

        Transform filho = pai.Find(nome);
        return filho != null ? filho.gameObject : null;
    }

    static Button BuscarBotao(Transform pai, string nome)
    {
        if (pai == null) return null;

        Transform filho = pai.Find(nome);
        return filho != null ? filho.GetComponent<Button>() : null;
    }

    /// <summary>Separa as abas, que vinham todas na mesma posição na cena.</summary>
    void OrganizarAbas()
    {
        Button[] abas = { botaoPedras, botaoStatus, botaoHabilidades, botaoMaquinas };
        float espacamento = 190f;
        float inicio = -espacamento * (abas.Length - 1) / 2f;

        for (int i = 0; i < abas.Length; i++)
        {
            if (abas[i] == null) continue;

            RectTransform rt = abas[i].transform as RectTransform;
            if (rt == null) continue;

            rt.anchoredPosition = new Vector2(inicio + espacamento * i, 0f);
            rt.sizeDelta = new Vector2(176f, 36f);
        }
    }

    void ConfigurarBotao(Button botao, int indice)
    {
        if (botao == null) return;

        botao.onClick.RemoveAllListeners();
        botao.onClick.AddListener(() => MostrarPainel(indice));
    }

    public void MostrarPainel(int indice)
    {
        if (indice < 0 || indice > 3) return;

        PainelAtual = indice;

        if (painelPedras != null) painelPedras.SetActive(indice == 0);
        if (painelStatus != null) painelStatus.SetActive(indice == 1);
        if (painelHabilidades != null) painelHabilidades.SetActive(indice == 2);
        if (painelMaquinas != null) painelMaquinas.SetActive(indice == 3);

        if (OnPainelTrocado != null)
            OnPainelTrocado(indice);
    }

    /// <summary>Aceita "pedras", "status", "habilidades" ou "maquinas".</summary>
    public void MostrarPainel(string nome)
    {
        switch ((nome ?? "").ToLowerInvariant())
        {
            case "pedras": MostrarPainel(0); break;
            case "status": MostrarPainel(1); break;
            case "habilidades": MostrarPainel(2); break;
            case "maquinas": MostrarPainel(3); break;
        }
    }
}
