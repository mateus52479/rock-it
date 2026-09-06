using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class ConfiguracoesManager : MonoBehaviour
{
    public static ConfiguracoesManager Instance;

    public GameObject painelConfiguracoes;

    [Header("Audio")]
    public Slider sliderVolumeGeral;
    public Slider sliderVolumeMusica;
    public Slider sliderVolumeEfeitos;
    public AudioMixer audioMixer;

    [Header("Video")]
    public TMP_Dropdown dropdownResolucao;
    public Toggle toggleFullscreen;

    [Header("Idioma")]
    public TMP_Dropdown dropdownIdioma;

    private Resolution[] resolucoes;

    void Awake()
    {
        Instance = this;
        painelConfiguracoes.SetActive(false);
    }

    void Start()
    {
        CarregarConfiguracoes();
        PopularResolucoes();
    }

    public void AbrirConfiguracoes()
    {
        painelConfiguracoes.SetActive(true);
    }

    public void FecharConfiguracoes()
    {
        painelConfiguracoes.SetActive(false);
        SalvarConfiguracoes();
    }

    // --- AUDIO ---

    public void MudarVolumeGeral(float valor)
    {
        audioMixer.SetFloat("VolumeGeral", Mathf.Log10(valor) * 20);
    }

    public void MudarVolumeMusica(float valor)
    {
        audioMixer.SetFloat("VolumeMusica", Mathf.Log10(valor) * 20);
    }

    public void MudarVolumeEfeitos(float valor)
    {
        audioMixer.SetFloat("VolumeEfeitos", Mathf.Log10(valor) * 20);
    }

    // --- VIDEO ---

    void PopularResolucoes()
    {
        resolucoes = Screen.resolutions;
        dropdownResolucao.ClearOptions();

        var opcoes = new System.Collections.Generic.List<string>();
        int indiceAtual = 0;

        for (int i = 0; i < resolucoes.Length; i++)
        {
            string opcao = resolucoes[i].width + " x " + resolucoes[i].height;
            opcoes.Add(opcao);
            if (resolucoes[i].width == Screen.currentResolution.width &&
                resolucoes[i].height == Screen.currentResolution.height)
                indiceAtual = i;
        }

        dropdownResolucao.AddOptions(opcoes);
        dropdownResolucao.value = indiceAtual;
        dropdownResolucao.RefreshShownValue();
    }

    public void MudarResolucao(int indice)
    {
        Resolution r = resolucoes[indice];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
    }

    public void MudarFullscreen(bool ativo)
    {
        Screen.fullScreen = ativo;
    }

    // --- IDIOMA ---

    public void MudarIdioma(int indice)
    {
        // 0 = PT-BR, 1 = EN
        string idioma = indice == 0 ? "pt-BR" : "en";
        PlayerPrefs.SetString("Idioma", idioma);
        Debug.Log("Idioma: " + idioma);
        // Sistema de localização vai ser expandido depois
    }

    // --- SAVE DE CONFIGURAÇÕES ---

    void SalvarConfiguracoes()
    {
        PlayerPrefs.SetFloat("VolumeGeral", sliderVolumeGeral.value);
        PlayerPrefs.SetFloat("VolumeMusica", sliderVolumeMusica.value);
        PlayerPrefs.SetFloat("VolumeEfeitos", sliderVolumeEfeitos.value);
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    void CarregarConfiguracoes()
    {
        sliderVolumeGeral.value = PlayerPrefs.GetFloat("VolumeGeral", 1f);
        sliderVolumeMusica.value = PlayerPrefs.GetFloat("VolumeMusica", 1f);
        sliderVolumeEfeitos.value = PlayerPrefs.GetFloat("VolumeEfeitos", 1f);
        toggleFullscreen.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        MudarVolumeGeral(sliderVolumeGeral.value);
        MudarVolumeMusica(sliderVolumeMusica.value);
        MudarVolumeEfeitos(sliderVolumeEfeitos.value);
    }
}