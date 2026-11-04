using UnityEngine;

/// <summary>
/// Carrega o roteiro do tutorial e começa a tocar quando a partida é nova.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    void Start()
    {
        // Roda quando o jogador escolheu "Novo Jogo" no menu. Também roda se
        // a cena Jogo for aberta direto no editor sem nenhum save, para
        // facilitar o teste.
        bool partidaNova = SaveManager.jogoNovo;

        if (!partidaNova &&
            SaveManager.Instance != null &&
            !SaveManager.Instance.ExisteAlgumSlot())
        {
            partidaNova = true;
        }

        if (!partidaNova)
        {
            Destroy(gameObject);
            return;
        }

        SaveManager.jogoNovo = false;

        TextAsset arquivo = Resources.Load<TextAsset>("Dialogos/tutorial");

        if (arquivo == null)
        {
            Debug.LogWarning("TutorialManager: Resources/Dialogos/tutorial.json não encontrado.");
            Destroy(gameObject);
            return;
        }

        RoteiroDialogo roteiro = JsonUtility.FromJson<RoteiroDialogo>(arquivo.text);

        if (roteiro == null || roteiro.passos == null || roteiro.passos.Length == 0)
        {
            Debug.LogWarning("TutorialManager: roteiro vazio ou inválido.");
            Destroy(gameObject);
            return;
        }

        DialogoManager.Instalar().Executar(roteiro);
    }
}

public static class TutorialBootstrap
{
    public static void Instalar()
    {
        if (Object.FindAnyObjectByType<TutorialManager>() != null)
            return;

        GameObject go = new GameObject("Tutorial");
        go.AddComponent<TutorialManager>();
    }
}
