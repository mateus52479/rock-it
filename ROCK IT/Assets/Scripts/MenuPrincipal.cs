using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipal : MonoBehaviour
{
    public Button botaoContinuar;

    void Start()
    {
        // Desativa continuar se não tiver save
        if (botaoContinuar != null)
            botaoContinuar.interactable = SaveManager.Instance.TemSave();
    }

    public void NovoJogo()
    {
        SceneManager.LoadScene("Jogo");
    }

    public void ContinuarJogo()
    {
        SceneManager.LoadScene("Jogo");
        // O carregamento acontece na cena do jogo
    }

    public void AbrirConfiguracoes()
    {
        ConfiguracoesManager.Instance.AbrirConfiguracoes();
    }

    public void AbrirCreditos()
    {
        // Implementar depois
        Debug.Log("Créditos");
    }

    public void Sair()
    {
        Application.Quit();
    }
}