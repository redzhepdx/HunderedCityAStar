using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class Generate : MonoBehaviour {

    public Material[] materials;/*#Visual Code*/

    public GameObject node;/*#Visual Code*/
    public InputField startIndexText;/*#Visual Code*/
    public InputField endIndexText;/*#Visual Code*/

    public Text Qtext;/*#Visual Code*/
    public Text Wtext;/*#Visual Code*/
    public Text Ttext;/*#Visual Code*/
    public Text Info;/*#Visual Code*/
    public Text Info2;/*#Visual Code*/
    public Text pressButton;/*#Visual Code*/
    public Text space;/*#Visual Code*/
    public Text esc;/*#Visual Code*/

    /*#Visual Code*/
    enum graphDrawState
    {
        msp_edges, random_edges, path_edges, idle
    };

    graphDrawState state = graphDrawState.idle;/*#Visual Code*/

    public int cityCount = 100;//City Number

    public float defaultZpos;/*#Visual Code*/

    public Vector2 range;/*#Visual Code*/

    List<Structures.Vertex> cities;//City Coordinates
    List<Structures.Vertex> path;//Cities on path

    Dictionary<Vector3, Vector3> coordinates;/*#Visual Code*/

    List<GameObject> nodes;/*#Visual Code*/

    Structures.Graph graph;//Graph

    List<Structures.Edge> allEdges;//All Ways in graph
    List<Structures.Edge> pathEdges;//Path Ways
    List<Structures.Edge> resultEdges;//Msp Ways
    List<Structures.Edge> randomEdges;//Random Ways

    bool simulateFlag = false;/*Visual Code*/

    int startNodeIndex;//Start City Index
    int endNodeIndex;//End City Index

    string dirPath;
    string filePath = "info.txt";

    void Awake()
    {
        dirPath = Application.dataPath;

        cities = new List<Structures.Vertex>();

        coordinates = new Dictionary<Vector3, Vector3>();

        nodes = new List<GameObject>();

        dirPath += "/Infos";

        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        
    }
    
    /*#Visual Code*/
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(0);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (simulateFlag)
        {
            startIndexText.enabled = false;
            endIndexText.enabled = false;

            if (Input.GetKeyDown(KeyCode.Q))
            {
                state = graphDrawState.msp_edges;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                state = graphDrawState.random_edges;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                state = graphDrawState.path_edges;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                state = graphDrawState.idle;
            }

            switch (state)
            {
                case graphDrawState.msp_edges:
                    Invoke("DrawMspEdges", 1f);
                    break;
                case graphDrawState.random_edges:
                    Invoke("DrawRandomEdges", 1f);
                    break;
                case graphDrawState.path_edges:
                    Invoke("DrawPathEdges", 1f);
                    state = graphDrawState.idle;
                    break;
            }
        }

    }

    void Run()
    {
        CreateRandomCities();//Generating Random City Coordinates

        graph = new Structures.Graph(cities.Count, allEdges.Count);

        graph.vertices = new List<Structures.Vertex>(cities);

        graph.edges = new List<Structures.Edge>(allEdges);

        resultEdges = Algorithms.MSPKruskal(graph);//Kruskal Minimum Spanning Tree

        graph.edges = new List<Structures.Edge>(resultEdges);

        randomEdges = Algorithms.GenerateRandomEdges(graph);//Generating Random Ways

        Algorithms.AddEdgesToGraph(randomEdges, graph);//Combining Edge lists

        graph.FindAllConnections();//Finding all Connections between all individual cities 

        WriteFileAllEdgesInGraph();
        float t1 = Time.fixedTime;
        path = Algorithms.AStarPathfinding(graph.vertices[startNodeIndex], graph.vertices[endNodeIndex], graph , Path.Combine(dirPath , filePath));//A* Pathfinding
        pathEdges = FindEdges(path);/*#Visual Code*/
    }

    public void SimulatePressed()
    {
        startNodeIndex = int.Parse(startIndexText.GetComponent<InputField>().text);
        endNodeIndex = int.Parse(endIndexText.GetComponent<InputField>().text);

        simulateFlag = true;

        Run();

        Qtext.enabled = false;
        Wtext.enabled = false;
        Ttext.enabled = false;
        Info.enabled = false;
        Info2.enabled = false;
        pressButton.enabled = false;
        space.enabled = false;
        esc.enabled = false;
        ClearNotNecessaryEdges();
    }

    /*#Visual Code*/
    void DrawPathEdges()
    {
        SetNodeFeatures();
        StartCoroutine(MarkEdges(pathEdges, 2, 5.5f));
    }
    /*#Visual Code*/
    void DrawMspEdges()
    {
        StartCoroutine(MarkEdges(resultEdges, 0, 2.5f));
    }
    /*#Visual Code*/
    void DrawRandomEdges()
    {
        StartCoroutine(MarkEdges(randomEdges, 0, 2.5f));
    }
    /*#Visual Code*/
    void SetNodeFeatures()
    {
        foreach(Structures.Vertex v in path)
        {
            GameObject node = GameObject.Find("node" + v.index);
            node.transform.localScale *= 1.5f;
            node.GetComponent<Renderer>().material = materials[3];
        }
    }

    /*#Visual Code*/
    IEnumerator MarkEdges(List<Structures.Edge> resultEdges , int materialNum , float size)
    {
        state = graphDrawState.idle;
        foreach (Structures.Edge g_edge in allEdges)
        {
            foreach (Structures.Edge r_edge in resultEdges)
            {
                if (g_edge.Equals(r_edge))
                {
                    Transform lineRenderer = nodes[g_edge.source.index].transform
                                            .FindChild("Line(" + r_edge.source.index + "," + r_edge.destination.index + ")");
                    lineRenderer.GetComponent<LineRenderer>().material = materials[materialNum];
                    lineRenderer.GetComponent<LineRenderer>().SetWidth(size, size);
                    lineRenderer.GetComponent<LineRenderer>().receiveShadows = true;
                    

                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }

    void ClearNotNecessaryEdges()
    {
        foreach (Structures.Edge g_edge in allEdges)
        {
            foreach (Structures.Edge r_edge in graph.edges)
            {
                if (g_edge.Equals(r_edge))
                {
                    Transform lineRenderer = nodes[g_edge.source.index].transform
                                            .FindChild("Line(" + g_edge.source.index + "," + g_edge.destination.index + ")");
                    lineRenderer.GetComponent<LineRenderer>().SetPosition(0, new Vector3(cities[g_edge.source.index].xPos, cities[g_edge.source.index].yPos, defaultZpos + 15f));
                    lineRenderer.GetComponent<LineRenderer>().SetPosition(1, new Vector3(cities[g_edge.destination.index].xPos, cities[g_edge.destination.index].yPos, defaultZpos + 15f));

                }
            }
        }
    }
    
    //Writing all way infos inside graph
    void WriteFileAllEdgesInGraph()
    {
        TextWriter tw = new StreamWriter(Path.Combine(dirPath, filePath), true);

        tw.WriteLine("All Ways Between Each City");

        foreach (Structures.Edge edge in graph.edges)
        {
              
            string a = "Way Between " + edge.source.index.ToString() + ".City and " + edge.destination.index.ToString() + ".City \n";
            string b = "Length : " + edge.Weight.ToString() + "\n";
            tw.WriteLine(a);
            tw.WriteLine(b);

        }
        
        tw.Close();
    }

    /*#Visual Code*/
    List<Structures.Edge> FindEdges(List<Structures.Vertex> vertices)
    {
        List<Structures.Edge> edges = new List<Structures.Edge>();

        for (int i = 0; i < (vertices.Count - 1); i++)
        {
            foreach(Structures.Edge edge in graph.edges)
            {
                if((edge.source.Equals(vertices[i]) && edge.destination.Equals(vertices[i+1])) || 
                   (edge.destination.Equals(vertices[i]) && edge.source.Equals(vertices[i + 1])))
                {
                    edges.Add(edge);
                }
                
            }
        }
        
        return edges;
    }

    //Random City Coordinate Generating
    void CreateRandomCities()
    {
        for (int i = 0; i < cityCount; i++)
        {
            float posX = Random.Range(-range.x, range.x);
            float posY = Random.Range(-range.y, range.y);

            Vector3 newPos = new Vector3(posX, posY, defaultZpos);

            //If new city coordinate exists , generate new coordinate
            while (coordinates.ContainsKey(newPos))
            {
                posX = Random.Range(-range.x, range.x);
                posY = Random.Range(-range.y, range.y);

                newPos = new Vector3(posX, posY, defaultZpos);
            }

            Structures.Vertex newVertex = new Structures.Vertex(newPos.x, newPos.y, 0, i, i);
            
            cities.Add(newVertex);

            GameObject newNode = Instantiate(node, newPos, Quaternion.identity) as GameObject;
            newNode.name = "node" + i;
            nodes.Add(newNode);

        }
        allEdges = new List<Structures.Edge>(CreateEdges(cities, nodes));
    }
    /*#Visual Code*/
    List<Structures.Edge> CreateEdges(List<Structures.Vertex> cities , List<GameObject> nodes)
    {
        List<Structures.Edge> newAllEdges = new List<Structures.Edge>();
        
        for(int i = 0; i < cities.Count - 1; i++)
        {
            for (int j = i + 1; j < cities.Count; j++)
            {
                Structures.Edge edge = new Structures.Edge(cities[i], cities[j]);
                GameObject lineRendererHolder = new GameObject();
                lineRendererHolder.name = "Line(" + i + "," + j + ")";
                lineRendererHolder.AddComponent<LineRenderer>();
                lineRendererHolder.GetComponent<LineRenderer>().material = materials[1];
                lineRendererHolder.GetComponent<LineRenderer>().SetWidth(.3f, .3f);
                lineRendererHolder.GetComponent<LineRenderer>().SetPosition(0, new Vector3(cities[i].xPos, cities[i].yPos, defaultZpos));
                lineRendererHolder.GetComponent<LineRenderer>().SetPosition(1, new Vector3(cities[j].xPos, cities[j].yPos, defaultZpos));
                lineRendererHolder.transform.parent = nodes[i].transform;
                newAllEdges.Add(edge);
            }
        }

        return newAllEdges;

    }
}
