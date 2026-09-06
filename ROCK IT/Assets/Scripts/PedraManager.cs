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

    private PedraDados dados;
    private int vidaAtual;
    private bool estaAtiva = false;
    private bool tremendo = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Inicializar(PedraDados d)
    {
        dados = d;
        vidaAtual = d.vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = d.cor;
        AtualizarTextoVida();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
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
        spriteRenderer.color = d.cor;
        AtualizarTextoVida();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direcao = Random.insideUnitCircle.normalized;
            rb.AddForce(direcao * 2f, ForceMode2D.Impulse);
        }
    }

    void OnMouseDown()
    {
        if (!estaAtiva)
        {
            CentralizarEAumentar();
        }
        else
        {
            if (!tremendo)
                StartCoroutine(AplicarDano());
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
        vidaAtual--;
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
        float recompensa = Random.Range(dados.recompensaMin, dados.recompensaMax + 1);
        GameManager.Instance.AdicionarDinheiro(recompensa);
        Debug.Log("Recompensa: R$ " + recompensa);
        Destroy(gameObject);
    }

    public string GetNomePedra()
    {
        return dados != null ? dados.nome : "";
    }

    public int GetVidaAtual()
    {
        return vidaAtual;
    }
}