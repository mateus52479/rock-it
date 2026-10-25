using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Balão de fala sem retrato. Fica ao lado do NPC e acompanha o voo dele.
/// </summary>
public class BalaoDialogo : MonoBehaviour
{
    public RectTransform painel;
    public RectTransform canvasRect;
    public NPCVoador npc;

    public TextMeshProUGUI textoNome;
    public TextMeshProUGUI texto;

    [Header("Tamanho do texto")]
    [Tooltip("Tamanho padrão da fonte das falas. Mude aqui para aumentar ou diminuir todas de uma vez.")]
    public float tamanhoFonteTexto = 30f;

    public Button botaoContinuar;
    public RectTransform containerRespostas;

    private string textoCompleto = "";
    private bool continuou;
    private bool digitando;
    private int ultimaResposta = -1;

    public int UltimaResposta { get { return ultimaResposta; } }

    public static BalaoDialogo Criar(Transform canvas, NPCVoador npc)
    {
        Image fundo = UIUtil.CriarImagem(
            canvas, "Balao",
            new Color(0.10f, 0.10f, 0.14f, 0.96f),
            new Vector2(480f, 230f),
            Vector2.zero);

        BalaoDialogo balao = fundo.gameObject.AddComponent<BalaoDialogo>();

        balao.painel = fundo.rectTransform;
        balao.canvasRect = (RectTransform)canvas;
        balao.npc = npc;

        balao.textoNome = UIUtil.CriarTexto(
            fundo.transform, "Nome", "",
            22f, new Vector2(440f, 28f), new Vector2(0f, 90f),
            TextAlignmentOptions.Left);

        balao.textoNome.color = new Color(1f, 0.85f, 0.3f);
        balao.textoNome.fontStyle = FontStyles.Bold;

        balao.texto = UIUtil.CriarTexto(
            fundo.transform, "Texto", "",
            balao.tamanhoFonteTexto, new Vector2(440f, 110f), new Vector2(0f, 20f),
            TextAlignmentOptions.TopLeft);

        balao.botaoContinuar = UIUtil.CriarBotao(
            fundo.transform, "Continuar", "Continuar ▶",
            new Vector2(170f, 40f), new Vector2(130f, -85f),
            new Color(0.22f, 0.55f, 0.30f),
            balao.AoClicarContinuar);

        balao.containerRespostas = UIUtil.CriarRetangulo(
            fundo.transform, "Respostas");

        balao.containerRespostas.sizeDelta = new Vector2(440f, 50f);
        balao.containerRespostas.anchoredPosition = new Vector2(0f, -85f);

        balao.painel.gameObject.SetActive(false);

        return balao;
    }

    void Update()
    {
        if (painel == null || !painel.gameObject.activeSelf || npc == null)
            return;

        Vector2 posicaoNPC = npc.PosicaoAtual();

        Rect area = canvasRect.rect;
        float largura = painel.sizeDelta.x;
        float altura = painel.sizeDelta.y;

        float x = posicaoNPC.x + 60f + largura / 2f;

        if (x + largura / 2f > area.xMax - 10f)
            x = posicaoNPC.x - 60f - largura / 2f;

        x = Mathf.Clamp(
            x,
            area.xMin + largura / 2f + 10f,
            area.xMax - largura / 2f - 10f);

        float y = Mathf.Clamp(
            posicaoNPC.y + 30f,
            area.yMin + altura / 2f + 10f,
            area.yMax - altura / 2f - 10f);

        painel.anchoredPosition = new Vector2(x, y);
    }

    /// <summary>
    /// Define o tamanho da fonte desta fala. Passe 0 para voltar ao
    /// tamanho padrão configurado em tamanhoFonteTexto.
    /// </summary>
    public void DefinirTamanhoTexto(float tamanho)
    {
        if (texto == null) return;

        texto.fontSize = tamanho > 0f ? tamanho : tamanhoFonteTexto;
    }

    public IEnumerator Digitar(string personagem, string conteudo)
    {
        painel.gameObject.SetActive(true);

        textoNome.text = personagem ?? "";
        textoCompleto = conteudo ?? "";
        texto.text = "";
        ultimaResposta = -1;
        digitando = true;

        botaoContinuar.gameObject.SetActive(false);
        LimparRespostas();

        for (int i = 0; i < textoCompleto.Length; i++)
        {
            texto.text = textoCompleto.Substring(0, i + 1);
            yield return new WaitForSeconds(0.018f);
        }

        texto.text = textoCompleto;
        digitando = false;
    }

    void AoClicarContinuar()
    {
        if (digitando)
        {
            texto.text = textoCompleto;
            digitando = false;
            return;
        }

        continuou = true;
    }

    public IEnumerator EsperarContinuar()
    {
        continuou = false;
        botaoContinuar.gameObject.SetActive(true);
        botaoContinuar.interactable = true;

        yield return new WaitUntil(() => continuou);

        botaoContinuar.gameObject.SetActive(false);
    }

    public IEnumerator EscolherResposta(string[] respostas)
    {
        ultimaResposta = -1;
        LimparRespostas();

        if (respostas == null || respostas.Length == 0)
        {
            ultimaResposta = 0;
            yield break;
        }

        for (int i = 0; i < respostas.Length; i++)
        {
            int indice = i;
            float x = (i - (respostas.Length - 1) / 2f) * 170f;

            Button botao = UIUtil.CriarBotao(
                containerRespostas,
                "Resposta" + i,
                respostas[i],
                new Vector2(160f, 44f),
                new Vector2(x, 0f),
                new Color(0.24f, 0.42f, 0.62f),
                () => ultimaResposta = indice);

            botao.gameObject.SetActive(true);
        }

        yield return new WaitUntil(() => ultimaResposta >= 0);

        LimparRespostas();
    }

    void LimparRespostas()
    {
        if (containerRespostas == null) return;

        foreach (Transform filho in containerRespostas)
        {
            filho.gameObject.SetActive(false);
            Destroy(filho.gameObject);
        }
    }

    public void Fechar()
    {
        if (painel != null)
            painel.gameObject.SetActive(false);

        LimparRespostas();
    }
}
