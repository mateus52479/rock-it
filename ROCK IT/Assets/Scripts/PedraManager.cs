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

    [Tooltip("Força do tremor ao acertar a pedra (em unidades de mundo).")]
    public float intensidadeTremor = 0.06f;

    public float tempoParaArrastar = 0.2f;

    [Header("Escala")]
    [Tooltip("Largura visível da pedra em unidades de mundo (ignora a margem transparente do PNG).")]
    public float larguraMundoAlvo = 1.8f;

    [Header("Animação")]
    [Tooltip("Quadros por segundo das pedras com segundo sprite. 0 desativa a animação.")]
    public float framesPorSegundo = 2f;

    [Header("Queda ao comprar")]
    [Tooltip("Altura de onde a pedra cai ao ser comprada (em unidades de mundo).")]
    public float alturaQueda = 3.2f;

    [Tooltip("Duração da queda (em segundos).")]
    public float duracaoQueda = 0.55f;

    [Tooltip("Inclinação inicial da pedra na queda (em graus, para cada lado).")]
    public float inclinacaoQueda = 25f;

    [Tooltip("Quanto a pedra achata ao bater na mesa (0 desliga o efeito).")]
    [Range(0f, 0.6f)]
    public float esmagamentoImpacto = 0.2f;

    [Tooltip("Empurrãozinho aleatório depois que a pedra pousa.")]
    public float impulsoAoPousar = 1.5f;

    [Header("Ordem de desenho")]
    [Tooltip("As máquinas desenham na ordem 0; a pedra fica na frente delas.")]
    public int ordemDesenho = 10;

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

    /// <summary>
    /// Resultado sorteado quando a pedra aparece. Fica guardado para o Scanner
    /// poder dizer se o valor é positivo ou negativo sem "mentir" depois.
    /// </summary>
    private int resultadoSorteado = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        PrepararColisor();
        AplicarOrdemDeDesenho();
    }

    /// <summary>
    /// As máquinas desenham na ordem 0. A pedra (e o texto de vida) sobem para
    /// a ordem configurada em <see cref="ordemDesenho"/>, então nunca ficam
    /// escondidas atrás de uma máquina. O contorno do Scanner usa a mesma
    /// ordem da pedra e fica atrás dela pelo Z.
    /// </summary>
    void AplicarOrdemDeDesenho()
    {
        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = ordemDesenho;

        if (vidaText == null) return;

        MeshRenderer desenhoDoTexto = vidaText.GetComponent<MeshRenderer>();

        if (desenhoDoTexto != null)
            desenhoDoTexto.sortingOrder = ordemDesenho + 1;
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
        resultadoSorteado = SortearResultado(d);

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        PrepararColisor();

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        AtualizarSpritePorVida();
        AtualizarTextoVida();
        AplicarOrdemDeDesenho();

        MaquinaScanner.NotificarNovaPedra(this);

        // Se o ponto de spawn cair fora da mesa, a pedra cai na beirada.
        transform.position = LimiteMesa.Limitar(transform.position, MetadeDoColisor());

        // Só a compra tem animação de queda: pedras carregadas do save
        // (InicializarComVida) já aparecem paradas na mesa.
        StartCoroutine(CairNaMesa());
    }

    /// <summary>
    /// Animação de spawn da pedra comprada: ela cai de cima da mesa, achata no
    /// impacto e só depois volta a aceitar clique e arrasto. A queda é feita na
    /// mão porque o Rigidbody2D do prefab tem gravidade 0.
    /// </summary>
    IEnumerator CairNaMesa()
    {
        Vector3 destino = transform.position;
        Vector3 posicaoInicial = destino + Vector3.up * Mathf.Max(0f, alturaQueda);
        Vector3 escalaAlvo = escalaBase;
        float rotacaoInicial = Random.Range(-inclinacaoQueda, inclinacaoQueda);

        transform.position = posicaoInicial;
        transform.rotation = Quaternion.Euler(0f, 0f, rotacaoInicial);
        transform.localScale = escalaAlvo;

        interacaoBloqueada = true;
        estaAtiva = false;

        if (rb != null)
        {
            // A física fica desligada durante a animação para o Rigidbody2D
            // não brigar com a posição que estamos definindo.
            rb.simulated = false;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        float duracao = Mathf.Max(0.05f, duracaoQueda);
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracao);

            // t² dá a sensação de gravidade: começa devagar e acelera.
            float queda = t * t;

            transform.position = Vector3.Lerp(posicaoInicial, destino, queda);
            transform.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Lerp(rotacaoInicial, 0f, queda)
            );
            yield return null;
        }

        transform.position = destino;
        transform.rotation = Quaternion.identity;

        yield return StartCoroutine(AchatarNoImpacto(escalaAlvo));

        if (rb != null)
        {
            rb.simulated = true;

            if (impulsoAoPousar > 0f)
            {
                Vector2 direcao = Random.insideUnitCircle.normalized;
                rb.AddForce(direcao * impulsoAoPousar, ForceMode2D.Impulse);
            }
        }

        interacaoBloqueada = false;
    }

    /// <summary>
    /// Squash do impacto: começa achatada e volta ao normal com um leve
    /// overshoot, como se a pedra tivesse batido na mesa.
    /// </summary>
    IEnumerator AchatarNoImpacto(Vector3 escalaAlvo)
    {
        if (esmagamentoImpacto <= 0f)
            yield break;

        float duracao = 0.22f;
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracao);

            // Cosseno decaindo: achatado no quadro do impacto, esticado de
            // leve na volta e zerado no fim.
            float achatamento =
                esmagamentoImpacto * Mathf.Cos(t * Mathf.PI * 2f) * (1f - t);

            transform.localScale = new Vector3(
                escalaAlvo.x * (1f + achatamento),
                escalaAlvo.y * (1f - achatamento),
                escalaAlvo.z
            );
            yield return null;
        }

        transform.localScale = escalaAlvo;
    }

    public void InicializarComVida(PedraDados d, int vida)
    {
        dados = d;
        vidaAtual = vida;
        garantirMaximo = false;
        resultadoSorteado = SortearResultado(d);

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        PrepararColisor();

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        AtualizarSpritePorVida();
        AtualizarTextoVida();
        AplicarOrdemDeDesenho();

        // Save antigo podia ter guardado uma pedra fora da mesa: traz de volta.
        transform.position = LimiteMesa.Limitar(transform.position, MetadeDoColisor());
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
        SoltarPedra();
    }

    /// <summary>
    /// Termina o clique/arrasto. Também é chamado pelo Update quando o botão é
    /// solto fora do colisor: com a pedra presa na borda da mesa, o OnMouseUp
    /// pode não chegar e ela ficaria seguindo o mouse para sempre.
    /// </summary>
    void SoltarPedra()
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

        if (segurando)
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

        if ((segurando || arrastando) && Input.GetMouseButtonUp(0))
            SoltarPedra();

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

            // Mover o transform na mão ignora as paredes, então o arrasto é
            // limitado à mesa: a pedra para na borda em vez de escapar.
            transform.position = LimiteMesa.Limitar(
                posicaoMouse + offset,
                MetadeDoColisor()
            );
        }
    }

    /// <summary>Metade do tamanho do collider em unidades de mundo.</summary>
    Vector2 MetadeDoColisor()
    {
        if (colisor == null)
            PrepararColisor();

        return colisor != null
            ? (Vector2)colisor.bounds.extents
            : new Vector2(0.5f, 0.5f);
    }

    /// <summary>
    /// Prende a pedra dentro da mesa. As máquinas movem a pedra para a própria
    /// posição, que fica na beirada, então elas chamam isso ao devolver a pedra.
    /// </summary>
    public void ManterNaMesa()
    {
        transform.position = LimiteMesa.Limitar(transform.position, MetadeDoColisor());
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

        float intensidade = critico ? intensidadeTremor * 2f : intensidadeTremor;
        float duracao = critico ? duracaoTremor * 1.5f : duracaoTremor;

        StartCoroutine(PulsarImpacto(critico));

        Vector3 pos = transform.position;
        float tempo = 0f;
        float fase = Random.Range(0f, Mathf.PI * 2f);

        while (tempo < duracao)
        {
            // Oscilação suave que perde força: o sorteio de posição quadro a
            // quadro vibrava demais na pedra ampliada e parecia um terremoto.
            float progresso = Mathf.Clamp01(tempo / duracao);
            float forca = intensidade * (1f - progresso);
            float onda = tempo * 42f + fase;

            transform.position = pos + new Vector3(
                Mathf.Sin(onda) * forca,
                Mathf.Cos(onda * 1.3f) * forca * 0.6f,
                0f
            );

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

        // O texto flutuante nasce com a ordem do prefab (10). Sobe para ficar
        // acima da pedra e do texto de vida, senão some atrás deles.
        MeshRenderer desenhoDoTexto = obj.GetComponent<MeshRenderer>();

        if (desenhoDoTexto != null)
            desenhoDoTexto.sortingOrder = ordemDesenho + 2;

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

        // Usa o resultado sorteado quando a pedra apareceu. É o mesmo que o
        // Scanner mostrou, então a borda nunca contradiz a recompensa.
        int recompensaBase = resultadoSorteado;

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

    /// <summary>
    /// Sorteia o resultado uma única vez, quando a pedra aparece, para o
    /// Scanner poder mostrar verde/vermelho sem revelar errado.
    /// </summary>
    static int SortearResultado(PedraDados pedra)
    {
        if (pedra == null || pedra.tabelaValores == null || pedra.tabelaValores.Length == 0)
            return 0;

        int nivelSorte = StatusManager.Instance != null
            ? StatusManager.Instance.nivelSorte
            : 1;

        return pedra.SortearValor(nivelSorte);
    }

    public void BloquearInteracao(bool bloquear)
    {
        interacaoBloqueada = bloquear;
        Rigidbody2D corpo = GetComponent<Rigidbody2D>();
        if (corpo != null) corpo.isKinematic = bloquear;
    }

    [Header("Scanner")]
    public Color corScannerLucro = new Color(0.2f, 0.9f, 0.2f, 0.6f);
    public Color corScannerPrejuizo = new Color(0.9f, 0.2f, 0.2f, 0.6f);

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

            // Um pouquinho atrás da pedra no eixo Z: com a mesma ordem de
            // desenho (0) ele fica acima da mesa (-1), mas continua atrás da
            // pedra — antes ele usava ordem -1 e sumia atrás da mesa.
            contorno.transform.localPosition = new Vector3(0f, 0f, 0.02f);
            contorno.transform.localScale = Vector3.one * 1.2f;

            contornoScanner = contorno.AddComponent<SpriteRenderer>();
            contornoScanner.sprite = spriteRenderer != null ? spriteRenderer.sprite : null;
            contornoScanner.sortingLayerID = spriteRenderer != null
                ? spriteRenderer.sortingLayerID
                : 0;
            contornoScanner.sortingOrder = spriteRenderer != null
                ? spriteRenderer.sortingOrder
                : 0;
        }

        contornoScanner.enabled = true;

        // A pedra pode ter mudado de ordem (ou o contorno foi criado antes de
        // AplicarOrdemDeDesenho): mantém o contorno na mesma ordem da pedra,
        // acima das máquinas e atrás do desenho da pedra pelo Z.
        if (spriteRenderer != null)
        {
            contornoScanner.sortingLayerID = spriteRenderer.sortingLayerID;
            contornoScanner.sortingOrder = spriteRenderer.sortingOrder;
        }

        if (dados == null || dados.tabelaValores == null)
            return;

        // O resultado já foi sorteado quando a pedra apareceu: verde quando
        // ela vai pagar um valor positivo, vermelho quando for zero/negativo.
        contornoScanner.color = RecompensaBasePrevista() > 0f
            ? corScannerLucro
            : corScannerPrejuizo;
    }

    /// <summary>
    /// Valor que a pedra vai render (antes de Fortuna/trevo, que só
    /// multiplicam resultados positivos), já considerando o Inversor.
    /// </summary>
    float RecompensaBasePrevista()
    {
        if (dados == null) return 0f;

        if (garantirMaximo)
        {
            int maior = MaiorValorDaTabela();
            return valorInvertido ? -maior + dados.precoCompra : maior;
        }

        return valorInvertido
            ? -resultadoSorteado + dados.precoCompra
            : resultadoSorteado;
    }

    public void InverterValores()
    {
        valorInvertido = !valorInvertido;

        // O Inversor troca o sinal da recompensa: a cor do Scanner
        // precisa ser recalculada.
        if (contornoScanner != null && contornoScanner.enabled)
            AtualizarContornoScanner(true);
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
