using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MaquinaQuebrador : MonoBehaviour, IMaquinaRecebedora
{
    public SpriteRenderer spriteRenderer;
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

    /// <summary>Pedras que estão sendo quebradas agora, na ordem em que entraram.</summary>
    public List<PedraManager> PedrasProcessando
    {
        get { return pedras; }
    }

    void Awake()
    {
        GarantirSprite();
        posicaoBase = transform.position;
    }

    void GarantirSprite()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Ativar()
    {
        GarantirSprite();

        ativo = true;
        posicaoBase = transform.position;
        gameObject.SetActive(true);

        if (spriteRenderer != null)
            spriteRenderer.color = corVazia;
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

        if (spriteRenderer != null)
            spriteRenderer.color = corOcupada;

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

        if (spriteRenderer != null)
            spriteRenderer.color = corVazia;

        transform.position = posicaoBase;
    }

    IEnumerator Tremer()
    {
        float tempo = 0f;

        while (pedras.Count > 0)
        {
            tempo += Time.deltaTime;

            transform.position = posicaoBase + new Vector3(
                Mathf.Sin(tempo * 52f) * amplitudeTremor,
                Mathf.Cos(tempo * 67f) * amplitudeTremor * 0.6f,
                0f);

            yield return null;
        }

        transform.position = posicaoBase;
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
}
