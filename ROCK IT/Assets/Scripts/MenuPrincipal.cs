using UnityEngine;
using UnityEngine.UI;

public class MenuPrincipal : MonoBehaviour
{
    public Button botaoContinuar;

    void Start()
    {
        AtualizarBotaoContinuar();
    }

    public void AtualizarBotaoContinuar()
    {
        if (botaoContinuar != null && SaveManager.Instance != null)
            botaoContinuar.interactable = SaveManager.Instance.ExisteAlgumSlot();
    }

    public void NovoJogo()
    {
        SlotSaveUI.Abrir(SlotSaveUI.Modo.NovoJogo);
    }

    public void ContinuarJogo()
    {
        if (SaveManager.Instance == null || !SaveManager.Instance.ExisteAlgumSlot())
            return;

        SlotSaveUI.Abrir(SlotSaveUI.Modo.Continuar);
    }

    public void AbrirConfiguracoes()
    {
        ConfiguracoesManager.Instance.AbrirConfiguracoes();
    }

    public void AbrirCreditos()
    {
        CreditosUI.Abrir();
    }

    public void Sair()
    {
        Application.Quit();
    }
}
