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

        if (textoNome != null)
            textoNome.text = Traducao.T(dados.nome);

        if (textoDescricao != null)
            textoDescricao.text = Traducao.T(dados.descricao);

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

                // O PNG do lixo de ouro é igual ao normal; o dourado vem do
                // tingimento para a linha do upgrade não parecer a mesma.
                imagemMaquina.color = maquina.id == "lixo_luxo"
                    ? TemaUI.Dourado
                    : Color.white;
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

        if (botaoAcao != null)
            botaoAcao.onClick.AddListener(AcaoBotao);

        AtualizarEstado();
    }

    void AtualizarEstado()
    {
        if (dados == null) return;

        // Máquina ainda não implementada: aparece na loja, mas não dá para comprar.
        if (!dados.disponivel)
        {
            textoBotao.text = Traducao.T("Em breve");
            textoPreco.text = "—";
            textoPreco.color = TemaUI.TextoFraco;
            botaoAcao.interactable = false;
            return;
        }

        bool comprada = MaquinaManager.Instance.TemMaquina(dados.id);

        // Se o upgrade desta máquina já foi comprado, ela conta como
        // evoluída: não pode voltar a oferecer "Comprar" e rebaixar.
        bool temUpgrade = !string.IsNullOrEmpty(dados.idUpgrade) &&
                          MaquinaManager.Instance.TemMaquina(dados.idUpgrade);

        if (comprada && !temUpgrade)
        {
            // Verifica se tem upgrade disponível
            if (!string.IsNullOrEmpty(dados.idUpgrade))
            {
                MaquinaDados upgrade = MaquinaDatabase.Instance.BuscarPorId(dados.idUpgrade);
                if (upgrade != null && upgrade.disponivel)
                {
                    textoBotao.text = Traducao.T("Evoluir");
                    textoPreco.text = "R$ " + upgrade.preco;
                    textoPreco.color = GameManager.Instance.dinheiro >= upgrade.preco
                        ? TemaUI.Acento
                        : TemaUI.Vermelho;
                    botaoAcao.interactable = true;
                    return;
                }
            }

            textoBotao.text = Traducao.T("Comprado");
            textoPreco.text = Traducao.T("Comprado");
            textoPreco.color = TemaUI.Acento;
            botaoAcao.interactable = false;
        }
        else if (comprada || temUpgrade)
        {
            textoBotao.text = Traducao.T("Comprado");
            textoPreco.text = Traducao.T("Comprado");
            textoPreco.color = TemaUI.Acento;
            botaoAcao.interactable = false;
        }
        else
        {
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
        bool comprada = MaquinaManager.Instance.TemMaquina(dados.id);

        bool temUpgrade = !string.IsNullOrEmpty(dados.idUpgrade) &&
                          MaquinaManager.Instance.TemMaquina(dados.idUpgrade);

        if (temUpgrade)
            return;

        if (comprada && !string.IsNullOrEmpty(dados.idUpgrade))
        {
            MaquinaDados upgrade = MaquinaDatabase.Instance.BuscarPorId(dados.idUpgrade);
            if (upgrade != null && upgrade.disponivel)
                MaquinaManager.Instance.ComprarMaquina(upgrade);
        }
        else
        {
            MaquinaManager.Instance.ComprarMaquina(dados);
        }
    }
}
