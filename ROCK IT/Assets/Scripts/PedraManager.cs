using System.Collections;
using UnityEngine;
using TMPro;

public class PedraManager : MonoBehaviour
{
    [Header("Referências")]
    public TextMeshPro vidaText;

    [Header("Configurações")]
    public float escalaAumentada = 2.5f;
    public float duracaoTremor = 0.3f;
    public float intensidadeTremor = 0.1f;
    public float tempoParaArrastar = 0.2f; // segundos segurando para virar arrasto

    private PedraDados dados;
    private int vidaAtual;
    private bool estaAtiva = false;
    private bool tremendo = false;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // Arrastar
    private bool segurando = false;
    private bool arrastando = false;
    private float tempoSegurando = 0f;
    private Vector3 offset;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Inicializar(PedraDados d)
    {
        dados = d;
        vidaAtual = d.vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer.color = d.cor;
        AtualizarTextoVida();

        if (rb != null)
        {
            Vector2 direcao = Random.insideUnitCircle.normalized;
            rb.AddForce(direcao * 2f, ForceMode2D.Impulse);
        }
    }

    public void InicializarComVida(PedraDados d, int vida)
    {
        dados = d;
        vidaAtual = vida;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer.color = d.cor;
        AtualizarTextoVida();
    }

    void OnMouseDown()
    {
        segurando = true;
        tempoSegurando = 0f;
        arrastando = false;

        // Calcula offset entre mouse e pedra
        Vector3 posicaoMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        posicaoMouse.z = 0f;
        offset = transform.position - posicaoMouse;
    }

    void OnMouseUp()
    {
        if (arrastando)
        {
            // Solta a pedra — volta física normal
            rb.isKinematic = false;
            arrastando = false;
            segurando = false;
            return;
        }

        if (segurando && !arrastando)
        {
            // Foi um clique rápido — aplica dano
            segurando = false;
            if (!estaAtiva)
                CentralizarEAumentar();
            else if (!tremendo)
                StartCoroutine(AplicarDano());
        }
    }

    void Update()
    {
        if (!segurando) return;

        tempoSegurando += Time.deltaTime;

        // Passou do tempo limite — vira arrasto
        if (!arrastando && tempoSegurando >= tempoParaArrastar)
        {
            arrastando = true;

            // Se estava centralizada, volta para escala normal
            if (estaAtiva)
            {
                estaAtiva = false;
                transform.localScale = Vector3.one;
            }

            // Desativa física durante o arrasto
            rb.isKinematic = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Move a pedra com o mouse durante arrasto
        if (arrastando)
        {
            Vector3 posicaoMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            posicaoMouse.z = 0f;
            transform.position = posicaoMouse + offset;
        }
    }

    void CentralizarEAumentar()
    {
        estaAtiva = true;
        transform.position = Camera.main.ViewportToWorldPoint(
            new Vector3(0.5f, 0.5f, 10f)
        );
        transform.localScale = Vector3.one * escalaAumentada;
    }

    IEnumerator AplicarDano()
    {
        tremendo = true;

        int dano = Mathf.Max(1, Mathf.RoundToInt(StatusManager.Instance.GetDanoForca()));
        vidaAtual -= dano;
        AtualizarTextoVida();

        Vector3 pos = transform.position;
        float tempo = 0f;

        while (tempo < duracaoTremor)
        {
            float ox = Random.Range(-intensidadeTremor, intensidadeTremor);
            float oy = Random.Range(-intensidadeTremor, intensidadeTremor);
            transform.position = pos + new Vector3(ox, oy, 0);
            tempo += Time.deltaTime;
            yield return null;
        }

        transform.position = pos;
        tremendo = false;

        if (vidaAtual <= 0)
            PedraDestruida();
    }

    void AtualizarTextoVida()
    {
        if (vidaText != null)
            vidaText.text = "Vida: " + vidaAtual;
    }

    void PedraDestruida()
    {
        float recompensaBase = Random.Range(dados.recompensaMin, dados.recompensaMax + 1);
        float sorte = StatusManager.Instance.GetMultiplicadorSorte();
        float fortuna = StatusManager.Instance.GetMultiplicadorFortuna();
        float recompensaFinal = recompensaBase * sorte * fortuna;

        GameManager.Instance.AdicionarDinheiro(recompensaFinal);
        Debug.Log("Recompensa: R$ " + recompensaFinal.ToString("F2"));
        Destroy(gameObject);
    }

    public string GetNomePedra() => dados != null ? dados.nome : "";
    public int GetVidaAtual() => vidaAtual;
}