using System.Collections;
using UnityEngine;
using TMPro;

public class PedraManager : MonoBehaviour
{
    [Header("Referências")]
    public TextMeshPro vidaText;

    [Header("Texto Flutuante")]
    public GameObject prefabTextoFlutuante;

    [Header("Configurações")]
    public float escalaAumentada = 2.5f;
    public float duracaoTremor = 0.3f;
    public float intensidadeTremor = 0.1f;
    public float tempoParaArrastar = 0.2f;

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
    private Vector3 posicaoUltimoClique;

    private bool interacaoBloqueada = false;
    private bool valorInvertido = false;
    private SpriteRenderer contornoScanner;

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

        MaquinaScanner.NotificarNovaPedra(this);
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
        if (interacaoBloqueada) return;

        segurando = true;
        tempoSegurando = 0f;
        arrastando = false;

        Vector3 posicaoMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        posicaoMouse.z = 0f;
        offset = transform.position - posicaoMouse;
        posicaoUltimoClique = posicaoMouse;
    }

    void OnMouseUp()
    {
        if (arrastando)
        {
            rb.isKinematic = false;
            arrastando = false;
            segurando = false;

            // Verifica se soltou em cima de uma máquina
            Collider2D[] colisoes = Physics2D.OverlapCircleAll(transform.position, 0.5f);
            foreach (Collider2D col in colisoes)
            {
                MaquinaLixo lixo = col.GetComponent<MaquinaLixo>();
                if (lixo != null) { lixo.ReceberPedra(this); return; }

                MaquinaQuebrador quebrador = col.GetComponent<MaquinaQuebrador>();
                if (quebrador != null) { quebrador.ReceberPedra(this); return; }

                MaquinaInversor inversor = col.GetComponent<MaquinaInversor>();
                if (inversor != null) { inversor.ReceberPedra(this); return; }
            }
            return;
        }

        if (segurando && !arrastando)
        {
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

        if (!arrastando && tempoSegurando >= tempoParaArrastar)
        {
            arrastando = true;

            if (estaAtiva)
            {
                estaAtiva = false;
                transform.localScale = Vector3.one;
            }

            rb.isKinematic = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

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
        StartCoroutine(ViajarAoCentro());
    }

    IEnumerator ViajarAoCentro()
    {
        Vector3 destino = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
        Vector3 escalaDestino = Vector3.one * escalaAumentada;
        Vector3 posInicial = transform.position;
        Vector3 escalaInicial = transform.localScale;

        float tempo = 0f;
        float duracao = 0.4f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracao;
            float tSmooth = 1f - Mathf.Pow(1f - t, 3f);

            transform.position = Vector3.Lerp(posInicial, destino, tSmooth);
            transform.localScale = Vector3.Lerp(escalaInicial, escalaDestino, tSmooth);
            yield return null;
        }

        transform.position = destino;
        transform.localScale = escalaDestino;

        StartCoroutine(Bounce());
    }

    IEnumerator Bounce()
    {
        Vector3 escalaBase = Vector3.one * escalaAumentada;
        Vector3 escalaEsticada = escalaBase * 1.2f;

        float tempo = 0f;
        float duracao = 0.15f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracao;
            transform.localScale = Vector3.Lerp(escalaBase, escalaEsticada, t);
            yield return null;
        }

        tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracao;
            transform.localScale = Vector3.Lerp(escalaEsticada, escalaBase, t);
            yield return null;
        }

        transform.localScale = escalaBase;
    }

    IEnumerator AplicarDano()
    {
        tremendo = true;

        int danoBase = Mathf.Max(1, Mathf.RoundToInt(StatusManager.Instance.GetDanoForca()));

        float chanceCritico = StatusManager.Instance.TemHabilidade("whey_protein") ? 0.3f : 0.05f;
        int danoFinal = danoBase;
        bool critico = Random.value < chanceCritico;
        if (critico)
        {
            danoFinal = danoBase * 2;
            Debug.Log("CRÍTICO!");
        }

        vidaAtual -= danoFinal;
        AtualizarTextoVida();
        SpawnarTextoDano(danoFinal, critico, posicaoUltimoClique + Vector3.up * 0.3f);

        float intensidade = critico ? intensidadeTremor * 3f : intensidadeTremor;
        float duracao = critico ? duracaoTremor * 1.5f : duracaoTremor;

        StartCoroutine(PulsarImpacto(critico));

        Vector3 pos = transform.position;
        float tempo = 0f;

        while (tempo < duracao)
        {
            float ox = Random.Range(-intensidade, intensidade);
            float oy = Random.Range(-intensidade, intensidade);
            transform.position = pos + new Vector3(ox, oy, 0);
            tempo += Time.deltaTime;
            yield return null;
        }

        transform.position = pos;
        tremendo = false;

        if (vidaAtual <= 0)
            PedraDestruida();
    }

    IEnumerator PulsarImpacto(bool critico)
    {
        Vector3 escalaAtual = transform.localScale;
        Vector3 escalaImpacto = critico
            ? escalaAtual * 0.6f
            : escalaAtual * 0.85f;

        float tempo = 0f;
        float duracao = 0.08f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            transform.localScale = Vector3.Lerp(escalaAtual, escalaImpacto, tempo / duracao);
            yield return null;
        }

        tempo = 0f;

        Vector3 escalaOvershoot = escalaAtual * (critico ? 1.3f : 1.15f);
        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            transform.localScale = Vector3.Lerp(escalaImpacto, escalaOvershoot, tempo / duracao);
            yield return null;
        }

        tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            transform.localScale = Vector3.Lerp(escalaOvershoot, escalaAtual, tempo / duracao);
            yield return null;
        }

        transform.localScale = escalaAtual;
    }

    void SpawnarTextoDano(int dano, bool critico, Vector3 posicaoClique)
    {
        if (prefabTextoFlutuante == null) return;

        GameObject obj = Instantiate(prefabTextoFlutuante, posicaoClique, Quaternion.identity);
        TextoFlutuante tf = obj.GetComponent<TextoFlutuante>();

        string conteudo = "-" + dano;
        Color cor = critico ? new Color(1f, 0.2f, 0.2f) : new Color(1f, 1f, 1f);
        float tamanho = critico ? 8f : 5f;

        tf.Inicializar(conteudo, cor, tamanho);
    }

    void AtualizarTextoVida()
    {
        if (vidaText != null)
            vidaText.text = "Vida: " + vidaAtual;
    }

    void PedraDestruida()
    {
        float recompensaBase = Random.Range(dados.recompensaMin, dados.recompensaMax + 1);

        if (StatusManager.Instance.TemHabilidade("trevo_quatro_folhas"))
            if (recompensaBase >= dados.recompensaMax)
                recompensaBase *= 2f;

        if (StatusManager.Instance.TemHabilidade("analista_profissional"))
            if (Random.value < 0.4f)
                recompensaBase = dados.recompensaMax;

        float sorte = StatusManager.Instance.GetMultiplicadorSorte();
        float fortuna = StatusManager.Instance.GetMultiplicadorFortuna();
        float recompensaFinal = recompensaBase * sorte * fortuna;

        if (StatusManager.Instance.TemHabilidade("buque_trevos"))
            recompensaFinal *= 1.5f;

        // Inversor de polaridade
        if (valorInvertido)
            recompensaFinal *= -1f;

        GameManager.Instance.AdicionarDinheiro(recompensaFinal);
        Debug.Log("Recompensa: R$ " + recompensaFinal.ToString("F2"));
        Destroy(gameObject);
    }

    public void BloquearInteracao(bool bloquear)
    {
        interacaoBloqueada = bloquear;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = bloquear;
    }

    public void AtualizarContornoScanner(bool ativo)
    {
        if (!ativo)
        {
            if (contornoScanner != null)
                contornoScanner.enabled = false;
            return;
        }

        if (contornoScanner == null)
        {
            GameObject contorno = new GameObject("Contorno");
            contorno.transform.SetParent(transform);
            contorno.transform.localPosition = Vector3.zero;
            contorno.transform.localScale = Vector3.one * 1.2f;
            contornoScanner = contorno.AddComponent<SpriteRenderer>();
            contornoScanner.sprite = GetComponent<SpriteRenderer>().sprite;
            contornoScanner.sortingOrder = -1;
        }

        contornoScanner.enabled = true;
        float valorMedio = (dados.recompensaMin + dados.recompensaMax) / 2f;
        contornoScanner.color = valorMedio >= 0
            ? new Color(0.2f, 0.9f, 0.2f, 0.5f)
            : new Color(0.9f, 0.2f, 0.2f, 0.5f);
    }

    public void InvertерValores()
    {
        valorInvertido = !valorInvertido;
    }

    public int GetPrecoCompra()
    {
        return dados != null ? dados.precoCompra : 0;
    }

    public bool ReceberDanoExterno(int dano)
    {
        vidaAtual -= dano;
        AtualizarTextoVida();
        SpawnarTextoDano(dano, false, transform.position + Vector3.up * 0.3f);

        if (vidaAtual <= 0)
        {
            PedraDestruida();
            return true;
        }
        return false;
    }

    public string GetNomePedra() => dados != null ? dados.nome : "";
    public int GetVidaAtual() => vidaAtual;
}