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

    void Update()
    {
        AtualizarEstado();
    }

    public void Configurar(MaquinaDados maquina)
    {
        dados = maquina;
        imagemMaquina.color = Color.gray; // placeholder
        textoNome.text = maquina.nome;
        textoDescricao.text = maquina.descricao;
        botaoAcao.onClick.AddListener(AcaoBotao);
        AtualizarEstado();
    }

    void AtualizarEstado()
    {
        if (dados == null) return;

        // Máquina ainda não implementada: aparece na loja, mas não dá para comprar.
        if (!dados.disponivel)
        {
            textoBotao.text = "Em breve";
            textoPreco.text = "—";
            textoPreco.color = Color.gray;
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
                    textoBotao.text = "Evoluir";
                    textoPreco.text = "R$ " + upgrade.preco;
                    textoPreco.color = GameManager.Instance.dinheiro >= upgrade.preco
                        ? new Color(0.2f, 0.9f, 0.2f)
                        : new Color(0.9f, 0.2f, 0.2f);
                    botaoAcao.interactable = true;
                    return;
                }
            }

            textoBotao.text = "Comprado";
            textoPreco.text = "✓";
            textoPreco.color = new Color(0.2f, 0.9f, 0.2f);
            botaoAcao.interactable = false;
        }
        else if (comprada || temUpgrade)
        {
            textoBotao.text = "Comprado";
            textoPreco.text = "✓";
            textoPreco.color = new Color(0.2f, 0.9f, 0.2f);
            botaoAcao.interactable = false;
        }
        else
        {
            textoBotao.text = "Comprar";
            textoPreco.text = "R$ " + dados.preco;
            textoPreco.color = GameManager.Instance.dinheiro >= dados.preco
                ? new Color(0.2f, 0.9f, 0.2f)
                : new Color(0.9f, 0.2f, 0.2f);
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