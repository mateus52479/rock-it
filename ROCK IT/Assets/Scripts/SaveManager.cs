
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Resumo de um slot para a tela de seleção (não vai para o JSON).
/// </summary>
public struct ResumoSlot
{
    public bool existe;
    public float dinheiro;
    public int nivelForca;
    public System.DateTime data;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    public const int TotalSlots = 3;

    /// <summary>Slot usado para salvar e carregar. Escolhido no menu.</summary>
    public static int slotAtivo = 1;

    /// <summary>Verdadeiro quando o jogador entrou por "Novo Jogo".</summary>
    public static bool jogoNovo = false;

    private float timerAutoSave = 60f;

    private bool primeiraComumComprada = false;

    private static bool migracaoFeita = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        MigrarSaveAntigo();
        CarregarEstadoInicial();

        // Partida nova: grava o slot logo no começo. Sem isso, se o jogador
        // fechar o jogo antes da primeira compra, o slot escolhido nem existia
        // e a partida se perdia.
        if (jogoNovo)
            timerAutoSave = 2f;
    }

    void Update()
    {
        // No menu principal não existe partida para salvar.
        if (GameManager.Instance == null)
            return;

        timerAutoSave -= Time.deltaTime;

        if (timerAutoSave <= 0f)
        {
            Salvar();
            timerAutoSave = 60f;
        }
    }

    // --- CAMINHOS ---

    public static string CaminhoSlot(int slot)
    {
        return Path.Combine(
            Application.persistentDataPath,
            "slot_" + slot + ".json"
        );
    }

    string CaminhoSaveAtual()
    {
        return CaminhoSlot(slotAtivo);
    }

    // --- MIGRAÇÃO DO SAVE ANTIGO ---

    /// <summary>
    /// O jogo antigo tinha um único save.json. Na primeira execução ele vira
    /// o slot 1 e o arquivo original é renomeado para backup.
    /// </summary>
    void MigrarSaveAntigo()
    {
        if (migracaoFeita) return;
        migracaoFeita = true;

        string antigo = Path.Combine(Application.persistentDataPath, "save.json");
        string destino = CaminhoSlot(1);
        string backup = Path.Combine(Application.persistentDataPath, "save_backup.json");

        try
        {
            if (!File.Exists(antigo)) return;

            if (!File.Exists(destino))
                File.Copy(antigo, destino);

            // Guarda uma cópia e remove o nome antigo para não migrar de novo
            // caso o slot 1 seja apagado depois.
            File.Copy(antigo, backup, true);
            File.Delete(antigo);

            Debug.Log("Save antigo migrado para o slot 1.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao migrar o save antigo: " + e.Message);
        }
    }

    // --- ESTADO INICIAL ---

    // Carrega o estado salvo sem depender dos outros managers.
    void CarregarEstadoInicial()
    {
        string caminho = CaminhoSaveAtual();

        if (!File.Exists(caminho))
        {
            primeiraComumComprada = false;
            return;
        }

        try
        {
            string json = File.ReadAllText(caminho);
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

    // --- SLOTS ---

    public bool ExisteSlot(int slot)
    {
        return File.Exists(CaminhoSlot(slot));
    }

    public bool ExisteAlgumSlot()
    {
        for (int i = 1; i <= TotalSlots; i++)
            if (ExisteSlot(i))
                return true;

        return false;
    }

    public ResumoSlot LerResumo(int slot)
    {
        string caminho = CaminhoSlot(slot);

        if (!File.Exists(caminho))
            return new ResumoSlot { existe = false };

        try
        {
            string json = File.ReadAllText(caminho);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            return new ResumoSlot
            {
                existe = true,
                dinheiro = data != null ? data.dinheiro : 0f,
                nivelForca = data != null && data.status != null ? data.status.nivelForca : 1,
                data = File.GetLastWriteTime(caminho)
            };
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao ler o resumo do slot " + slot + ": " + e.Message);

            return new ResumoSlot
            {
                existe = true,
                nivelForca = 1,
                data = File.GetLastWriteTime(caminho)
            };
        }
    }

    public void ApagarSlot(int slot)
    {
        string caminho = CaminhoSlot(slot);

        try
        {
            if (File.Exists(caminho))
                File.Delete(caminho);

            Debug.Log("Slot " + slot + " apagado.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao apagar o slot " + slot + ": " + e.Message);
        }
    }

    /// <summary>Apaga o slot e marca a partida como nova.</summary>
    public void IniciarNovoJogo(int slot)
    {
        slotAtivo = slot;
        jogoNovo = true;
        primeiraComumComprada = false;
        ApagarSlot(slot);
    }

    /// <summary>Marca que o jogador vai continuar um slot existente.</summary>
    public void IniciarContinuacao(int slot)
    {
        slotAtivo = slot;
        jogoNovo = false;
    }

    // --- SALVAR / CARREGAR ---

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
            File.WriteAllText(CaminhoSaveAtual(), json);

            Debug.Log("Jogo salvo no slot " + slotAtivo + ": " + CaminhoSaveAtual());
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
        return ExisteAlgumSlot();
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
        string caminho = CaminhoSaveAtual();

        if (!File.Exists(caminho))
        {
            Debug.Log("Nenhum save encontrado no slot " + slotAtivo + ".");
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
            string json = File.ReadAllText(caminho);
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
                    {
                        pm.InicializarComVida(dados, ps.vidaAtual);

                        // O Scanner precisa marcar as pedras recriadas.
                        MaquinaScanner.NotificarNovaPedra(pm);
                    }
                }
            }

            Debug.Log("Save carregado com sucesso do slot " + slotAtivo + ".");
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "Erro ao carregar o save: " + e.Message
            );
        }
    }
}
