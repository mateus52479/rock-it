using UnityEngine;

public class PedraDatabase : MonoBehaviour
{
    public static PedraDatabase Instance;

    public PedraDados[] todasAsPedras;

    void Awake()
    {
        Instance = this;

        todasAsPedras = new PedraDados[]
        {
            new PedraDados {
                nome        = "Pedra Comum",
                descricao   = "Uma pedra qualquer. Pode ter algo dentro, pode não ter.",
                cor         = new Color(0.5f, 0.4f, 0.3f),
                precoCompra = 0,
                recompensaMin = -2,
                recompensaMax = 5,
                vidaMaxima  = 10
            },
            new PedraDados {
                nome        = "Pedra Arenosa",
                descricao   = "Mais fraca, quebra fácil. Recompensa pequena.",
                cor         = new Color(0.9f, 0.8f, 0.5f),
                precoCompra = 10,
                recompensaMin = -1,
                recompensaMax = 8,
                vidaMaxima  = 6
            },
            new PedraDados {
                nome        = "Pedra Escura",
                descricao   = "Pesada e densa. Alto risco, alta recompensa.",
                cor         = new Color(0.2f, 0.2f, 0.25f),
                precoCompra = 30,
                recompensaMin = -5,
                recompensaMax = 20,
                vidaMaxima  = 15
            },
            new PedraDados {
                nome        = "Pedra Cristalina",
                descricao   = "Brilha um pouco. Sinal de algo precioso dentro?",
                cor         = new Color(0.6f, 0.9f, 1f),
                precoCompra = 60,
                recompensaMin = 0,
                recompensaMax = 40,
                vidaMaxima  = 12
            },
            new PedraDados {
                nome        = "Pedra Vulcânica",
                descricao   = "Vinda de lugares profundos. Perigosa e valiosa.",
                cor         = new Color(0.8f, 0.2f, 0.05f),
                precoCompra = 120,
                recompensaMin = -10,
                recompensaMax = 80,
                vidaMaxima  = 20
            }
        };
    }

    public PedraDados BuscarPorNome(string nome)
    {
        foreach (PedraDados p in todasAsPedras)
            if (p.nome == nome) return p;
        return null;
    }
}