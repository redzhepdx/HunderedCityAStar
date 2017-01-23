using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class Algorithms {

    /***************************************MINIMUM SPANNING TREE KRUSKAL ALGORİTHM********************************************/
    public static List<Structures.Edge> MSPKruskal(Structures.Graph graph)
    {
        List<Structures.Edge> resultEdges = new List<Structures.Edge>();
        int edgeCount = 0;
        int i = 0;

        graph.edges.Sort();//Sorting All Edges
        
        while(edgeCount < graph.vertices.Count - 1)
        {
                
            Structures.Edge edge = graph.edges[i];
            Structures.Vertex sourceParent = findParent(graph.vertices, edge.source);//Find parent

            Structures.Vertex destinationParent = findParent(graph.vertices, edge.destination);//Find parent
            
            //if there is no cycle with this way
            if (sourceParent.parentID != destinationParent.parentID)
            {
                //Add to the tree
                Union(graph.vertices, sourceParent, destinationParent);
                //Add to the msp edge list
                resultEdges.Add(edge);
                edgeCount++;
            }
            
            i++;
        }

        return resultEdges;

    }

    //Finding parent vertex from Disjoint-Set for the cycyle detect 
    static Structures.Vertex findParent(List<Structures.Vertex> vertices , Structures.Vertex vertex)
    {
        Structures.Vertex v = vertex;

        while (true)
        {
            if (vertices[v.index].parentID != v.index)
            {
                v = vertices[v.parentID];
            }
            else
            {
                return vertices[v.parentID];
            }
        }
    }
    
    //Adding vertices to the Tree with given edge infos(vertices will represent rank order inside tree)
    static void Union(List<Structures.Vertex> vertices, Structures.Vertex source , Structures.Vertex destination)
    {
        Structures.Vertex v_source_root = findParent(vertices ,source);
        Structures.Vertex v_destination_root = findParent(vertices, destination);

        if (v_source_root.rank > v_destination_root.rank)
        {
            vertices[v_destination_root.index].parentID = v_source_root.parentID;
        }
        else if(v_source_root.rank < v_destination_root.rank)
        {
            vertices[v_source_root.index].parentID = v_destination_root.parentID;
        }
        else
        {
            vertices[v_destination_root.index].parentID = v_source_root.parentID;
            vertices[v_source_root.index].rank++;
        }

    }

    //Random Way Generation
    public static List<Structures.Edge> GenerateRandomEdges(Structures.Graph graph)
    {
        List<Structures.Edge> randomEdges = new List<Structures.Edge>();
        Dictionary<Structures.Edge, Structures.Edge> edgeCheck = new Dictionary<Structures.Edge, Structures.Edge>();

        int index = 0;

        while(index < graph.vertices.Count - 1)
        {
            Structures.Vertex vertexA = graph.vertices[index];

            int randomIndex = (int)Random.Range(0, graph.vertices.Count);

            while (index == randomIndex)
            {
                randomIndex = (int)Random.Range(0, graph.vertices.Count);
            }

            Structures.Vertex vertexB = graph.vertices[randomIndex];
            
            Structures.Edge edge = (vertexA.index < vertexB.index) ? new Structures.Edge(vertexA, vertexB) : new Structures.Edge(vertexB, vertexA);

            if (!graph.edges.Contains(edge) && !edgeCheck.ContainsKey(edge))
            {
                randomEdges.Add(edge);
                edgeCheck.Add(edge, edge);
                index++;
            }
            
        }


        return randomEdges;
    }
    
    //Adding Edge new Edge list inside to graph 
    public static void AddEdgesToGraph(List<Structures.Edge> edges , Structures.Graph graph)
    {
        foreach(Structures.Edge edge in edges)
        {
            graph.edges.Add(edge);
        }
    }

    /***************************************A-STAR PATHFINDING ALGORITIHM********************************************/
    public static List<Structures.Vertex> AStarPathfinding(Structures.Vertex startVertex, Structures.Vertex endvertex, Structures.Graph graph , string filePath)
    {
        List<Structures.Vertex> openSet = new List<Structures.Vertex>();//Reachable Vertices(Like Queue)
        HashSet<Structures.Vertex> closedSet = new HashSet<Structures.Vertex>();//Reached Vertices
        List<Structures.Vertex> path = new List<Structures.Vertex>();//Path vertices
        openSet.Add(startVertex);
        
        while(openSet.Count > 0)
        {
            Structures.Vertex currentVertex = openSet[0];

            //Find lowest vertex which have lowest f_cost in reachable vertex
            for(int i = 1; i < openSet.Count; i++)
            {
                if(openSet[i].f_cost < currentVertex.f_cost || openSet[i].f_cost == currentVertex.f_cost)
                {
                    if (openSet[i].h_cost < currentVertex.h_cost)
                        currentVertex = openSet[i];
                }

            }
            
            
            openSet.Remove(currentVertex);
            closedSet.Add(currentVertex);

            //If path found retrace all path for the check errors
            if (currentVertex.Equals(endvertex))
            {
                TextWriter tw = new StreamWriter(filePath, true);
                
                /*Writing Cities inide Queue*/
                tw.WriteLine("Last State of Queue \n");

                foreach(Structures.Vertex v in openSet)
                {
                    tw.WriteLine(v.index + ".City \n");
                }

                tw.WriteLine("Number of elements picked from Queue : " + closedSet.Count + "\n");

                tw.Close();

                //Retracing Path
                path = RetracePath(startVertex, endvertex);
                return path;
            }
          
            /*For all neigbour vertices
              If neigbour not reached and its g_cost lower than current city's g_cost  
              Add neigbour in the Queue 
              Neigbours parent vertex is currentVertex
            */   
            foreach (Structures.Vertex neighbourVertex in graph.connections[currentVertex.index])
            {
                
                if (closedSet.Contains(neighbourVertex))
                {
                    continue;
                }

                float newCostToNeighbour = currentVertex.g_cost + currentVertex.CalculateDistanceWithPercent(neighbourVertex);
                if (newCostToNeighbour < neighbourVertex.g_cost || !openSet.Contains(neighbourVertex))
                {
                    
                    graph.vertices[neighbourVertex.index].g_cost = newCostToNeighbour;
                    graph.vertices[neighbourVertex.index].h_cost = neighbourVertex.CalculateDistanceWithPercent(endvertex);
                    graph.vertices[neighbourVertex.index].parent = graph.vertices[currentVertex.index];

                    if (!openSet.Contains(graph.vertices[neighbourVertex.index]))
                    {
                        openSet.Add(graph.vertices[neighbourVertex.index]);
                    }
                }
            }
            
        }

        return null;
    }

    //Retracing found path and getting list of ways
    private static List<Structures.Vertex> RetracePath(Structures.Vertex startVertex , Structures.Vertex endVertex)
    {
        List<Structures.Vertex> path = new List<Structures.Vertex>();

        Structures.Vertex currentVertex = endVertex;

        while (!currentVertex.Equals(startVertex))
        {
            path.Add(currentVertex);

            currentVertex = currentVertex.parent;
           
        }

        path.Add(startVertex);

        path.Reverse();

        return path;
    }

}
