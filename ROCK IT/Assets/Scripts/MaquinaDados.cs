using UnityEngine;

[System.Serializable]
public class MaquinaDados
{
    public string id;
    public string nome;
    public string descricao;
    public int preco;
    public string idUpgrade; // id da maquina que substitui esta, se houver
}