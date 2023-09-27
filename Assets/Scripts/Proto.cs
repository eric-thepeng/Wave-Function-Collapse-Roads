using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Proto : MonoBehaviour
{
    public enum Adjacency {
    Dirt,
    Pond,
    River
    }
    public Adjacency front1, front2, left1, left2, back1, back2, right1, right2;// 
    public int weight = 1;
    public GameObject prefab;
    public float rotationIndex = 0;
    public bool doesNotRotate = false;
    
    public struct AdjacencySet
    {
        public Adjacency adj1, adj2;
        public AdjacencySet(Adjacency firstAdj, Adjacency secondAdj)
        {
            adj1 = firstAdj;
            adj2 = secondAdj;
        }
    }

    public static bool AdjacencySetMatch(AdjacencySet aSet1, AdjacencySet aSet2)
    {
        return aSet1.adj1 == aSet2.adj2 && aSet1.adj2 == aSet2.adj1;
    }
    /*
    public static bool AdjacencyMatch(Adjacency tile1, Adjacency tile2)
    {
        if (tile1.Count != tile2.Count) return false;
        foreach (Adjacency t in tile1)
        {
            if (!tile2.Contains(t)) return false;
        }
        return true;
    }
    /*
    public Proto(Proto copyFrom, int rotateTimes) //rotate clockwise
    {
        prefab = copyFrom.prefab;
        frontAdjacency = copyFrom.frontAdjacency;
        backAdjacency= copyFrom.backAdjacency;
        leftAdjacency= copyFrom.leftAdjacency;
        rightadjacency = copyFrom.rightadjacency;
        for(int i = 0; i < rotateTimes; i++)
        {
            List<Adjacency> tempFront = frontAdjacency;
            frontAdjacency = rightadjacency;
            rightadjacency = backAdjacency;
            backAdjacency = leftAdjacency;
            leftAdjacency = tempFront;
        }
    }*/

    public List<ProtoData> GetAllProtoDataVariations()
    {
        List<ProtoData> returnList = new List<ProtoData>();
        for (int i = 0; i < weight; i++)
        {
            if (doesNotRotate)
            {
                returnList.Add(new ProtoData(this, 0));
            }
            else
            {
                returnList.Add(new ProtoData(this, 0));
                returnList.Add(new ProtoData(this, 1));
                returnList.Add(new ProtoData(this, 2));
                returnList.Add(new ProtoData(this, 3));
            }
        }

        return
            returnList; //new List<ProtoData>() {new ProtoData(this, 0), new ProtoData(this, 1), new ProtoData(this, 2), new ProtoData(this, 3) };
    }

    public interface IWeighted
    {
        public float GetWeight();
    }

    public class ProtoData:IWeighted
    {
        //public List<Adjacency> frontAdjacency, backAdjacency, leftAdjacency, rightadjacency;
        public Adjacency front1, front2, left1, left2, back1, back2, right1, right2;
        public float weight;
        public GameObject prefab;
        public float rotationIndex;
        public ProtoData(Proto copyFrom, int rotateTimes)//rotate clockwise
        {
            prefab = copyFrom.prefab;
            weight = copyFrom.weight;
            rotationIndex = rotateTimes;


            front1 = copyFrom.front1;
            left1 = copyFrom.left1;
            back1 = copyFrom.back1;
            right1 = copyFrom.right1;
            front2 = copyFrom.front2;
            left2 = copyFrom.left2;
            back2 = copyFrom.back2;
            right2 = copyFrom.right2;
            for (int i = 0; i < rotateTimes; i++)
            {
                Adjacency tempFront1 = front1;
                Adjacency tempFront2 = front2;
                front1 = right1;
                front2 = right2;
                right1 = back1;
                right2= back2;
                back1 = left1;
                back2= left2;
                left1 = tempFront1;
                left2 = tempFront2;
            }
        }

        public AdjacencySet GetAdjacencySetByDirection(Vector2Int coord)
        {
            if (coord == Vector2Int.left) return new AdjacencySet(left1, left2);
            if (coord == Vector2Int.right) return new AdjacencySet(right1, right2);
            if (coord == Vector2Int.up) return new AdjacencySet(back1, back2);
            return new AdjacencySet(front1, front2);
        }

        public override string ToString()
        {
            return "name: " + prefab.name + " rotation: " + rotationIndex; // + "\n left: " + agencyListToString(leftAdjacency) + "\n right: " + agencyListToString(rightadjacency);
        }

        string  AdjacencyListToString(Adjacency adj)
        {
            string retString = "";
            if (adj == Adjacency.Dirt) retString += " Dirt";
            else if (adj == Adjacency.Pond) retString += " Pond";
            else if (adj == Adjacency.River) retString += " River";
            return retString;
        }

        string AdjacencySetToString(AdjacencySet aset)
        {
            string export = "";
            export += "first: " + AdjacencyListToString(aset.adj1) + " second: " + AdjacencyListToString(aset.adj2);
            return export;
        }

        public float GetWeight() { return weight; }
    }
}
