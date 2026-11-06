using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MaquinaQuebrador : MonoBehaviour, IMaquinaRecebedora
{
    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("Liquidificador vazio. Se ficar vazio, carrega de Resources/Sprites/Maquinas.")]
    public Sprite spriteVazio;

    [Tooltip("Liquidificador com pedra dentro, primeiro desenho.")]
    public Sprite spriteComCoisa1;

    [Tooltip("Liquidificador com pedra dentro, segundo desenho.")]
    public Sprite spriteComCoisa2;

    [Tooltip("Ajuste de tamanho do liquidificador na mesa.")]
    public float escalaVisual = 0.4f;

    [Tooltip("Tempo entre os dois desenhos enquanto há pedra dentro, em segundos.")]
    public float intervaloAnimacao = 1f;

    [Header("Cores de reserva")]
    [Tooltip("Usadas só se o sprite correspondente não for encontrado.")]
    public Color corVazia = Color.white;
    public Color corOcupada = Color.cyan;

    [Header("Dano")]
    [Tooltip("Fração do dano do jogador aplicada por segundo. 1 = dano normal, sem redução.")]
    public float percentualDoDanoDoJogador = 1f;

    [Tooltip("Quantos golpes por segundo o quebrador aplica em cada pedra.")]
    public int ataquesPorSegundo = 1;

    [Header("Tremor")]
    [Tooltip("Quanto o quebrador anda para os lados enquanto processa pedras.")]
    public float amplitudeTremor = 0.06f;

    private bool ativo = false;
    private readonly List<PedraManager> pedras = new List<PedraManager>();
    private Coroutine rotinaQuebra;
    private Coroutine rotinaTremor;
    private Vector3 posicaoBase;
    private bool painelAberto;

    private const string CaminhoVazio = "Sprites/Maquinas/liquidificador 0";
    private const string CaminhoComCoisa1 = "Sprites/Maquinas/liquidificador com coisa 1";
    private const string CaminhoComCoisa2 = "Sprites/Maquinas/liquidificador com coisa 2";

    private float tempoAnimacao;
    private bool segundoDesenho;

    /// <summary>Pedras que estão sendo quebradas agora, na ordem em que entraram.</summary>
    public List<PedraManager> PedrasProcessando
    {
        get { return pedras; }
    }

    void Awake()
    {
        GarantirSprite();
        CarregarSprites();
        AplicarSprite(spriteVazio, corVazia);
        posicaoBase = transform.position;
    }

    void GarantirSprite()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void CarregarSprites()
    {
        if (spriteVazio == null)
            spriteVazio = SpriteUtil.CarregarDeResources(CaminhoVazio);

        if (spriteComCoisa1 == null)
            spriteComCoisa1 = SpriteUtil.CarregarDeResources(CaminhoComCoisa1);

        if (spriteComCoisa2 == null)
            spriteComCoisa2 = SpriteUtil.CarregarDeResources(CaminhoComCoisa2);
    }

    public void Ativar()
    {
        GarantirSprite();
        CarregarSprites();

        ativo = true;
        posicaoBase = transform.position;
        gameObject.SetActive(true);

        AplicarSprite(spriteVazio, corVazia);
    }

    /// <summary>Aceita quantas pedras o jogador conseguir arrastar para cima.</summary>
    public bool PodeReceber(PedraManager pedra)
    {
        return ativo;
    }

    public void ReceberPedra(PedraManager pedra)
    {
        if (pedra == null || !ativo || pedras.Contains(pedra)) return;

        GarantirSprite();

        pedras.Add(pedra);

        pedra.BloquearInteracao(true);
        pedra.EsconderTemporariamente();
        pedra.transform.position = posicaoBase;

        segundoDesenho = false;
        tempoAnimacao = 0f;
        AplicarSprite(spriteComCoisa1, corOcupada);

        if (rotinaQuebra == null)
            rotinaQuebra = StartCoroutine(QuebrarAutomaticamente());

        if (rotinaTremor == null)
            rotinaTremor = StartCoroutine(Tremer());
    }

    IEnumerator QuebrarAutomaticamente()
    {
        while (pedras.Count > 0)
        {
            yield return new WaitForSeconds(1f / Mathf.Max(1, ataquesPorSegundo));

            int dano = DanoPorSegundo();

            // Percorre de trás para frente porque uma pedra destruída sai da
            // lista no mesmo quadro.
            for (int i = pedras.Count - 1; i >= 0; i--)
            {
                PedraManager pedra = pedras[i];

                if (pedra == null)
                {
                    pedras.RemoveAt(i);
                    continue;
                }

                bool destruida = pedra.ReceberDanoExterno(dano, false);

                if (destruida)
                    pedras.RemoveAt(i);
            }
        }

        rotinaQuebra = null;

        AplicarSprite(spriteVazio, corVazia);

        transform.position = posicaoBase;
    }

    IEnumerator Tremer()
    {
        float tempo = 0f;

        while (pedras.Count > 0)
        {
            tempo += Time.deltaTime;
            tempoAnimacao += Time.deltaTime;

            if (tempoAnimacao >= Mathf.Max(0.05f, intervaloAnimacao))
            {
                tempoAnimacao -= Mathf.Max(0.05f, intervaloAnimacao);
                segundoDesenho = !segundoDesenho;
                AplicarSprite(
                    segundoDesenho ? spriteComCoisa2 : spriteComCoisa1,
                    corOcupada);
            }

            transform.position = posicaoBase + new Vector3(
                Mathf.Sin(tempo * 52f) * amplitudeTremor,
                Mathf.Cos(tempo * 67f) * amplitudeTremor * 0.6f,
                0f);

            yield return null;
        }

        transform.position = posicaoBase;
        segundoDesenho = false;
        tempoAnimacao = 0f;
        AplicarSprite(spriteVazio, corVazia);
        rotinaTremor = null;
    }

    int DanoPorSegundo()
    {
        float dano = StatusManager.Instance != null
            ? StatusManager.Instance.GetDanoForca()
            : 1f;

        dano *= Mathf.Max(0f, percentualDoDanoDoJogador);

        return Mathf.Max(1, (int)Mathf.Min(dano, 2000000000f));
    }

    /// <summary>
    /// Clique abre/fecha a telinha com a conclusão das pedras. Ela fica ao
    /// lado do quebrador, como a janela de status, só que menor.
    /// </summary>
    void OnMouseDown()
    {
        if (!ativo)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        if (PauseManager.Instance != null && PauseManager.Instance.EstaPausado)
            return;

        if (LojaManager.LojaAberta)
            return;

        painelAberto = !painelAberto;

        if (painelAberto)
            QuebradorPainelUI.Mostrar(this);
        else
            QuebradorPainelUI.Esconder(this);
    }

    public void FecharPainel()
    {
        painelAberto = false;
        QuebradorPainelUI.Esconder(this);
    }

    void OnDisable()
    {
        // O OnDisable também roda quando o MaquinaManager esconde a máquina
        // ainda não comprada. Nesse caso não mexe na posição: ela já foi
        // ajustada pelo manager e o Ativar() vai capturar a posição certa.
        if (ativo)
        {
            if (rotinaQuebra != null)
            {
                StopCoroutine(rotinaQuebra);
                rotinaQuebra = null;
            }

            if (rotinaTremor != null)
            {
                StopCoroutine(rotinaTremor);
                rotinaTremor = null;
            }

            transform.position = posicaoBase;
        }

        FecharPainel();
    }

    public void Evoluir()
    {
        ataquesPorSegundo++;
        Debug.Log("Quebrador evoluído: " + ataquesPorSegundo + " ataque(s)/s");
    }

    public int GetAtaquesPorSegundo() { return ataquesPorSegundo; }

    void AplicarSprite(Sprite alvo, Color corReserva)
    {
        if (spriteRenderer == null)
            return;

        if (alvo == null)
        {
            spriteRenderer.color = corReserva;
            return;
        }

        if (spriteRenderer.sprite != alvo)
        {
            spriteRenderer.sprite = alvo;
            AjustarTamanho(alvo);
        }

        spriteRenderer.color = Color.white;
    }

    /// <summary>
    /// O liquidificador tem 1000 px. A escala é fixa para ele ocupar o mesmo
    /// tipo de espaço das outras máquinas da mesa; o collider acompanha o
    /// desenho para o clique continuar pegando o corpo inteiro.
    /// </summary>
    void AjustarTamanho(Sprite sprite)
    {
        Bounds visivel = SpriteUtil.ObterBoundsVisiveis(sprite);

        transform.localScale = Vector3.one * escalaVisual;

        BoxCollider2D colisor = GetComponent<BoxCollider2D>();

        if (colisor != null)
        {
            colisor.size = visivel.size;
            colisor.offset = visivel.center;
        }
    }
}
