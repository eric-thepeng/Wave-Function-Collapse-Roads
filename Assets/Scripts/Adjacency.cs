using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]public class Adjacency
{
    public enum Type
    {
        Soil,
        Rock,
        River,
        Pond,
        Bug
    }

    public Type Type1;
    public Type Type2;
}
