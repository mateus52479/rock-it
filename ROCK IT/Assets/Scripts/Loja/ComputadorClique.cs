using UnityEngine;
using UnityEngine.EventSystems;

public class ComputadorClique : MonoBehaviour
{
    void OnMouseDown()
    {
        // Sem esta guarda, clicar em qualquer botão da UI que fique sobre o
        // computador (fechar as Configurações, por exemplo) atravessa o
        // canvas e abre a loja sem o jogador querer.
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        if (LojaManager.Instance != null)
            LojaManager.Instance.AbrirLoja();
    }
}