using UnityEngine;

public class MaquinaLixo : MonoBehaviour, IMaquinaRecebedora
{
    public SpriteRenderer spriteRenderer;
    public Color corLixoNormal = Color.gray;
    public Color corLixoLuxo = Color.yellow;

    private string idAtual = "";
    private bool ativo = false;

    void Awake()
    {
        GarantirSprite();
    }

    void GarantirSprite()
    {
        // O spriteRenderer pode vir vazio da cena; resolve pelo componente
        // do próprio objeto para não estourar NullReferenceException.
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Ativar(string id)
    {
        GarantirSprite();

        idAtual = id;
        ativo = true;
        gameObject.SetActive(true);

        if (spriteRenderer != null)
            spriteRenderer.color = id == "lixo_luxo" ? corLixoLuxo : corLixoNormal;
    }

    public bool PodeReceber(PedraManager pedra)
    {
        return ativo;
    }

    // Chamado pelo PedraManager quando a pedra é solta em cima do lixo.
    public void ReceberPedra(PedraManager pedra)
    {
        if (!ativo || pedra == null) return;

        if (idAtual == "lixo_luxo")
        {
            // Recebe 25% do valor de compra da pedra.
            float recompensa = pedra.GetPrecoCompra() * 0.25f;
            GameManager.Instance.AdicionarDinheiro(recompensa);
            Debug.Log("Lixo de Luxo: +" + recompensa);
        }

        Destroy(pedra.gameObject);
    }
}
