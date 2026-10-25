using System.Collections;
using UnityEngine;
using TMPro;

public class PedraManager : MonoBehaviour
{
    /// <summary>Disparado quando uma pedra é destruída (antes do Destroy).</summary>
    public static event System.Action<PedraManager> OnPedraDestruida;

    [Header("Referências")]
    public TextMeshPro vidaText;

    [Header("Texto Flutuante")]
    public GameObject prefabTextoFlutuante;

    [Header("Configurações")]
    public float escalaAumentada = 2.5f;
    public float duracaoTremor = 0.3f;
    public float intensidadeTremor = 0.1f;
    public float tempoParaArrastar = 0.2f;

    [Header("Escala")]
    [Tooltip("Largura visível da pedra em unidades de mundo (ignora a margem transparente do PNG).")]
    public float larguraMundoAlvo = 1.8f;

    [Header("Animação")]
    [Tooltip("Quadros por segundo das pedras com segundo sprite. 0 desativa a animação.")]
    public float framesPorSegundo = 2f;

    private PedraDados dados;
    private int vidaAtual;
    private bool estaAtiva = false;
    private bool tremendo = false;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private BoxCollider2D colisor;

    private Vector3 escalaBase = Vector3.one;
    private readonly Sprite[] parAtual = new Sprite[2];
    private int frameAtual;
    private float proximoTrocaDeFrame;

    private bool segurando = false;
    private bool arrastando = false;
    private float tempoSegurando = 0f;
    private Vector3 offset;
    private Vector3 posicaoUltimoClique;

    private bool interacaoBloqueada = false;
    private bool valorInvertido = false;
    private bool garantirMaximo = false;
    private SpriteRenderer contornoScanner;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        PrepararColisor();
    }

    /// <summary>
    /// Garante um BoxCollider2D e desliga qualquer outro collider antigo.
    /// O tamanho é ajustado em AplicarEscalaEColisor() conforme o sprite atual.
    /// </summary>
    void PrepararColisor()
    {
        colisor = GetComponent<BoxCollider2D>();

        if (colisor == null)
            colisor = gameObject.AddComponent<BoxCollider2D>();

        foreach (Collider2D outro in GetComponents<Collider2D>())
        {
            if (outro != colisor)
                outro.enabled = false;
        }
    }

    public void Inicializar(PedraDados d, bool garantirMaximo = false)
    {
        dados = d;
        vidaAtual = d.vidaMaxima;
        this.garantirMaximo = garantirMaximo;

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        PrepararColisor();

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        AtualizarSpritePorVida();
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
        garantirMaximo = false;

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        PrepararColisor();

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        AtualizarSpritePorVida();
        AtualizarTextoVida();
    }

    void AtualizarSpritePorVida()
    {
        if (dados == null || spriteRenderer == null || dados.vidaMaxima <= 0)
            return;

        float porcentagemVida = (float)vidaAtual / dados.vidaMaxima * 100f;
        Sprite spriteA;
        Sprite spriteB;

        if (porcentagemVida > 75f)
        {
            spriteA = dados.sprite100;
            spriteB = dados.sprite100B;
        }
        else if (porcentagemVida > 50f)
        {
            spriteA = dados.sprite75;
            spriteB = dados.sprite75B;
        }
        else if (porcentagemVida > 25f)
        {
            spriteA = dados.sprite50;
            spriteB = dados.sprite50B;
        }
        else
        {
            spriteA = dados.sprite25;
            spriteB = dados.sprite25B;
        }

        if (spriteA != null)
            spriteRenderer.sprite = spriteA;

        // O contorno do Scanner é um sprite separado: acompanha o dano.
        if (contornoScanner != null && contornoScanner.enabled)
            contornoScanner.sprite = spriteA;

        parAtual[0] = spriteA;
        parAtual[1] = dados.animarFrames ? spriteB : null;
        frameAtual = 0;
        proximoTrocaDeFrame = Time.time + IntervaloDeFrame();

        AplicarEscalaEColisor();
    }

    /// <summary>
    /// Ajusta escala e collider para a área realmente desenhada do sprite atual.
    /// A pedra passa a ter sempre a largura de mundo desejada e a hitbox cobre o desenho inteiro.
    /// </summary>
    void AplicarEscalaEColisor()
    {
        Sprite referencia = parAtual[0];

        if (referencia == null && spriteRenderer != null)
            referencia = spriteRenderer.sprite;

        if (referencia == null)
            return;

        Bounds visivel = SpriteUtil.ObterBoundsVisiveis(referencia);

        // Quando a pedra tem dois frames, a hitbox usa a união dos dois,
        // assim ela não "pulsa" a cada troca de frame.
        if (parAtual[1] != null)
            visivel.Encapsulate(SpriteUtil.ObterBoundsVisiveis(parAtual[1]));

        float largura = Mathf.Max(visivel.size.x, 0.0001f);
        escalaBase = Vector3.one * (larguraMundoAlvo / largura);

        transform.localScale = estaAtiva ? escalaBase * escalaAumentada : escalaBase;

        if (colisor == null)
            PrepararColisor();

        if (colisor != null)
        {
            colisor.size = new Vector2(
                Mathf.Max(visivel.size.x, 0.01f),
                Mathf.Max(visivel.size.y, 0.01f)
            );
            colisor.offset = visivel.center;
        }
    }

    float IntervaloDeFrame()
    {
        return framesPorSegundo > 0f ? 1f / framesPorSegundo : float.PositiveInfinity;
    }

    void AtualizarAnimacao()
    {
        if (parAtual[1] == null || framesPorSegundo <= 0f || spriteRenderer == null)
            return;

        if (Time.time < proximoTrocaDeFrame)
            return;

        proximoTrocaDeFrame = Time.time + IntervaloDeFrame();
        frameAtual = 1 - frameAtual;
        spriteRenderer.sprite = frameAtual == 0 ? parAtual[0] : parAtual[1];
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
            if (rb != null)
                rb.isKinematic = false;

            arrastando = false;
            segurando = false;

            Collider2D[] colisoes = Physics2D.OverlapCircleAll(transform.position, 1.3f);

            // Escolhe a máquina ativa mais próxima do ponto onde a pedra foi
            // solta. Antes o primeiro collider da lista vencia e, como todas
            // estavam empilhadas, só o Lixo recebia.
            IMaquinaRecebedora maquinaEscolhida = null;
            float menorDistancia = float.MaxValue;

            foreach (Collider2D col in colisoes)
            {
                IMaquinaRecebedora maquina = col.GetComponent<IMaquinaRecebedora>();
                if (maquina == null || !maquina.PodeReceber(this))
                    continue;

                // Distância até a borda do collider: fica generoso com
                // sprites pequenos sem favorecer uma máquina só porque o
                // pivô dela está mais perto.
                Vector2 ponto = col.bounds.ClosestPoint(transform.position);
                float distancia = Vector2.Distance(ponto, transform.position);

                if (distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    maquinaEscolhida = maquina;
                }
            }

            if (maquinaEscolhida != null)
                maquinaEscolhida.ReceberPedra(this);

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
        AtualizarAnimacao();

        if (!segurando) return;

        tempoSegurando += Time.deltaTime;

        if (!arrastando && tempoSegurando >= tempoParaArrastar)
        {
            arrastando = true;

            if (estaAtiva)
            {
                estaAtiva = false;
                transform.localScale = escalaBase;
            }

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
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
        Vector3 escalaDestino = escalaBase * escalaAumentada;
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
        Vector3 escalaAlvo = escalaBase * escalaAumentada;
        Vector3 escalaEsticada = escalaAlvo * 1.2f;

        float tempo = 0f;
        float duracao = 0.15f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracao;
            transform.localScale = Vector3.Lerp(escalaAlvo, escalaEsticada, t);
            yield return null;
        }

        tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracao;
            transform.localScale = Vector3.Lerp(escalaEsticada, escalaAlvo, t);
            yield return null;
        }

        transform.localScale = escalaAlvo;
    }

    IEnumerator AplicarDano()
    {
        tremendo = true;

        float danoBaseFlutuante = Mathf.Max(1f, StatusManager.Instance.GetDanoForca());

        float chanceCritico = StatusManager.Instance.TemHabilidade("whey_protein") ? 0.3f : 0.05f;
        bool critico = Random.value < chanceCritico;
        float danoFlutuante = critico ? danoBaseFlutuante * 2f : danoBaseFlutuante;

        // Converte com teto: Mathf.RoundToInt sem limite estourava em níveis
        // altos de Força e o dano voltava para 1.
        int danoFinal = (int)Mathf.Min(danoFlutuante, 2000000000f);

        if (critico)
            Debug.Log("CRÍTICO!");

        vidaAtual = Mathf.Max(0, vidaAtual - danoFinal);
        AtualizarSpritePorVida();
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
        Vector3 escalaImpacto = critico ? escalaAtual * 0.6f : escalaAtual * 0.85f;

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

        if (tf == null) return;

        string conteudo = "-" + dano;
        Color cor = critico ? new Color(1f, 0.2f, 0.2f) : Color.white;
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
        float recompensaFinal = CalcularRecompensa();

        GameManager.Instance.AdicionarDinheiro(recompensaFinal);
        Debug.Log("Recompensa: R$ " + recompensaFinal.ToString("F2"));

        if (OnPedraDestruida != null)
            OnPedraDestruida(this);

        Destroy(gameObject);
    }

    float CalcularRecompensa()
    {
        int maiorValor = MaiorValorDaTabela();

        if (garantirMaximo)
            return valorInvertido
                ? -maiorValor + dados.precoCompra
                : maiorValor;

        int nivelSorte = StatusManager.Instance.nivelSorte;
        int recompensaBase = dados.SortearValor(nivelSorte);

        if (valorInvertido)
            return CalcularRecompensaInvertida(recompensaBase);

        if (StatusManager.Instance.TemHabilidade("trevo_quatro_folhas"))
            if (recompensaBase >= maiorValor)
                recompensaBase *= 2;

        if (StatusManager.Instance.TemHabilidade("analista_profissional"))
            if (Random.value < 0.1f)
                recompensaBase = maiorValor;

        float recompensa = recompensaBase;

        // Fortuna multiplica apenas o lucro. Se o resultado da pedra for
        // negativo, o multiplicador não pode aumentar o prejuízo.
        if (recompensa > 0f)
            recompensa *= StatusManager.Instance.GetMultiplicadorFortuna();

        if (StatusManager.Instance.TemHabilidade("buque_trevos"))
            recompensa *= 1.5f;

        return recompensa;
    }

    /// <summary>
    /// O Inversor troca o sinal do valor e soma o preço pago de volta, então
    /// a média fica neutra ("justa"). Fortuna e buquê continuam valendo só
    /// sobre o lucro, para não aumentar o prejuízo.
    /// </summary>
    float CalcularRecompensaInvertida(int recompensaBase)
    {
        float recompensa = -recompensaBase + dados.precoCompra;

        if (recompensa > 0f)
        {
            recompensa *= StatusManager.Instance.GetMultiplicadorFortuna();

            if (StatusManager.Instance.TemHabilidade("buque_trevos"))
                recompensa *= 1.5f;
        }

        return recompensa;
    }

    int MaiorValorDaTabela()
    {
        int maiorValor = int.MinValue;

        foreach (ValorPedra vp in dados.tabelaValores)
            if (vp.valor > maiorValor) maiorValor = vp.valor;

        return maiorValor;
    }

    public void BloquearInteracao(bool bloquear)
    {
        interacaoBloqueada = bloquear;
        Rigidbody2D corpo = GetComponent<Rigidbody2D>();
        if (corpo != null) corpo.isKinematic = bloquear;
    }

    [Header("Scanner (cor do contorno)")]
    [Tooltip("Margem de empate: dentro de ±2% do preço a pedra é considerada 'empate' (amarelo).")]
    public float margemScanner = 0.02f;

    public Color corScannerLucro = new Color(0.2f, 0.9f, 0.2f, 0.55f);
    public Color corScannerEmpate = new Color(1f, 0.85f, 0.2f, 0.55f);
    public Color corScannerPrejuizo = new Color(0.9f, 0.2f, 0.2f, 0.55f);

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
            contornoScanner.sprite = spriteRenderer != null ? spriteRenderer.sprite : null;
            contornoScanner.sortingOrder = -1;
        }

        contornoScanner.enabled = true;

        if (dados == null || dados.tabelaValores == null)
            return;

        // A cor compara a recompensa média (com Sorte e Fortuna atuais) com o
        // preço. A margem existe porque as tabelas são arredondadas: sem ela,
        // uma diferença de 0,1 (0,4%) já pintava a pedra de verde ou vermelho.
        float valorEsperado = dados.ValorEsperado(
            StatusManager.Instance.nivelSorte,
            StatusManager.Instance.GetMultiplicadorFortuna());

        float preco = dados.precoCompra;
        float margem = Mathf.Abs(preco) * Mathf.Max(0f, margemScanner);

        if (valorEsperado > preco + margem)
            contornoScanner.color = corScannerLucro;      // lucro médio
        else if (valorEsperado < preco - margem)
            contornoScanner.color = corScannerPrejuizo;   // prejuízo médio
        else
            contornoScanner.color = corScannerEmpate;     // empata no longo prazo
    }

    public void InverterValores()
    {
        valorInvertido = !valorInvertido;
    }

    public int GetPrecoCompra()
    {
        return dados != null ? dados.precoCompra : 0;
    }

    public bool ReceberDanoExterno(int dano)
    {
        vidaAtual = Mathf.Max(0, vidaAtual - Mathf.Max(0, dano));
        AtualizarSpritePorVida();
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
