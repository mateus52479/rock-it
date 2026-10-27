using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Escuta o carregamento das cenas e reaplica o tema em cada uma.</summary>
public class EstiloUIHook : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
    }

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        EstiloUI.Aplicar();
    }
}
