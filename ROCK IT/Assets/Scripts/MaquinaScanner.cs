using UnityEngine;

public class MaquinaScanner : MonoBehaviour
{
    private bool ativo = false;

    public void Ativar()
    {
        ativo = true;
        gameObject.SetActive(true);
        AtualizarTodasPedras();
    }

    void AtualizarTodasPedras()
    {
        PedraManager[] pedras = FindObjectsByType<PedraManager>(FindObjectsSortMode.None);
        foreach (PedraManager p in pedras)
            p.AtualizarContornoScanner(ativo);
    }

    // Chama isso quando uma nova pedra spawna
    public static void NotificarNovaPedra(PedraManager pedra)
    {
        if (MaquinaManager.Instance.TemMaquina("scanner"))
            pedra.AtualizarContornoScanner(true);
    }
}