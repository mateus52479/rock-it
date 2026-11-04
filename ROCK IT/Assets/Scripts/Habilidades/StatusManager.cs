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
    [Header("Custos iniciais")]
    [Tooltip("Preço do primeiro upgrade de Força. O primeiro sai logo depois " +
             "da pedra grátis do tutorial.")]
    public int custoBaseForca = 10;

    [Tooltip("Preço do primeiro upgrade de Sorte. Um pouco mais caro que Força " +
             "porque também melhora as chances e reduz o prejuízo.")]
    public int custoBaseSorte = 15;

    [Tooltip("Preço do primeiro upgrade de Fortuna. É o status que mais mexe no " +
             "lucro, então começa um degrau acima dos outros.")]
    public int custoBaseFortuna = 20;

    [Header("Curva de custo")]
    [Tooltip("Quanto o próximo nível custa em relação ao anterior. 1.65 deixa " +
             "o começo acessível; a aceleração abaixo segura o fim do jogo.")]
    public float multiplicadorCustoNivel = 1.65f;

    [Tooltip("Aceleração do custo. A cada nível o preço cresce um pouco mais " +
             "que o anterior, então os upgrades nunca ficam trivialmente baratos " +
             "no fim do jogo.")]
    public float aceleracaoCusto = 0.008f;

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

    [Tooltip("Multiplicador de dano por nível. 1.55 mantém o dano crescendo " +
             "sem transformar as pedras caras em um clique só.")]
    public float fatorDanoNivel = 1.55f;

    [Tooltip("Teto do dano. A vida das pedras fica na casa dos milhares, então esse " +
             "limite não muda a jogabilidade — só impede o número de estourar em níveis absurdos.")]
    public float danoMaximo = 1e9f;

    public float GetDanoForca()
    {
        return CalcularPotencia(danoBase, fatorDanoNivel, nivelForca, danoMaximo);
    }

    /// <summary>Dano que a Força teria no nível seguinte. Usado na vitrine da loja.</summary>
    public float GetDanoProximoNivel()
    {
        return CalcularPotencia(danoBase, fatorDanoNivel, nivelForca + 1, danoMaximo);
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

    /// <summary>
    /// Custo do próximo nível: um multiplicador fixo por nível mais uma
    /// aceleração que cresce com o quadrado do nível. A aceleração é o que
    /// impede o dinheiro de crescer mais rápido que os upgrades no fim do jogo.
    /// </summary>
    float CalcularCusto(int baseCusto, int nivelAtual)
    {
        double n = Mathf.Max(0, nivelAtual - 1);

        double valor = baseCusto
            * System.Math.Pow(Mathf.Max(1.01f, multiplicadorCustoNivel), n)
            * System.Math.Pow(1.0 + Mathf.Max(0f, aceleracaoCusto), n * n);

        if (double.IsNaN(valor) || valor <= 0.0)
            return 1f;

        return Mathf.Max(1f,
            (float)System.Math.Min(valor, System.Math.Max(0.0, custoMaximo)));
    }

    public float GetCustoProximoNivelForca()
    {
        return CalcularCusto(custoBaseForca, nivelForca);
    }

    public bool UpgradeForca()
    {
        float custo = GetCustoProximoNivelForca();
        if (!GameManager.Instance.GastarDinheiro(custo)) return false;
        nivelForca++;
        AudioManager.Compra();
        return true;
    }

    // --- SORTE ---
    /// <summary>
    /// Cada nível de Sorte move 1,5 ponto percentual dos piores resultados
    /// (prejuízo primeiro, depois o zero e os prêmios menores) para o prêmio
    /// máximo. O teto é toda a chance fora do prêmio máximo da tabela mais
    /// arriscada (Pedacinho do Sol: 50 + 15 + 30 = 95 pontos). Assim a Sorte
    /// só chega ao fim no nível 65, quando toda pedra passa a pagar o prêmio
    /// máximo. Além disso, o valor do prejuízo encolhe 2% por nível (no
    /// máximo 30%).
    /// </summary>
    public const float DeslocamentoSortePorNivel = 1.5f;

    /// <summary>
    /// Quanto a chance se desloca para o prêmio máximo, no total. 95 é o
    /// máximo que as tabelas permitem drenar sem tocar no próprio prêmio
    /// máximo, então no nível 65 o prêmio máximo fica com 100% de chance.
    /// </summary>
    public const float DeslocamentoSorteMaximo = 95f;

    /// <summary>Quanto do prejuízo original encolhe em cada nível de Sorte.</summary>
    public const float ReducaoPerdaPorNivel = 0.02f;

    /// <summary>O prejuízo nunca fica menor que 70% do valor original.</summary>
    public const float FatorPerdaMinimo = 0.70f;

    public static float DeslocamentoSorteNoNivel(int nivel)
    {
        return Mathf.Clamp(
            (Mathf.Max(1, nivel) - 1) * DeslocamentoSortePorNivel,
            0f, DeslocamentoSorteMaximo);
    }

    public static float FatorPerdaNoNivel(int nivel)
    {
        return Mathf.Clamp(
            1f - ReducaoPerdaPorNivel * Mathf.Max(0, nivel - 1),
            FatorPerdaMinimo, 1f);
    }

    public float GetDeslocamentoSorte()
    {
        return DeslocamentoSorteNoNivel(nivelSorte);
    }

    public float GetDeslocamentoSorteProximoNivel()
    {
        return DeslocamentoSorteNoNivel(nivelSorte + 1);
    }

    /// <summary>Quanto o prejuízo já encolheu, em pontos percentuais.</summary>
    public float GetReducaoPerda()
    {
        return (1f - FatorPerdaNoNivel(nivelSorte)) * 100f;
    }

    /// <summary>Redução de prejuízo que a Sorte teria no nível seguinte.</summary>
    public float GetReducaoPerdaProximoNivel()
    {
        return (1f - FatorPerdaNoNivel(nivelSorte + 1)) * 100f;
    }

    /// <summary>
    /// A Sorte termina quando toda a chance fora do prêmio máximo já foi
    /// drenada (95 pontos, nível 65) e o prejuízo já encolheu 30%.
    /// </summary>
    public bool SorteNoMaximo()
    {
        return DeslocamentoSorteNoNivel(nivelSorte) >= DeslocamentoSorteMaximo
            && FatorPerdaNoNivel(nivelSorte) <= FatorPerdaMinimo + 0.0001f;
    }

    public float GetCustoProximoNivelSorte()
    {
        return CalcularCusto(custoBaseSorte, nivelSorte);
    }

    public bool UpgradeSorte()
    {
        float custo = GetCustoProximoNivelSorte();
        if (!GameManager.Instance.GastarDinheiro(custo)) return false;
        nivelSorte++;
        AudioManager.Compra();
        return true;
    }

    // --- FORTUNA ---
    [Tooltip("Quanto o lucro aumenta por nível de Fortuna. 0.08 = +8% do valor " +
             "base por nível. O crescimento é linear, então nunca vira um " +
             "multiplicador absurdo no fim do jogo.")]
    public float bonusFortunaPorNivel = 0.08f;

    [Tooltip("Teto do multiplicador de Fortuna. Serve só como proteção para " +
             "valores extremos.")]
    public float multiplicadorFortunaMaximo = 1e15f;

    public float GetMultiplicadorFortuna()
    {
        return MultiplicadorFortunaNoNivel(nivelFortuna);
    }

    /// <summary>Multiplicador que a Fortuna teria no nível seguinte.</summary>
    public float GetMultiplicadorFortunaProximoNivel()
    {
        return MultiplicadorFortunaNoNivel(nivelFortuna + 1);
    }

    float MultiplicadorFortunaNoNivel(int nivel)
    {
        double multiplicador = 1.0
            + Mathf.Max(0f, bonusFortunaPorNivel) * Mathf.Max(0, nivel - 1);

        double teto = Mathf.Max(1f, multiplicadorFortunaMaximo);

        return (float)System.Math.Min(
            System.Math.Max(1.0, multiplicador),
            teto);
    }

    public float GetCustoProximoNivelFortuna()
    {
        return CalcularCusto(custoBaseFortuna, nivelFortuna);
    }

    public bool UpgradeFortuna()
    {
        float custo = GetCustoProximoNivelFortuna();
        if (!GameManager.Instance.GastarDinheiro(custo)) return false;
        nivelFortuna++;
        AudioManager.Compra();
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

        AudioManager.Compra();
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