using UnityEngine;

public class MaquinaDatabase : MonoBehaviour
{
    public static MaquinaDatabase Instance;

    public MaquinaDados[] todasMaquinas;

    void Awake()
    {
        Instance = this;

        todasMaquinas = new MaquinaDados[]
        {
            new MaquinaDados {
                id        = "lixo",
                nome      = "Lixo",
                descricao = "Arraste uma pedra até aqui para descartá-la.",
                preco     = 50,
                idUpgrade = "lixo_luxo"
            },
            new MaquinaDados {
                id        = "lixo_luxo",
                nome      = "Lixo de Luxo",
                descricao = "Ao descartar uma pedra receba 25% do valor de compra dela.",
                preco     = 200,
                idUpgrade = ""
            },
            new MaquinaDados {
                id        = "scanner",
                nome      = "Scanner",
                descricao = "Todas as pedras ganham contorno verde (positivo) ou vermelho (negativo).",
                preco     = 150,
                idUpgrade = ""
            },
            new MaquinaDados {
                id        = "quebrador",
                nome      = "Quebrador Automático",
                descricao = "Coloque uma pedra aqui. Ela será quebrada automaticamente a 20% do seu dano por segundo.",
                preco     = 300,
                idUpgrade = ""
            },
            new MaquinaDados {
                id        = "inversor",
                nome      = "Inversor de Polaridade",
                descricao = "Coloque uma pedra aqui para inverter seu valor de recompensa.",
                preco     = 400,
                idUpgrade = ""
            },
            new MaquinaDados {
                id        = "loja_cosmeticos",
                nome      = "Loja de Cosméticos",
                descricao = "Compre skins para sua pedra de estimação.",
                preco     = 500,
                idUpgrade = ""
            }
        };
    }

    public MaquinaDados BuscarPorId(string id)
    {
        foreach (MaquinaDados m in todasMaquinas)
            if (m.id == id) return m;
        return null;
    }
}