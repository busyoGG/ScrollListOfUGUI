using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class ScrollViewScript<T> : MonoBehaviour
{
    /// <summary>
    /// ��Ԫ��Ԥ����
    /// </summary>
    public List<Button> PrefabItem;
    /// <summary>
    /// ��Ԫ����սڵ�
    /// </summary>
    public GameObject Stack;
    /// <summary>
    /// �����ڵ�
    /// </summary>
    public ScrollRect Scroll;
    /// <summary>
    /// ����Ӧ��С
    /// </summary>
    public bool FitSize;
    /// <summary>
    /// X����Ԫ����
    /// </summary>
    public float SpaceX;
    /// <summary>
    /// Y����Ԫ����
    /// </summary>
    public float SpaceY;
    /// <summary>
    /// X����ƫ��
    /// </summary>
    public float OffsetX;
    /// <summary>
    /// Y����ƫ��
    /// </summary>
    public float OffsetY;
    /// <summary>
    /// X�����ظ���Ԫ�����
    /// </summary>
    public int RepeatX;
    /// <summary>
    /// Y�����ظ���Ԫ�����
    /// </summary>
    public int RepeatY;
    /// <summary>
    /// Ĭ���б�����
    /// </summary>
    public List<T> data
    {
        get
        {
            return _data;
        }
        set
        {
            RefreshList(value);
        }
    }
    /// <summary>
    /// �б�Ԫ������
    /// </summary>
    protected List<int[]> itemType
    {
        get
        {
            return _itemType;
        }
        set
        {
            _itemType = value;
        }
    }

    protected List<Action<ItemTransformData, int>> actions
    {
        get
        {
            return _actions;
        }
    }
    private bool _inited = false;
    private List<int[]> _itemType = new List<int[]>();
    /// <summary>
    /// Ĭ���б�����
    /// </summary>
    private List<T> _data;
    /// <summary>
    /// ��Ԫ�������������ֵ
    /// </summary>
    private int _length;
    /// <summary>
    /// ��ʾ�ĵ�Ԫ���б�
    /// </summary>
    private List<ItemTransformData> _items = new List<ItemTransformData>();
    /// <summary>
    /// ���յĵ�Ԫ���б�
    /// </summary>
    private List<Stack<ItemTransformData>> _itemStack = new List<Stack<ItemTransformData>>();
    /// <summary>
    /// content���transform
    /// </summary>
    private RectTransform _rectTransform;
    /// <summary>
    /// content��ͼ�߶�
    /// </summary>
    private float _viewHeight;
    /// <summary>
    /// content��ͼ���
    /// </summary>
    private float _viewWidth;

    //private int _repeatX;

    private ItemTransformData _head;

    private ItemTransformData _tail;

    private Vector2 _lastScrollPos = new Vector2();

    private List<Vector2> _itemPos = new List<Vector2>();

    private Dictionary<int, Dictionary<int, Vector2>> _itemSize = new Dictionary<int, Dictionary<int, Vector2>>();

    private int _scrollDirection = 0;

    private List<Action<ItemTransformData, int>> _actions = new List<Action<ItemTransformData, int>>();

    public void Start()
    {
        _rectTransform = GetComponent<RectTransform>();

        _viewWidth = transform.parent.GetComponent<RectTransform>().rect.width;
        _viewHeight = transform.parent.GetComponent<RectTransform>().rect.height;

        for (int i = 0, len = PrefabItem.Count; i < len; i++)
        {
            //��������ջ�ڵ��Ӧ���͵Ļ��սڵ�
            GameObject obj = new GameObject();
            obj.name = "ItemType_" + i;
            obj.transform.SetParent(Stack.transform);

            Button item = Instantiate(PrefabItem[i]);
            item.transform.SetParent(obj.transform);
            RectTransform itemRect = item.GetComponent<RectTransform>();

            ItemTransformData itd = new ItemTransformData();
            itd.item = item;
            itd.pos = new Vector2();
            itd.item_type = i;

            if (_itemStack.Count <= i)
            {
                _itemStack.Add(new Stack<ItemTransformData>());
            }
            _itemStack[i].Push(itd);

            for (int j = 0, len2 = (int)MathF.Ceiling(_viewHeight / itd.height); j < len2; j++)
            {
                Button item2 = Instantiate(PrefabItem[i]);
                item2.transform.SetParent(obj.transform);
                RectTransform itemRect2 = item.GetComponent<RectTransform>();

                ItemTransformData itd2 = new ItemTransformData();
                itd2.item = item2;
                itd2.pos = new Vector2();
                itd2.item_type = i;

                _itemStack[i].Push(itd2);
            }
        }

        if (FitSize)
        {
            Scroll.vertical = false;
            Scroll.horizontal = false;
        }
        else
        {
            if (RepeatY != 0)
            {
                Scroll.vertical = false;
            }
            else
            {
                Scroll.horizontal = false;
            }
        }

        Scroll.onValueChanged.AddListener(OnScroll);
        _inited = true;
        if (data != null && data.Count > 0)
        {
            data = data;
        }
    }

    private void OnScroll(Vector2 v)
    {
        ////Debug.Log(v);
        if (_head == null || _tail == null) return;
        float rect;
        float diff;
        if (RepeatY != 0)
        {
            rect = _rectTransform.localPosition.x;
            diff = _lastScrollPos.x - rect;
        }
        else
        {
            rect = _rectTransform.localPosition.y;
            diff = rect - _lastScrollPos.y;
        }

        if (diff > 0)
        {
            _scrollDirection = 1;
        }
        else if (diff < 0)
        {
            _scrollDirection = -1;
        }
        else
        {
            _scrollDirection = 0;
        }

        if (_scrollDirection == 1)
        {
            bool checkRecycleHead;
            if (RepeatY != 0)
            {
                checkRecycleHead = _head.pos.x + _head.width + rect < 0;
            }
            else
            {
                checkRecycleHead = _head.pos.y - _head.height + rect > 0;
            }

            if (checkRecycleHead)
            {
                if (_head.next != null)
                {
                    //Debug.Log("����ͷ" + _head.cell_index + " == " + _head.next.cell_index);
                    RecycleCell(_head.item_type, _head);
                    _head = _head.next;
                }
            }

            bool checkRecycleTail;
            if (RepeatY != 0)
            {
                checkRecycleTail = _tail.pos.x + rect < _viewWidth;
            }
            else
            {
                checkRecycleTail = _tail.pos.y + rect > -_viewHeight;
            }
            //Debug.Log(_tail.pos.y + rectY + " : " + _viewHeight);
            if (checkRecycleTail)
            {
                int index = _tail.cell_index + 1;
                if (index < _itemType.Count)
                {
                    int[] itemType = _itemType[index];
                    ItemTransformData item = CreateItem(itemType[0]);
                    Vector2 pos = _itemPos[index];
                    item.cell_index = index;
                    item.item_index = itemType[1];
                    item.parent = _tail;
                    _tail.next = item;
                    item.pos = pos;
                    item.item.transform.localPosition = pos;
                    item.item.name = index + "";
                    _tail = item;
                    //Debug.Log("����β" + _tail.cell_index + " " + pos + " " + index);
                    if (itemType[0] < _actions.Count)
                    {
                        _actions[itemType[0]](item, item.item_index);
                    }
                }
            }
        }
        else if (_scrollDirection == -1)
        {
            bool checkRecycleHead;
            if (RepeatY != 0)
            {
                checkRecycleHead = _head.pos.x + _head.width + rect > 0;
            }
            else
            {
                checkRecycleHead = _head.pos.y - _head.height + rect < 0;
            }
            //Debug.Log(_head.pos.y - _head.height + rectY);
            if (checkRecycleHead)
            {
                int index = _head.cell_index - 1;
                if (index >= 0)
                {
                    int[] itemType = _itemType[index];
                    ItemTransformData item = CreateItem(itemType[0]);
                    Vector2 pos = _itemPos[index];
                    item.cell_index = index;
                    item.item_index = itemType[1];
                    item.next = _head;
                    _head.parent = item;
                    item.pos = pos;
                    item.item.transform.localPosition = pos;
                    item.item.name = index + "";
                    _head = item;
                    //Debug.Log("����ͷ" + _head.cell_index + " " + pos + " " + index);
                    if (itemType[0] < _actions.Count)
                    {
                        _actions[itemType[0]](item, item.item_index);
                    }
                }
            }

            bool checkRecycleTail;
            if (RepeatY != 0)
            {
                checkRecycleTail = _tail.pos.x + rect > _viewWidth;
            }
            else
            {
                checkRecycleTail = _tail.pos.y + rect < -_viewHeight;
            }
            //Debug.Log(_tail.pos.y + rectY + " : " + -_viewHeight);
            if (checkRecycleTail)
            {
                if (_tail.parent != null)
                {
                    //Debug.Log("����β" + _tail.cell_index);
                    RecycleCell(_tail.item_type, _tail);
                    _tail = _tail.parent;
                }
            }
        }

        _lastScrollPos.x = _rectTransform.localPosition.x;
        _lastScrollPos.y = _rectTransform.localPosition.y;
    }

    /// <summary>
    /// ����ItemTransformData
    /// </summary>
    /// <param name="itemType"></param>
    /// <returns></returns>
    private ItemTransformData CreateItem(int itemType, bool init = true)
    {
        ItemTransformData item;
        //Debug.Log(itemType + "  " + _itemStack.Count);
        if (_itemStack[itemType].Count > 0)
        {
            item = _itemStack[itemType].Pop();
        }
        else
        {
            Button button = Instantiate(PrefabItem[itemType]);
            item = new ItemTransformData();
            item.item = button;
        }

        if (init)
        {
            item.item.transform.SetParent(transform);
            _items.Add(item);
        }
        else
        {
            _itemStack[itemType].Push(item);
        }

        return item;
    }

    /// <summary>
    /// �������е�Ԫ��
    /// </summary>
    private void RecycleAllCells()
    {
        for (int i = 0, len = _items.Count; i < len; i++)
        {
            ItemTransformData item = _items[i];
            item.item.transform.SetParent(Stack.transform.GetChild(item.item_type));
            _itemStack[item.item_type].Push(item);
        }
        _items.Clear();
        _itemPos.Clear();
    }

    /// <summary>
    /// ����һ����Ԫ��
    /// </summary>
    /// <param name="itemType"></param>
    /// <param name="itd"></param>
    private void RecycleCell(int itemType, ItemTransformData itd)
    {
        itd.item.transform.SetParent(Stack.transform.GetChild(itemType));
        //itd.next = null;
        //itd.parent = null;
        _items.Remove(itd);
        _itemStack[itemType].Push(itd);
    }

    private Vector2 CalculatePosition(ItemTransformData last, ItemTransformData current)
    {

        float bonusX = 0;
        float bonusY = 0;
        if (RepeatX != 0)
        {
            if (current.cell_index == 0)
            {
                bonusX = OffsetX;
                bonusY = OffsetY;
            }
            else if (current.cell_index % RepeatX == 0)
            {
                //����
                bonusX = -last.pos.x + OffsetX;
                bonusY = last.height + SpaceY;
            }
            else
            {
                bonusX = last.width + SpaceX;
            }
        }
        else if (RepeatY != 0)
        {
            if (current.cell_index == 0)
            {
                bonusY = OffsetY;
                bonusX = OffsetX;
            }
            else if (current.cell_index % RepeatY == 0)
            {
                //����
                bonusX = last.width + SpaceX;
                bonusY = last.pos.y + OffsetY;
            }
            else
            {
                bonusY = last.height + SpaceY;
            }
            Debug.Log(bonusY);
        }
        else
        {
            if (current.cell_index == 0)
            {
                bonusX = OffsetX;
                bonusY = OffsetY;
            }
            else if (last.pos.x + last.width + SpaceX + current.width > _viewWidth)
            {
                //����
                bonusX = -last.pos.x + OffsetX;
                bonusY = last.height + SpaceY;
            }
            else
            {
                bonusX = last.width + SpaceX;
            }
        }

        //_tempVec2.x = last.pos.x + bonusX;
        //_tempVec2.y = last.pos.y - bonusY;
        Vector2 pos = new Vector2(last.pos.x + bonusX, last.pos.y - bonusY);
        return pos;
    }

    protected void RefreshList(List<T> data)
    {
        //Debug.Log("��������");
        _data = data;

        if (_inited)
        {
            RecycleAllCells();
            InitItemType();
            ResetCells();
        }
    }

    private void ResetCells()
    {
        if (FitSize)
        {
            _viewWidth = transform.parent.GetComponent<RectTransform>().rect.width;
            _viewHeight = transform.parent.GetComponent<RectTransform>().rect.height;
        }

        ItemTransformData last = new ItemTransformData();
        int i = 0;
        float maxY = 0;
        float maxX = 0;
        float maxHeight = 0;
        float maxWidth = 0;
        while (i < itemType.Count)
        {
            int type = itemType[i][0];

            bool isInit = true;
            if (!FitSize)
            {
                if (RepeatY != 0)
                {
                    isInit = maxX < _viewHeight;
                }
                else
                {
                    isInit = maxY > -_viewHeight;
                }
            }
            ItemTransformData itd = CreateItem(type, isInit);
            itd.cell_index = i;
            itd.item_index = itemType[i][1];
            itd.item.name = i + "";

            if (isInit)
            {
                if (type < _actions.Count)
                {
                    _actions[type](itd, itd.item_index);
                }
            }

            Vector2 pos = CalculatePosition(last, itd);
            _itemPos.Add(pos);
            //��һ���������ݵ�ʱ���ʼ����Ԫ���С ֮�����õ�Ԫ���С
            Dictionary<int, Vector2> sizeDic;
            _itemSize.TryGetValue(type, out sizeDic);
            if (sizeDic == null)
            {
                sizeDic = new Dictionary<int, Vector2>();
                _itemSize.Add(type, sizeDic);
            }
            if (!sizeDic.ContainsKey(itd.item_index))
            {
                sizeDic.Add(itd.item_index, new Vector2(itd.width, itd.height));
            }
            else
            {
                itd.size = sizeDic[itd.item_index];
            }

            itd.pos = pos;
            last.next = itd;
            if (i != 0)
            {
                itd.parent = last;
            }
            last = itd;

            maxY = pos.y;
            maxX = pos.x;
            float maxH = pos.y - itd.height;
            float maxW = pos.x + itd.width;
            if (maxH < maxHeight)
            {
                maxHeight = maxH;
            }
            if (maxW > maxWidth)
            {
                maxWidth = maxW;
            }

            i++;
            //Debug.Log("����item " + (i - 1) + "  " + maxHeight + " " + pos.y + " " + itd.height);
        }

        if (_items.Count > 0)
        {
            _head = _items[0];
            _tail = _items[_items.Count - 1];
        }
        else
        {
            _head = null;
            _tail = null;
        }


        //����UI��С
        _rectTransform.sizeDelta = new Vector2(maxWidth, -maxHeight);

        if (FitSize)
        {
            float contentHeight = _rectTransform.rect.height;
            _viewHeight = contentHeight;
            float contentWidth = _rectTransform.rect.width;
            _viewWidth = contentWidth;
            //if (contentHeight < _viewHeight)
            //{
            //    _viewHeight = contentHeight;
            //}

            RectTransform scrollRect = Scroll.GetComponent<RectTransform>();
            scrollRect.sizeDelta = new Vector2(maxWidth, -maxHeight);
        }
    }

    private void Resize()
    {
        RecycleAllCells();
        ResetCells();
    }

    /// <summary>
    /// ��ʼ����Ԫ������
    /// </summary>
    /// <param name="itemType">int[2] 0Ϊbutton���� 1Ϊ��Ӧ���͵���������</param>
    protected virtual void InitItemType()
    {
        List<int[]> type = new List<int[]>();
        for (int i = 0, len = _data.Count; i < len; i++)
        {
            type.Add(new int[] { 0, i });
        }

        itemType = type;
    }

    /// <summary>
    /// ���õ�Ԫ���С
    /// </summary>
    /// <param name="itd"></param>
    /// <param name="size"></param>
    protected void SetCellSize(ItemTransformData itd, Vector2 size, bool resize = true)
    {
        _itemSize[itd.item_type][itd.item_index] = size;
        if (resize)
        {
            Resize();
        }
    }
}
