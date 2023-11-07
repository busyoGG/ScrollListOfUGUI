using UnityEngine;
using UnityEngine.UI;

public class ItemTransformData
{
    /// <summary>
    /// 坐标
    /// </summary>
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
    /// <summary>
    /// 高度
    /// </summary>
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
    /// <summary>
    /// 宽度
    /// </summary>
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
    /// <summary>
    /// 宽高
    /// </summary>
    public Vector2 size
    {
        set
        {
            _item_rect.sizeDelta = value;
        }
    }
    /// <summary>
    /// 虚拟列表索引
    /// </summary>
    public int cell_index { get; set; }
    /// <summary>
    /// 数据列表索引
    /// </summary>
    public int item_index { get; set; }
    /// <summary>
    /// 单元格对象
    /// </summary>
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
    /// <summary>
    /// 单元格类型
    /// </summary>
    public int item_type { get; set; }
    /// <summary>
    /// 下一单元格
    /// </summary>
    public ItemTransformData next { get; set; }
    /// <summary>
    /// 上一单元格
    /// </summary>
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
