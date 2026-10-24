using System;
using UnityEngine;

[Serializable]
public class PassoDialogo
{
    public string id;
    public string personagem;
    public string texto;

    [Header("Posição do NPC (pixels a partir do centro da tela)")]
    public float npcX;
    public float npcY;

    [Tooltip("Nome de um objeto da cena/UI, 'pedra' ou 'itemLoja'.")]
    public string destacar;

    [Tooltip("clique | pergunta | abrirLoja | fecharLoja | comprarPedra | quebrarPedra | painelPedras | painelStatus | painelHabilidades | painelMaquinas")]
    public string esperar;

    [Tooltip("Usado quando esperar = pergunta.")]
    public string[] respostas;

    [Tooltip("Se a resposta escolhida não for a primeira, pula para o passo com este id. Vazio encerra.")]
    public string pularPara;

    [Header("Ações antes do passo")]
    public bool abrirLoja;
    public bool fecharLoja;
    public string painel; // pedras, status, habilidades, maquinas
}

[Serializable]
public class RoteiroDialogo
{
    public PassoDialogo[] passos;
}
