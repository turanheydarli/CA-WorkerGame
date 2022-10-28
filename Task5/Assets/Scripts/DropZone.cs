using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DropZone : MonoBehaviour
{
    //public Stack<GameObject> StackedDropItems = new Stack<GameObject>();
    public Queue<GameObject> StackedDropItems;
    [SerializeField] float SellTime = 0.5f;

    void Start()
    {
        StackedDropItems = new Queue<GameObject>();
    }

    public IEnumerator SellDropedItems()
    {
        foreach (var item in StackedDropItems)
        {
            yield return new WaitForSeconds(0.05f);
            item.transform.DOScale(1.5f, 0.1f).SetLoops(2, LoopType.Yoyo);
        }

        while (StackedDropItems.Count > 0)
        {
            yield return new WaitForSeconds(SellTime);
            //GameObject go = StackedDropItems.Pop();
            GameObject go = StackedDropItems.Dequeue();
            go.transform.DOScale(0, 0.3f);
        }
    }
}
