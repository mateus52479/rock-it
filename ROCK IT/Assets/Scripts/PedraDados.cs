using System;
using UnityEngine;

[Serializable]
public class ValorPedra
{
    public int valor;
    public float chance; // Probabilidade entre 0 e 100
}

[Serializable]
public class PedraDados
{
    public string nome;
    public string descricao;
    public Color cor;
    public int precoCompra;
    public int vidaMaxima;
    public ValorPedra[] tabelaValores;

    [Header("Sprites conforme a vida")]
    public Sprite sprite100;
    public Sprite sprite75;
    public Sprite sprite50;
    public Sprite sprite25;

    public int SortearValor(int nivelSorte)
    {
        if (tabelaValores == null || tabelaValores.Length == 0)
        {
            Debug.LogError("A pedra " + nome + " não possui tabela de valores.");
            return 0;
        }

        float[] chances = new float[tabelaValores.Length];
        float ajuste = Mathf.Max(0, nivelSorte - 1) * 1f;

        for (int i = 0; i < tabelaValores.Length; i++)
            chances[i] = tabelaValores[i].chance;

        // Mantém a regra anterior: transfere a chance do último resultado <= 0
        // para o primeiro resultado positivo, sem alterar a tabela original.
        int ultimoNaoPositivo = -1;
        int primeiroPositivo = -1;

        for (int i = 0; i < tabelaValores.Length; i++)
        {
            if (tabelaValores[i].valor <= 0)
                ultimoNaoPositivo = i;
            else if (primeiroPositivo == -1)
                primeiroPositivo = i;
        }

        if (ultimoNaoPositivo >= 0 && primeiroPositivo >= 0)
        {
            float ajusteReal = Mathf.Min(ajuste, chances[ultimoNaoPositivo]);
            chances[ultimoNaoPositivo] -= ajusteReal;
            chances[primeiroPositivo] += ajusteReal;
        }

        float roll = UnityEngine.Random.Range(0f, 100f);
        float acumulado = 0f;

        for (int i = 0; i < tabelaValores.Length; i++)
        {
            acumulado += chances[i];
            if (roll < acumulado)
                return tabelaValores[i].valor;
        }

        return tabelaValores[tabelaValores.Length - 1].valor;
    }
}
