using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Generator : MonoBehaviour
{

    static Generator instance;
    public static Generator i
    {
        get {
            if (instance == null)
            {
                instance = FindObjectOfType<Generator>();
            }
            
            return instance; 
        }
    }

    const int GRID_WIDTH = 19;
    const int GRID_HEIGHT = 11;
    private const float GRID_SIZE = 1f;
    const int MAX_TRIES = 30;
    
    [SerializeField] private List<GameObject> allProtoPrefabs;
    private List<Proto> allProtos = new List<Proto>();
    private List<Proto.ProtoData> allProtoData = new List<Proto.ProtoData>();
    
    SuperPosition[,] spGrid;
    private GameObject[,] tileGrid;
    Proto.ProtoData[,] protoDataGrid;

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
        GenerateProtos();
        InitGrid();
        while (DoUnobservedNodesExist())
        {
            Vector2Int node = GetNextUnobservedNode();
            if (node.x == -1 || !spGrid[node.x,node.y].HasPossibilities())
            {
                return false; //failure
            }

            Proto.ProtoData observedValue = Observe(node);
            protoDataGrid[node.x, node.y] = observedValue;
            PropogateNeighbors(node, observedValue);
        }

        return true; //success
    }
    
    public void GenerateProtos()
    {
        foreach (GameObject protoPrefab in allProtoPrefabs)
        {
            allProtos.Add(protoPrefab.GetComponent<Proto>());
        }
        allProtoData.Clear();
        foreach (Proto proto in allProtos)
        {
            foreach(Proto.ProtoData ppd in proto.GetAllProtoDataVariations())
            {
                allProtoData.Add(ppd);
            }
        }
    }

    void DrawTiles() {
        //StartCoroutine(DrawTilesRec());
        
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                GameObject tile = SpawnTile(new Vector2Int(x,y), protoDataGrid[x, y]); //= GameObject.Instantiate(_tileset[_grid[x, y].GetObservedValue()].gameObject);
                //tile.transform.position = tile.transform.position + new Vector3(x, y, 0f) - new Vector3((GRID_WIDTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, 0f);
            }
        }
    }

    public void StartRebalance(Vector2Int coord)
    {
        //get coord of ppd
        /*
        Vector2Int coord = new Vector2Int(0,0);
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (protoDataGrid[x, y] == ppd)
                {
                    coord = new Vector2Int(x, y);
                    break;
                }
            }
        }*/

        //change that ppd to random
        
        spGrid[coord.x, coord.y].Refill(allProtoData);

        if(IsCellUnchangeable(coord + new Vector2Int(-1, 0))) PropogateTo(coord + new Vector2Int(-1, 0), new Vector2Int(1,0), protoDataGrid[(coord + new Vector2Int(-1, 0)).x, (coord + new Vector2Int(-1, 0)).y]);
        else if(IsCellUnchangeable(coord + new Vector2Int(1, 0))) PropogateTo(coord + new Vector2Int(1, 0), new Vector2Int(-1,0), protoDataGrid[(coord + new Vector2Int(1, 0)).x, (coord + new Vector2Int(1, 0)).y]);
        else if(IsCellUnchangeable(coord + new Vector2Int(0, 1))) PropogateTo(coord + new Vector2Int(0, 1), new Vector2Int(0,-1), protoDataGrid[(coord + new Vector2Int(0, 1)).x, (coord + new Vector2Int(0, 1)).y]);
        else if(IsCellUnchangeable(coord + new Vector2Int(0, -1))) PropogateTo(coord + new Vector2Int(0, -1), new Vector2Int(0,1), protoDataGrid[(coord + new Vector2Int(0, -1)).x, (coord + new Vector2Int(0, -1)).y]);

        /*
        int changeTo = 0;
        do { changeTo = UnityEngine.Random.Range(0, allProtoData.Count); } while (allProtoData[changeTo] == spGrid[coord.x, coord.y].GetObservedValue());*/
        spGrid[coord.x, coord.y].Observe();
        Proto.ProtoData newTilePPD = ReplaceTile(coord, spGrid[coord.x, coord.y].GetObservedValue());
        
        //propogate rebalance neightbor start
        PropogateRebalance(coord, new Vector2Int(-1, 0), newTilePPD); //to left
        PropogateRebalance(coord, new Vector2Int(1, 0), newTilePPD); //to right
        PropogateRebalance(coord, new Vector2Int(0, -1), newTilePPD); //to down
        PropogateRebalance(coord, new Vector2Int(0, 1), newTilePPD); //to up
    }

    public void PropogateRebalance(Vector2Int orgCoord, Vector2Int direction, Proto.ProtoData mustWorkAdjacentTo)
    {
        Vector2Int newCoord = orgCoord + direction;
        
        //does not exist then return
        //if (newCoord.x < 0 || newCoord.y < 0 || newCoord.x >= GRID_WIDTH || newCoord.y >= GRID_HEIGHT) return;
        if(!ExistCellAndCanChange(newCoord))return;
        
        SuperPosition spToCheck = spGrid[newCoord.x, newCoord.y];

        if (direction == new Vector2Int(0, 1))
        {
            if(spToCheck.GetObservedValue().front1 == mustWorkAdjacentTo.back2 && 
                spToCheck.GetObservedValue().front2 == mustWorkAdjacentTo.back1) return;
            //if (_tileset[spToCheck.GetObservedValue()]._downRoad == mustWorkAdjacentTo._upRoad) return;
        }
        else if (direction == new Vector2Int(1, 0))
        {
            if(spToCheck.GetObservedValue().left1 == mustWorkAdjacentTo.right2 && 
               spToCheck.GetObservedValue().left2 == mustWorkAdjacentTo.right1) return;
              //  if (_tileset[spToCheck.GetObservedValue()]._leftRoad == mustWorkAdjacentTo._rightRoad) return;
        }
        else if (direction == new Vector2Int(0, -1))
        {
            if(spToCheck.GetObservedValue().back1 == mustWorkAdjacentTo.front2 && 
               spToCheck.GetObservedValue().back2 == mustWorkAdjacentTo.front1) return;
               // if (_tileset[spToCheck.GetObservedValue()]._upRoad == mustWorkAdjacentTo._downRoad) return;
        }
        else
        {
            if(spToCheck.GetObservedValue().right1 == mustWorkAdjacentTo.left2 && 
               spToCheck.GetObservedValue().right2 == mustWorkAdjacentTo.left1) return;
                //if (_tileset[spToCheck.GetObservedValue()]._rightRoad == mustWorkAdjacentTo._leftRoad) return;
        }

        // Need to refill and redo
        spToCheck.Refill(allProtoData);

        PropogateTo(orgCoord, direction, protoDataGrid[orgCoord.x, orgCoord.y]);
        if (ExistCellAndCanChange(newCoord + new Vector2Int(-1, 0))) { 
            PropogateTo(newCoord + new Vector2Int(-1, 0), new Vector2Int(1, 0), protoDataGrid[(newCoord + new Vector2Int(-1, 0)).x, (newCoord + new Vector2Int(-1, 0)).y]); 
        }
        if (ExistCellAndCanChange(newCoord + new Vector2Int(1, 0))) { 
            PropogateTo(newCoord + new Vector2Int(1, 0), new Vector2Int(-1, 0), protoDataGrid[(newCoord + new Vector2Int(1, 0)).x, (newCoord + new Vector2Int(1, 0)).y]); 
        }
        if (ExistCellAndCanChange(newCoord + new Vector2Int(0, -1))) {
            PropogateTo(newCoord + new Vector2Int(0, -1), new Vector2Int(0, 1), protoDataGrid[(newCoord + new Vector2Int(0, -1)).x, (newCoord + new Vector2Int(0, -1)).y]);
        }
        if (ExistCellAndCanChange(newCoord + new Vector2Int(0, 1))) { 
            PropogateTo(newCoord + new Vector2Int(0, 1), new Vector2Int(0, -1), protoDataGrid[(newCoord + new Vector2Int(0, 1)).x, (newCoord + new Vector2Int(0, 1)).y]);
        }
        
        //Deal with remaining possibilities
        if (spToCheck.HasPossibilities())
        {
            protoDataGrid[newCoord.x, newCoord.y] = spToCheck.Observe();
            ReplaceTile(newCoord, spToCheck.GetObservedValue());
        }
        else
        {
            spToCheck.Refill(allProtoData);
            PropogateTo(orgCoord, direction, protoDataGrid[orgCoord.x, orgCoord.y]);
            protoDataGrid[newCoord.x, newCoord.y] = spToCheck.Observe();
            ReplaceTile(newCoord, spToCheck.GetObservedValue());

            if(direction != new Vector2Int(1,0))PropogateRebalance(newCoord, new Vector2Int(-1, 0), protoDataGrid[newCoord.x, newCoord.y]);
            if(direction != new Vector2Int(-1,0))PropogateRebalance(newCoord, new Vector2Int(1, 0), protoDataGrid[newCoord.x, newCoord.y]);
            if(direction != new Vector2Int(0,1))PropogateRebalance(newCoord, new Vector2Int(0, -1), protoDataGrid[newCoord.x, newCoord.y]);
            if(direction != new Vector2Int(0,-1))PropogateRebalance(newCoord, new Vector2Int(0, 1), protoDataGrid[newCoord.x, newCoord.y]);
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

    bool ExistCellAndCanChange(Vector2Int coord)
    {
        if (coord.x < 0 || coord.y < 0 || coord.x >= GRID_WIDTH || coord.y >= GRID_HEIGHT) return false;
        //if (Math.Abs(coord.x - (GRID_HEIGHT / 2)) <= 1 || Math.Abs(coord.y - (GRID_WIDTH / 2)) <= 1) return false;
        if (IsCellUnchangeable(coord)) return false;
        return true;
    }

    bool IsCellUnchangeable(Vector2Int coord)
    {
        for (int x = GRID_WIDTH / 2 - 1; x < GRID_WIDTH / 2 + 2; x++)
        {
            for (int y = GRID_HEIGHT / 2 - 1; y < GRID_HEIGHT / 2 + 2; y++)
            {
                if (coord.x == x && coord.y == y) return true;
            }
        }

        return false;
    }


    public Proto.ProtoData ReplaceTile(Vector2Int coord, Proto.ProtoData ppd)
    {
        //print("replacing " + coord);
        Destroy(tileGrid[coord.x, coord.y].gameObject);

        GameObject tile = SpawnTile(coord, ppd);
        tileGrid[coord.x, coord.y] = tile;
        protoDataGrid[coord.x, coord.y] = spGrid[coord.x, coord.y].OverrideObserve(ppd);
        return ppd;
    }

    public GameObject SpawnTile(Vector2Int coord, Proto.ProtoData ppd)
    {
        GameObject tile = Instantiate(ppd.prefab);
        tile.transform.position = tile.transform.position + new Vector3(coord.x * GRID_SIZE, coord.y * GRID_SIZE, 0f) - GRID_SIZE * new Vector3((GRID_WIDTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, 0f);
        tile.transform.localScale = new Vector3(GRID_SIZE, GRID_SIZE, 1);
        tile.transform.Rotate(new Vector3(0, 0, 1), -90 * ppd.rotationIndex);
        tileGrid[coord.x, coord.y] = tile;
        protoDataGrid[coord.x, coord.y] = ppd;
        spGrid[coord.x, coord.y].OverrideObserve(ppd);
        //_grid[coord.x, coord.y].OverrideObserve(t); TODO
        return tile;
    }

    IEnumerator DrawTilesRec()
    {
        int count = 0;
        /*
        while (count < spawnOrder.Count)
        {
            GameObject tile = GameObject.Instantiate(_tileset[_grid[spawnOrder[count].x, spawnOrder[count].y].GetObservedValue()].gameObject);
            tile.transform.position = tile.transform.position + new Vector3(spawnOrder[count].x, spawnOrder[count].y, 0f) - new Vector3((GRID_WIDTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, 0f);
            tileGrid[spawnOrder[count].x, spawnOrder[count].y] = tile.GetComponent<Proto>();

            count++;
            yield return new WaitForSeconds(0.01f);
        }*/
        yield return new WaitForSeconds(0.01f);
    }

    bool DoUnobservedNodesExist()
    {
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (spGrid[x, y].IsObserved() == false) {
                    return true;
                }
            }
        }

        return false;
    }

    Proto.ProtoData Observe(Vector2Int node)
    {
        //spawnOrder.Add(node);
        return spGrid[node.x, node.y].Observe();
    }


    private void InitGrid()
    {
        spGrid = new SuperPosition[GRID_WIDTH, GRID_HEIGHT];
        tileGrid = new GameObject[GRID_WIDTH, GRID_HEIGHT];
        protoDataGrid = new Proto.ProtoData[GRID_WIDTH, GRID_HEIGHT];
        
        Proto.ProtoData rockPPD = null, soilPPD = null;
        foreach (Proto.ProtoData ppd in allProtoData)
        {
            if (ppd.specialIndex == 1) rockPPD = ppd;
            if (ppd.specialIndex == 2) soilPPD = ppd;
        }
        
        
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                spGrid[x, y] = new SuperPosition(allProtoData);
            }
        }
        
        for (int x = GRID_WIDTH / 2 - 1; x < GRID_WIDTH / 2 + 2; x++)
        {
            for (int y = GRID_HEIGHT / 2 - 1; y < GRID_HEIGHT / 2 + 2; y++)
            {
                if (x == GRID_WIDTH / 2 && y == GRID_HEIGHT / 2)
                {
                    protoDataGrid[x, y] = spGrid[x, y].OverrideObserve(rockPPD);
                }
                else
                {
                    protoDataGrid[x, y] = spGrid[x, y].OverrideObserve(soilPPD);
                }
            }
        }

        for (int x = GRID_WIDTH / 2 - 1; x < GRID_WIDTH / 2 + 2; x++)
        {
            for (int y = GRID_HEIGHT / 2 - 1; y < GRID_HEIGHT / 2 + 2; y++)
            {
                //if(x == GRID_WIDTH / 2 && y == GRID_HEIGHT / 2) continue;
                PropogateNeighbors(new Vector2Int(x,y),spGrid[x,y].GetObservedValue());
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
         //press r to remake the whole scene
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.Q))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.W))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.E))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.R))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.T))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.Y))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.U))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.I))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.O))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.P))RebalanceByInput();
        
        if (Input.GetKeyDown(KeyCode.A))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.S))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.D))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.F))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.G))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.H))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.J))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.K))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.L))RebalanceByInput();
        
        if (Input.GetKeyDown(KeyCode.Z))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.X))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.C))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.V))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.B))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.N))RebalanceByInput();
        if (Input.GetKeyDown(KeyCode.M))RebalanceByInput();
        
        if (Input.GetKeyDown(KeyCode.Space))RebalanceByInput();
    }

    void RebalanceByInput()
    {
        int runTimes = 1;//Random.Range(1, 3);
        for (int i = 0; i < runTimes; i++)
        {
            Vector2Int toSelect = new Vector2Int(0,0);
            do
            {
                toSelect = new Vector2Int(Random.Range(0, GRID_WIDTH), Random.Range(0, GRID_HEIGHT));
            } while (IsCellUnchangeable(toSelect));
            
            StartRebalance(toSelect);
        }
    }
    
    void PropogateNeighbors(Vector2Int node, Proto.ProtoData observedValue)
    {
        PropogateTo(node, new Vector2Int(-1, 0), observedValue);
        PropogateTo(node, new Vector2Int(1, 0), observedValue);
        PropogateTo(node, new Vector2Int(0, -1), observedValue);
        PropogateTo(node, new Vector2Int(0, 1), observedValue);
    }

    Vector2Int GetOppositeDirection(Vector2Int orgDirection)
    {
        if (orgDirection.x == 0) return new Vector2Int(orgDirection.x, -orgDirection.y);
        return new Vector2Int(-orgDirection.x, orgDirection.y);
    }
    
    void PropogateTo(Vector2Int node, Vector2Int direction, Proto.ProtoData mustWorkAdjacentTo)
    {
        // Your code for 1-c goes here:

        // Remove impossible values from neighbor node based on the constrains of both tiles
        // Don't forget to check for out of bounds

        Vector2Int newNode = node + direction;

        if (ExistCellAndCanChange(newNode))
        {
            
        }
        else
        {
            return;
        }

        //SuperPosition spOrigional = _grid[node.x, node.y];
        SuperPosition spToCheck = spGrid[newNode.x, newNode.y];
        if (spToCheck.IsObserved()) return;

        if (direction == new Vector2Int(0, 1))
        {
            for (int i = spToCheck.RemainPossibleValues().Count - 1; i >= 0; i--)
            {
                if (spToCheck.RemainPossibleValues()[i].front1 != mustWorkAdjacentTo.back2 || spToCheck.RemainPossibleValues()[i].front2 != mustWorkAdjacentTo.back1) spToCheck.RemovePossibleValue(spToCheck.RemainPossibleValues()[i]);
            }
            //foreach (Proto.ProtoData ppd in allProtoData)
            {
              //  if (ppd.front1 != mustWorkAdjacentTo.back2 || ppd.front2 != mustWorkAdjacentTo.back1) spToCheck.RemovePossibleValue(ppd);
            }
        }
        else if (direction == new Vector2Int(1, 0)) {
            for (int i = spToCheck.RemainPossibleValues().Count - 1; i >= 0; i--)
            {
                if (spToCheck.RemainPossibleValues()[i].left1 != mustWorkAdjacentTo.right2 || spToCheck.RemainPossibleValues()[i].left2 != mustWorkAdjacentTo.right1) spToCheck.RemovePossibleValue(spToCheck.RemainPossibleValues()[i]);
            }
            //foreach (Proto.ProtoData ppd in allProtoData)
            {
             //   if (ppd.left1 != mustWorkAdjacentTo.right2 || ppd.left2 != mustWorkAdjacentTo.right1) spToCheck.RemovePossibleValue(ppd);
            }
        }
        else if (direction == new Vector2Int(0, -1)) {
            for (int i = spToCheck.RemainPossibleValues().Count - 1; i >= 0; i--)
            {
                if (spToCheck.RemainPossibleValues()[i].back1 != mustWorkAdjacentTo.front2 || spToCheck.RemainPossibleValues()[i].back2 != mustWorkAdjacentTo.front1) spToCheck.RemovePossibleValue(spToCheck.RemainPossibleValues()[i]);
            }
            //foreach (Proto.ProtoData ppd in allProtoData)
            {
               // if (ppd.back1 != mustWorkAdjacentTo.front2 || ppd.back2 != mustWorkAdjacentTo.front1) spToCheck.RemovePossibleValue(ppd);
            }
        }
        else {
            for (int i = spToCheck.RemainPossibleValues().Count - 1; i >= 0; i--)
            {
                if (spToCheck.RemainPossibleValues()[i].right1 != mustWorkAdjacentTo.left2 || spToCheck.RemainPossibleValues()[i].right2 != mustWorkAdjacentTo.left1) spToCheck.RemovePossibleValue(spToCheck.RemainPossibleValues()[i]);
            }
            //foreach (Proto.ProtoData ppd in allProtoData)
            {
             //   if (ppd.right1 != mustWorkAdjacentTo.left2 || ppd.right2 != mustWorkAdjacentTo.left1) spToCheck.RemovePossibleValue(ppd);
            }
            /*
            for (int i = 0; i < _tileset.Count; i++)
            {
                if (_tileset[i]._rightRoad != mustWorkAdjacentTo._leftRoad) spToCheck.RemovePossibleValue(i);
            }*/
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
        int minPossibleAmount = 2147483646;
        Vector2Int minPossibleCoord = new Vector2Int(0, 0);
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (!(spGrid[x, y].IsObserved()) &&spGrid[x, y].NumOptions < minPossibleAmount)
                {
                    minPossibleAmount = spGrid[x, y].NumOptions;
                    minPossibleCoord = new Vector2Int(x, y);
                }
            }
        }
        //return the coordinates of the unobserved node with the fewest possible options
        return minPossibleCoord; //replace me
    }
}
