using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SuperPosition
{
    List<int> _possibleValues = new List<int>();
    bool _observed = false;

    public SuperPosition(int maxValue)
    {
        for (int i = 0; i < maxValue; i++)
        {
            _possibleValues.Add(i);
        }
    }

    public int GetObservedValue()
    {
        return _possibleValues[0];
    }

    public int Observe()
    {
        // Your code for 1-b goes here:

        //pick one of the possible values at random and then remove all other possible values
        //also set _observed to true
        //return the observed value

        int chosenValue = _possibleValues[Random.Range(0,_possibleValues.Count)];
        _possibleValues = new List<int> { chosenValue};
        _observed= true;

        return GetObservedValue();
    }

    public void Refill(int maxValue)
    {
        _possibleValues.Clear();
        for (int i = 0; i < maxValue; i++)
        {
            _possibleValues.Add(i);
        }
        _observed = false;
    }

    public void OverrideObserve(int newValue)
    {
        _possibleValues = new List<int> { newValue };
    }

    public bool IsObserved()
    {
        return _observed;
    }

    public void RemovePossibleValue(int value)
    {
        _possibleValues.Remove(value);
    }

    public bool HasPossibilities()
    {
        return _possibleValues.Count > 0;
    }

    public List<int> RemainPossibleValues()
    {
        return _possibleValues;
    }

    public int NumOptions{
        get
        {
            return _possibleValues.Count;
        }
    }

}
