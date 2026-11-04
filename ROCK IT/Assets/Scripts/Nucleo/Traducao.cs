using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Tradução do jogo. O português é o texto original e serve de chave; as
/// versões em inglês ficam em Resources/Idiomas/ingles.json, no formato
/// { "pt": "Texto em português", "en": "English text" }.
///
/// Para corrigir ou acrescentar uma frase, edite só esse JSON. Nada aqui
/// precisa ser recompilado: o arquivo é lido na primeira tradução.
/// </summary>
public static class Traducao
{
    public const string IdiomaPortugues = "pt-BR";
    public const string IdiomaIngles = "en";

    /// <summary>Idioma em uso agora.</summary>
    public static bool Ingles { get; private set; }

    /// <summary>Avisa quem precisa reescrever textos montados por código.</summary>
    public static event Action OnIdiomaMudou;

    /// <summary>Sufixos de número grande: 1.5K / 1.5M (ou 1.5 mil / 1.5 mi).</summary>
    private static readonly string[] sufixosPortugues =
    {
        "", " mil", " mi", " bi", " tri", " qua",
        " qui", " sex", " sep", " oct", " non", " dec"
    };

    private static readonly string[] sufixosIngles =
    {
        "", "K", "M", "B", "T", "Qa",
        "Qi", "Sx", "Sp", "Oc", "No", "Dc"
    };

    private static Dictionary<string, string> paraIngles;
    private static Dictionary<string, string> paraPortugues;
    private static bool carregado;

    [Serializable]
    private class Par
    {
        public string pt = null;
        public string en = null;
    }

    [Serializable]
    private class Arquivo
    {
        public Par[] textos = null;
    }

    /// <summary>Lê o idioma salvo antes da primeira cena montar a interface.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Iniciar()
    {
        // Só lê a preferência aqui. O JSON é carregado no primeiro texto, já
        // com a cena montada, para não depender do Resources antes da hora.
        Ingles = PlayerPrefs.GetString("Idioma", IdiomaPortugues) == IdiomaIngles;
    }

    /// <summary>
    /// Troca o idioma, salva e repinta tudo que já está na tela.
    /// </summary>
    public static void DefinirIdioma(string codigo)
    {
        Ingles = codigo == IdiomaIngles;

        PlayerPrefs.SetString("Idioma", Ingles ? IdiomaIngles : IdiomaPortugues);
        PlayerPrefs.Save();

        AplicarEmTudo();

        if (OnIdiomaMudou != null)
            OnIdiomaMudou();
    }

    public static string[] SufixosMilhar
    {
        get { return Ingles ? sufixosIngles : sufixosPortugues; }
    }

    /// <summary>
    /// Traduz um texto. Em português devolve o original; em inglês devolve a
    /// versão do JSON, ou o próprio texto quando não existe tradução.
    /// </summary>
    public static string T(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;

        CarregarTextos();

        if (Ingles)
        {
            if (paraIngles.TryGetValue(texto, out string ingles) &&
                !string.IsNullOrEmpty(ingles))
            {
                return ingles;
            }

            return texto;
        }

        // Ao voltar para o português, o texto na tela está em inglês.
        if (paraPortugues.TryGetValue(texto, out string portugues))
            return portugues;

        return texto;
    }

    /// <summary>Traduz e preenche os {0}, {1}... do texto.</summary>
    public static string TF(string texto, params object[] valores)
    {
        string traduzido = T(texto);

        if (valores == null || valores.Length == 0)
            return traduzido;

        try
        {
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                traduzido,
                valores);
        }
        catch (FormatException)
        {
            // Chave sem espaços de formatação: devolve como está.
            return traduzido;
        }
    }

    /// <summary>
    /// Repassa todos os textos da interface pelo tradutor. Chamado depois de
    /// montar as telas e sempre que o idioma muda.
    /// </summary>
    public static void AplicarEmTudo()
    {
        TextMeshProUGUI[] textos = UnityEngine.Object.FindObjectsByType<
            TextMeshProUGUI>(FindObjectsInactive.Include);

        foreach (TextMeshProUGUI texto in textos)
        {
            if (texto == null)
                continue;

            string atual = texto.text;

            if (string.IsNullOrEmpty(atual))
                continue;

            string novo = T(atual);

            if (novo != atual)
                texto.text = novo;
        }
    }

    private static void CarregarTextos()
    {
        if (carregado)
            return;

        carregado = true;

        paraIngles = new Dictionary<string, string>();
        paraPortugues = new Dictionary<string, string>();

        TextAsset arquivo = Resources.Load<TextAsset>("Idiomas/ingles");

        if (arquivo == null)
        {
            Debug.LogWarning(
                "Traducao: Resources/Idiomas/ingles.json não encontrado. " +
                "A interface segue em português.");
            return;
        }

        Arquivo dados = JsonUtility.FromJson<Arquivo>(arquivo.text);

        if (dados == null || dados.textos == null)
            return;

        foreach (Par par in dados.textos)
        {
            if (par == null || string.IsNullOrEmpty(par.pt))
                continue;

            paraIngles[par.pt] = par.en;

            // A volta é usada quando o jogador volta para o português.
            if (!string.IsNullOrEmpty(par.en) &&
                !paraPortugues.ContainsKey(par.en))
            {
                paraPortugues[par.en] = par.pt;
            }
        }
    }
}
