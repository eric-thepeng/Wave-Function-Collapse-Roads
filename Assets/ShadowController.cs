using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowController : MonoBehaviour
{
    private SpriteRenderer sr;
    private float time = 0;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        time += Time.deltaTime;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b,  0.3f + 0.4f * (Mathf.Sin(time*3)+1)/2);
    }
}
