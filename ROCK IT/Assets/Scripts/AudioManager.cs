using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/// <summary>
/// Todos os sons do jogo num objeto criado sozinho antes da primeira cena.
/// Nada precisa ser montado no editor: os clipes são carregados de
/// Resources/Musicas e a música troca conforme a cena que está carregada.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private const string PastaClipes = "Musicas/";

    [Header("Músicas")]
    [Tooltip("Música do menu. Se ficar vazio, carrega Resources/Musicas/menu.")]
    public AudioClip musicaMenu;

    [Tooltip("Música do jogo. Se ficar vazio, carrega Resources/Musicas/GameMusic.")]
    public AudioClip musicaJogo;

    [Header("Efeitos")]
    [Tooltip("Barulho da pedra batendo na mesa. Se ficar vazio, carrega Resources/Musicas/PedraCaiu.")]
    public AudioClip clipePedraCaiu;

    [Tooltip("Arrasto da pedra, em looping enquanto ela se move. Se ficar vazio, carrega Resources/Musicas/ArrastandoPedra.")]
    public AudioClip clipeArrastandoPedra;

    [Tooltip("Clique do mouse. Se ficar vazio, carrega Resources/Musicas/MouseClick.")]
    public AudioClip clipeMouseClick;

    [Header("Volumes (0 a 1)")]
    [Range(0f, 1f)] public float volumeMusica = 0.6f;
    [Range(0f, 1f)] public float volumeEfeitos = 0.9f;
    [Range(0f, 1f)] public float volumeArrasto = 0.5f;

    [Tooltip("Volume do clique em relação aos outros efeitos.")]
    [Range(0f, 1f)] public float volumeDoClique = 0.7f;

    private AudioSource fonteMusica;
    private AudioSource fonteArrasto;
    private AudioSource fonteEfeitos;

    /// <summary>Identificação da música que está tocando, para não reiniciar.</summary>
    private string musicaAtual = "";

    /// <summary>Evita procurar o mixer de novo a cada troca de cena.</summary>
    private bool mixerAplicado = false;

    /// <summary>
    /// Cria o AudioManager antes da primeira cena. Assim ele existe tanto no
    /// menu quanto no jogo, sem precisar colocar objeto nenhum nas cenas.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Criar()
    {
        if (Instance != null) return;

        GameObject obj = new GameObject("AudioManager");
        obj.AddComponent<AudioManager>();
        DontDestroyOnLoad(obj);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CarregarClipes();

        // Música em looping, arrasto em looping e um canal para os efeitos
        // avulsos (pedra caindo e clique).
        fonteMusica = CriarFonte("Musica", volumeMusica, true);
        fonteArrasto = CriarFonte("ArrastandoPedra", volumeArrasto, true);
        fonteEfeitos = CriarFonte("Efeitos", volumeEfeitos, false);

        SceneManager.sceneLoaded += AoCarregarCena;
    }

    void Start()
    {
        AplicarMixer();
        TocarMusicaDaCena(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;

        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        // Todo clique do jogo, inclusive em cima de botões e da pedra.
        if (Input.GetMouseButtonDown(0))
            TocarMouseClick();
    }

    AudioSource CriarFonte(string nome, float volume, bool loop)
    {
        GameObject filho = new GameObject(nome);
        filho.transform.SetParent(transform, false);

        AudioSource fonte = filho.AddComponent<AudioSource>();
        fonte.playOnAwake = false;
        fonte.loop = loop;
        fonte.volume = volume;
        fonte.spatialBlend = 0f; // 2D: o som não depende da posição na cena
        return fonte;
    }

    /// <summary>
    /// Liga as fontes nos grupos do AudioMixer usado nas Configurações, para
    /// os sliders de volume continuarem valendo. Sem o mixer, o som toca
    /// direto no Master.
    /// </summary>
    void AplicarMixer()
    {
        if (mixerAplicado)
            return;

        AudioMixer mixer = ConfiguracoesManager.Instance != null
            ? ConfiguracoesManager.Instance.audioMixer
            : null;

        if (mixer == null)
            return;

        fonteMusica.outputAudioMixerGroup = Grupo(mixer, "Musica");
        fonteArrasto.outputAudioMixerGroup = Grupo(mixer, "Efeitos");
        fonteEfeitos.outputAudioMixerGroup = Grupo(mixer, "Efeitos");

        mixerAplicado = true;
    }

    static AudioMixerGroup Grupo(AudioMixer mixer, string nome)
    {
        AudioMixerGroup[] grupos = mixer.FindMatchingGroups(nome);

        return grupos != null && grupos.Length > 0 ? grupos[0] : null;
    }

    void CarregarClipes()
    {
        if (musicaMenu == null)
            musicaMenu = Resources.Load<AudioClip>(PastaClipes + "menu");

        if (musicaJogo == null)
            musicaJogo = Resources.Load<AudioClip>(PastaClipes + "GameMusic");

        if (clipePedraCaiu == null)
            clipePedraCaiu = Resources.Load<AudioClip>(PastaClipes + "PedraCaiu");

        if (clipeArrastandoPedra == null)
            clipeArrastandoPedra = Resources.Load<AudioClip>(PastaClipes + "ArrastandoPedra");

        if (clipeMouseClick == null)
            clipeMouseClick = Resources.Load<AudioClip>(PastaClipes + "MouseClick");
    }

    void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        // Se o jogador sair da cena no meio de um arrasto, o barulho não pode
        // ficar tocando para sempre.
        PararArrasto();

        // A cena nova pode ter o ConfiguracoesManager que faltava na anterior.
        AplicarMixer();

        TocarMusicaDaCena(cena.name);
    }

    void TocarMusicaDaCena(string nomeCena)
    {
        if (nomeCena == "MenuPrincipal")
            TocarMusica(musicaMenu, "menu");
        else if (nomeCena == "Jogo")
            TocarMusica(musicaJogo, "jogo");
    }

    void TocarMusica(AudioClip clipe, string id)
    {
        if (clipe == null || fonteMusica == null)
            return;

        if (musicaAtual == id && fonteMusica.isPlaying)
            return;

        musicaAtual = id;
        fonteMusica.clip = clipe;
        fonteMusica.loop = true;
        fonteMusica.Play();
    }

    public void TocarPedraCaiu()
    {
        TocarEfeito(clipePedraCaiu, 1f);
    }

    public void TocarMouseClick()
    {
        TocarEfeito(clipeMouseClick, volumeDoClique);
    }

    void TocarEfeito(AudioClip clipe, float volume)
    {
        if (clipe == null || fonteEfeitos == null)
            return;

        fonteEfeitos.PlayOneShot(clipe, volume);
    }

    public void IniciarArrasto()
    {
        if (clipeArrastandoPedra == null || fonteArrasto == null)
            return;

        if (fonteArrasto.isPlaying)
            return;

        fonteArrasto.clip = clipeArrastandoPedra;
        fonteArrasto.pitch = Random.Range(0.95f, 1.05f);
        fonteArrasto.Play();
    }

    public void PararArrasto()
    {
        if (fonteArrasto != null && fonteArrasto.isPlaying)
            fonteArrasto.Stop();
    }

    // --- Atalhos usados pelos outros scripts (não precisam checar nulo) ---

    public static void PedraCaiu()
    {
        if (Instance != null)
            Instance.TocarPedraCaiu();
    }

    /// <summary>Liga/desliga o arrasto conforme a pedra está se movendo.</summary>
    public static void Arrasto(bool movendo)
    {
        if (Instance == null)
            return;

        if (movendo)
            Instance.IniciarArrasto();
        else
            Instance.PararArrasto();
    }
}
