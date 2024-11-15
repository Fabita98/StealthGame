using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowDissolve : MonoBehaviour
{
    [SerializeField] private ShadowController _shadowController;
    private CapsuleCollider _appearanceCapsuleCollider;
    private float _defaultRadius;
    private Coroutine _dissolveCoroutine;
    public AudioSource scarySound;

    void Start()
    {
        _appearanceCapsuleCollider = GetComponent<CapsuleCollider>();
        _defaultRadius = _appearanceCapsuleCollider.radius;
    }

    void Update()
    {
        SetAppearanceRadius(_shadowController.appearanceRadius);
    }

    public void Reset()
    {
        GetComponent<Renderer>().material.SetFloat("_DissolveAmount", 1);
    }

    private void SetAppearanceRadius(float radius)
    {
        _appearanceCapsuleCollider.radius = _defaultRadius * radius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            scarySound.Play();
            _dissolveCoroutine = StartCoroutine(DissolveShaderCR(.5f, 0));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_dissolveCoroutine != null)
            {
                StopCoroutine(_dissolveCoroutine); 
            }
            _dissolveCoroutine = StartCoroutine(DissolveShaderCR(0, .5f));
        }
    }

    private IEnumerator DissolveShaderCR(float startingAmount, float endingAmount)
    {
        Renderer rend = GetComponent<Renderer>();
        float dissolveAmount = startingAmount;
        float elapsedTime = 0.0f;

        if (rend.material.HasProperty("_DissolveAmount"))
        {
            rend.material.SetFloat("_DissolveAmount", dissolveAmount);

            if (endingAmount == 0)
            {
                while (dissolveAmount > endingAmount)
                {
                    elapsedTime += Time.deltaTime;
                    dissolveAmount = Mathf.Lerp(startingAmount, endingAmount, elapsedTime / _shadowController.dissolveTime);
                    rend.material.SetFloat("_DissolveAmount", dissolveAmount);
                    yield return null;
                }   
            }
            else
            {
                while (dissolveAmount < endingAmount)
                {
                    elapsedTime += Time.deltaTime;
                    dissolveAmount = Mathf.Lerp(startingAmount, endingAmount, elapsedTime / _shadowController.dissolveTime);
                    rend.material.SetFloat("_DissolveAmount", dissolveAmount);
                    yield return null;
                }
            }
            rend.material.SetFloat("_DissolveAmount", endingAmount);
        }
        else
        {
            Debug.Log("The material does not have a 'DissolveAmount' property.");
        }
    }
}
