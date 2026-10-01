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
                id          = "hab_inicio",
                nome        = "Primeiros Passos",
                descricao   = "O começo de tudo. Desbloqueia as demais habilidades.",
                custo       = 100,
                requisitoIds = new string[0], // sem requisito
                tipo        = TipoHabilidade.Geral,
                valorBonus  = 0
            },
            new HabilidadeDados {
                id          = "hab_forca1",
                nome        = "Punho de Ferro",
                descricao   = "Seus golpes ficam mais pesados.",
                custo       = 300,
                requisitoIds = new string[] { "hab_inicio" },
                tipo        = TipoHabilidade.Forca,
                valorBonus  = 1
            },
            new HabilidadeDados {
                id          = "hab_sorte1",
                nome        = "Olho Clínico",
                descricao   = "Você reconhece pedras com potencial.",
                custo       = 300,
                requisitoIds = new string[] { "hab_inicio" },
                tipo        = TipoHabilidade.Sorte,
                valorBonus  = 1
            },
            new HabilidadeDados {
                id          = "hab_fortuna1",
                nome        = "Toque de Midas",
                descricao   = "Tudo que você toca vale um pouco mais.",
                custo       = 500,
                requisitoIds = new string[] { "hab_inicio" },
                tipo        = TipoHabilidade.Fortuna,
                valorBonus  = 1
            },
            new HabilidadeDados {
                id          = "hab_geral1",
                nome        = "Minerador Nato",
                descricao   = "Experiência em tudo. Bônus em força e sorte.",
                custo       = 400,
                requisitoIds = new string[] { "hab_inicio" },
                tipo        = TipoHabilidade.Geral,
                valorBonus  = 0
            },
            new HabilidadeDados {
                id          = "hab_forca2",
                nome        = "Golpe Preciso",
                descricao   = "Requer Punho de Ferro. Dano ainda maior.",
                custo       = 800,
                requisitoIds = new string[] { "hab_forca1" },
                tipo        = TipoHabilidade.Forca,
                valorBonus  = 2
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