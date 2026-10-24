using UnityEngine;
using System.Collections;

public class MaquinaQuebrador : MonoBehaviour, IMaquinaRecebedora
{
    public SpriteRenderer spriteRenderer;
    public Color corVazia = Color.white;
    public Color corOcupada = Color.cyan;

    [Header("Dano")]
    [Tooltip("Fatia do dano do jogador aplicada a cada ataque. Assim o quebrador acompanha a progressão.")]
    public float percentualDoDanoDoJogador = 0.2f;

    private bool ativo = false;
    private PedraManager pedraAtual = null;
    private int ataquesPorSegundo = 1;

    void Awake()
    {
        GarantirSprite();
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
        gameObject.SetActive(true);

        if (spriteRenderer != null)
            spriteRenderer.color = corVazia;
    }

    public bool PodeReceber(PedraManager pedra)
    {
        return ativo && pedraAtual == null;
    }

    public void ReceberPedra(PedraManager pedra)
    {
        if (pedra == null || !PodeReceber(pedra)) return;

        GarantirSprite();

        pedraAtual = pedra;
        pedra.transform.position = transform.position;
        pedra.BloquearInteracao(true);

        if (spriteRenderer != null)
            spriteRenderer.color = corOcupada;

        StartCoroutine(QuebrarAutomaticamente());
    }

    IEnumerator QuebrarAutomaticamente()
    {
        while (pedraAtual != null)
        {
            yield return new WaitForSeconds(1f / Mathf.Max(1, ataquesPorSegundo));

            if (pedraAtual == null) break;

            int dano = Mathf.Max(
                1,
                Mathf.RoundToInt(StatusManager.Instance.GetDanoForca() * percentualDoDanoDoJogador)
            );

            bool destruida = pedraAtual.ReceberDanoExterno(dano);
            if (destruida)
            {
                pedraAtual = null;

                if (spriteRenderer != null)
                    spriteRenderer.color = corVazia;
            }
        }
    }

    public void Evoluir()
    {
        ataquesPorSegundo++;
        Debug.Log("Quebrador evoluído: " + ataquesPorSegundo + " ataque(s)/s");
    }

    public int GetAtaquesPorSegundo() { return ataquesPorSegundo; }
}
