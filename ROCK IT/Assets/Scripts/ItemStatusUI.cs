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
                textoCusto.text = "R$ " + StatusManager.Instance.GetCustoProximoNivelForca();
                break;
            case 1:
                textoNome.text = "Sorte";
                textoNivel.text = "Nível " + StatusManager.Instance.nivelSorte +
                    "\nBônus: +" + ((StatusManager.Instance.GetMultiplicadorSorte() - 1) * 100).ToString("F0") + "%";
                textoCusto.text = "R$ " + StatusManager.Instance.GetCustoProximoNivelSorte();
                break;
            case 2:
                textoNome.text = "Fortuna";
                textoNivel.text = "Nível " + StatusManager.Instance.nivelFortuna +
                    "\nMultiplicador: x" + StatusManager.Instance.GetMultiplicadorFortuna().ToString("F1");
                textoCusto.text = "R$ " + StatusManager.Instance.GetCustoProximoNivelFortuna();
                break;
        }
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