using UnityEngine;
using TMPro;

/// <summary>
/// Aviso de "Salvando..." no canto inferior direito. É montado uma vez e só
/// ligado/desligado a cada save. O texto não captura clique: o jogador
/// continua livre para voltar ao jogo enquanto o aviso está na tela.
/// </summary>
public class SalvandoUI : MonoBehaviour
{
    // Acima do tutorial (300) e do jackpot (350): o aviso aparece mesmo
    // quando o save é feito pelo menu de pausa.
    private const int Ordem = 400;

    // O arquivo é gravado num frame só; sem um tempo mínimo o aviso piscava
    // rápido demais para ser lido.
    private const float TempoMinimo = 1.5f;

    // Ritmo da animação dos pontinhos.
    private const float IntervaloPontos = 0.3f;

    private static SalvandoUI instancia;

    private TextMeshProUGUI texto;
    private float inicio;
    private float mostrarAte;
    private bool gravando;

    /// <summary>Chamado quando um save começa.</summary>
    public static void Mostrar()
    {
        if (instancia == null)
        {
            GameObject go = new GameObject("SalvandoUI");
            DontDestroyOnLoad(go);
            instancia = go.AddComponent<SalvandoUI>();
        }

        instancia.Exibir();
    }

    /// <summary>Chamado quando a gravação termina (com ou sem erro).</summary>
    public static void Concluido()
    {
        if (instancia == null)
            return;

        instancia.gravando = false;

        // Se a gravação foi rápida, segura o aviso até o tempo mínimo.
        instancia.mostrarAte = Mathf.Max(
            Time.unscaledTime,
            instancia.inicio + TempoMinimo
        );
    }

    private void Exibir()
    {
        if (texto == null)
            CriarInterface();

        if (texto == null)
            return;

        bool estavaOculto = !texto.gameObject.activeSelf;

        gravando = true;
        mostrarAte = 0f;

        if (estavaOculto)
        {
            inicio = Time.unscaledTime;
            texto.gameObject.SetActive(true);
        }

        AtualizarPontos();
    }

    private void CriarInterface()
    {
        GameObject canvasObj = UIUtil.CriarCanvas("SalvandoCanvas", Ordem);

        // O Canvas acompanha a instância persistente.
        canvasObj.transform.SetParent(transform, false);

        texto = UIUtil.CriarTexto(
            canvasObj.transform,
            "Texto",
            "",
            22f,
            new Vector2(320f, 44f),
            Vector2.zero,
            TextAlignmentOptions.BottomRight
        );

        RectTransform rt = texto.rectTransform;
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-28f, 24f);

        TemaUI.EstilizarTexto(
            texto,
            22f,
            TemaUI.TextoForte,
            FontStyles.Bold,
            TextAlignmentOptions.BottomRight,
            0.5f
        );

        // Sombra dura, para o texto continuar legível sobre a madeira clara.
        TemaUI.Sombra(texto, 2f, 0.7f);

        texto.raycastTarget = false;

        // O aviso já nasce escondido; o Canvas fica parado esperando o save.
        texto.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (texto == null || !texto.gameObject.activeSelf)
            return;

        AtualizarPontos();

        if (!gravando && Time.unscaledTime >= mostrarAte)
            texto.gameObject.SetActive(false);
    }

    private void AtualizarPontos()
    {
        int pontos = 1 + Mathf.FloorToInt(
            (Time.unscaledTime - inicio) / IntervaloPontos
        ) % 3;

        string cor = ColorUtility.ToHtmlStringRGB(TemaUI.Acento);

        texto.text = Traducao.T("Salvando") +
            "<color=#" + cor + ">" +
            new string('.', pontos) +
            "</color>";
    }
}
