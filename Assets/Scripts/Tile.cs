using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Adjacency.Type upAdjacency;
    public Adjacency.Type leftAdjacency;
    public Adjacency.Type downAdjacency;
    public Adjacency.Type rightAdjacency;

    public float weight = 1;
    
    public bool _leftRoad;
    public bool _rightRoad;
    public bool _upRoad;
    public bool _downRoad;
}
