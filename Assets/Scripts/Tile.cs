using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Adjacency upAdjacency;
    public Adjacency leftAdjacency;
    public Adjacency downAdjacency;
    public Adjacency rightAdjacency;

    public float weight = 1;
    
    public bool _leftRoad;
    public bool _rightRoad;
    public bool _upRoad;
    public bool _downRoad;
}
