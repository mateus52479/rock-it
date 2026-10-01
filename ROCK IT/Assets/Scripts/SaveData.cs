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
public class StatusJogador
{
    public int nivelForca = 1;
    public int nivelSorte = 1;
    public int nivelFortuna = 1;
    public string[] habilidadesDesbloqueadas = new string[0];
}

[Serializable]
public class SaveData
{
    public float dinheiro;
    public List<PedrasSalva> pedras = new List<PedrasSalva>();
    public StatusJogador status = new StatusJogador();
}