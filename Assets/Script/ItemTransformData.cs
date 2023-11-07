using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemTransformData
{
    public Vector2 pos
    {
        get
        {
            return _pos;
        }
        set
        {
            _pos = value;
            item.transform.localPosition = pos;
        }
    }

    public float height
    {
        get
        {
            if (_item_rect)
            {
                return _item_rect.rect.height;
            }
            else
            {
                return 0f;
            }
        }
    }

    public float width
    {
        get
        {
            if (_item_rect)
            {
                return _item_rect.rect.width;
            }
            else
            {
                return 0f;
            }
        }
    }

    public Vector2 size
    {
        set
        {
            _item_rect.sizeDelta = value;
        }
    }

    public int cell_index { get; set; }

    public int item_index { get; set; }

    public Button item
    {
        get
        {
            return _item;
        }
        set
        {
            _item = value;
            _item_rect = _item.GetComponent<RectTransform>();
        }
    }

    public int item_type { get; set; }

    public ItemTransformData next { get; set; }

    public ItemTransformData parent { get; set; }

    private Button _item;

    private RectTransform _item_rect;

    private Vector2 _pos;

    public void CloneTo(ItemTransformData itd)
    {
        itd.pos = pos;
        itd.cell_index = cell_index;
        itd.item_index = item_index;
        itd.item = item;
        itd.item_type = item_type;
    }

    public ItemTransformData Clone()
    {
        ItemTransformData itd = new ItemTransformData();
        CloneTo(itd);
        return itd;
    }
}
