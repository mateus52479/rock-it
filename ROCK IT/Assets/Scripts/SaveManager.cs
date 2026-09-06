using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string caminhoSave;
    private float timerAutoSave = 60f;

    void Awake()
    {
        Instance = this;
        caminhoSave = Application.persistentDataPath + "/save.json";
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

    public void Salvar()
    {
        SaveData data = new SaveData();
        data.dinheiro = GameManager.Instance.dinheiro;

        // Salva todas as pedras na cena
        PedraManager[] pedras = FindObjectsByType<PedraManager>(FindObjectsSortMode.None);
        foreach (PedraManager p in pedras)
        {
            PedrasSalva ps = new PedrasSalva();
            ps.nomePedra = p.GetNomePedra();
            ps.posX = p.transform.position.x;
            ps.posY = p.transform.position.y;
            ps.vidaAtual = p.GetVidaAtual();
            data.pedras.Add(ps);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(caminhoSave, json);
        Debug.Log("Jogo salvo!");
    }

    public bool TemSave()
    {
        return File.Exists(caminhoSave);
    }

    public void Carregar()
    {
        if (!TemSave()) return;

        string json = File.ReadAllText(caminhoSave);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        GameManager.Instance.CarregarDinheiro(data.dinheiro);

        // Destroi pedras existentes antes de carregar
        PedraManager[] pedrasExistentes = FindObjectsByType<PedraManager>(FindObjectsSortMode.None);
        foreach (PedraManager p in pedrasExistentes)
            Destroy(p.gameObject);

        // Spawna pedras salvas
        foreach (PedrasSalva ps in data.pedras)
        {
            PedraDados dados = PedraDatabase.Instance.BuscarPorNome(ps.nomePedra);
            if (dados == null) continue;

            Vector3 pos = new Vector3(ps.posX, ps.posY, 0f);
            GameObject novaPedra = Instantiate(
                LojaManager.Instance.prefabPedra, pos, Quaternion.identity
            );
            PedraManager pm = novaPedra.GetComponent<PedraManager>();
            pm.InicializarComVida(dados, ps.vidaAtual);
        }

        Debug.Log("Jogo carregado!");
    }
}