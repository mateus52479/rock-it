using UnityEngine;

public class MaquinaLixo : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Color corLixoNormal = Color.gray;
    public Color corLixoLuxo = Color.yellow;

    private string idAtual = "";
    private bool ativo = false;

    public void Ativar(string id)
    {
        idAtual = id;
        ativo = true;
        gameObject.SetActive(true);
        spriteRenderer.color = id == "lixo_luxo" ? corLixoLuxo : corLixoNormal;
    }

    // Chamado pelo PedraManager quando a pedra é solta em cima do lixo
    public void ReceberPedra(PedraManager pedra)
    {
        if (!ativo) return;

        if (idAtual == "lixo_luxo")
        {
            // Recebe 25% do valor de compra da pedra
            float recompensa = pedra.GetPrecoCompra() * 0.25f;
            GameManager.Instance.AdicionarDinheiro(recompensa);
            Debug.Log("Lixo de Luxo: +" + recompensa);
        }

        Destroy(pedra.gameObject);
    }
}