using UnityEngine;
using System.Collections.Generic;

public class StatusManager : MonoBehaviour
{
    public static StatusManager Instance;

    // Níveis atuais
    public int nivelForca = 1;
    public int nivelSorte = 1;
    public int nivelFortuna = 1;

    // Habilidades desbloqueadas
    private List<string> habilidadesDesbloqueadas = new List<string>();

    // Custo base e multiplicador por nível
    private int custoBaseForca = 50;
    private int custoBaseSorte = 50;
    private int custoBaseFortuna = 100;
    private float multiplicadorCusto = 2.5f;

    void Awake()
    {
        Instance = this;
    }

    // --- FORÇA ---
    public float GetDanoForca()
    {
        return 1f + (nivelForca - 1) * 0.5f; // cada nível adiciona 0.5 de dano
    }

    public int GetCustoProximoNivelForca()
    {
        return Mathf.RoundToInt(custoBaseForca * Mathf.Pow(multiplicadorCusto, nivelForca - 1));
    }

    public bool UpgradeForca()
    {
        int custo = GetCustoProximoNivelForca();
        if (!GameManager.Instance.GastarDinheiro(custo)) return false;
        nivelForca++;
        return true;
    }

    // --- SORTE ---
    public float GetMultiplicadorSorte()
    {
        return 1f + (nivelSorte - 1) * 0.1f; // cada nível adiciona 10% na recompensa
    }

    public int GetCustoProximoNivelSorte()
    {
        return Mathf.RoundToInt(custoBaseSorte * Mathf.Pow(multiplicadorCusto, nivelSorte - 1));
    }

    public bool UpgradeSorte()
    {
        int custo = GetCustoProximoNivelSorte();
        if (!GameManager.Instance.GastarDinheiro(custo)) return false;
        nivelSorte++;
        return true;
    }

    // --- FORTUNA ---
    public float GetMultiplicadorFortuna()
    {
        return 1f + (nivelFortuna - 1) * 0.1f; // começa em 1.0, sobe 0.1 por nível
    }

    public int GetCustoProximoNivelFortuna()
    {
        return Mathf.RoundToInt(custoBaseFortuna * Mathf.Pow(multiplicadorCusto, nivelFortuna - 1));
    }

    public bool UpgradeFortuna()
    {
        int custo = GetCustoProximoNivelFortuna();
        if (!GameManager.Instance.GastarDinheiro(custo)) return false;
        nivelFortuna++;
        return true;
    }

    // --- HABILIDADES ---
    public bool TemHabilidade(string id)
    {
        return habilidadesDesbloqueadas.Contains(id);
    }

    public bool ComprarHabilidade(HabilidadeDados hab)
    {
        if (TemHabilidade(hab.id)) return false;

        // Verifica requisitos
        foreach (string req in hab.requisitoIds)
        {
            if (!TemHabilidade(req))
            {
                Debug.Log("Requisito não atendido: " + req);
                return false;
            }
        }

        if (!GameManager.Instance.GastarDinheiro(hab.custo)) return false;

        habilidadesDesbloqueadas.Add(hab.id);

        // Aplica o bônus da habilidade
        switch (hab.tipo)
        {
            case TipoHabilidade.Forca:
                nivelForca += Mathf.RoundToInt(hab.valorBonus);
                break;
            case TipoHabilidade.Sorte:
                nivelSorte += Mathf.RoundToInt(hab.valorBonus);
                break;
            case TipoHabilidade.Fortuna:
                nivelFortuna += Mathf.RoundToInt(hab.valorBonus);
                break;
        }

        return true;
    }

    public List<string> GetHabilidadesDesbloqueadas()
    {
        return habilidadesDesbloqueadas;
    }

    public void CarregarStatus(StatusJogador dados)
    {
        nivelForca = dados.nivelForca;
        nivelSorte = dados.nivelSorte;
        nivelFortuna = dados.nivelFortuna;
        habilidadesDesbloqueadas = new List<string>(dados.habilidadesDesbloqueadas);
    }
}