using System.Collections.Generic;
using TMPro;

public class TestScrollView : ScrollViewScript<int>
{
    private List<int> _subData = new List<int>();

    void Awake()
    {
        for (int i = 0; i < 10; i++)
        {
            _subData.Add(i);
        }

        actions.Add(UpdateItem);
        actions.Add(UpdateItem);
    }

    protected override void InitItemType()
    {
        List<int[]> type = new List<int[]>();

        int i = 0;
        int j = 0;
        while (i < data.Count || j < _subData.Count)
        {
            if (i < data.Count)
            {
                type.Add(new int[] { 0, i++ });
            }
            if (j < _subData.Count)
            {
                type.Add(new int[] { 1, j++ });
            }
        }

        itemType = type;
    }

    private void UpdateItem(ItemTransformData cell, int index)
    {
        cell.item.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "µ¥Ôª¸ñ " + data[index];
    }
}
