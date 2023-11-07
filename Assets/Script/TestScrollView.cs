using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestScrollView : ScrollViewScript<int>
{
    // Start is called before the first frame update
    void Awake()
    {
        actions.Add(UpdateItem);
    }


    private void UpdateItem(ItemTransformData cell,int index)
    {
        cell.item.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "µ¥Ôª¸ñ "+ data[index];
    }
}
