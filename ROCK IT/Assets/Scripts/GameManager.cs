using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float dinheiro = 0f;
    public TextMeshProUGUI textoDinheiro;
    public TextMeshProUGUI textoDiferenca; // texto do +X ou -X ao lado

    [Header("Animação do contador")]
    [Tooltip("Duração da animação para valores pequenos (em segundos).")]
    public float duracaoMinima = 0.25f;

    [Tooltip("Duração máxima da animação, mesmo para valores muito altos (em segundos).")]
    public float duracaoMaxima = 1.2f;

    private float dinheiroExibido = 0f;
    private float alvoAnimacao = 0f;
    private float inicioAnimacao = 0f;
    private float tempoAnimacao = 0f;
    private float duracaoAnimacao = 0f;
    private Coroutine corotinaDiferenca;

    void Awake()
    {
        Instance = this;
        TutorialBootstrap.Instalar();
    }

    void Start()
    {
        AtualizarHUD();
    }

    void Update()
    {
        // O valor mudou (recompensa, compra, carregamento)? Recomeça a
        // animação a partir do número que está na tela.
        if (dinheiro != alvoAnimacao)
        {
            alvoAnimacao = dinheiro;
            inicioAnimacao = dinheiroExibido;
            tempoAnimacao = 0f;

            float distancia = Mathf.Abs(alvoAnimacao - inicioAnimacao);

            // Sem distância não há o que animar.
            if (distancia <= 0.001f)
            {
                dinheiroExibido = alvoAnimacao;
                AtualizarHUD();
            }
            else
            {
                duracaoAnimacao = CalcularDuracao(distancia);
            }
        }

        if (dinheiroExibido == alvoAnimacao)
            return;

        tempoAnimacao += Time.deltaTime;

        float t = duracaoAnimacao > 0f
            ? Mathf.Clamp01(tempoAnimacao / duracaoAnimacao)
            : 1f;

        // Desacelera perto do final, mas termina exatamente no alvo.
        float suave = 1f - Mathf.Pow(1f - t, 3f);
        dinheiroExibido = Mathf.Lerp(inicioAnimacao, alvoAnimacao, suave);

        if (t >= 1f)
            dinheiroExibido = alvoAnimacao;

        AtualizarHUD();
    }

    /// <summary>
    /// Valores pequenos terminam rápido e valores altos levam um pouco mais,
    /// mas sempre têm um fim (nada de aproximação infinita).
    /// </summary>
    float CalcularDuracao(float distancia)
    {
        float duracao = 0.25f + 0.35f * Mathf.Log10(1f + distancia);

        // Garante limites válidos mesmo se os campos do Inspector forem
        // configurados ao contrário.
        float minimo = Mathf.Max(0.01f, Mathf.Min(duracaoMinima, duracaoMaxima));
        float maximo = Mathf.Max(minimo, duracaoMaxima);

        return Mathf.Clamp(duracao, minimo, maximo);
    }

    public void AdicionarDinheiro(float valor)
    {
        // Recompensa negativa não pode deixar o saldo negativo: se isso
        // acontecesse o jogador não conseguiria comprar nem a Pedra Comum.
        dinheiro = Mathf.Max(0f, dinheiro + valor);
        MostrarDiferenca(valor);
    }

    public bool GastarDinheiro(float valor)
    {
        if (dinheiro < valor) return false;
        dinheiro -= valor;
        MostrarDiferenca(-valor);
        return true;
    }

    public void CarregarDinheiro(float valor)
    {
        dinheiro = valor;
        dinheiroExibido = valor;
        alvoAnimacao = valor;
        tempoAnimacao = 0f;
        AtualizarHUD();
    }

    void AtualizarHUD()
    {
        if (textoDinheiro != null)
            textoDinheiro.text = "R$ " + dinheiroExibido.ToString("F2");
    }

    void MostrarDiferenca(float valor)
    {
        if (textoDiferenca == null) return;

        if (corotinaDiferenca != null)
            StopCoroutine(corotinaDiferenca);

        corotinaDiferenca = StartCoroutine(AnimarDiferenca(valor));
    }

    IEnumerator AnimarDiferenca(float valor)
    {
        textoDiferenca.gameObject.SetActive(true);

        if (valor >= 0)
        {
            textoDiferenca.text = "+" + valor.ToString("F2");
            textoDiferenca.color = new Color(0.2f, 0.9f, 0.2f, 1f);
        }
        else
        {
            textoDiferenca.text = valor.ToString("F2");
            textoDiferenca.color = new Color(0.9f, 0.2f, 0.2f, 1f);
        }

        // Aparece por 1.5 segundos depois some gradualmente
        yield return new WaitForSeconds(1.5f);

        float tempo = 0f;
        float duracao = 0.5f;
        Color corInicial = textoDiferenca.color;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, tempo / duracao);
            textoDiferenca.color = new Color(corInicial.r, corInicial.g, corInicial.b, alpha);
            yield return null;
        }

        textoDiferenca.gameObject.SetActive(false);
    }
}