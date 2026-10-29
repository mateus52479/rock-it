using System;
using System.Collections.Generic;
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

    [Header("Arquivos e Animação")]
    [Tooltip("Nome base dos arquivos em Resources/Sprites/Pedras. Vazio = usa o nome da pedra em maiúsculas.")]
    public string nomeArquivo;

    [Tooltip("Quando marcado, carrega um segundo frame (PT 2) para cada estado de vida.")]
    public bool animarFrames;

    [Header("Sprites conforme a vida")]
    public Sprite sprite100;
    public Sprite sprite75;
    public Sprite sprite50;
    public Sprite sprite25;

    [Header("Segundo frame (animação)")]
    public Sprite sprite100B;
    public Sprite sprite75B;
    public Sprite sprite50B;
    public Sprite sprite25B;

    public int SortearValor(int nivelSorte)
    {
        if (tabelaValores == null || tabelaValores.Length == 0)
        {
            Debug.LogError("A pedra " + nome + " não possui tabela de valores.");
            return 0;
        }

        float[] chances = ObterChancesAjustadas(nivelSorte);

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

    /// <summary>
    /// Chances com o deslocamento de Sorte já aplicado. Cada nível tira dois
    /// pontos percentuais dos piores resultados (prejuízo primeiro, depois o
    /// zero) e joga tudo no prêmio máximo. Assim, Sorte alta realmente tira o
    /// prejuízo do caminho, em vez de só melhorar um pouco a pior chance.
    /// </summary>
    public float[] ObterChancesAjustadas(int nivelSorte)
    {
        int total = tabelaValores.Length;
        float[] chances = new float[total];

        for (int i = 0; i < total; i++)
            chances[i] = tabelaValores[i].chance;

        int indiceMelhor = 0;

        for (int i = 1; i < total; i++)
            if (tabelaValores[i].valor > tabelaValores[indiceMelhor].valor)
                indiceMelhor = i;

        // Nunca dá para tirar mais chance do que existe fora do prêmio máximo.
        float disponivel = 0f;

        for (int i = 0; i < total; i++)
            if (i != indiceMelhor)
                disponivel += chances[i];

        float ajuste = Mathf.Min(
            Mathf.Max(0, (nivelSorte - 1) * 2f),
            disponivel
        );

        // Drena do pior para o melhor: primeiro o prejuízo grande, depois o
        // zero e, por fim, os prêmios menores.
        while (ajuste > 0f)
        {
            int indice = -1;

            for (int i = 0; i < total; i++)
            {
                if (i == indiceMelhor || chances[i] <= 0f)
                    continue;

                if (indice < 0 || tabelaValores[i].valor < tabelaValores[indice].valor)
                    indice = i;
            }

            if (indice < 0)
                break;

            float tirada = Mathf.Min(ajuste, chances[indice]);
            chances[indice] -= tirada;
            chances[indiceMelhor] += tirada;
            ajuste -= tirada;
        }

        return chances;
    }

    /// <summary>
    /// Recompensa média já com Sorte (chance) e Fortuna (multiplicador do
    /// lucro) aplicadas. Usado pelo Scanner para dizer se a pedra dá lucro.
    /// </summary>
    public float ValorEsperado(int nivelSorte, float multiplicadorFortuna)
    {
        if (tabelaValores == null || tabelaValores.Length == 0)
            return 0f;

        float[] chances = ObterChancesAjustadas(nivelSorte);
        float total = 0f;

        for (int i = 0; i < tabelaValores.Length; i++)
        {
            float valor = tabelaValores[i].valor;

            if (valor > 0f)
                valor *= multiplicadorFortuna;

            total += valor * chances[i] / 100f;
        }

        return total;
    }
}

/// <summary>
/// Utilidades para medir a área realmente visível de um sprite.
/// Os PNGs das pedras têm uma margem transparente grande em volta do desenho,
/// então usar sprite.bounds (que inclui a margem) deixa a pedra menor do que
/// o esperado e o collider maior do que o desenho.
/// </summary>
public static class SpriteUtil
{
    private static readonly Dictionary<Sprite, Bounds> cacheBounds = new Dictionary<Sprite, Bounds>();
    private static readonly Dictionary<Sprite, Sprite> cacheRecorte = new Dictionary<Sprite, Sprite>();

    /// <summary>
    /// Carrega um sprite de Resources. Muitos PNGs estão no modo "Multiple"
    /// (sprite sheet), então o Load simples pode não devolver nada; nesse caso
    /// pega o primeiro recorte.
    /// </summary>
    public static Sprite CarregarDeResources(string caminho)
    {
        Sprite sprite = Resources.Load<Sprite>(caminho);

        if (sprite != null)
            return sprite;

        Sprite[] recortes = Resources.LoadAll<Sprite>(caminho);

        return recortes != null && recortes.Length > 0 ? recortes[0] : null;
    }

    /// <summary>
    /// Retorna o retângulo que o desenho realmente ocupa, em unidades locais do sprite.
    /// Usa os vértices da malha (Mesh Type: Tight), que seguem o contorno do desenho.
    /// </summary>
    public static Bounds ObterBoundsVisiveis(Sprite sprite)
    {
        if (sprite == null)
            return new Bounds(Vector3.zero, Vector3.one);

        if (cacheBounds.TryGetValue(sprite, out Bounds guardado))
            return guardado;

        Vector2[] vertices = sprite.vertices;
        Bounds resultado;

        if (vertices == null || vertices.Length == 0)
        {
            resultado = sprite.bounds;
        }
        else
        {
            Vector2 minimo = vertices[0];
            Vector2 maximo = vertices[0];

            for (int i = 1; i < vertices.Length; i++)
            {
                minimo = Vector2.Min(minimo, vertices[i]);
                maximo = Vector2.Max(maximo, vertices[i]);
            }

            resultado = new Bounds(
                new Vector3((minimo.x + maximo.x) * 0.5f, (minimo.y + maximo.y) * 0.5f, 0f),
                new Vector3(maximo.x - minimo.x, maximo.y - minimo.y, 0f)
            );
        }

        if (resultado.size.x <= 0.0001f || resultado.size.y <= 0.0001f)
            resultado = sprite.bounds;
        else if (!TamanhoPlausivel(resultado, sprite.bounds))
            resultado = sprite.bounds;

        cacheBounds[sprite] = resultado;
        return resultado;
    }

    /// <summary>
    /// Proteção: se os vértices vierem em uma unidade inesperada (pixels, 0..1 etc.),
    /// o resultado seria absurdo. Nesse caso voltamos para sprite.bounds.
    /// </summary>
    private static bool TamanhoPlausivel(Bounds medido, Bounds referencia)
    {
        float proporcaoX = medido.size.x / Mathf.Max(referencia.size.x, 0.0001f);
        float proporcaoY = medido.size.y / Mathf.Max(referencia.size.y, 0.0001f);

        return proporcaoX >= 0.05f && proporcaoX <= 1.25f
            && proporcaoY >= 0.05f && proporcaoY <= 1.25f;
    }

    /// <summary>
    /// Cria (e guarda em cache) uma versão do sprite já recortada na área visível.
    /// Usado na UI para o desenho preencher o slot em vez de ficar cercado de transparência.
    /// </summary>
    public static Sprite RecortarParaVisivel(Sprite sprite)
    {
        if (sprite == null)
            return null;

        if (cacheRecorte.TryGetValue(sprite, out Sprite emCache))
            return emCache;

        // Com Mesh Type: Tight, o próprio importador já grava em textureRect o
        // retângulo justo do desenho. Somar o pivô de novo deslocava o recorte
        // e cortava a pedra pela metade na loja.
        Rect area = sprite.textureRect;

        if (area.width <= 0f || area.height <= 0f)
            return sprite;

        Sprite recortado = Sprite.Create(
            sprite.texture,
            area,
            new Vector2(0.5f, 0.5f),
            sprite.pixelsPerUnit,
            0,
            SpriteMeshType.FullRect
        );

        recortado.name = sprite.name + " (recorte)";
        cacheRecorte[sprite] = recortado;
        return recortado;
    }
}
