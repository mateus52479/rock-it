using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    public GameObject painelPause;

    private bool pausado = false;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            AlternarPause();
    }

    public void AlternarPause()
    {
        pausado = !pausado;
        painelPause.SetActive(pausado);
        Time.timeScale = pausado ? 0f : 1f;
    }

    public void Retomar()
    {
        pausado = false;
        painelPause.SetActive(false);
        Time.timeScale = 1f;
    }

    /// <summary>Chamado pelo botão "Salvar" do menu de pausa.</summary>
    public void Salvar()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Salvar();
    }

    public void SalvarEVoltar()
    {
        SaveManager.Instance.Salvar();
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuPrincipal");
    }

    public void Sair()
    {
        SaveManager.Instance.Salvar();
        Application.Quit();
    }
}