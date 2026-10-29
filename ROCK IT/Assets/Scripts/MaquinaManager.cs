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

    [Header("Posições na mesa")]
    [Tooltip("Cada máquina aparece no seu lugar só depois de comprada. A lixeira fica onde está na cena.")]
    public Vector2 posicaoScanner = new Vector2(-7.4f, 2.5f);
    public Vector2 posicaoQuebrador = new Vector2(-7.4f, 1.1f);
    public Vector2 posicaoInversor = new Vector2(-7.4f, -0.3f);

    void Awake()
    {
        Instance = this;
        PrepararMaquinas();
    }

    /// <summary>
    /// Separa as máquinas (antes estavam todas empilhadas) e deixa todas
    /// escondidas até a compra. As posições podem ser ajustadas no Inspector.
    /// </summary>
    void PrepararMaquinas()
    {
        // A lixeira não é posicionada aqui: o lugar dela é o transform salvo
        // na cena, para poder ser ajustado no editor.
        Posicionar(maquinaScanner, posicaoScanner);
        Posicionar(maquinaQuebrador, posicaoQuebrador);
        Posicionar(maquinaInversor, posicaoInversor);

        Desativar(maquinaLixo);
        Desativar(maquinaScanner);
        Desativar(maquinaQuebrador);
        Desativar(maquinaInversor);
    }

    void Posicionar(Component maquina, Vector2 posicao)
    {
        if (maquina == null) return;

        Vector3 atual = maquina.transform.position;
        maquina.transform.position = new Vector3(posicao.x, posicao.y, atual.z);
    }

    void Desativar(Component maquina)
    {
        if (maquina != null)
            maquina.gameObject.SetActive(false);
    }

    public bool TemMaquina(string id)
    {
        return maquinasCompradas.Contains(id);
    }

    public bool ComprarMaquina(MaquinaDados dados)
    {
        if (dados == null || !dados.disponivel) return false;
        if (TemMaquina(dados.id)) return false;
        if (!GameManager.Instance.GastarDinheiro(dados.preco)) return false;

        // Se é upgrade, remove a anterior.
        foreach (MaquinaDados m in MaquinaDatabase.Instance.todasMaquinas)
            if (m.idUpgrade == dados.id)
                maquinasCompradas.Remove(m.id);

        maquinasCompradas.Add(dados.id);
        AtivarMaquina(dados.id);
        AudioManager.Compra();
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
            // loja_cosmeticos: implementada numa próxima rodada.
        }
    }

    public List<string> GetMaquinasCompradas() { return maquinasCompradas; }

    public void CarregarMaquinas(List<string> ids)
    {
        maquinasCompradas = ids ?? new List<string>();

        foreach (string id in maquinasCompradas)
            AtivarMaquina(id);
    }
}
