using UnityEngine;

/// <summary>
/// Paredes invisíveis em volta da mesa. Sem elas, uma pedra empurrada podia
/// sair da mesa e ficar perdida no cenário. As paredes são criadas em runtime
/// a partir do tamanho do sprite da mesa, sem precisar editar a cena.
/// </summary>
public static class LimiteMesa
{
    private const string NomeObjeto = "LimiteMesa";

    /// <summary>
    /// Quanto da parte de baixo do sprite da mesa são os pés, e não o tampo
    /// (medido no PNG: cerca de 21% da altura). Serve para a parede de baixo
    /// ficar na beirada da mesa em vez de lá embaixo, nos pés.
    /// </summary>
    private const float RecuoPernas = 0.2f;

    /// <summary>Espessura das paredes (bem grossa para nenhuma pedra vazar).</summary>
    private const float EspessuraParede = 1.5f;

    private static Rect area;
    private static bool areaPronta = false;

    public static void Instalar()
    {
        // A checagem é pelo objeto da cena, não por uma flag estática: as
        // paredes somem ao trocar de cena, mas os campos static continuam.
        if (GameObject.Find(NomeObjeto) != null)
            return;

        GameObject mesa = GameObject.Find("mesa_0");

        if (mesa == null)
        {
            Debug.LogWarning("LimiteMesa: objeto 'mesa_0' não encontrado.");
            return;
        }

        SpriteRenderer desenho = mesa.GetComponent<SpriteRenderer>();

        if (desenho == null || desenho.sprite == null)
        {
            Debug.LogWarning("LimiteMesa: 'mesa_0' não tem SpriteRenderer.");
            return;
        }

        Bounds limites = desenho.bounds;
        float pes = limites.size.y * RecuoPernas;

        area = new Rect(
            limites.min.x,
            limites.min.y + pes,
            limites.size.x,
            limites.size.y - pes
        );

        areaPronta = true;

        MontarParedes();
    }

    static void MontarParedes()
    {
        GameObject raiz = new GameObject(NomeObjeto);
        Transform pai = raiz.transform;

        float espessura = EspessuraParede;
        float altura = area.height + espessura * 2f;
        float largura = area.width + espessura * 2f;

        // O lado interno de cada parede fica exatamente na borda da mesa.
        CriarParede(
            pai,
            "Esquerda",
            new Vector2(area.xMin - espessura * 0.5f, area.center.y),
            new Vector2(espessura, altura)
        );

        CriarParede(
            pai,
            "Direita",
            new Vector2(area.xMax + espessura * 0.5f, area.center.y),
            new Vector2(espessura, altura)
        );

        CriarParede(
            pai,
            "Baixo",
            new Vector2(area.center.x, area.yMin - espessura * 0.5f),
            new Vector2(largura, espessura)
        );

        CriarParede(
            pai,
            "Cima",
            new Vector2(area.center.x, area.yMax + espessura * 0.5f),
            new Vector2(largura, espessura)
        );
    }

    static void CriarParede(Transform pai, string nome, Vector2 centro, Vector2 tamanho)
    {
        GameObject parede = new GameObject(nome);
        parede.transform.SetParent(pai, false);
        parede.transform.position = centro;

        // Collider sem Rigidbody2D = parede estática. Nenhum renderer, então
        // ela é completamente invisível.
        BoxCollider2D colisor = parede.AddComponent<BoxCollider2D>();
        colisor.size = tamanho;
    }

    /// <summary>
    /// Prende a posição dentro da mesa. Usado no arrasto, porque mover o
    /// transform na mão passa por cima das paredes.
    /// </summary>
    public static Vector3 Limitar(Vector3 posicao, Vector2 metadeDoObjeto)
    {
        if (!areaPronta)
            return posicao;

        float minX = area.xMin + metadeDoObjeto.x;
        float maxX = area.xMax - metadeDoObjeto.x;

        if (minX <= maxX)
            posicao.x = Mathf.Clamp(posicao.x, minX, maxX);

        float minY = area.yMin + metadeDoObjeto.y;
        float maxY = area.yMax - metadeDoObjeto.y;

        if (minY <= maxY)
            posicao.y = Mathf.Clamp(posicao.y, minY, maxY);

        return posicao;
    }
}
