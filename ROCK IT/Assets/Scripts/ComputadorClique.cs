using UnityEngine;

public class ComputadorClique : MonoBehaviour
{
    void OnMouseDown()
    {
        LojaManager.Instance.AbrirLoja();
    }
}