using UnityEngine;

/// <summary>
/// Troca o desenho da mesa pelo "mesa rock it" em runtime, sem precisar
/// alterar a cena. A mesa antiga era um upscale; o desenho novo vem em
/// 500x500 e é reescalado para manter a mesma largura de jogo.
/// </summary>
public static class MesaVisual
{
    private const string NomeObjeto = "mesa_0";
    private const string CaminhoMesa = "Sprites/Mesa/mesa rock it";

    /// <summary>Largura que a mesa antiga ocupava na cena: 3,92 x 3,7.</summary>
    private const float LarguraMundoAlvo = 14.5f;

    public static void Instalar()
    {
        GameObject mesa = GameObject.Find(NomeObjeto);

        if (mesa == null)
        {
            Debug.LogWarning("MesaVisual: objeto 'mesa_0' não encontrado.");
            return;
        }

        SpriteRenderer desenho = mesa.GetComponent<SpriteRenderer>();

        if (desenho == null)
        {
            Debug.LogWarning("MesaVisual: 'mesa_0' não tem SpriteRenderer.");
            return;
        }

        Sprite sprite = SpriteUtil.CarregarDeResources(CaminhoMesa);

        if (sprite == null)
        {
            Debug.LogWarning("MesaVisual: sprite não encontrado em Resources/" + CaminhoMesa + ".");
            return;
        }

        Bounds visivel = SpriteUtil.ObterBoundsVisiveis(sprite);
        float escala = LarguraMundoAlvo / Mathf.Max(visivel.size.x, 0.0001f);

        // O PNG novo tem margem transparente e o centro do desenho não fica
        // exatamente no pivô. Compensa para a mesa continuar no mesmo lugar.
        Vector3 centroVisivel = visivel.center * escala;
        Vector3 posicaoOriginal = mesa.transform.position;

        desenho.sprite = sprite;
        mesa.transform.localScale = new Vector3(escala, escala, 1f);
        mesa.transform.position = new Vector3(
            posicaoOriginal.x - centroVisivel.x,
            posicaoOriginal.y - centroVisivel.y,
            posicaoOriginal.z
        );
    }
}
