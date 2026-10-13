using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float dinheiro = 0f;
    public TextMeshProUGUI textoDinheiro;
    public TextMeshProUGUI textoDiferenca; // texto do +X ou -X ao lado

    private float dinheiroExibido = 0f;
    private float velocidadeContador = 5f; // quão rápido o contador sobe/desce
    private Coroutine corotinaContador;
    private Coroutine corotinaDiferenca;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Mathf.Abs(dinheiroExibido - dinheiro) > 0.01f)
        {
            float velocidade = Mathf.Abs(dinheiro - dinheiroExibido) / 0.8f; // completa em 0.8 segundos
            velocidade = Mathf.Max(velocidade, 10f); // velocidade mínima de 1 por segundo
            dinheiroExibido = Mathf.MoveTowards(dinheiroExibido, dinheiro, velocidade * Time.deltaTime);
            textoDinheiro.text = "R$ " + dinheiroExibido.ToString("F2");
        }
        else
        {
            dinheiroExibido = dinheiro;
            textoDinheiro.text = "R$ " + dinheiro.ToString("F2");
        }
    }

    public void AdicionarDinheiro(float valor)
    {
        dinheiro += valor;
        MostrarDiferenca(valor);
    }

    public bool GastarDinheiro(float valor)
    {
        if (dinheiro < valor) return false;
        dinheiro -= valor;
        MostrarDiferenca(-valor);
        return true;
    }

    public void CarregarDinheiro(float valor)
    {
        dinheiro = valor;
        dinheiroExibido = valor;
        AtualizarHUD();
    }

    void AtualizarHUD()
    {
        if (textoDinheiro != null)
            textoDinheiro.text = "R$ " + dinheiro.ToString("F2");
    }

    void MostrarDiferenca(float valor)
    {
        if (textoDiferenca == null) return;

        if (corotinaDiferenca != null)
            StopCoroutine(corotinaDiferenca);

        corotinaDiferenca = StartCoroutine(AnimarDiferenca(valor));
    }

    IEnumerator AnimarDiferenca(float valor)
    {
        textoDiferenca.gameObject.SetActive(true);

        if (valor >= 0)
        {
            textoDiferenca.text = "+" + valor.ToString("F2");
            textoDiferenca.color = new Color(0.2f, 0.9f, 0.2f, 1f);
        }
        else
        {
            textoDiferenca.text = valor.ToString("F2");
            textoDiferenca.color = new Color(0.9f, 0.2f, 0.2f, 1f);
        }

        // Aparece por 1.5 segundos depois some gradualmente
        yield return new WaitForSeconds(1.5f);

        float tempo = 0f;
        float duracao = 0.5f;
        Color corInicial = textoDiferenca.color;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, tempo / duracao);
            textoDiferenca.color = new Color(corInicial.r, corInicial.g, corInicial.b, alpha);
            yield return null;
        }

        textoDiferenca.gameObject.SetActive(false);
    }
}