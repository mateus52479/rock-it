using UnityEngine;

public class PedraDatabase : MonoBehaviour
{
    public static PedraDatabase Instance;

    public PedraDados[] todasAsPedras;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        todasAsPedras = new PedraDados[]
        {
            new PedraDados {
                nome = "Pedra Comum",
                descricao = "Uma pedra qualquer. Pode ter algo dentro, pode não ter.",
                cor = new Color(0.5f, 0.4f, 0.3f),
                precoCompra = 0,
                vidaMaxima = 12,
                tabelaValores = new ValorPedra[]
                {
                    new ValorPedra { valor = -5, chance = 0f },
                    new ValorPedra { valor = 0, chance = 0f },
                    new ValorPedra { valor = 5, chance = 0f },
                    new ValorPedra { valor = 10000, chance = 100f },
                }
            },
            new PedraDados {
                nome = "Pedra Bonita",
                descricao = "Tem um brilho diferente. Pode valer algo.",
                cor = new Color(0.8f, 0.6f, 0.8f),
                precoCompra = 7,
                vidaMaxima = 24,
                tabelaValores = new ValorPedra[]
                {
                    new ValorPedra { valor = -10, chance = 30f },
                    new ValorPedra { valor = 0, chance = 30f },
                    new ValorPedra { valor = 10, chance = 35f },
                    new ValorPedra { valor = 20, chance = 5f },
                }
            },
            new PedraDados {
                nome = "Ametista",
                descricao = "Roxa e misteriosa. Alto risco.",
                cor = new Color(0.6f, 0.2f, 0.8f),
                precoCompra = 14,
                vidaMaxima = 48,
                tabelaValores = new ValorPedra[]
                {
                    new ValorPedra { valor = -20, chance = 40f },
                    new ValorPedra { valor = 0, chance = 45f },
                    new ValorPedra { valor = 20, chance = 12.5f },
                    new ValorPedra { valor = 40, chance = 2.5f },
                }
            },
            new PedraDados {
                nome = "Safira",
                descricao = "Azul profundo. Raramente vale muito.",
                cor = new Color(0.1f, 0.3f, 0.9f),
                precoCompra = 28,
                vidaMaxima = 96,
                tabelaValores = new ValorPedra[]
                {
                    new ValorPedra { valor = -40, chance = 45f },
                    new ValorPedra { valor = 0, chance = 50f },
                    new ValorPedra { valor = 40, chance = 4f },
                    new ValorPedra { valor = 80, chance = 1f },
                }
            },
            new PedraDados {
                nome = "Ferro",
                descricao = "Pesado e denso. Difícil de quebrar.",
                cor = new Color(0.4f, 0.4f, 0.45f),
                precoCompra = 56,
                vidaMaxima = 192,
                tabelaValores = new ValorPedra[]
                {
                    new ValorPedra { valor = -80, chance = 57.5f },
                    new ValorPedra { valor = 0, chance = 40f },
                    new ValorPedra { valor = 80, chance = 2f },
                    new ValorPedra { valor = 160, chance = 0.5f },
                }
            },
            new PedraDados {
                nome = "Ouro",
                descricao = "Reluz sob a luz. Mas nem sempre vale.",
                cor = new Color(1f, 0.85f, 0.1f),
                precoCompra = 112,
                vidaMaxima = 384,
                tabelaValores = new ValorPedra[]
                {
                    new ValorPedra { valor = -160, chance = 63.75f },
                    new ValorPedra { valor = 0, chance = 35f },
                    new ValorPedra { valor = 160, chance = 1f },
                    new ValorPedra { valor = 320, chance = 0.25f },
                }
            },
            new PedraDados {
                nome = "Platina",
                descricao = "Rara e pesada. Quase sempre decepciona.",
                cor = new Color(0.8f, 0.9f, 0.95f),
                precoCompra = 224,
                vidaMaxima = 768,
                tabelaValores = new ValorPedra[]
                {
                    new ValorPedra { valor = -320, chance = 60f },
                    new ValorPedra { valor = 0, chance = 38.75f },
                    new ValorPedra { valor = 320, chance = 1f },
                    new ValorPedra { valor = 640, chance = 0.25f },
                }
            },
            new PedraDados {
                nome = "Diamante",
                descricao = "Brilha como estrela. Raramente.",
                cor = new Color(0.7f, 0.95f, 1f),
                precoCompra = 448,
                vidaMaxima = 1536,
                tabelaValores = new ValorPedra[]
                {
                    new ValorPedra { valor = -640, chance = 60f },
                    new ValorPedra { valor = 0, chance = 38.75f },
                    new ValorPedra { valor = 640, chance = 1f },
                    new ValorPedra { valor = 1280, chance = 0.25f },
                }
            },
            new PedraDados {
                nome = "Paládio",
                descricao = "Metal precioso e raro. Muito difícil de lucrar.",
                cor = new Color(0.6f, 0.75f, 0.8f),
                precoCompra = 1340,
                vidaMaxima = 3072,
                tabelaValores = new ValorPedra[]
                {
                    new ValorPedra { valor = -1280, chance = 60f },
                    new ValorPedra { valor = 0, chance = 38.75f },
                    new ValorPedra { valor = 1280, chance = 1f },
                    new ValorPedra { valor = 2560, chance = 0.25f },
                }
            },
            new PedraDados {
                nome = "Glacial",
                descricao = "Formada em temperaturas extremas. Perigosa.",
                cor = new Color(0.5f, 0.85f, 1f),
                precoCompra = 3360,
                vidaMaxima = 6144,
                tabelaValores = new ValorPedra[]
                {
                    new ValorPedra { valor = -5120, chance = 60f },
                    new ValorPedra { valor = -2560, chance = 38.75f },
                    new ValorPedra { valor = 2560, chance = 1f },
                    new ValorPedra { valor = 5120, chance = 0.25f },
                }
            },
            new PedraDados {
                nome = "Vulcânica",
                descricao = "Vinda das profundezas. Extremamente arriscada.",
                cor = new Color(0.9f, 0.2f, 0.05f),
                precoCompra = 8400,
                vidaMaxima = 12288,
                tabelaValores = new ValorPedra[]
                {
                    new ValorPedra { valor = -10240, chance = 60f },
                    new ValorPedra { valor = -5120, chance = 38.75f },
                    new ValorPedra { valor = 5120, chance = 1f },
                    new ValorPedra { valor = 10240, chance = 0.25f },
                }
            },
            new PedraDados {
                nome = "Asteroide",
                descricao = "Veio do espaço. Ninguém sabe o que tem dentro.",
                cor = new Color(0.3f, 0.3f, 0.35f),
                precoCompra = 21000,
                vidaMaxima = 24576,
                tabelaValores = new ValorPedra[]
                {
                    new ValorPedra { valor = -20480, chance = 60f },
                    new ValorPedra { valor = -10240, chance = 38.75f },
                    new ValorPedra { valor = 10240, chance = 1f },
                    new ValorPedra { valor = 20480, chance = 0.25f },
                }
            },
            new PedraDados {
                nome = "Pedra da Lua",
                descricao = "Trazida de satélites distantes. Raramente lucrativa.",
                cor = new Color(0.9f, 0.9f, 0.95f),
                precoCompra = 52500,
                vidaMaxima = 49152,
                tabelaValores = new ValorPedra[]
                {
                    new ValorPedra { valor = -40960, chance = 60f },
                    new ValorPedra { valor = -20480, chance = 38.75f },
                    new ValorPedra { valor = 20480, chance = 1f },
                    new ValorPedra { valor = 40960, chance = 0.25f },
                }
            },
            new PedraDados {
                nome = "Pedacinho do Sol",
                descricao = "Impossível de existir. E ainda assim, aqui está.",
                cor = new Color(1f, 0.9f, 0.2f),
                precoCompra = 200000,
                vidaMaxima = 98304,
                tabelaValores = new ValorPedra[]
                {
                    new ValorPedra { valor = -81920, chance = 60f },
                    new ValorPedra { valor = -40960, chance = 38.75f },
                    new ValorPedra { valor = 40960, chance = 1f },
                    new ValorPedra { valor = 81920, chance = 0.25f },
                }
            },
        };

        // Só carregamos os sprites depois que o catálogo inteiro foi criado.
        foreach (PedraDados pedra in todasAsPedras) { 
            CarregarSprites(pedra);
        }
    }



    void CarregarSprites(PedraDados pedra)
    {
        string nome = pedra.nome.ToUpperInvariant();

        pedra.sprite100 = Resources.Load<Sprite>(
            "Sprites/Pedras/100/100 " + nome
        );

        pedra.sprite75 = Resources.Load<Sprite>(
            "Sprites/Pedras/75/75 " + nome
        );

        pedra.sprite50 = Resources.Load<Sprite>(
            "Sprites/Pedras/50/50 " + nome
        );

        pedra.sprite25 = Resources.Load<Sprite>(
            "Sprites/Pedras/25/25 " + nome
        );

        Debug.Log(
            pedra.nome +
            " | 100%: " + (pedra.sprite100 != null) +
            " | 75%: " + (pedra.sprite75 != null) +
            " | 50%: " + (pedra.sprite50 != null) +
            " | 25%: " + (pedra.sprite25 != null)
        );
    }



    public PedraDados BuscarPorNome(string nome)
    {
        foreach (PedraDados pedra in todasAsPedras)
        {
            if (pedra.nome == nome)
                return pedra;
        }

        return null;
    }
}
