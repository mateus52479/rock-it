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
                preco     = 100,
                idUpgrade = "lixo_luxo"
            },
            new MaquinaDados {
                id        = "lixo_luxo",
                nome      = "Lixo de Luxo",
                descricao = "Ao descartar uma pedra receba 25% do valor de compra dela.",
                preco     = 1200,
                idUpgrade = ""
            },
            new MaquinaDados {
                id        = "scanner",
                nome      = "Scanner",
                descricao = "Todas as pedras ganham contorno verde (positivo) ou vermelho (negativo).",
                preco     = 2500,
                idUpgrade = ""
            },
            new MaquinaDados {
                id        = "quebrador",
                nome      = "Quebrador Automático",
                descricao = "Coloque uma pedra aqui. Ela será quebrada automaticamente a 20% do seu dano por segundo.",
                preco     = 6000,
                idUpgrade = ""
            },
            new MaquinaDados {
                id        = "inversor",
                nome      = "Inversor de Polaridade",
                descricao = "Inverte o valor da pedra e devolve o preço pago. Na média fica justo: ganha na maioria das vezes, mas o prêmio de 18x vira um prejuízo enorme.",
                preco     = 15000,
                idUpgrade = ""
            },
            new MaquinaDados {
                id         = "loja_cosmeticos",
                nome       = "Loja de Cosméticos",
                descricao  = "Em breve: compre skins para sua pedra de estimação.",
                preco      = 500,
                idUpgrade  = "",
                disponivel = false
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
