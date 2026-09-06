using System;
using System.Collections.Generic;

[Serializable]
public class PedrasSalva
{
    public string nomePedra;
    public float posX;
    public float posY;
    public int vidaAtual;
}

[Serializable]
public class SaveData
{
    public float dinheiro;
    public List<PedrasSalva> pedras = new List<PedrasSalva>();
}