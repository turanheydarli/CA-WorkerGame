using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Dreamteck.Splines;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

public class PeteController : MonoBehaviour
{
   public UnityEvent<bool> OnFinish;
    
    float _horizontal;
    private float speed = 5;
    private float _distanceZ = 0;
    private Animator _animator;
    private SplineFollower _splineFollower;
    private Rigidbody _rigidbody;
    private bool _isDied;
    Collider[] colliders;
    [SerializeField] Transform detectTransform;
    public List<GameObject> boxes;
    private CinemachineVirtualCamera _cinemachineVirtualCamera;
    private bool _isFinish;
    float NextDropTime;
    [SerializeField] float DropRate = 1;
    [SerializeField] float DropSecond = 1;
    [SerializeField] Transform DropArea;
    int dropCount = 0;
    [SerializeField] float DropDistanceBetween = 1f;
    [SerializeField] float detectionRange = 1;
    [SerializeField] LayerMask layer;
    [SerializeField] Transform holdTransform;
    [SerializeField] float endScale = 0.3f;
    [SerializeField] private int storageItems = 0;
    [SerializeField] private float distanceBetween = 1.25f;

    // Start is called before the first frame update
    void Start()
    {
        _cinemachineVirtualCamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _splineFollower = GetComponentInParent<SplineFollower>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(detectTransform.position, detectionRange);
    }

    // Update is called once per frame
    void Update()
    {
        if (_isFinish)
            LerpMovement();

        float movement = (speed * Input.GetAxis("Horizontal")) * Time.deltaTime;
        transform.Translate(1 * movement, 0, 0);
        transform.localPosition = new Vector3((Mathf.Clamp(transform.localPosition.x, -2, 2)),
            transform.localPosition.y, transform.localPosition.z);

        // _horizontal = Input.GetAxis("Horizontal");
        // transform.localPosition += new Vector3(_horizontal * 5f, 0, 0) * Time.deltaTime;


        colliders = Physics.OverlapSphere(detectTransform.position, detectionRange, layer);

        foreach (var hit in colliders)
        {
            if (hit.CompareTag("Collectable"))
            {
                boxes.Add(hit.gameObject);

                hit.TryGetComponent<BeamController>(out BeamController beam);
                beam.transform.DOKill(true);
                _animator.SetTrigger("IsCharged");
                hit.tag = "Collected";

                hit.transform.parent = holdTransform;

                var seq = DOTween.Sequence();
                seq.Append(hit.transform.DOLocalJump(new Vector3(0, boxes.Count * 0.1f), 2, 1, 0.3f))
                    .Insert(0.1f, hit.transform.DOScale(0.5f, 0.2f));
                seq.AppendCallback(() => { hit.transform.localRotation = Quaternion.Euler(0, 0, 90); });
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Stone"))
        {
            _animator.SetTrigger("Tripped");

            // speed = 0;
            // _splineFollower.follow = false;

            for (int i = boxes.Count - 1; i >= 0; i--)
            {
                boxes[i].AddComponent<Rigidbody>();
                boxes[i].transform.parent = null;
                boxes.Remove(boxes[i]);
            }

            if (boxes.Count < 1)
            {
                _animator.SetBool("IsCharged", false);
            }
        }

        if (other.CompareTag("DropZone"))
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (Time.time >= NextDropTime && other.transform.CompareTag("DropZone"))
        {
            StartCoroutine(nameof(StopFollower));
            _animator.SetBool("IsFinish", true);
            _isFinish = true;
            speed = 0;
            if (boxes.Count <= 0) return;
            GameObject go = boxes[boxes.Count - 1];
            var Seq = DOTween.Sequence();
            go.transform.parent = DropArea;
            

            if (dropCount < 2)
            {
                Seq.Append(go.transform.DOJump(
                        DropArea.position + new Vector3(0, (dropCount * DropDistanceBetween), _distanceZ),
                        .3f,
                        1, 0.3f))
                    .AppendCallback(() => { go.transform.rotation = Quaternion.Euler(0, 0, 90); });

                boxes.Remove(go);
                dropCount++;
                NextDropTime = Time.time + DropSecond / DropRate;
            }
            else
            {
                dropCount = 0;
                _distanceZ -= 0.1f;

                if (_distanceZ < -.6f)
                {
                    _distanceZ = 0;
                    DropDistanceBetween += .1f;
                }
            }
            
            if (boxes.Count == 0)
            {
                OnFinish.Invoke(dropCount != 0);
                StartCoroutine(nameof(DanceLerpMovement));
            }
        }
    }

    // private void OnTriggerExit(Collider other)
    // {
    //     StartCoroutine(other.GetComponent<DropZone>().SellDropedItems());
    //     dropCount = 0;
    // }

    private IEnumerator StopFollower()
    {
        yield return new WaitForSeconds(.3f);
        _splineFollower.follow = false;
    }

    private void LerpMovement()
    {
        _cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_BindingMode =
            CinemachineTransposer.BindingMode.LockToTargetOnAssign;

        _cinemachineVirtualCamera.Follow = null;
        _cinemachineVirtualCamera.LookAt = null;

        _cinemachineVirtualCamera.transform.position = Vector3.Lerp(_cinemachineVirtualCamera.transform.position,
            new Vector3(0, 1, 2), 1f * Time.deltaTime);

        _cinemachineVirtualCamera.transform.rotation = Quaternion.Lerp(_cinemachineVirtualCamera.transform.rotation,
            Quaternion.Euler(new Vector3(-6, 75, 0)), 1f * Time.deltaTime);
    }

    private IEnumerator DanceLerpMovement()
    {
        yield return new WaitForSeconds(.4f);
        _animator.SetTrigger("Dance");
        _cinemachineVirtualCamera.m_Lens.FieldOfView = 25f;
    }
}