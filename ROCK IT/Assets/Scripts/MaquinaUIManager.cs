using UnityEngine;

public class MaquinaUIManager : MonoBehaviour
{
    public Transform containerMaquinas;
    public GameObject prefabItemMaquina;

    void OnEnable()
    {
        PopularMaquinas();
    }

    void PopularMaquinas()
    {
        foreach (Transform filho in containerMaquinas)
            Destroy(filho.gameObject);

        foreach (MaquinaDados maquina in MaquinaDatabase.Instance.todasMaquinas)
        {
            // Não mostra upgrades como itens separados
            bool eUpgrade = false;
            foreach (MaquinaDados m in MaquinaDatabase.Instance.todasMaquinas)
                if (m.idUpgrade == maquina.id) { eUpgrade = true; break; }
            if (eUpgrade) continue;

            GameObject item = Instantiate(prefabItemMaquina, containerMaquinas);
            ItemMaquinaUI ui = item.GetComponent<ItemMaquinaUI>();
            ui.Configurar(maquina);
        }
    }
}