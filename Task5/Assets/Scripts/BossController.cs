using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossController : MonoBehaviour
{
    private GameObject _player;
    private PeteController _peteController;
    private Animator _animator;

    void Start()
    {
        _player = GameObject.FindWithTag("Player");
        _peteController = _player.GetComponent<PeteController>();
        _animator = GetComponent<Animator>();
        _peteController.OnFinish.AddListener(BoosReaction);
    }

    void Update()
    {
        transform.LookAt(_player.transform);
    }

    void BoosReaction(bool isSuccess)
    {
        _animator.SetTrigger(isSuccess ? "Clap" : "Angry");
    }
}