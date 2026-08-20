using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float dinheiro = 0f;
    public TextMeshProUGUI textoDinheiro; // arrasta o texto de HUD aqui

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
        if (dinheiro < valor) return false; // sem dinheiro suficiente
        dinheiro -= valor;
        AtualizarHUD();
        return true;
    }

    void AtualizarHUD()
    {
        if (textoDinheiro != null)
            textoDinheiro.text = "R$ " + dinheiro.ToString("F2");
    }
}