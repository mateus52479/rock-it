using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

/// <summary>
/// Liga os controles das Configurações, aplica na hora e guarda tudo em
/// PlayerPrefs. Os controles da cena não têm eventos para resolução, tela
/// cheia e idioma, então as ligações são feitas aqui no Start.
/// </summary>
public class ConfiguracoesManager : MonoBehaviour
{
    public static ConfiguracoesManager Instance;

    private const string ChaveVolumeGeral = "VolumeGeral";
    private const string ChaveVolumeMusica = "VolumeMusica";
    private const string ChaveVolumeEfeitos = "VolumeEfeitos";
    private const string ChaveFullscreen = "Fullscreen";
    private const string ChaveIdioma = "Idioma";
    private const string ChaveResolucaoLargura = "ResolucaoLargura";
    private const string ChaveResolucaoAltura = "ResolucaoAltura";

    [Header("Paineis")]
    public GameObject painelConfiguracoes;
    public GameObject painelMenu;

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

    private Resolution[] resolucoes = new Resolution[0];
    private bool controlesLigados;

    private TextMeshProUGUI rotuloVolumeGeral;
    private TextMeshProUGUI rotuloVolumeMusica;
    private TextMeshProUGUI rotuloVolumeEfeitos;

    /// <summary>
    /// A resolução e a tela cheia só precisam ser aplicadas uma vez por
    /// execução; trocar de cena não deve piscar a tela.
    /// </summary>
    private static bool telaInicialAplicada;

    void Awake()
    {
        Instance = this;

        if (painelConfiguracoes != null)
            painelConfiguracoes.SetActive(false);
    }

    void Start()
    {
        ResolverReferencias();
        ResolverRotulosVolume();
        AjustarScrollResolucoes();
        PopularIdiomas();
        CarregarConfiguracoes();
        LigarControles();
        AtualizarRotulosVolume();

        // O idioma pode ser trocado pelo dropdown; quem monta rótulos por
        // código precisa saber para reescrevê-los em outro idioma.
        Traducao.OnIdiomaMudou += AoMudarIdioma;
    }

    void OnDestroy()
    {
        Traducao.OnIdiomaMudou -= AoMudarIdioma;

        if (Instance == this)
            Instance = null;
    }

    void AoMudarIdioma()
    {
        SincronizarControles();
    }

    void OnApplicationQuit()
    {
        SalvarConfiguracoes();
    }

    public void AbrirConfiguracoes()
    {
        if (painelConfiguracoes == null)
        {
            Debug.LogWarning("ConfiguracoesManager: painel de configurações não configurado.");
            return;
        }

        // Os controles podem ter sido abertos em outra cena ou ficado com
        // valores antigos; relê o que está salvo antes de mostrar.
        if (resolucoes.Length == 0)
            PopularResolucoes();

        SincronizarControles();

        painelConfiguracoes.SetActive(true);

        if (painelMenu != null)
            painelMenu.SetActive(false);
    }

    public void FecharConfiguracoes()
    {
        SalvarConfiguracoes();

        if (painelConfiguracoes != null)
            painelConfiguracoes.SetActive(false);

        if (painelMenu != null)
            painelMenu.SetActive(true);
    }

    // --- Áudio ----------------------------------------------------------

    public void MudarVolumeGeral(float valor)
    {
        valor = Mathf.Clamp01(valor);
        AplicarVolume("VolumeGeral", valor);
        PlayerPrefs.SetFloat(ChaveVolumeGeral, valor);
        AtualizarRotuloVolume(rotuloVolumeGeral, "Volume Geral", valor);
    }

    public void MudarVolumeMusica(float valor)
    {
        valor = Mathf.Clamp01(valor);
        AplicarVolume("VolumeMusica", valor);
        PlayerPrefs.SetFloat(ChaveVolumeMusica, valor);
        AtualizarRotuloVolume(rotuloVolumeMusica, "Volume Música", valor);
    }

    public void MudarVolumeEfeitos(float valor)
    {
        valor = Mathf.Clamp01(valor);
        AplicarVolume("VolumeEfeitos", valor);
        PlayerPrefs.SetFloat(ChaveVolumeEfeitos, valor);
        AtualizarRotuloVolume(rotuloVolumeEfeitos, "Volume Efeitos", valor);
    }

    void AplicarVolume(string parametro, float valor)
    {
        if (audioMixer == null)
            return;

        // Log10(0) dá -infinito; -80 dB é o silêncio do mixer.
        float decibeis = valor <= 0.0001f ? -80f : Mathf.Log10(valor) * 20f;
        audioMixer.SetFloat(parametro, decibeis);
    }

    void AplicarTodosOsVolumes()
    {
        AplicarVolume("VolumeGeral", sliderVolumeGeral != null
            ? sliderVolumeGeral.value
            : PlayerPrefs.GetFloat(ChaveVolumeGeral, 1f));

        AplicarVolume("VolumeMusica", sliderVolumeMusica != null
            ? sliderVolumeMusica.value
            : PlayerPrefs.GetFloat(ChaveVolumeMusica, 1f));

        AplicarVolume("VolumeEfeitos", sliderVolumeEfeitos != null
            ? sliderVolumeEfeitos.value
            : PlayerPrefs.GetFloat(ChaveVolumeEfeitos, 1f));
    }

    // --- Vídeo ----------------------------------------------------------

    /// <summary>
    /// A lista de resoluções do dropdown usa um ScrollRect próprio. Deixa a
    /// roda do mouse na mesma velocidade das listas da loja.
    /// </summary>
    void AjustarScrollResolucoes()
    {
        if (dropdownResolucao == null || dropdownResolucao.template == null)
            return;

        ScrollRect scroll = dropdownResolucao.template
            .GetComponentInChildren<ScrollRect>(true);

        if (scroll != null)
            scroll.scrollSensitivity = TemaUI.SensibilidadeScroll;
    }

    void PopularResolucoes()
    {
        if (dropdownResolucao == null)
            return;

        List<Resolution> unicas = new List<Resolution>();
        HashSet<long> vistas = new HashSet<long>();

        foreach (Resolution r in Screen.resolutions)
        {
            if (r.width <= 0 || r.height <= 0)
                continue;

            // Screen.resolutions repete a mesma resolução para cada taxa de
            // atualização; o menu mostra só uma de cada.
            long chave = ((long)r.width << 32) | (uint)r.height;

            if (vistas.Add(chave))
                unicas.Add(r);
        }

        if (unicas.Count == 0)
            unicas.Add(Screen.currentResolution);

        unicas.Sort((a, b) =>
        {
            if (a.width != b.width)
                return a.width.CompareTo(b.width);

            return a.height.CompareTo(b.height);
        });

        resolucoes = unicas.ToArray();

        List<string> opcoes = new List<string>();

        foreach (Resolution r in resolucoes)
            opcoes.Add(r.width + " × " + r.height);

        dropdownResolucao.ClearOptions();
        dropdownResolucao.AddOptions(opcoes);
        dropdownResolucao.SetValueWithoutNotify(IndiceResolucaoSalva());
        dropdownResolucao.RefreshShownValue();
    }

    int IndiceResolucaoSalva()
    {
        if (resolucoes == null || resolucoes.Length == 0)
            return 0;

        int larguraSalva = PlayerPrefs.GetInt(ChaveResolucaoLargura, 0);
        int alturaSalva = PlayerPrefs.GetInt(ChaveResolucaoAltura, 0);

        if (larguraSalva > 0 && alturaSalva > 0)
        {
            for (int i = 0; i < resolucoes.Length; i++)
            {
                if (resolucoes[i].width == larguraSalva &&
                    resolucoes[i].height == alturaSalva)
                    return i;
            }
        }

        for (int i = 0; i < resolucoes.Length; i++)
        {
            if (resolucoes[i].width == Screen.width &&
                resolucoes[i].height == Screen.height)
                return i;
        }

        return Mathf.Clamp(resolucoes.Length - 1, 0, resolucoes.Length - 1);
    }

    public void MudarResolucao(int indice)
    {
        if (resolucoes == null || indice < 0 || indice >= resolucoes.Length)
            return;

        Resolution r = resolucoes[indice];

        Screen.SetResolution(r.width, r.height, Screen.fullScreen);

        PlayerPrefs.SetInt(ChaveResolucaoLargura, r.width);
        PlayerPrefs.SetInt(ChaveResolucaoAltura, r.height);
        PlayerPrefs.Save();
    }

    public void MudarFullscreen(bool ativo)
    {
        Screen.fullScreen = ativo;

        PlayerPrefs.SetInt(ChaveFullscreen, ativo ? 1 : 0);
        PlayerPrefs.Save();
    }

    // --- Idioma ---------------------------------------------------------

    void PopularIdiomas()
    {
        if (dropdownIdioma == null)
            return;

        List<string> opcoes = new List<string>
        {
            "Português (Brasil)",
            "English"
        };

        dropdownIdioma.ClearOptions();
        dropdownIdioma.AddOptions(opcoes);
        dropdownIdioma.SetValueWithoutNotify(IndiceIdiomaSalvo());
        dropdownIdioma.RefreshShownValue();
    }

    int IndiceIdiomaSalvo()
    {
        return Traducao.Ingles ? 1 : 0;
    }

    public void MudarIdioma(int indice)
    {
        // Traduz na hora, salva em PlayerPrefs e repinta tudo que está aberto.
        Traducao.DefinirIdioma(indice == 1 ? "en" : "pt-BR");
    }

    // --- Carregar, ligar e salvar ---------------------------------------

    void CarregarConfiguracoes()
    {
        // Na primeira execução mantém o estado atual (no editor, janela) em
        // vez de forçar tela cheia.
        bool telaCheia = PlayerPrefs.HasKey(ChaveFullscreen)
            ? PlayerPrefs.GetInt(ChaveFullscreen) == 1
            : Screen.fullScreen;

        if (sliderVolumeGeral != null)
        {
            sliderVolumeGeral.SetValueWithoutNotify(
                PlayerPrefs.GetFloat(ChaveVolumeGeral, 1f));
        }

        if (sliderVolumeMusica != null)
        {
            sliderVolumeMusica.SetValueWithoutNotify(
                PlayerPrefs.GetFloat(ChaveVolumeMusica, 1f));
        }

        if (sliderVolumeEfeitos != null)
        {
            sliderVolumeEfeitos.SetValueWithoutNotify(
                PlayerPrefs.GetFloat(ChaveVolumeEfeitos, 1f));
        }

        if (toggleFullscreen != null)
            toggleFullscreen.SetIsOnWithoutNotify(telaCheia);

        PopularResolucoes();
        AplicarTodosOsVolumes();

        if (!telaInicialAplicada)
        {
            telaInicialAplicada = true;
            Screen.fullScreen = telaCheia;

            if (resolucoes.Length > 0)
            {
                int indice = IndiceResolucaoSalva();
                Resolution r = resolucoes[indice];

                if (PlayerPrefs.HasKey(ChaveResolucaoLargura) &&
                    PlayerPrefs.HasKey(ChaveResolucaoAltura))
                {
                    Screen.SetResolution(r.width, r.height, telaCheia);
                }
            }
        }
    }

    void SincronizarControles()
    {
        // Na primeira execução mantém o estado atual (no editor, janela) em
        // vez de forçar tela cheia.
        bool telaCheia = PlayerPrefs.HasKey(ChaveFullscreen)
            ? PlayerPrefs.GetInt(ChaveFullscreen) == 1
            : Screen.fullScreen;

        if (sliderVolumeGeral != null)
        {
            sliderVolumeGeral.SetValueWithoutNotify(
                PlayerPrefs.GetFloat(ChaveVolumeGeral, 1f));
        }

        if (sliderVolumeMusica != null)
        {
            sliderVolumeMusica.SetValueWithoutNotify(
                PlayerPrefs.GetFloat(ChaveVolumeMusica, 1f));
        }

        if (sliderVolumeEfeitos != null)
        {
            sliderVolumeEfeitos.SetValueWithoutNotify(
                PlayerPrefs.GetFloat(ChaveVolumeEfeitos, 1f));
        }

        if (toggleFullscreen != null)
            toggleFullscreen.SetIsOnWithoutNotify(telaCheia);

        if (dropdownResolucao != null)
        {
            dropdownResolucao.SetValueWithoutNotify(IndiceResolucaoSalva());
            dropdownResolucao.RefreshShownValue();
        }

        if (dropdownIdioma != null)
        {
            dropdownIdioma.SetValueWithoutNotify(IndiceIdiomaSalvo());
            dropdownIdioma.RefreshShownValue();
        }

        AplicarTodosOsVolumes();
        AtualizarRotulosVolume();
    }

    void LigarControles()
    {
        if (controlesLigados)
            return;

        controlesLigados = true;

        LigarSlider(sliderVolumeGeral, MudarVolumeGeral);
        LigarSlider(sliderVolumeMusica, MudarVolumeMusica);
        LigarSlider(sliderVolumeEfeitos, MudarVolumeEfeitos);

        if (dropdownResolucao != null)
        {
            dropdownResolucao.onValueChanged.RemoveListener(MudarResolucao);
            dropdownResolucao.onValueChanged.AddListener(MudarResolucao);
        }

        if (toggleFullscreen != null)
        {
            toggleFullscreen.onValueChanged.RemoveListener(MudarFullscreen);
            toggleFullscreen.onValueChanged.AddListener(MudarFullscreen);
        }

        if (dropdownIdioma != null)
        {
            dropdownIdioma.onValueChanged.RemoveListener(MudarIdioma);
            dropdownIdioma.onValueChanged.AddListener(MudarIdioma);
        }
    }

    void LigarSlider(Slider slider, UnityEngine.Events.UnityAction<float> acao)
    {
        if (slider == null)
            return;

        // Os sliders da cena do Jogo têm chamadas persistentes com alvo vazio
        // (fileID 0): elas contam como "já ligado", mas não chamam nada. Por
        // isso desliga as persistentes e usa sempre o listener daqui.
        int persistentes = slider.onValueChanged.GetPersistentEventCount();

        for (int i = 0; i < persistentes; i++)
        {
            slider.onValueChanged.SetPersistentListenerState(
                i, UnityEngine.Events.UnityEventCallState.Off);
        }

        slider.onValueChanged.RemoveListener(acao);
        slider.onValueChanged.AddListener(acao);
    }

    void ResolverReferencias()
    {
        if (sliderVolumeGeral == null)
            sliderVolumeGeral = BuscarSlider("SliderVolumeGeral");

        if (sliderVolumeMusica == null)
            sliderVolumeMusica = BuscarSlider("SliderVolumeMusica");

        if (sliderVolumeEfeitos == null)
            sliderVolumeEfeitos = BuscarSlider("SliderVolumeEfeitos");

        if (dropdownResolucao == null)
            dropdownResolucao = BuscarDropdown("DropdownResolucao");

        if (dropdownIdioma == null)
            dropdownIdioma = BuscarDropdown("DropdownIdioma");

        if (toggleFullscreen == null)
        {
            foreach (Toggle t in FindObjectsByType<Toggle>(
                FindObjectsInactive.Include))
            {
                if (t.name == "ToggleFullscreen")
                {
                    toggleFullscreen = t;
                    break;
                }
            }
        }
    }

    Slider BuscarSlider(string nome)
    {
        foreach (Slider s in FindObjectsByType<Slider>(
            FindObjectsInactive.Include))
        {
            if (s.name == nome)
                return s;
        }

        return null;
    }

    TMP_Dropdown BuscarDropdown(string nome)
    {
        foreach (TMP_Dropdown d in FindObjectsByType<TMP_Dropdown>(
            FindObjectsInactive.Include))
        {
            if (d.name == nome)
                return d;
        }

        return null;
    }

    void ResolverRotulosVolume()
    {
        if (painelConfiguracoes == null)
            return;

        foreach (TextMeshProUGUI texto in
            painelConfiguracoes.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            // Alguns objetos da cena têm aspas no próprio nome.
            string nome = texto.gameObject.name.Trim().Trim('"');

            if (nome == "Volume Geral")
                rotuloVolumeGeral = texto;
            else if (nome == "Volume Efeitos")
                rotuloVolumeEfeitos = texto;
            else if (nome.StartsWith("Volume"))
                rotuloVolumeMusica = texto;
        }
    }

    void AtualizarRotulosVolume()
    {
        if (rotuloVolumeGeral != null && sliderVolumeGeral != null)
            AtualizarRotuloVolume(rotuloVolumeGeral, "Volume Geral", sliderVolumeGeral.value);

        if (rotuloVolumeMusica != null && sliderVolumeMusica != null)
            AtualizarRotuloVolume(rotuloVolumeMusica, "Volume Música", sliderVolumeMusica.value);

        if (rotuloVolumeEfeitos != null && sliderVolumeEfeitos != null)
            AtualizarRotuloVolume(rotuloVolumeEfeitos, "Volume Efeitos", sliderVolumeEfeitos.value);
    }

    void AtualizarRotuloVolume(TextMeshProUGUI rotulo, string nome, float valor)
    {
        if (rotulo == null)
            return;

        int porcentagem = Mathf.RoundToInt(Mathf.Clamp01(valor) * 100f);

        rotulo.text = Traducao.T(nome) + "  <color=#E8C979>" + porcentagem + "%</color>";
    }

    public void SalvarConfiguracoes()
    {
        if (sliderVolumeGeral != null)
            PlayerPrefs.SetFloat(ChaveVolumeGeral, sliderVolumeGeral.value);

        if (sliderVolumeMusica != null)
            PlayerPrefs.SetFloat(ChaveVolumeMusica, sliderVolumeMusica.value);

        if (sliderVolumeEfeitos != null)
            PlayerPrefs.SetFloat(ChaveVolumeEfeitos, sliderVolumeEfeitos.value);

        if (toggleFullscreen != null)
            PlayerPrefs.SetInt(ChaveFullscreen, toggleFullscreen.isOn ? 1 : 0);

        if (dropdownIdioma != null)
            PlayerPrefs.SetString(ChaveIdioma, dropdownIdioma.value == 1 ? "en" : "pt-BR");

        if (dropdownResolucao != null &&
            resolucoes != null &&
            dropdownResolucao.value >= 0 &&
            dropdownResolucao.value < resolucoes.Length)
        {
            Resolution r = resolucoes[dropdownResolucao.value];

            PlayerPrefs.SetInt(ChaveResolucaoLargura, r.width);
            PlayerPrefs.SetInt(ChaveResolucaoAltura, r.height);
        }

        PlayerPrefs.Save();
    }
}
