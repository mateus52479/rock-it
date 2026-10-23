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
                precoCompra = 1,
                vidaMaxima = 12,
                tabelaValores = new ValorPedra[]
                {
                    new ValorPedra { valor = -5, chance = 20f },
                    new ValorPedra { valor = 0, chance = 40f },
                    new ValorPedra { valor = 5, chance = 35f },
                    new ValorPedra { valor = 10, chance = 5f },
                }
            },
            new PedraDados {
                nome = "Pedra Bonita",
                descricao = "Tem um brilho diferente. Pode valer algo.",
                cor = new Color(0.8f, 0.6f, 0.8f),
                precoCompra = 7,
                vidaMaxima = 24,
                tabelaValores = TabelaPadrao(7),
            },
            new PedraDados {
                nome = "Ametista",
                descricao = "Roxa e misteriosa. Alto risco.",
                cor = new Color(0.6f, 0.2f, 0.8f),
                precoCompra = 14,
                vidaMaxima = 48,
                tabelaValores = TabelaPadrao(14),
            },
            new PedraDados {
                nome = "Safira",
                descricao = "Azul profundo. Raramente vale muito.",
                cor = new Color(0.1f, 0.3f, 0.9f),
                precoCompra = 28,
                vidaMaxima = 96,
                tabelaValores = TabelaPadrao(28),
            },
            new PedraDados {
                nome = "Ferro",
                descricao = "Pesado e denso. Difícil de quebrar.",
                cor = new Color(0.4f, 0.4f, 0.45f),
                precoCompra = 56,
                vidaMaxima = 192,
                tabelaValores = TabelaPadrao(56),
            },
            new PedraDados {
                nome = "Ouro",
                descricao = "Reluz sob a luz. Mas nem sempre vale.",
                cor = new Color(1f, 0.85f, 0.1f),
                precoCompra = 112,
                vidaMaxima = 384,
                tabelaValores = TabelaPadrao(112),
            },
            new PedraDados {
                nome = "Platina",
                descricao = "Rara e pesada. Quase sempre decepciona.",
                cor = new Color(0.8f, 0.9f, 0.95f),
                precoCompra = 224,
                vidaMaxima = 768,
                tabelaValores = TabelaPadrao(224),
            },
            new PedraDados {
                nome = "Diamante",
                descricao = "Brilha como estrela. Raramente.",
                cor = new Color(0.7f, 0.95f, 1f),
                precoCompra = 448,
                vidaMaxima = 1536,
                tabelaValores = TabelaPadrao(448),
            },
            new PedraDados {
                nome = "Paládio",
                descricao = "Metal precioso e raro. Muito difícil de lucrar.",
                cor = new Color(0.6f, 0.75f, 0.8f),
                precoCompra = 1340,
                vidaMaxima = 3072,
                tabelaValores = TabelaArriscada(1340),
            },
            new PedraDados {
                nome = "Glacial",
                descricao = "Formada em temperaturas extremas. Perigosa.",
                cor = new Color(0.5f, 0.85f, 1f),
                precoCompra = 3360,
                vidaMaxima = 6144,
                tabelaValores = TabelaArriscada(3360),
            },
            new PedraDados {
                nome = "Vulcânica",
                descricao = "Vinda das profundezas. Extremamente arriscada.",
                cor = new Color(0.9f, 0.2f, 0.05f),
                precoCompra = 8400,
                vidaMaxima = 12288,
                tabelaValores = TabelaArriscada(8400),
            },
            new PedraDados {
                nome = "Asteroide",
                descricao = "Veio do espaço. Ninguém sabe o que tem dentro.",
                cor = new Color(0.3f, 0.3f, 0.35f),
                precoCompra = 21000,
                vidaMaxima = 24576,
                tabelaValores = TabelaArriscada(21000),
            },
            new PedraDados {
                nome = "Pedra da Lua",
                descricao = "Trazida de satélites distantes. Raramente lucrativa.",
                cor = new Color(0.9f, 0.9f, 0.95f),
                precoCompra = 52500,
                vidaMaxima = 49152,
                tabelaValores = TabelaArriscada(52500),
            },
            new PedraDados {
                nome = "Pedacinho do Sol",
                nomeArquivo = "PEDRA DO SOL",
                animarFrames = true,
                descricao = "Impossível de existir. E ainda assim, aqui está.",
                cor = new Color(1f, 0.9f, 0.2f),
                precoCompra = 200000,
                vidaMaxima = 98304,
                tabelaValores = TabelaArriscada(200000),
            },
        };

        // Só carregamos os sprites depois que o catálogo inteiro foi criado.
        foreach (PedraDados pedra in todasAsPedras) { 
            CarregarSprites(pedra);
        }
    }



    /// <summary>
    /// Perfil de risco das pedras normais. Os valores são múltiplos do preço
    /// de compra, então a economia se mantém igual em qualquer faixa.
    /// Metade das vezes você perde a compra e parte do troco; 25% não ganha
    /// nada; 20% dobra o dinheiro; 5% paga 18x. Média: o valor da compra.
    /// </summary>
    private static ValorPedra[] TabelaPadrao(int preco)
    {
        return new ValorPedra[]
        {
            new ValorPedra { valor = Mathf.RoundToInt(-0.6f * preco), chance = 50f },
            new ValorPedra { valor = 0,                               chance = 25f },
            new ValorPedra { valor = Mathf.RoundToInt( 2.0f * preco), chance = 20f },
            new ValorPedra { valor = Mathf.RoundToInt(18.0f * preco), chance = 5f  },
        };
    }

    /// <summary>
    /// Perfil das pedras mais caras: perde mais vezes e perde mais, mas o
    /// prêmio raro é maior. Média 4% abaixo do preço, com bem mais variância.
    /// </summary>
    private static ValorPedra[] TabelaArriscada(int preco)
    {
        return new ValorPedra[]
        {
            new ValorPedra { valor = Mathf.RoundToInt(-0.8f * preco), chance = 55f },
            new ValorPedra { valor = 0,                               chance = 22f },
            new ValorPedra { valor = Mathf.RoundToInt( 2.5f * preco), chance = 18f },
            new ValorPedra { valor = Mathf.RoundToInt(19.0f * preco), chance = 5f  },
        };
    }

    void CarregarSprites(PedraDados pedra)
    {
        string nomeBase = string.IsNullOrEmpty(pedra.nomeArquivo)
            ? pedra.nome.ToUpperInvariant()
            : pedra.nomeArquivo;

        string sufixoFrameA = pedra.animarFrames ? " PT 1" : "";

        pedra.sprite100 = CarregarSpriteEstado(nomeBase, "100", sufixoFrameA);
        pedra.sprite75 = CarregarSpriteEstado(nomeBase, "75", sufixoFrameA);
        pedra.sprite50 = CarregarSpriteEstado(nomeBase, "50", sufixoFrameA);
        pedra.sprite25 = CarregarSpriteEstado(nomeBase, "25", sufixoFrameA);

        if (pedra.animarFrames)
        {
            pedra.sprite100B = CarregarSpriteEstado(nomeBase, "100", " PT 2");
            pedra.sprite75B = CarregarSpriteEstado(nomeBase, "75", " PT 2");
            pedra.sprite50B = CarregarSpriteEstado(nomeBase, "50", " PT 2");
            pedra.sprite25B = CarregarSpriteEstado(nomeBase, "25", " PT 2");
        }
    }

    Sprite CarregarSpriteEstado(string nomeBase, string estado, string sufixo)
    {
        string caminho = "Sprites/Pedras/" + estado + "/" + estado + " " + nomeBase + sufixo;
        Sprite sprite = Resources.Load<Sprite>(caminho);

        if (sprite == null)
            Debug.LogWarning("PedraDatabase: sprite nao encontrado em Resources/" + caminho);

        return sprite;
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
