
using System;
using System.Collections.Generic;

[Serializable]
public class PedrasSalva
{
    public string nomePedra;
    public float posX;
    public float posY;
    public int vidaAtual;

    // Resultado sorteado quando a pedra foi comprada. Sem isso, carregar o
    // save sorteava de novo e o Scanner trocava a cor da pedra.
    public int resultadoSorteado;

    // Diferencia "resultado 0 de verdade" de um save antigo, que não tinha
    // esse campo. Save antigo continua sorteando na primeira carga.
    public bool temResultadoSorteado;

    // O primeiro Carvão tem prêmio máximo garantido: a garantia precisa
    // sobreviver ao save para continuar valendo depois de carregar.
    public bool garantirMaximo;

    // Segundos que a pedra já fermentou na mesa. Sem isso, carregar o save
    // zerava o bônus da Fermentação.
    public float tempoFermentacao;
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

    public List<string> maquinasCompradas = new List<string>();

    // Registra se o primeiro Carvão já foi comprado.
    public bool primeiraComumComprada = false;
}
