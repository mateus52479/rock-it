using UnityEngine;

public class MaquinaScanner : MonoBehaviour
{
    private bool ativo = false;

    public bool EstaAtivo() { return ativo; }

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

    // Chama isso quando uma nova pedra spawna.
    public static void NotificarNovaPedra(PedraManager pedra)
    {
        if (pedra == null) return;

        MaquinaManager manager = MaquinaManager.Instance;
        if (manager != null && manager.TemMaquina("scanner"))
            pedra.AtualizarContornoScanner(true);
    }
}
