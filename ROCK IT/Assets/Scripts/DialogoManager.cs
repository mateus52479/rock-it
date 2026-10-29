using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Toca um roteiro de diálogo: cria o canvas, o NPC voador, o balão e o
/// destaque, e espera as ações do jogador quando o passo pedir.
/// </summary>
public class DialogoManager : MonoBehaviour
{
    private RectTransform canvasRect;
    private NPCVoador npc;
    private BalaoDialogo balao;
    private DestaqueUI destaque;

    private RoteiroDialogo roteiroAtual;

    private bool comprouPedra;
    private bool quebrouPedra;
    private int painelAtual = -1;

    public static DialogoManager Instalar()
    {
        DialogoManager existente = Object.FindAnyObjectByType<DialogoManager>();

        if (existente != null)
            return existente;

        GameObject go = new GameObject("DialogoManager");
        return go.AddComponent<DialogoManager>();
    }

    void Awake()
    {
        ConstruirUI();
    }

    void OnEnable()
    {
        LojaManager.OnPedraComprada += AoComprarPedra;
        PedraManager.OnPedraDestruida += AoQuebrarPedra;
        NavegacaoLoja.OnPainelTrocado += AoTrocarPainel;
    }

    void OnDisable()
    {
        LojaManager.OnPedraComprada -= AoComprarPedra;
        PedraManager.OnPedraDestruida -= AoQuebrarPedra;
        NavegacaoLoja.OnPainelTrocado -= AoTrocarPainel;
    }

    void ConstruirUI()
    {
        GameObject canvas = UIUtil.CriarCanvas("DialogoCanvas", 300);
        canvasRect = (RectTransform)canvas.transform;

        destaque = DestaqueUI.Criar(canvasRect);
        npc = NPCVoador.Criar(canvasRect);
        balao = BalaoDialogo.Criar(canvasRect, npc);
    }

    public void Executar(RoteiroDialogo roteiro)
    {
        if (roteiro == null || roteiro.passos == null || roteiro.passos.Length == 0)
            return;

        roteiroAtual = roteiro;

        npc.gameObject.SetActive(true);
        npc.PosicionarSemVoar(-760f, -420f);

        StopAllCoroutines();
        StartCoroutine(Rotina());
    }

    IEnumerator Rotina()
    {
        int indice = 0;

        while (indice < roteiroAtual.passos.Length)
        {
            PassoDialogo passo = roteiroAtual.passos[indice];

            comprouPedra = false;
            quebrouPedra = false;

            yield return PrepararCena(passo);

            npc.IrPara(passo.npcX, passo.npcY);
            yield return new WaitForSeconds(0.7f);

            destaque.Destacar(passo.destacar);

            balao.DefinirTamanhoTexto(passo.tamanhoTexto);

            yield return balao.Digitar(
                Traducao.T(passo.personagem),
                Traducao.T(passo.texto));

            if (passo.esperar == "pergunta")
            {
                yield return balao.EscolherResposta(TraduzirRespostas(passo.respostas));

                if (balao.UltimaResposta != 0)
                {
                    if (string.IsNullOrEmpty(passo.pularPara))
                    {
                        Encerrar();
                        yield break;
                    }

                    int destino = System.Array.FindIndex(
                        roteiroAtual.passos,
                        p => p.id == passo.pularPara);

                    indice = destino >= 0 ? destino : roteiroAtual.passos.Length;
                    continue;
                }
            }
            else if (string.IsNullOrEmpty(passo.esperar) || passo.esperar == "clique")
            {
                yield return balao.EsperarContinuar();
            }
            else
            {
                yield return EsperarAcao(passo.esperar);
            }

            balao.Fechar();
            destaque.Limpar();

            indice++;
        }

        Encerrar();
    }

    IEnumerator PrepararCena(PassoDialogo passo)
    {
        if (passo.abrirLoja && LojaManager.Instance != null)
        {
            LojaManager.Instance.AbrirLoja();

            // Espera o Start do NavegacaoLoja rodar antes de trocar a aba.
            yield return null;
            yield return null;
        }

        if (!string.IsNullOrEmpty(passo.painel) &&
            LojaManager.Instance != null &&
            LojaManager.Instance.painelLoja != null)
        {
            NavegacaoLoja nav = LojaManager.Instance.painelLoja
                .GetComponentInChildren<NavegacaoLoja>(true);

            if (nav != null)
                nav.MostrarPainel(passo.painel);
        }

        if (passo.fecharLoja && LojaManager.Instance != null)
            LojaManager.Instance.FecharLoja();
    }

    IEnumerator EsperarAcao(string tipo)
    {
        switch (tipo)
        {
            case "abrirLoja":
                yield return new WaitUntil(() => LojaManager.LojaAberta);
                break;

            case "fecharLoja":
                yield return new WaitUntil(() => !LojaManager.LojaAberta);
                break;

            case "comprarPedra":
                yield return new WaitUntil(() => comprouPedra);
                break;

            case "quebrarPedra":
                yield return new WaitUntil(() => quebrouPedra);
                break;

            case "painelPedras":
                yield return new WaitUntil(() => painelAtual == 0);
                break;

            case "painelStatus":
                yield return new WaitUntil(() => painelAtual == 1);
                break;

            case "painelHabilidades":
                yield return new WaitUntil(() => painelAtual == 2);
                break;

            case "painelMaquinas":
                yield return new WaitUntil(() => painelAtual == 3);
                break;

            default:
                // Tipo desconhecido: não trava o tutorial.
                yield return balao.EsperarContinuar();
                break;
        }
    }

    void AoComprarPedra(PedraDados pedra)
    {
        comprouPedra = true;
    }

    /// <summary>
    /// As respostas ficam em português no roteiro (é a chave do JSON de
    /// tradução); aqui só as converte para o idioma em uso.
    /// </summary>
    static string[] TraduzirRespostas(string[] respostas)
    {
        if (respostas == null)
            return null;

        string[] traduzidas = new string[respostas.Length];

        for (int i = 0; i < respostas.Length; i++)
            traduzidas[i] = Traducao.T(respostas[i]);

        return traduzidas;
    }

    void AoQuebrarPedra(PedraManager pedra)
    {
        quebrouPedra = true;
    }

    void AoTrocarPainel(int indice)
    {
        painelAtual = indice;
    }

    void Encerrar()
    {
        if (balao != null) balao.Fechar();
        if (destaque != null) destaque.Limpar();
        if (npc != null) npc.gameObject.SetActive(false);
    }
}
