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
                textoNome.text = "Força";
                textoNivel.text = "Nível " + StatusManager.Instance.nivelForca +
                    "\nDano: " + StatusManager.Instance.GetDanoForca().ToString("F1");
                AtualizarCorCusto(StatusManager.Instance.GetCustoProximoNivelForca());
                break;
            case 1:
                textoNome.text = "Sorte";
                textoNivel.text = "Nível " + StatusManager.Instance.nivelSorte +
                    "\nBônus: +" + ((StatusManager.Instance.GetMultiplicadorSorte() - 1) * 100).ToString("F0") + "%";
                AtualizarCorCusto(StatusManager.Instance.GetCustoProximoNivelSorte());
                break;
            case 2:
                textoNome.text = "Fortuna";
                textoNivel.text = "Nível " + StatusManager.Instance.nivelFortuna +
                    "\nMultiplicador: x" + StatusManager.Instance.GetMultiplicadorFortuna().ToString("F1");
                AtualizarCorCusto(StatusManager.Instance.GetCustoProximoNivelFortuna());
                break;
        }
    }

    void AtualizarCorCusto(int custo)
    {
        textoCusto.text = "R$ " + custo;
        textoCusto.color = GameManager.Instance.dinheiro >= custo
            ? new Color(0.2f, 0.9f, 0.2f)
            : new Color(0.9f, 0.2f, 0.2f);
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