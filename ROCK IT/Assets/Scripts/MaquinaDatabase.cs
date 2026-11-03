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
                preco     = 400,
                idUpgrade = ""
            },
            new MaquinaDados {
                id        = "scanner",
                nome      = "Scanner",
                descricao = "Todas as pedras ganham contorno verde (positivo) ou vermelho (negativo).",
                preco     = 900,
                idUpgrade = ""
            },
            new MaquinaDados {
                id        = "quebrador",
                nome      = "Quebrador Automático",
                descricao = "Coloque pedras aqui. Cada uma é quebrada automaticamente com o seu dano por segundo.",
                preco     = 2500,
                idUpgrade = ""
            },
            new MaquinaDados {
                id        = "inversor",
                nome      = "Inversor de Polaridade",
                descricao = "Inverte o valor da pedra e devolve o preço pago com 25% de bônus. Na média fica justo: ganha na maioria das vezes, mas o prêmio máximo vira um prejuízo enorme.",
                preco     = 7000,
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
