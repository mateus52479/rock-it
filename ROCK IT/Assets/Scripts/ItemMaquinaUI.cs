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

        bool comprada = MaquinaManager.Instance.TemMaquina(dados.id);

        if (comprada)
        {
            // Verifica se tem upgrade disponível
            if (!string.IsNullOrEmpty(dados.idUpgrade))
            {
                MaquinaDados upgrade = MaquinaDatabase.Instance.BuscarPorId(dados.idUpgrade);
                if (upgrade != null && !MaquinaManager.Instance.TemMaquina(dados.idUpgrade))
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

        if (comprada && !string.IsNullOrEmpty(dados.idUpgrade))
        {
            MaquinaDados upgrade = MaquinaDatabase.Instance.BuscarPorId(dados.idUpgrade);
            if (upgrade != null)
                MaquinaManager.Instance.ComprarMaquina(upgrade);
        }
        else
        {
            MaquinaManager.Instance.ComprarMaquina(dados);
        }
    }
}