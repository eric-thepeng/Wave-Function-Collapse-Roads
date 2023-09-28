using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextInputManager : MonoBehaviour
{
    public TMP_Text textDisplay;
    private string currentText = "";
    private float currentWaitTime = 0;

    public float solidWaitTime = 3f;
    public float maxWaitTime = 4f;
    
    private void Update()
    {
        string input = Input.inputString;
        if (!string.IsNullOrEmpty(input)) // Check if there is an input
        {
            // Check if the input is a letter
            char character = input[0];
            if (char.IsLetter(character) || character.Equals(' '))
            {
                currentText += character;
                textDisplay.text = currentText;
                currentWaitTime = 0;
                SetTextAlpha(1);
                return;
            }
        }
        
        // False State
        currentWaitTime += Time.deltaTime;
        if (currentWaitTime >= solidWaitTime)
        {
            SetTextAlpha(1 - (currentWaitTime - solidWaitTime) / (maxWaitTime - solidWaitTime));
        }
        
        if (currentWaitTime >= maxWaitTime)
        {
            currentText = "";
            textDisplay.text = currentText;
            currentWaitTime = 0;
        }

    }

    private void SetTextAlpha(float newAlpha)
    {
        textDisplay.color = new Color(textDisplay.color.r, textDisplay.color.g, textDisplay.color.b, newAlpha);
    }
}
