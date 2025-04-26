using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleAutoDisable : MonoBehaviour
{
    [SerializeField] private bool isAutoDisable;
    
    private ParticleSystem _particleSystem;

    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (_particleSystem && !_particleSystem.IsAlive() && isAutoDisable)
        {
            gameObject.SetActive(false);
        }
    }
}