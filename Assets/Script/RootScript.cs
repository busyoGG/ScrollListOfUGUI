using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class RootScript : MonoBehaviour
{
    public TestScrollView List;
    void Start()
    {
        List<int> list = new List<int>();
        for (int i = 0; i < 20; i++)
        {
            list.Add(i);
        }
        List.data = list;
    }
}
