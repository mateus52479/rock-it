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

    [Tooltip("Teto do custo de um nível. Sem isso o valor estourava o limite de " +
             "número inteiro por volta do nível 39 e ficava negativo (upgrade de graça).")]
    public float custoMaximo = 1e30f;

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

    [Tooltip("Teto do dano. A vida das pedras fica na casa dos milhares, então esse " +
             "limite não muda a jogabilidade — só impede o número de estourar em níveis absurdos.")]
    public float danoMaximo = 1e9f;

    public float GetDanoForca()
    {
        return CalcularPotencia(danoBase, fatorDanoPorNivel, nivelForca, danoMaximo);
    }

    /// <summary>
    /// Calcula base * fator^(nivel-1) usando double e limita o resultado.
    /// Antes o valor era convertido com Mathf.RoundToInt: acima de ~2,1 bilhões
    /// a conversão estourava e virava int.MinValue. Era por isso que, em níveis
    /// altos, o dano caía para 1 (a "forma básica") e o custo ficava negativo.
    /// </summary>
    static float CalcularPotencia(float baseValor, float fator, int nivel, float teto)
    {
        double valor = baseValor * System.Math.Pow(
            Mathf.Max(0.001f, fator),
            Mathf.Max(0, nivel - 1));

        if (double.IsNaN(valor) || valor <= 0.0)
            return 0f;

        return (float)System.Math.Min(valor, System.Math.Max(0.0, teto));
    }

    public float GetCustoProximoNivelForca()
    {
        return Mathf.Max(1f,
            CalcularPotencia(custoBaseForca, multiplicadorCusto, nivelForca, custoMaximo));
    }

    public bool UpgradeForca()
    {
        float custo = GetCustoProximoNivelForca();
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
        // O deslocamento nunca passa de 100 pontos: acima disso a chance já
        // teria migrado toda a perda para o prêmio máximo.
        return Mathf.Clamp(nivelSorte - 1, 0, 100);
    }

    public float GetCustoProximoNivelSorte()
    {
        return Mathf.Max(1f,
            CalcularPotencia(custoBaseSorte, multiplicadorCusto, nivelSorte, custoMaximo));
    }

    public bool UpgradeSorte()
    {
        float custo = GetCustoProximoNivelSorte();
        if (!GameManager.Instance.GastarDinheiro(custo)) return false;
        nivelSorte++;
        return true;
    }

    // --- FORTUNA ---
    [Tooltip("Multiplicador de Fortuna por nível. Fortuna multiplica apenas recompensas positivas.")]
    public float fatorFortunaPorNivel = 1.25f;

    [Tooltip("Teto do multiplicador de Fortuna. A partir daqui o valor fica fixo, " +
             "em vez de virar infinito (o que zerava a recompensa na prática).")]
    public float multiplicadorFortunaMaximo = 1e15f;

    public float GetMultiplicadorFortuna()
    {
        float multiplicador = CalcularPotencia(
            1f, fatorFortunaPorNivel, nivelFortuna, multiplicadorFortunaMaximo);

        return Mathf.Max(1f, multiplicador);
    }

    public float GetCustoProximoNivelFortuna()
    {
        return Mathf.Max(1f,
            CalcularPotencia(custoBaseFortuna, multiplicadorCusto, nivelFortuna, custoMaximo));
    }

    public bool UpgradeFortuna()
    {
        float custo = GetCustoProximoNivelFortuna();
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
        // Um save antigo com valor estourado podia trazer nível negativo e,
        // com isso, o status "voltava" para um valor sem sentido.
        nivelForca = Mathf.Max(1, dados.nivelForca);
        nivelSorte = Mathf.Max(1, dados.nivelSorte);
        nivelFortuna = Mathf.Max(1, dados.nivelFortuna);

        habilidadesDesbloqueadas = dados.habilidadesDesbloqueadas != null
            ? new List<string>(dados.habilidadesDesbloqueadas)
            : new List<string>();
    }
}

/// <summary>
/// Formata números grandes de forma legível (1,8 mil / 36,4 mi / 700,0 bi).
/// </summary>
public static class NumeroUtil
{
    public static string Formatar(float valor, int casasDecimais = 1)
    {
        if (float.IsNaN(valor))
            return "0";

        if (float.IsInfinity(valor))
            return valor > 0f ? "∞" : "-∞";

        float absoluto = Mathf.Abs(valor);

        if (absoluto < 1000f)
            return valor.ToString("F" + Mathf.Max(0, casasDecimais));

        int ordem = Mathf.FloorToInt(Mathf.Log10(absoluto) / 3f);

        // Acima de "dec" (10^33) não existe sufixo curto: usa notação científica.
        if (ordem <= 0)
            return valor.ToString("F" + Mathf.Max(0, casasDecimais));

        if (ordem >= Traducao.SufixosMilhar.Length)
            return valor.ToString("0.##E+0");

        float reduzido = valor / Mathf.Pow(1000f, ordem);
        int casas = Mathf.Abs(reduzido) >= 100f ? 0 : 1;

        return reduzido.ToString("F" + casas) + Traducao.SufixosMilhar[ordem];
    }

    /// <summary>
    /// Dinheiro com centavos enquanto o valor é pequeno e com sufixo
    /// (mil/mi/bi...) quando fica grande demais para caber na tela.
    /// </summary>
    public static string FormatarDinheiro(float valor)
    {
        if (Mathf.Abs(valor) < 1e6f)
            return "R$ " + valor.ToString("F2");

        return "R$ " + Formatar(valor);
    }
}