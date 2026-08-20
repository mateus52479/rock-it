using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PedraManager : MonoBehaviour, IPointerClickHandler
{
    [Header("Referências")]
    public TextMeshProUGUI vidaText;
    public Image imagemPedra;

    [Header("Configurações")]
    public float escalaAumentada = 2.5f;
    public float duracaoTremor = 0.3f;
    public float intensidadeTremor = 15f;

    private PedraDados dados;
    private int vidaAtual;
    private bool estaAtiva = false;
    private bool tremendo = false;
    private RectTransform pedraRect;

    void Start()
    {
        pedraRect = GetComponent<RectTransform>();
    }

    // Chamado pelo LojaManager ao spawnar a pedra
    public void Inicializar(PedraDados d)
    {
        dados = d;
        vidaAtual = d.vidaMaxima;
        imagemPedra.color = d.cor;
        AtualizarTextoVida();
    }

    public void OnPointerClick(PointerEventData eventData)
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
        pedraRect.anchoredPosition = Vector2.zero;
        pedraRect.localScale = Vector3.one * escalaAumentada;
    }

    IEnumerator AplicarDano()
    {
        tremendo = true;
        vidaAtual--;
        AtualizarTextoVida();

        Vector3 posOriginal = pedraRect.localPosition;
        float tempo = 0f;

        while (tempo < duracaoTremor)
        {
            float offsetX = Random.Range(-intensidadeTremor, intensidadeTremor);
            float offsetY = Random.Range(-intensidadeTremor, intensidadeTremor);
            pedraRect.localPosition = posOriginal + new Vector3(offsetX, offsetY, 0);
            tempo += Time.deltaTime;
            yield return null;
        }

        pedraRect.localPosition = posOriginal;
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

        string msg = recompensa >= 0
            ? "+" + recompensa.ToString("C2")
            : recompensa.ToString("C2");
        Debug.Log("Pedra destruída! Recompensa: " + msg);

        gameObject.SetActive(false);
    }
}