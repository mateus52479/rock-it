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
                custo        = 40,
                requisitoIds = new string[0],
                tipo         = TipoHabilidade.Geral,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "desconto_universitario",
                nome         = "Desconto Universitário",
                descricao    = "Pague 15% a menos em qualquer compra de pedra.",
                custo        = 120,
                requisitoIds = new string[] { "hab_inicio" },
                tipo         = TipoHabilidade.Geral,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "cashback",
                nome         = "Cashback",
                descricao    = "25% de chance de não gastar dinheiro ao comprar uma pedra.",
                custo        = 250,
                requisitoIds = new string[] { "desconto_universitario" },
                tipo         = TipoHabilidade.Geral,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "whey_protein",
                nome         = "Whey Protein",
                descricao    = "Maior chance de dar dano crítico nas pedras.",
                custo        = 180,
                requisitoIds = new string[] { "hab_inicio" },
                tipo         = TipoHabilidade.Forca,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "trevo_quatro_folhas",
                nome         = "Trevo de Quatro Folhas",
                descricao    = "Ao receber o prêmio máximo de uma pedra, receba-o 1,5x.",
                custo        = 180,
                requisitoIds = new string[] { "hab_inicio" },
                tipo         = TipoHabilidade.Sorte,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "buque_trevos",
                nome         = "Buquê de Trevos",
                descricao    = "Receba sempre 1,25x de todos os valores de recompensa.",
                custo        = 350,
                requisitoIds = new string[] { "trevo_quatro_folhas" },
                tipo         = TipoHabilidade.Sorte,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "analista_profissional",
                nome         = "Analista Profissional",
                descricao    = "5% de chance de receber o prêmio máximo de qualquer pedra.",
                custo        = 220,
                requisitoIds = new string[] { "hab_inicio" },
                tipo         = TipoHabilidade.Sorte,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "engenheiro",
                nome         = "Engenheiro",
                descricao    = "Todas as máquinas na mesa trabalham 25% mais rápido.",
                custo        = 300,
                requisitoIds = new string[] { "hab_inicio" },
                tipo         = TipoHabilidade.Geral,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "bomba",
                nome         = "Bomba",
                descricao    = "O dano crítico passa a ser 4x o dano padrão.",
                custo        = 400,
                requisitoIds = new string[] { "whey_protein" },
                tipo         = TipoHabilidade.Forca,
                valorBonus   = 0
            },
            new HabilidadeDados {
                id           = "fermentacao",
                nome         = "Fermentação",
                descricao    = "Quanto mais tempo a pedra fica na mesa, mais ela vale: +0,2x a cada minuto.",
                custo        = 300,
                requisitoIds = new string[] { "hab_inicio" },
                tipo         = TipoHabilidade.Fortuna,
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