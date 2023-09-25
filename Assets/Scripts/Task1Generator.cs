using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Task1Generator : MonoBehaviour
{

    static Task1Generator instance;
    public static Task1Generator i
    {
        get {
            if (instance == null)
            {
                instance = FindObjectOfType<Task1Generator>();
            }
            
            return instance; 
        }
    }

    const int GRID_WIDTH = 17;
    const int GRID_HEIGHT = 9;
    const int MAX_TRIES = 10;
    [SerializeField] List<Tile> _tileset;
    SuperPosition[,] _grid;
    Tile[,] tileGrid;
    List<Vector2Int> spawnOrder = new List<Vector2Int>();

    // Start is called before the first frame update
    void Start()
    {
        int tries = 0;
        bool result;
        do
        {
            tries++;
            result = RunWFC();
        }
        while (result == false && tries < MAX_TRIES);

        if (result == false)
        {
            print("Unable to solve wave function collapse after " + tries + " tries.");
        }
        else
        {
            print("run amount " + tries);
            DrawTiles();
        }
    }

    bool RunWFC()
    {
        InitGrid();
        spawnOrder = new List<Vector2Int>();
        while (DoUnobservedNodesExist())
        {
            Vector2Int node = GetNextUnobservedNode();
            if (node.x == -1)
            {
                return false; //failure
            }

            int observedValue = Observe(node);
            PropogateNeighbors(node, observedValue);
        }

        return true; //success
    }

    void DrawTiles() {
        StartCoroutine(DrawTilesRec());
        /*
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                GameObject tile = GameObject.Instantiate(_tileset[_grid[x, y].GetObservedValue()].gameObject);
                tile.transform.position = tile.transform.position + new Vector3(x, y, 0f) - new Vector3((GRID_WIDTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, 0f);
            }
        }*/
    }

    public void StartRebalance(Tile t)
    {
        Vector2Int coord = new Vector2Int(0,0);
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (tileGrid[x, y] == t)
                {
                    coord = new Vector2Int(x, y);
                    break;
                }
            }
        }
        int changeTo = 0;
        do { changeTo = UnityEngine.Random.Range(0, _tileset.Count); } while (changeTo == _grid[coord.x, coord.y].GetObservedValue());
        Tile newTile = ReplaceTile(coord, changeTo);
        //at start
        PropogateRebalance(coord, new Vector2Int(-1, 0), newTile); //to left
        PropogateRebalance(coord, new Vector2Int(1, 0), newTile); //to right
        PropogateRebalance(coord, new Vector2Int(0, -1), newTile); //to down
        PropogateRebalance(coord, new Vector2Int(0, 1), newTile); //to up
    }

    public void PropogateRebalance(Vector2Int orgCoord, Vector2Int direction, Tile mustWorkAdjacentTo)
    {
        Vector2Int newCoord = orgCoord + direction;
        //print("pr " + newCoord);

        if (newCoord.x < 0 || newCoord.y < 0 || newCoord.x >= GRID_WIDTH || newCoord.y >= GRID_HEIGHT) return;

       SuperPosition spToCheck = _grid[newCoord.x, newCoord.y]; 

        if (direction == new Vector2Int(0, 1))
        {
            if (_tileset[spToCheck.GetObservedValue()]._downRoad == mustWorkAdjacentTo._upRoad) return;
        }
        else if (direction == new Vector2Int(1, 0))
        {
                if (_tileset[spToCheck.GetObservedValue()]._leftRoad == mustWorkAdjacentTo._rightRoad) return;
        }
        else if (direction == new Vector2Int(0, -1))
        {
                if (_tileset[spToCheck.GetObservedValue()]._upRoad == mustWorkAdjacentTo._downRoad) return;
        }
        else
        {
                if (_tileset[spToCheck.GetObservedValue()]._rightRoad == mustWorkAdjacentTo._leftRoad) return;
        }

        spToCheck.Refill(_tileset.Count);

        PropogateTo(orgCoord, direction, tileGrid[orgCoord.x, orgCoord.y]);
        if (existCell(newCoord + new Vector2Int(-1, 0))) { 
            PropogateTo(newCoord + new Vector2Int(-1, 0), new Vector2Int(1, 0), tileGrid[(newCoord + new Vector2Int(-1, 0)).x, (newCoord + new Vector2Int(-1, 0)).y]); 
        }
        if (existCell(newCoord + new Vector2Int(1, 0))) { 
            PropogateTo(newCoord + new Vector2Int(1, 0), new Vector2Int(-1, 0), tileGrid[(newCoord + new Vector2Int(1, 0)).x, (newCoord + new Vector2Int(1, 0)).y]); 
        }
        if (existCell(newCoord + new Vector2Int(0, -1))) {
            PropogateTo(newCoord + new Vector2Int(0, -1), new Vector2Int(0, 1), tileGrid[(newCoord + new Vector2Int(0, -1)).x, (newCoord + new Vector2Int(0, -1)).y]);
        }
        if (existCell(newCoord + new Vector2Int(0, 1))) { 
            PropogateTo(newCoord + new Vector2Int(0, 1), new Vector2Int(0, -1), tileGrid[(newCoord + new Vector2Int(0, 1)).x, (newCoord + new Vector2Int(0, 1)).y]);
        }
        
        if (spToCheck.HasPossibilities())
        {
            spToCheck.Observe();
            ReplaceTile(newCoord, spToCheck.GetObservedValue());

        }
        else
        {
            spToCheck.Refill(_tileset.Count);
            PropogateTo(orgCoord, direction, tileGrid[orgCoord.x, orgCoord.y]);
            spToCheck.Observe();
            ReplaceTile(newCoord, spToCheck.GetObservedValue());

            PropogateRebalance(newCoord, new Vector2Int(-1, 0), tileGrid[newCoord.x, newCoord.y]);
            PropogateRebalance(newCoord, new Vector2Int(1, 0), tileGrid[newCoord.x, newCoord.y]);
            PropogateRebalance(newCoord, new Vector2Int(0, -1), tileGrid[newCoord.x, newCoord.y]);
            PropogateRebalance(newCoord, new Vector2Int(0, 1), tileGrid[newCoord.x, newCoord.y]);
        }


        /*
        spToCheck.Observe();

        foreach(int i in spToCheck.RemainPossibleValues())
        {
            bool fitAll = true;
            if (!(!existCell(newCoord + new Vector2Int(-1, 0)) || _tileset[i]._leftRoad == tileGrid[(newCoord + new Vector2Int(-1, 0)).x, (newCoord + new Vector2Int(-1, 0)).y]._rightRoad))
            {
                fitAll = false;
                break;
            }
        }*/


    }

    bool existCell(Vector2Int coord)
    {
        if (coord.x < 0 || coord.y < 0 || coord.x >= GRID_WIDTH || coord.y >= GRID_HEIGHT) return false;
        return true;
    }


    public Tile ReplaceTile(Vector2Int coord, int t)
    {
        //print("replacing " + coord);
        Destroy(tileGrid[coord.x, coord.y].gameObject);

        GameObject tile = GameObject.Instantiate(_tileset[t].gameObject);
        tile.transform.position = tile.transform.position + new Vector3(coord.x, coord.y, 0f) - new Vector3((GRID_WIDTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, 0f);
        tileGrid[coord.x, coord.y] = tile.GetComponent<Tile>();
        _grid[coord.x, coord.y].OverrideObserve(t);
        return tile.GetComponent<Tile>();
    }

    IEnumerator DrawTilesRec()
    {
        int count = 0;
        while (count < spawnOrder.Count)
        {
            GameObject tile = GameObject.Instantiate(_tileset[_grid[spawnOrder[count].x, spawnOrder[count].y].GetObservedValue()].gameObject);
            tile.transform.position = tile.transform.position + new Vector3(spawnOrder[count].x, spawnOrder[count].y, 0f) - new Vector3((GRID_WIDTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, 0f);
            tileGrid[spawnOrder[count].x, spawnOrder[count].y] = tile.GetComponent<Tile>();

            count++;
            yield return new WaitForSeconds(0.01f);
        }
    }

    bool DoUnobservedNodesExist()
    {
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (_grid[x, y].IsObserved() == false) {
                    return true;
                }
            }
        }

        return false;
    }

    int Observe(Vector2Int node)
    {
        spawnOrder.Add(node);
        return _grid[node.x, node.y].Observe();
    }


    private void InitGrid()
    {
        _grid = new SuperPosition[GRID_WIDTH, GRID_HEIGHT];
        tileGrid = new Tile[GRID_WIDTH, GRID_HEIGHT];
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                _grid[x, y] = new SuperPosition(_tileset.Count);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    void PropogateNeighbors(Vector2Int node, int observedValue)
    {
        PropogateTo(node, new Vector2Int(-1, 0), _tileset[observedValue]);
        PropogateTo(node, new Vector2Int(1, 0), _tileset[observedValue]);
        PropogateTo(node, new Vector2Int(0, -1), _tileset[observedValue]);
        PropogateTo(node, new Vector2Int(0, 1), _tileset[observedValue]);
    }

    Vector2Int GetOppositeDirection(Vector2Int orgDirection)
    {
        if (orgDirection.x == 0) return new Vector2Int(orgDirection.x, -orgDirection.y);
        return new Vector2Int(-orgDirection.x, orgDirection.y);
    }


    void PropogateTo(Vector2Int node, Vector2Int direction, Tile mustWorkAdjacentTo)
    {
        // Your code for 1-c goes here:

        // Remove impossible values from neighbor node based on the constrains of both tiles
        // Don't forget to check for out of bounds

        Vector2Int newNode = node + direction;
        if (newNode.x < 0 || newNode.y < 0 || newNode.x >= GRID_WIDTH || newNode.y >= GRID_HEIGHT) return;
        //SuperPosition spOrigional = _grid[node.x, node.y];
        SuperPosition spToCheck = _grid[newNode.x, newNode.y];
        if (spToCheck.IsObserved()) return;

        if (direction == new Vector2Int(0, 1))
        {
            for(int i = 0; i< _tileset.Count; i++)
            {
                if (_tileset[i]._downRoad != mustWorkAdjacentTo._upRoad) spToCheck.RemovePossibleValue(i);
            }
        }
        else if (direction == new Vector2Int(1, 0)) {
            for (int i = 0; i < _tileset.Count; i++)
            {
                if (_tileset[i]._leftRoad != mustWorkAdjacentTo._rightRoad) spToCheck.RemovePossibleValue(i);
            }
        }
        else if (direction == new Vector2Int(0, -1)) {
            for (int i = 0; i < _tileset.Count; i++)
            {
                if (_tileset[i]._upRoad != mustWorkAdjacentTo._downRoad) spToCheck.RemovePossibleValue(i);
            }
        }
        else {
            for (int i = 0; i < _tileset.Count; i++)
            {
                if (_tileset[i]._rightRoad != mustWorkAdjacentTo._leftRoad) spToCheck.RemovePossibleValue(i);
            }
        }

        /*
        for(int i = spToCheck.RemainPossibleValues().Count-1; i>=0; i--)
        {
            if (GetSideInfo(spToCheck.RemainPossibleValues()[i], GetOppositeDirection(direction)) != GetSideInfo(spOrigional.GetObservedValue(), direction))
            {
                spToCheck.RemovePossibleValue(spToCheck.RemainPossibleValues()[i]);
            }
        }*/


    }

    Vector2Int GetNextUnobservedNode()
    {
        // Your code for 1-a goes here:
        int minPossibleAmount = 2147483646;
        Vector2Int minPossibleCoord = new Vector2Int(0, 0);
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (!(_grid[x, y].IsObserved()) &&_grid[x, y].NumOptions < minPossibleAmount)
                {
                    minPossibleAmount = _grid[x, y].NumOptions;
                    minPossibleCoord = new Vector2Int(x, y);
                }
            }
        }
        //return the coordinates of the unobserved node with the fewest possible options
        return minPossibleCoord; //replace me
    }
}
