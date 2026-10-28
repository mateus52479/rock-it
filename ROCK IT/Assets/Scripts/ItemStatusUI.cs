using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemStatusUI : MonoBehaviour
{
    public TextMeshProUGUI textoNome;
    public TextMeshProUGUI textoNivel;
    public TextMeshProUGUI textoCusto;
    public Button botaoUpgrade;

    private int tipoStatus; // 0=Forca, 1=Sorte, 2=Fortuna

    void Awake()
    {
        OrganizarLinha();
        AplicarEstilo();
        AtualizarRotuloBotao();
        Traducao.OnIdiomaMudou += AtualizarRotuloBotao;
    }

    void OnDestroy()
    {
        Traducao.OnIdiomaMudou -= AtualizarRotuloBotao;
    }

    /// <summary>
    /// O texto "Melhorar" vem do prefab; traduz na hora porque o item nasce
    /// depois que a cena já passou pelo tradutor.
    /// </summary>
    void AtualizarRotuloBotao()
    {
        if (botaoUpgrade == null)
            return;

        TextMeshProUGUI rotulo =
            botaoUpgrade.GetComponentInChildren<TextMeshProUGUI>(true);

        if (rotulo != null)
            rotulo.text = Traducao.T("Melhorar");
    }

    void OrganizarLinha()
    {
        TemaUI.PrepararLinha(transform, 96f);

        Image fundo = GetComponent<Image>();
        if (fundo != null)
            TemaUI.AplicarPlaca(fundo, TemaUI.SpriteLinhaItem(), Color.white);

        RectTransform coluna = TemaUI.ColunaTextos(transform, 1f);

        if (textoNome != null)
        {
            textoNome.rectTransform.SetParent(coluna, false);
            TemaUI.AjustarAltura(textoNome, 26f);
            textoNome.textWrappingMode = TextWrappingModes.NoWrap;
        }

        if (textoNivel != null)
        {
            textoNivel.rectTransform.SetParent(coluna, false);
            TemaUI.AjustarAltura(textoNivel, 42f);
            textoNivel.textWrappingMode = TextWrappingModes.Normal;
        }

        if (textoCusto != null)
        {
            textoCusto.rectTransform.SetParent(transform, false);
            TemaUI.AjustarAltura(textoCusto, 40f);
            TemaUI.AjustarLargura(textoCusto, 170f);
            textoCusto.alignment = TextAlignmentOptions.Right;
            textoCusto.textWrappingMode = TextWrappingModes.NoWrap;
        }

        if (botaoUpgrade != null)
        {
            botaoUpgrade.transform.SetParent(transform, false);
            TemaUI.AjustarAltura(botaoUpgrade, 42f);
            TemaUI.AjustarLargura(botaoUpgrade, 128f);
        }

        coluna.SetAsLastSibling();

        if (textoCusto != null)
            textoCusto.transform.SetAsLastSibling();

        if (botaoUpgrade != null)
            botaoUpgrade.transform.SetAsLastSibling();
    }

    void AplicarEstilo()
    {
        TemaUI.EstilizarTexto(
            textoNome, 20f, TemaUI.TextoForte, FontStyles.Bold,
            TextAlignmentOptions.Left);

        TemaUI.EstilizarTexto(
            textoNivel, 13f, TemaUI.TextoSuave, FontStyles.Normal,
            TextAlignmentOptions.Left);

        TemaUI.EstilizarTexto(
            textoCusto, 18f, TemaUI.Texto, FontStyles.Bold,
            TextAlignmentOptions.Right);

        if (botaoUpgrade != null)
            TemaUI.EstilizarBotao(botaoUpgrade, TemaUI.EstiloBotao.Verde, 14f);
    }

    public void Configurar(int tipo)
    {
        tipoStatus = tipo;
        botaoUpgrade.onClick.AddListener(Upgrade);
        Atualizar();
    }

    public void Atualizar()
    {
        switch (tipoStatus)
        {
            case 0:
                textoNome.text = Traducao.T("Força");
                textoNivel.text = Traducao.TF(
                    "Nível {0}\nDano: {1}",
                    StatusManager.Instance.nivelForca,
                    NumeroUtil.Formatar(StatusManager.Instance.GetDanoForca(), 1));
                AtualizarCorCusto(StatusManager.Instance.GetCustoProximoNivelForca());
                break;
            case 1:
                textoNome.text = Traducao.T("Sorte");
                textoNivel.text = Traducao.TF(
                    "Nível {0}\nChance máx.: +{1}%",
                    StatusManager.Instance.nivelSorte,
                    NumeroUtil.Formatar(StatusManager.Instance.GetDeslocamentoSorte(), 0));
                AtualizarCorCusto(StatusManager.Instance.GetCustoProximoNivelSorte());
                break;
            case 2:
                textoNome.text = Traducao.T("Fortuna");
                textoNivel.text = Traducao.TF(
                    "Nível {0}\nMultiplicador: x{1}",
                    StatusManager.Instance.nivelFortuna,
                    NumeroUtil.Formatar(StatusManager.Instance.GetMultiplicadorFortuna(), 1));
                AtualizarCorCusto(StatusManager.Instance.GetCustoProximoNivelFortuna());
                break;
        }
    }

    void AtualizarCorCusto(float custo)
    {
        textoCusto.text = NumeroUtil.FormatarDinheiro(custo);
        textoCusto.color = GameManager.Instance.dinheiro >= custo
            ? TemaUI.Acento
            : TemaUI.Vermelho;
    }

    void Update()
    {
        // Atualiza a cor em tempo real conforme o dinheiro muda
        Atualizar();
    }

    void Upgrade()
    {
        bool sucesso = false;
        switch (tipoStatus)
        {
            case 0: sucesso = StatusManager.Instance.UpgradeForca(); break;
            case 1: sucesso = StatusManager.Instance.UpgradeSorte(); break;
            case 2: sucesso = StatusManager.Instance.UpgradeFortuna(); break;
        }

        if (!sucesso)
            Debug.Log("Dinheiro insuficiente!");
        else
            Atualizar();
    }
}
