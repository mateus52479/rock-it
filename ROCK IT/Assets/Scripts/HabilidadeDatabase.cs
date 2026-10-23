using UnityEngine;

public class HabilidadeDatabase : MonoBehaviour
{
    public static HabilidadeDatabase Instance;

    public HabilidadeDados[] todasHabilidades;

    void Awake()
    {
        Instance = this;

        todasHabilidades = new HabilidadeDados[]
        {
            new HabilidadeDados {
                id           = "hab_inicio",
                nome         = "Primeiros Passos",
                descricao    = "O começo de tudo. Desbloqueia as demais habilidades.",
                custo        = 100,
                requisitoIds = new string[0],
                tipo         = TipoHabilidade.Geral,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "desconto_universitario",
                nome         = "Desconto Universitário",
                descricao    = "Pague 25% a menos em qualquer compra de pedra.",
                custo        = 300,
                requisitoIds = new string[] { "hab_inicio" },
                tipo         = TipoHabilidade.Geral,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "cashback",
                nome         = "Cashback",
                descricao    = "50% de chance de não gastar dinheiro ao comprar uma pedra.",
                custo        = 600,
                requisitoIds = new string[] { "desconto_universitario" },
                tipo         = TipoHabilidade.Geral,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "whey_protein",
                nome         = "Whey Protein",
                descricao    = "Maior chance de dar dano crítico nas pedras.",
                custo        = 400,
                requisitoIds = new string[] { "hab_inicio" },
                tipo         = TipoHabilidade.Forca,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "trevo_quatro_folhas",
                nome         = "Trevo de Quatro Folhas",
                descricao    = "Ao receber o prêmio máximo de uma pedra, receba-o 2x.",
                custo        = 400,
                requisitoIds = new string[] { "hab_inicio" },
                tipo         = TipoHabilidade.Sorte,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "buque_trevos",
                nome         = "Buquê de Trevos",
                descricao    = "Receba sempre 1.5x de todos os valores de recompensa.",
                custo        = 800,
                requisitoIds = new string[] { "trevo_quatro_folhas" },
                tipo         = TipoHabilidade.Sorte,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "analista_profissional",
                nome         = "Analista Profissional",
                descricao    = "10% de chance de receber o prêmio máximo de qualquer pedra.",
                custo        = 500,
                requisitoIds = new string[] { "hab_inicio" },
                tipo         = TipoHabilidade.Sorte,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "engenheiro",
                nome         = "Engenheiro",
                descricao    = "Todas as máquinas na mesa trabalham 25% mais rápido.",
                custo        = 700,
                requisitoIds = new string[] { "hab_inicio" },
                tipo         = TipoHabilidade.Geral,
                valorBonus   = 0
            }
        };
    }

    public HabilidadeDados BuscarPorId(string id)
    {
        foreach (HabilidadeDados h in todasHabilidades)
            if (h.id == id) return h;
        return null;
    }
}