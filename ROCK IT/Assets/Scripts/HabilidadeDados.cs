using UnityEngine;

[System.Serializable]
public class HabilidadeDados
{
    public string id;
    public string nome;
    public string descricao;
    public int custo;
    public string[] requisitoIds; // IDs das habilidades necessárias antes
    public TipoHabilidade tipo;
    public float valorBonus; // quanto essa habilidade adiciona ao status
}

public enum TipoHabilidade
{
    Forca,
    Sorte,
    Fortuna,
    Geral
}