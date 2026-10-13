using UnityEngine;
using System.Collections.Generic;

public class MaquinaManager : MonoBehaviour
{
    public static MaquinaManager Instance;

    private List<string> maquinasCompradas = new List<string>();

    // Referências das máquinas na cena
    public MaquinaLixo maquinaLixo;
    public MaquinaScanner maquinaScanner;
    public MaquinaQuebrador maquinaQuebrador;
    public MaquinaInversor maquinaInversor;

    void Awake()
    {
        Instance = this;
    }

    public bool TemMaquina(string id)
    {
        return maquinasCompradas.Contains(id);
    }

    public bool ComprarMaquina(MaquinaDados dados)
    {
        if (TemMaquina(dados.id)) return false;
        if (!GameManager.Instance.GastarDinheiro(dados.preco)) return false;

        // Se é upgrade, remove a anterior
        foreach (MaquinaDados m in MaquinaDatabase.Instance.todasMaquinas)
            if (m.idUpgrade == dados.id)
                maquinasCompradas.Remove(m.id);

        maquinasCompradas.Add(dados.id);
        AtivarMaquina(dados.id);
        return true;
    }

    void AtivarMaquina(string id)
    {
        switch (id)
        {
            case "lixo":
            case "lixo_luxo":
                if (maquinaLixo != null) maquinaLixo.Ativar(id);
                break;
            case "scanner":
                if (maquinaScanner != null) maquinaScanner.Ativar();
                break;
            case "quebrador":
                if (maquinaQuebrador != null) maquinaQuebrador.Ativar();
                break;
            case "inversor":
                if (maquinaInversor != null) maquinaInversor.Ativar();
                break;
        }
    }

    public List<string> GetMaquinasCompradas() { return maquinasCompradas; }

    public void CarregarMaquinas(List<string> ids)
    {
        maquinasCompradas = ids;
        foreach (string id in ids)
            AtivarMaquina(id);
    }
}