using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SuperPosition
{
    List<Proto.ProtoData> _possibleValues = new List<Proto.ProtoData>();
    bool _observed = false;

    public SuperPosition(List<Proto.ProtoData> allProtoData)
    {
        foreach (var VARIABLE in allProtoData)
        {
            _possibleValues.Add(VARIABLE);
        }
    }

    public Proto.ProtoData GetObservedValue()
    {
        return _possibleValues[0];
    }

    public Proto.ProtoData Observe()
    {
        //pick one of the possible values at random and then remove all other possible values
        //also set _observed to true
        //return the observed value

        Proto.ProtoData chosenValue = _possibleValues[Random.Range(0,_possibleValues.Count)];
        _possibleValues = new List<Proto.ProtoData> { chosenValue};
        _observed= true;

        return GetObservedValue();
    }

    public void Refill(List<Proto.ProtoData> allProtoData)
    {
        _possibleValues.Clear();
        foreach (var VARIABLE in allProtoData)
        {
            _possibleValues.Add(VARIABLE);
        }
        _observed = false;
    }

    public Proto.ProtoData OverrideObserve(Proto.ProtoData ppd)
    {
        _possibleValues = new List<Proto.ProtoData> { ppd };
        _observed = true;
        return ppd;
    }

    public bool IsObserved()
    {
        return _observed;
    }

    public void RemovePossibleValue(Proto.ProtoData value)
    {
        _possibleValues.Remove(value);
    }

    public bool HasPossibilities()
    {
        return _possibleValues.Count > 0;
    }

    public List<Proto.ProtoData> RemainPossibleValues()
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
