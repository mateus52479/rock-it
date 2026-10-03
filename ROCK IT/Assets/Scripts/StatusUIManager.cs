using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusUIManager : MonoBehaviour
{
    public Transform containerStatus;
    public GameObject prefabItemStatus;

    void OnEnable()
    {
        PopularStatus();
    }

    void PopularStatus()
    {
        foreach (Transform filho in containerStatus)
            Destroy(filho.gameObject);

        for (int i = 0; i < 3; i++)
        {
            GameObject item = Instantiate(prefabItemStatus, containerStatus);

            // Força o tamanho do item
            RectTransform rt = item.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(0, rt.offsetMin.y);
            rt.offsetMax = new Vector2(0, rt.offsetMax.y);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 80);

            ItemStatusUI ui = item.GetComponent<ItemStatusUI>();
            ui.Configurar(i);
        }

        // Força o Content a recalcular
        LayoutRebuilder.ForceRebuildLayoutImmediate(containerStatus as RectTransform);
    }
}