using UnityEngine;
using System.Collections;

public class MaquinaInversor : MonoBehaviour, IMaquinaRecebedora
{
    public SpriteRenderer spriteRenderer;

    [Header("Sprites")]
    [Tooltip("Estado inativo: airfryer sem resultado. Se ficar vazio, carrega de Resources/Sprites/Maquinas.")]
    public Sprite spriteInativo;

    [Tooltip("Estado positivo usado durante o tremor, quando a pedra vai virar lucro.")]
    public Sprite spritePositivo;

    [Tooltip("Estado negativo usado durante o tremor, quando a pedra vai virar prejuízo.")]
    public Sprite spriteNegativo;

    [Tooltip("Ajuste de tamanho do desenho na mesa.")]
    public float escalaVisual = 0.4f;

    [Header("Tremor")]
    [Tooltip("Quanto o airfryer anda para os lados enquanto processa a pedra.")]
    public float amplitudeTremor = 0.06f;

    [Tooltip("Duração do tremor antes de liberar a pedra.")]
    public float duracaoTremor = 1f;

    private const string CaminhoInativo = "Sprites/Maquinas/airfryer";
    private const string CaminhoPositivo = "Sprites/Maquinas/airfryer positivo";
    private const string CaminhoNegativo = "Sprites/Maquinas/airfryer negativo";

    private bool ativo = false;
    private bool processando = false;
    private Vector3 posicaoBase;

    void Awake()
    {
        GarantirSprite();
        CarregarSprites();
        posicaoBase = transform.position;
        AplicarSprite(spriteInativo);
    }

    void GarantirSprite()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void CarregarSprites()
    {
        if (spriteInativo == null)
            spriteInativo = SpriteUtil.CarregarDeResources(CaminhoInativo);

        if (spritePositivo == null)
            spritePositivo = SpriteUtil.CarregarDeResources(CaminhoPositivo);

        if (spriteNegativo == null)
            spriteNegativo = SpriteUtil.CarregarDeResources(CaminhoNegativo);
    }

    public void Ativar()
    {
        GarantirSprite();
        CarregarSprites();

        ativo = true;
        processando = false;
        posicaoBase = transform.position;
        gameObject.SetActive(true);

        // Sem pedra nova, a máquina fica no estado inativo.
        AplicarSprite(spriteInativo);
    }

    public bool PodeReceber(PedraManager pedra)
    {
        return ativo && !processando;
    }

    public void ReceberPedra(PedraManager pedra)
    {
        if (!ativo || processando || pedra == null) return;
        StartCoroutine(InverterPedra(pedra));
    }

    IEnumerator InverterPedra(PedraManager pedra)
    {
        processando = true;
        posicaoBase = transform.position;

        // A pedra some assim que entra: o airfryer é quem fica tremendo.
        pedra.BloquearInteracao(true);
        pedra.EsconderTemporariamente();
        pedra.transform.position = posicaoBase;

        Sprite[] sequencia =
        {
            spriteInativo,
            spritePositivo,
            spriteNegativo,
            spriteInativo
        };

        int indiceSprite = -1;
        float tempo = 0f;
        float duracao = Mathf.Max(0.1f, duracaoTremor);

        while (tempo < duracao)
        {
            float progresso = Mathf.Clamp01(tempo / duracao);
            float forca = amplitudeTremor * (1f - progresso * 0.35f);

            transform.position = posicaoBase + new Vector3(
                Random.Range(-forca, forca),
                Random.Range(-forca, forca),
                0f);

            // O desenho troca durante o tremor em vez de ficar preso no
            // resultado depois que a pedra sai.
            int novoIndice = Mathf.FloorToInt(progresso * 10f);

            if (novoIndice != indiceSprite)
            {
                indiceSprite = novoIndice;
                AplicarSprite(sequencia[indiceSprite % sequencia.Length]);
            }

            tempo += Time.deltaTime;
            yield return null;
        }

        transform.position = posicaoBase;

        pedra.InverterValores();

        // Mostra o resultado por um instante, ainda dentro do tremor.
        AplicarSprite(pedra.RecompensaPrevista > 0f
            ? spritePositivo
            : spriteNegativo);

        yield return new WaitForSeconds(0.18f);

        // Devolve a pedra acima da máquina e longe da parede de baixo da
        // mesa. Antes ela voltava na posição exata do airfryer (que fica na
        // beirada) e o empurrão aleatório podia prendê-la no limite inferior.
        transform.position = posicaoBase;

        // Sobe o suficiente para não nascer dentro do desenho/collider da
        // máquina; senão a física empurra a pedra de volta para a beirada.
        BoxCollider2D colisorMaquina = GetComponent<BoxCollider2D>();
        float topoMaquina = colisorMaquina != null
            ? colisorMaquina.bounds.max.y
            : posicaoBase.y;

        float alturaLiberacao = Mathf.Max(
            0.9f,
            topoMaquina - posicaoBase.y + 0.8f);

        pedra.transform.position = posicaoBase
            + Vector3.up * alturaLiberacao;

        pedra.MostrarNovamente();
        pedra.ManterNaMesa();

        pedra.BloquearInteracao(false);

        Rigidbody2D rb = pedra.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.position = pedra.transform.position;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // Só empurra para cima e para o centro da mesa: nada de impulso
            // aleatório que possa jogar a pedra contra o limite de baixo.
            Vector2 centro = Camera.main != null
                ? (Vector2)Camera.main.ViewportToWorldPoint(
                    new Vector3(0.5f, 0.5f, 10f))
                : Vector2.zero;

            Vector2 direcao = (centro - (Vector2)pedra.transform.position) * 0.35f;
            direcao += Vector2.up;
            rb.AddForce(direcao.normalized * 3f, ForceMode2D.Impulse);
        }

        // Terminou: o airfryer volta ao estado normal, sem ficar marcado
        // para sempre com o resultado da última pedra.
        AplicarSprite(spriteInativo);
        processando = false;
    }

    void AplicarSprite(Sprite alvo)
    {
        if (spriteRenderer == null)
            return;

        if (alvo != null)
        {
            spriteRenderer.sprite = alvo;
            spriteRenderer.color = Color.white;
        }

        transform.localScale = Vector3.one * escalaVisual;

        BoxCollider2D colisor = GetComponent<BoxCollider2D>();
        if (colisor != null && spriteRenderer.sprite != null)
        {
            Bounds visivel = SpriteUtil.ObterBoundsVisiveis(spriteRenderer.sprite);
            colisor.size = visivel.size;
            colisor.offset = visivel.center;
        }
    }
}
