using UnityEngine;
using System.Collections;

public class MaquinaQuebrador : MonoBehaviour
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

    public void Ativar()
    {
        ativo = true;
        gameObject.SetActive(true);
        spriteRenderer.color = corVazia;
    }

    public void ReceberPedra(PedraManager pedra)
    {
        if (!ativo || pedraAtual != null) return;

        pedraAtual = pedra;
        pedra.transform.position = transform.position;
        pedra.BloquearInteracao(true);
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