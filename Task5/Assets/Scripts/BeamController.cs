using DG.Tweening;
using UnityEngine;

public class BeamController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.DORotate(new Vector3(0, 90, 0), 2).SetLoops(-1, LoopType.Incremental);
    }

    // Update is called once per frame
    void Update()
    {
    }
}