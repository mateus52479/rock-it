using UnityEngine;
using System.Collections;

public class MaquinaQuebrador : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Color corVazia = Color.white;
    public Color corOcupada = Color.cyan;

    private bool ativo = false;
    private PedraManager pedraAtual = null;
    private int danosPorSegundo = 1;

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
            yield return new WaitForSeconds(1f / danosPorSegundo);

            if (pedraAtual == null) break;

            bool destruida = pedraAtual.ReceberDanoExterno(danosPorSegundo);
            if (destruida)
            {
                pedraAtual = null;
                spriteRenderer.color = corVazia;
            }
        }
    }

    public void Evoluir()
    {
        danosPorSegundo++;
        Debug.Log("Quebrador evoluído: " + danosPorSegundo + " dano/s");
    }

    public int GetDanosPorSegundo() { return danosPorSegundo; }
}