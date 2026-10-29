using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemMaquinaUI : MonoBehaviour
{
    public Image imagemMaquina;
    public TextMeshProUGUI textoNome;
    public TextMeshProUGUI textoDescricao;
    public TextMeshProUGUI textoPreco;
    public Button botaoAcao;
    public TextMeshProUGUI textoBotao;

    private MaquinaDados dados;
    private MaquinaDados exibido;

    void Awake()
    {
        OrganizarLinha();
        AplicarEstilo();
        Traducao.OnIdiomaMudou += AtualizarTextos;
    }

    void OnDestroy()
    {
        Traducao.OnIdiomaMudou -= AtualizarTextos;
    }

    void AtualizarTextos()
    {
        if (dados == null)
            return;

        // Força Exibir a reescrever os textos no idioma novo.
        exibido = null;

        AtualizarEstado();
    }

    void OrganizarLinha()
    {
        TemaUI.PrepararLinha(transform, 104f);

        Image fundo = GetComponent<Image>();
        if (fundo != null)
            TemaUI.AplicarPlaca(fundo, TemaUI.SpriteLinhaItem(), Color.white);

        RectTransform coluna = TemaUI.ColunaTextos(transform, 2f);

        if (textoNome != null)
        {
            textoNome.rectTransform.SetParent(coluna, false);
            TemaUI.AjustarAltura(textoNome, 26f);
            textoNome.textWrappingMode = TextWrappingModes.NoWrap;
        }

        if (textoDescricao != null)
        {
            textoDescricao.rectTransform.SetParent(coluna, false);
            TemaUI.AjustarAltura(textoDescricao, 38f);
            textoDescricao.textWrappingMode = TextWrappingModes.Normal;
            textoDescricao.overflowMode = TextOverflowModes.Ellipsis;
        }

        if (textoPreco != null)
        {
            textoPreco.rectTransform.SetParent(coluna, false);
            TemaUI.AjustarAltura(textoPreco, 22f);
            textoPreco.textWrappingMode = TextWrappingModes.NoWrap;
        }

        if (imagemMaquina != null)
        {
            imagemMaquina.rectTransform.SetParent(transform, false);
            TemaUI.AjustarAltura(imagemMaquina, 76f);
            TemaUI.AjustarLargura(imagemMaquina, 76f);
            imagemMaquina.preserveAspect = true;
            imagemMaquina.raycastTarget = false;
        }

        if (botaoAcao != null)
        {
            botaoAcao.transform.SetParent(transform, false);
            TemaUI.AjustarAltura(botaoAcao, 42f);
            TemaUI.AjustarLargura(botaoAcao, 118f);
        }

        if (imagemMaquina != null)
            imagemMaquina.transform.SetAsLastSibling();

        coluna.SetAsLastSibling();

        if (botaoAcao != null)
            botaoAcao.transform.SetAsLastSibling();
    }

    void AplicarEstilo()
    {
        TemaUI.EstilizarTexto(
            textoNome, 20f, TemaUI.TextoForte, FontStyles.Bold,
            TextAlignmentOptions.Left);

        TemaUI.EstilizarTexto(
            textoDescricao, 13f, TemaUI.TextoSuave, FontStyles.Normal,
            TextAlignmentOptions.Left);

        TemaUI.EstilizarTexto(
            textoPreco, 16f, TemaUI.Acento, FontStyles.Bold,
            TextAlignmentOptions.Left);

        if (botaoAcao != null)
            TemaUI.EstilizarBotao(botaoAcao, TemaUI.EstiloBotao.Secundario, 14f);
    }

    void Update()
    {
        AtualizarEstado();
    }

    public void Configurar(MaquinaDados maquina)
    {
        dados = maquina;

        Exibir(maquina);

        if (botaoAcao != null)
            botaoAcao.onClick.AddListener(AcaoBotao);

        AtualizarEstado();
    }

    /// <summary>
    /// Mostra na linha os dados de uma máquina. A linha do lixo, por exemplo,
    /// passa a exibir o Lixo de Luxo assim que o upgrade fica disponível.
    /// </summary>
    void Exibir(MaquinaDados maquina)
    {
        if (maquina == null || maquina == exibido)
            return;

        exibido = maquina;

        if (imagemMaquina != null)
        {
            Sprite icone = null;

            if (maquina.id == "lixo")
                icone = SpriteUtil.CarregarDeResources("Sprites/Maquinas/LATA DE LIXO");
            else if (maquina.id == "lixo_luxo")
                icone = SpriteUtil.CarregarDeResources("Sprites/Maquinas/LATA DE LIXO DE OURO");
            else if (maquina.id == "scanner")
                icone = SpriteUtil.CarregarDeResources("Sprites/Maquinas/Scanner");

            if (icone != null)
            {
                imagemMaquina.sprite = icone;

                // O PNG do ouro já vem dourado; tingir por cima só escureceria.
                imagemMaquina.color = Color.white;
            }
            else
            {
                imagemMaquina.sprite = TemaUI.SpriteEngrenagem();
                imagemMaquina.color = TemaUI.TextoSuave;
            }

            imagemMaquina.preserveAspect = true;
        }

        if (textoNome != null)
            textoNome.text = Traducao.T(maquina.nome);

        if (textoDescricao != null)
            textoDescricao.text = Traducao.T(maquina.descricao);
    }

    MaquinaDados BuscarUpgrade()
    {
        if (dados == null || string.IsNullOrEmpty(dados.idUpgrade))
            return null;

        return MaquinaDatabase.Instance.BuscarPorId(dados.idUpgrade);
    }

    void AtualizarEstado()
    {
        if (dados == null) return;

        // Máquina ainda não implementada: aparece na loja, mas não dá para comprar.
        if (!dados.disponivel)
        {
            Exibir(dados);
            textoBotao.text = Traducao.T("Em breve");
            textoPreco.text = "—";
            textoPreco.color = TemaUI.TextoFraco;
            botaoAcao.interactable = false;
            return;
        }

        MaquinaDados upgrade = BuscarUpgrade();
        bool comprada = MaquinaManager.Instance.TemMaquina(dados.id);

        // Se o upgrade desta máquina já foi comprado, ela conta como
        // evoluída: não pode voltar a oferecer "Comprar" e rebaixar.
        bool temUpgrade = upgrade != null && MaquinaManager.Instance.TemMaquina(upgrade.id);

        if (comprada && !temUpgrade && upgrade != null && upgrade.disponivel)
        {
            // A próxima compra é o upgrade: a linha inteira passa a mostrar
            // o Lixo de Luxo (ícone dourado, nome e descrição novos).
            Exibir(upgrade);
            textoBotao.text = Traducao.T("Evoluir");
            textoPreco.text = "R$ " + upgrade.preco;
            textoPreco.color = GameManager.Instance.dinheiro >= upgrade.preco
                ? TemaUI.Acento
                : TemaUI.Vermelho;
            botaoAcao.interactable = true;
            return;
        }

        if (comprada || temUpgrade)
        {
            Exibir(temUpgrade ? upgrade : dados);
            textoBotao.text = Traducao.T("Comprado");
            textoPreco.text = Traducao.T("Comprado");
            textoPreco.color = TemaUI.Acento;
            botaoAcao.interactable = false;
        }
        else
        {
            Exibir(dados);
            textoBotao.text = Traducao.T("Comprar");
            textoPreco.text = "R$ " + dados.preco;
            textoPreco.color = GameManager.Instance.dinheiro >= dados.preco
                ? TemaUI.Acento
                : TemaUI.Vermelho;
            botaoAcao.interactable = true;
        }
    }

    void AcaoBotao()
    {
        MaquinaDados upgrade = BuscarUpgrade();

        // O upgrade já foi comprado: a linha está "Comprado", sem ação.
        if (upgrade != null && MaquinaManager.Instance.TemMaquina(upgrade.id))
            return;

        bool comprada = MaquinaManager.Instance.TemMaquina(dados.id);

        if (comprada && upgrade != null && upgrade.disponivel)
            MaquinaManager.Instance.ComprarMaquina(upgrade);
        else
            MaquinaManager.Instance.ComprarMaquina(dados);
    }
}
