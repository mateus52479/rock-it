using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Retângulo amarelo pulsante que destaca o objeto que o tutorial está
/// explicando. Funciona com elementos de UI e com objetos do mundo.
/// </summary>
public class DestaqueUI : MonoBehaviour
{
    public RectTransform canvasRect;

    private Image imagem;
    private RectTransform alvoUI;
    private Transform alvoMundo;
    private Camera cameraPrincipal;

    public static DestaqueUI Criar(Transform canvas)
    {
        Image imagem = UIUtil.CriarImagem(
            canvas, "Destaque",
            new Color(1f, 0.85f, 0.2f, 0.2f),
            new Vector2(100f, 100f),
            Vector2.zero);

        imagem.raycastTarget = false;

        DestaqueUI destaque = imagem.gameObject.AddComponent<DestaqueUI>();
        destaque.imagem = imagem;
        destaque.canvasRect = (RectTransform)canvas;
        destaque.gameObject.SetActive(false);

        return destaque;
    }

    public void Destacar(string alvo)
    {
        alvoUI = null;
        alvoMundo = null;
        gameObject.SetActive(false);

        if (string.IsNullOrEmpty(alvo))
            return;

        if (alvo == "pedra")
        {
            PedraManager pedra = Object.FindAnyObjectByType<PedraManager>();
            if (pedra != null)
                alvoMundo = pedra.transform;
        }
        else if (alvo == "itemLoja")
        {
            if (LojaManager.Instance != null &&
                LojaManager.Instance.containerItens != null &&
                LojaManager.Instance.containerItens.childCount > 0)
            {
                alvoUI = LojaManager.Instance.containerItens.GetChild(0) as RectTransform;
            }
        }
        else
        {
            GameObject objeto = GameObject.Find(alvo);

            if (objeto != null)
            {
                alvoUI = objeto.GetComponent<RectTransform>();

                if (alvoUI == null)
                    alvoMundo = objeto.transform;
            }
        }

        gameObject.SetActive(alvoUI != null || alvoMundo != null);
    }

    public void Limpar()
    {
        alvoUI = null;
        alvoMundo = null;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (alvoUI == null && alvoMundo == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (alvoUI != null && !alvoUI.gameObject.activeInHierarchy)
        {
            Limpar();
            return;
        }

        if (alvoMundo != null && !alvoMundo.gameObject.activeInHierarchy)
        {
            Limpar();
            return;
        }

        if (!CalcularArea(out Vector2 centro, out Vector2 tamanho))
        {
            Limpar();
            return;
        }

        imagem.rectTransform.anchoredPosition = centro;
        imagem.rectTransform.sizeDelta = tamanho + new Vector2(14f, 14f);

        float pulso = 0.14f + 0.10f * Mathf.Sin(Time.time * 4f);
        imagem.color = new Color(1f, 0.85f, 0.2f, pulso);
    }

    bool CalcularArea(out Vector2 centro, out Vector2 tamanho)
    {
        centro = Vector2.zero;
        tamanho = Vector2.one;

        if (alvoUI != null)
        {
            Vector3[] cantos = new Vector3[4];
            alvoUI.GetWorldCorners(cantos);

            Vector2 minimo = canvasRect.InverseTransformPoint(cantos[0]);
            Vector2 maximo = canvasRect.InverseTransformPoint(cantos[2]);

            centro = (minimo + maximo) / 2f;
            tamanho = new Vector2(
                Mathf.Abs(maximo.x - minimo.x),
                Mathf.Abs(maximo.y - minimo.y));

            return tamanho.x > 0f && tamanho.y > 0f;
        }

        if (alvoMundo != null)
        {
            cameraPrincipal = Camera.main;
            if (cameraPrincipal == null)
                return false;

            Bounds area = new Bounds(alvoMundo.position, Vector3.one);

            Collider2D colisor = alvoMundo.GetComponent<Collider2D>();
            if (colisor != null)
                area = colisor.bounds;
            else
            {
                SpriteRenderer sprite = alvoMundo.GetComponent<SpriteRenderer>();
                if (sprite != null)
                    area = sprite.bounds;
            }

            Vector3 minimo = cameraPrincipal.WorldToScreenPoint(area.min);
            Vector3 maximo = cameraPrincipal.WorldToScreenPoint(area.max);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, minimo, null, out Vector2 localMinimo);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, maximo, null, out Vector2 localMaximo);

            centro = (localMinimo + localMaximo) / 2f;
            tamanho = new Vector2(
                Mathf.Abs(localMaximo.x - localMinimo.x),
                Mathf.Abs(localMaximo.y - localMinimo.y));

            return tamanho.x > 0f && tamanho.y > 0f;
        }

        return false;
    }
}
