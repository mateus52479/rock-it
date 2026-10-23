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

    [Header("Curva de custo")]
    [Tooltip("Quanto o próximo nível custa em relação ao anterior. " +
             "1.6 acompanha o ganho de dinheiro das pedras; 2.5 tornava o nível 20 inalcançável.")]
    public float multiplicadorCusto = 1.6f;

    void Awake()
    {
        Instance = this;
    }

    // --- FORÇA ---
    [Header("Progressão de Força")]
    [Tooltip("Dano aplicado no nível 1.")]
    public float danoBase = 1f;

    [Tooltip("Multiplicador de dano por nível. 1.75 acompanha a vida das pedras " +
             "(o dano cresce no mesmo ritmo em que as pedras ficam mais duras).")]
    public float fatorDanoPorNivel = 1.75f;

    public float GetDanoForca()
    {
        return danoBase * Mathf.Pow(fatorDanoPorNivel, Mathf.Max(0, nivelForca - 1));
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
    /// <summary>
    /// Cada nível de Sorte move 1 ponto percentual de chance do pior resultado
    /// da pedra para o prêmio máximo. É isso que permite virar o jogo.
    /// </summary>
    public float GetDeslocamentoSorte()
    {
        return Mathf.Max(0, nivelSorte - 1);
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
    [Tooltip("Multiplicador de Fortuna por nível. Fortuna multiplica apenas recompensas positivas.")]
    public float fatorFortunaPorNivel = 1.25f;

    public float GetMultiplicadorFortuna()
    {
        return Mathf.Pow(fatorFortunaPorNivel, Mathf.Max(0, nivelFortuna - 1));
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

/// <summary>
/// Formata números grandes de forma legível (1,8 mil / 36,4 mi / 700,0 bi).
/// </summary>
public static class NumeroUtil
{
    private static readonly string[] sufixos = { "", " mil", " mi", " bi", " tri", " qua" };

    public static string Formatar(float valor, int casasDecimais = 1)
    {
        float absoluto = Mathf.Abs(valor);

        if (absoluto < 1000f)
            return valor.ToString("F" + Mathf.Max(0, casasDecimais));

        int ordem = Mathf.Min((int)(Mathf.Log10(absoluto) / 3f), sufixos.Length - 1);
        float reduzido = valor / Mathf.Pow(1000f, ordem);
        int casas = Mathf.Abs(reduzido) >= 100f ? 0 : 1;

        return reduzido.ToString("F" + casas) + sufixos[ordem];
    }

    public static string FormatarDinheiro(float valor)
    {
        return "R$ " + Formatar(valor);
    }
}