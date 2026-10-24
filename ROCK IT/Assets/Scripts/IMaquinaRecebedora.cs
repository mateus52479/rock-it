using UnityEngine;

/// <summary>
/// Implementado pelas máquinas que aceitam uma pedra arrastada.
/// O PedraManager procura a máquina ativa mais próxima que aceite a pedra.
/// </summary>
public interface IMaquinaRecebedora
{
    bool PodeReceber(PedraManager pedra);
    void ReceberPedra(PedraManager pedra);
}
