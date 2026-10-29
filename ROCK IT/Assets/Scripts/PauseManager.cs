using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    public GameObject painelPause;

    private bool pausado = false;

    /// <summary>Usado pelo tutorial para não ficar por cima do menu de pause.</summary>
    public bool EstaPausado { get { return pausado; } }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        // Se as configurações estão abertas, o Esc fecha tudo: as
        // configurações e o pause, voltando direto para o jogo. Antes ele
        // despausava escondido atrás do painel e deixava o pause na tela.
        if (ConfiguracoesManager.Instance != null &&
            ConfiguracoesManager.Instance.ConfiguracoesAbertas)
        {
            ConfiguracoesManager.Instance.FecharConfiguracoes();
            Retomar();
            return;
        }

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