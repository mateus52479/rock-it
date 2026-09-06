using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float dinheiro = 0f;
    public TextMeshProUGUI textoDinheiro;

    void Awake()
    {
        Instance = this;
    }

    public void AdicionarDinheiro(float valor)
    {
        dinheiro += valor;
        AtualizarHUD();
    }

    public bool GastarDinheiro(float valor)
    {
        if (dinheiro < valor) return false;
        dinheiro -= valor;
        AtualizarHUD();
        return true;
    }

    public void CarregarDinheiro(float valor)
    {
        dinheiro = valor;
        AtualizarHUD();
    }

    void AtualizarHUD()
    {
        if (textoDinheiro != null)
            textoDinheiro.text = "R$ " + dinheiro.ToString("F2");
    }
}