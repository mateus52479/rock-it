
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string caminhoSave;
    private float timerAutoSave = 60f;

    private bool primeiraComumComprada = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        caminhoSave = Path.Combine(
            Application.persistentDataPath,
            "save.json"
        );

        CarregarEstadoInicial();
    }

    void Update()
    {
        timerAutoSave -= Time.deltaTime;

        if (timerAutoSave <= 0f)
        {
            Salvar();
            timerAutoSave = 60f;
        }
    }

    // Carrega o estado salvo sem depender dos outros managers.
    void CarregarEstadoInicial()
    {
        if (!File.Exists(caminhoSave))
        {
            primeiraComumComprada = false;
            return;
        }

        try
        {
            string json = File.ReadAllText(caminhoSave);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            primeiraComumComprada =
                data != null && data.primeiraComumComprada;
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "Erro ao ler o save: " + e.Message
            );
        }
    }

    public void Salvar()
    {
        if (GameManager.Instance == null ||
            StatusManager.Instance == null)
        {
            Debug.LogWarning(
                "SaveManager: não foi possível salvar. " +
                "GameManager ou StatusManager não encontrado."
            );
            return;
        }

        SaveData data = new SaveData();

        // Dinheiro.
        data.dinheiro = GameManager.Instance.dinheiro;

        // Estado da primeira compra.
        data.primeiraComumComprada = primeiraComumComprada;

        // Status do jogador.
        data.status.nivelForca =
            StatusManager.Instance.nivelForca;

        data.status.nivelSorte =
            StatusManager.Instance.nivelSorte;

        data.status.nivelFortuna =
            StatusManager.Instance.nivelFortuna;

        data.status.habilidadesDesbloqueadas =
            StatusManager.Instance
                .GetHabilidadesDesbloqueadas()
                .ToArray();

        // Máquinas compradas.
        if (MaquinaManager.Instance != null)
        {
            data.maquinasCompradas =
                MaquinaManager.Instance.GetMaquinasCompradas();
        }

        // Pedras existentes na cena.
        PedraManager[] pedras =
            FindObjectsByType<PedraManager>(
                FindObjectsSortMode.None
            );

        foreach (PedraManager p in pedras)
        {
            if (p == null)
                continue;

            PedrasSalva ps = new PedrasSalva();

            ps.nomePedra = p.GetNomePedra();
            ps.posX = p.transform.position.x;
            ps.posY = p.transform.position.y;
            ps.vidaAtual = p.GetVidaAtual();

            data.pedras.Add(ps);
        }

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(caminhoSave, json);

            Debug.Log("Jogo salvo em: " + caminhoSave);
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "Erro ao salvar o jogo: " + e.Message
            );
        }
    }

    public bool TemSave()
    {
        return File.Exists(caminhoSave);
    }

    public bool PrimeiraComumComprada()
    {
        return primeiraComumComprada;
    }

    public void MarcarPrimeiraComum()
    {
        if (primeiraComumComprada)
            return;

        primeiraComumComprada = true;

        // Salva imediatamente para não perder o registro
        // caso o jogo seja fechado antes do autosave.
        Salvar();

        Debug.Log("Primeira Pedra Comum registrada no save.");
    }

    public void Carregar()
    {
        if (!TemSave())
        {
            Debug.Log("Nenhum save encontrado.");
            return;
        }

        if (GameManager.Instance == null ||
            StatusManager.Instance == null ||
            PedraDatabase.Instance == null)
        {
            Debug.LogError(
                "Não foi possível carregar: managers necessários ausentes."
            );
            return;
        }

        try
        {
            string json = File.ReadAllText(caminhoSave);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                Debug.LogError("O arquivo de save está inválido.");
                return;
            }

            // Compatibilidade com saves antigos:
            // se o campo não existir no JSON, o padrão será false.
            primeiraComumComprada = data.primeiraComumComprada;

            GameManager.Instance.CarregarDinheiro(data.dinheiro);

            if (data.status != null)
                StatusManager.Instance.CarregarStatus(data.status);

            if (MaquinaManager.Instance != null &&
                data.maquinasCompradas != null)
            {
                MaquinaManager.Instance.CarregarMaquinas(
                    data.maquinasCompradas
                );
            }

            // Remove as pedras atuais.
            PedraManager[] pedrasExistentes =
                FindObjectsByType<PedraManager>(
                    FindObjectsSortMode.None
                );

            foreach (PedraManager p in pedrasExistentes)
            {
                if (p != null)
                    Destroy(p.gameObject);
            }

            // Recria as pedras salvas.
            if (data.pedras != null &&
                LojaManager.Instance != null &&
                LojaManager.Instance.prefabPedra != null)
            {
                foreach (PedrasSalva ps in data.pedras)
                {
                    PedraDados dados =
                        PedraDatabase.Instance.BuscarPorNome(
                            ps.nomePedra
                        );

                    if (dados == null)
                        continue;

                    Vector3 pos = new Vector3(
                        ps.posX,
                        ps.posY,
                        0f
                    );

                    GameObject novaPedra = Instantiate(
                        LojaManager.Instance.prefabPedra,
                        pos,
                        Quaternion.identity
                    );

                    PedraManager pm =
                        novaPedra.GetComponent<PedraManager>();

                    if (pm != null)
                        pm.InicializarComVida(dados, ps.vidaAtual);
                }
            }

            Debug.Log("Save carregado com sucesso.");
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "Erro ao carregar o save: " + e.Message
            );
        }
    }
}
