using UnityEngine;

public class GameLoader : MonoBehaviour
{
    public static bool deveCarregar = false;

    void Start()
    {
        if (deveCarregar)
        {
            deveCarregar = false;
            SaveManager.Instance.Carregar();
        }
    }
}