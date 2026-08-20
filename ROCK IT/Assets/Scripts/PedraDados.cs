using UnityEngine;

[System.Serializable]
public class PedraDados
{
    public string nome;
    public string descricao;
    public Color cor;           // placeholder enquanto não tem sprite
    public int precoCompra;     // quanto custa na loja
    public int recompensaMin;   // pode ser negativo
    public int recompensaMax;
    public int vidaMaxima;
}