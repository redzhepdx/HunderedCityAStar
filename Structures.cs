using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public static class Structures{

    /*GRAPH*/
    public class Graph
    {
        int vertexCount;//City count
        int edgeCount;//Way count

        public List<Vertex> vertices = new List<Vertex>();//Cities
        public List<Edge> edges = new List<Edge>();//Ways
        public List<List<Vertex>> connections = new List<List<Vertex>>();//All connections between all cities
        
        //Constructor
        public Graph(int vertexCount , int edgeCount)
        {
            this.vertexCount = vertexCount;
            this.edgeCount = edgeCount;
        }

        /*Finding all neigbour city coordinates foreach city*/
        public void FindAllConnections()
        {
            
            foreach(Vertex v in vertices)
            {
                List<Vertex> v_connections = new List<Vertex>();
                List<int> indexes = new List<int>();
                foreach (Edge e in edges)
                {
                    if(e.source == v)
                    {

                        if (!indexes.Contains(e.destination.index))
                        {
                            v_connections.Add(e.destination);
                            indexes.Add(e.destination.index);
                        }    
                    }
                    else if(e.destination == v)
                    {
                        if (!indexes.Contains(e.source.index))
                        {
                            v_connections.Add(e.source);
                            indexes.Add(e.source.index);
                        }
                    }
                }

                connections.Add(v_connections);
            }
        }

    }

    //Edge 
    public class Edge : IComparable<Edge>   
    {
        //Every edge(way) have a source and destination coordinates
        public Vertex source;
        public Vertex destination;
        public float Weight;//Length of way

        //Constructor
        public Edge(Vertex source , Vertex destination)
        {
            this.source = source;
            this.destination = destination;
            this.Weight = source.CalculateDistanceWithPercent(destination);
        }

        public int CompareTo(Edge other)
        {
            return this.Weight.CompareTo(other.Weight);           
        }

        public override bool Equals(object other)
        {
            return (this.source.index == ((Edge)other).source.index) && (this.destination.index == ((Edge)other).destination.index);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    //Vertex class For Hold City Informations
    public class Vertex 
    {
        public Vertex parent;//Parent vertex 

        public float xPos;//X coordinate
        public float yPos;//Y coordinate
        public float g_cost = 0f;//Cost of the path from the start city
        public float h_cost = 0f;//Heuristic Cost

        public int parentID;//Parent index ID (for msp cycle detection)
        public int index;//City index
        public int rank;//Rank in tree
                
        public float f_cost
        {
            get
            {
                return g_cost + h_cost;
            }
        }

        //Constructor
        public Vertex(float xPos, float yPos , int rank ,int index, int parentID)
        {
            this.xPos = xPos;
            this.yPos = yPos;
            this.rank = rank;
            this.index = index;
            this.parentID = parentID;
        }

        //Distance between Two Vertex
        public float CalculateDistanceWithPercent(Vertex other)
        {
            float distance = this.CalculateRealDistance(other);

            float incrementPercent = UnityEngine.Random.Range(10.0f, 50.0f);//Increment percent (%10 - %50)

            return distance + distance * (incrementPercent / 100f);
        }

        public float CalculateRealDistance(Vertex other)
        {
            return Mathf.Sqrt(Mathf.Pow((this.xPos - other.xPos), 2) +
                              Mathf.Pow((this.yPos - other.yPos), 2));
        }

        public override bool Equals(object obj)
        {
            return (this.index == ((Vertex)obj).index);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

}
