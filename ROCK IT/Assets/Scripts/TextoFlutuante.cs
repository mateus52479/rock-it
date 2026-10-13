using System.Collections;
using UnityEngine;
using TMPro;

public class TextoFlutuante : MonoBehaviour
{
    public TextMeshPro texto;
    public float duracaoVida = 0.8f;
    public float velocidadeSubida = 1.5f;

    public void Inicializar(string conteudo, Color cor, float tamanho)
    {
        texto.text = conteudo;
        texto.color = cor;
        texto.fontSize = tamanho;
        StartCoroutine(Animar());
    }

    IEnumerator Animar()
    {
        float tempo = 0f;
        Vector3 posInicial = transform.position;
        Color corInicial = texto.color;

        while (tempo < duracaoVida)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracaoVida;

            // Sobe
            transform.position = posInicial + Vector3.up * velocidadeSubida * t;

            // Some gradualmente
            float alpha = Mathf.Lerp(1f, 0f, t * t);
            texto.color = new Color(corInicial.r, corInicial.g, corInicial.b, alpha);

            yield return null;
        }

        Destroy(gameObject);
    }
}