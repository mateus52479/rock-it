using UnityEngine;
using System.Collections;

public class MaquinaInversor : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    private bool ativo = false;

    public void Ativar()
    {
        ativo = true;
        gameObject.SetActive(true);
    }

    public void ReceberPedra(PedraManager pedra)
    {
        if (!ativo) return;
        StartCoroutine(InvertirPedra(pedra));
    }

    IEnumerator InvertirPedra(PedraManager pedra)
    {
        pedra.BloquearInteracao(true);
        pedra.transform.position = transform.position;

        // Tremor
        Vector3 pos = transform.position;
        float tempo = 0f;
        while (tempo < 1f)
        {
            float ox = Random.Range(-0.1f, 0.1f);
            float oy = Random.Range(-0.1f, 0.1f);
            pedra.transform.position = pos + new Vector3(ox, oy, 0);
            tempo += Time.deltaTime;
            yield return null;
        }

        pedra.transform.position = pos;
        pedra.InvertерValores();
        pedra.BloquearInteracao(false);

        // Solta por perto
        Rigidbody2D rb = pedra.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direcao = Random.insideUnitCircle.normalized;
            rb.AddForce(direcao * 3f, ForceMode2D.Impulse);
        }
    }
}