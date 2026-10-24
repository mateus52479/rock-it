using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quadrado (ou sprite) que voa pela tela até as coordenadas definidas em
/// cada passo do roteiro. As coordenadas são pixels a partir do centro da tela.
/// </summary>
public class NPCVoador : MonoBehaviour
{
    public RectTransform Rect { get; private set; }

    public float amplitudeFlutuacao = 6f;
    public float velocidadeFlutuacao = 2.5f;

    private Vector2 posicaoInicial;
    private Vector2 destino;
    private float inicioVoo;
    private float duracaoVoo = 1f;

    void Awake()
    {
        Rect = GetComponent<RectTransform>();

        destino = Rect.anchoredPosition;
        posicaoInicial = destino;
    }

    public static NPCVoador Criar(Transform canvas)
    {
        Image imagem = UIUtil.CriarImagem(
            canvas, "NPC", Color.white,
            new Vector2(64f, 64f),
            new Vector2(-760f, -420f));

        imagem.raycastTarget = false;

        NPCVoador npc = imagem.gameObject.AddComponent<NPCVoador>();
        npc.gameObject.SetActive(false);

        return npc;
    }

    public void IrPara(float x, float y, float duracao = 0.9f)
    {
        posicaoInicial = Rect.anchoredPosition;
        destino = new Vector2(x, y);
        inicioVoo = Time.time;
        duracaoVoo = Mathf.Max(0.05f, duracao);
    }

    public void PosicionarSemVoar(float x, float y)
    {
        Rect.anchoredPosition = new Vector2(x, y);
        destino = Rect.anchoredPosition;
        posicaoInicial = destino;
    }

    public Vector2 PosicaoAtual()
    {
        return Rect.anchoredPosition;
    }

    void Update()
    {
        float t = Mathf.Clamp01((Time.time - inicioVoo) / duracaoVoo);
        float suave = Mathf.SmoothStep(0f, 1f, t);

        Vector2 basePos = Vector2.Lerp(posicaoInicial, destino, suave);
        float flutuacao = Mathf.Sin(Time.time * velocidadeFlutuacao) * amplitudeFlutuacao;

        Rect.anchoredPosition = basePos + new Vector2(0f, flutuacao);
    }
}
