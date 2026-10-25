using UnityEngine;

public class MaquinaScanner : MonoBehaviour
{
    private bool ativo = false;

    public bool EstaAtivo() { return ativo; }

    public void Ativar()
    {
        ativo = true;
        gameObject.SetActive(true);
        AtualizarTodosContornos();
    }

    /// <summary>
    /// Recalcula a cor do contorno de todas as pedras na mesa.
    /// Chamado ao comprar o Scanner e quando Sorte/Fortuna mudam (as duas
    /// mudam o valor médio da pedra, então a cor antiga ficava desatualizada).
    /// </summary>
    public static void AtualizarTodosContornos()
    {
        MaquinaManager manager = MaquinaManager.Instance;

        if (manager == null || !manager.TemMaquina("scanner"))
            return;

        PedraManager[] pedras = FindObjectsByType<PedraManager>(FindObjectsSortMode.None);

        foreach (PedraManager p in pedras)
        {
            if (p != null)
                p.AtualizarContornoScanner(true);
        }
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
