using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    public int obstacleProximityPenalty = 500;
    LayerMask walkableMask;
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
    Node[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    int penaltyMin = int.MaxValue;
    int penaltyMax = int.MinValue;

    bool visible = true;

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }


    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        foreach (TerrainType region in walkableRegions) //adds every region given in the walkable regions array to the dictionary and assigns it a penalty value
        {
            walkableMask.value |= region.terrainMask.value;
            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
        }
    }

    private void Start()
    {
        Debug.Log("Grid waiting to be created");
        StartCoroutine(WaitSeconds());
    }

    /// <summary>
    /// Delays the grid creation to wait for the terrain mesh to be generated
    /// </summary>
    IEnumerator WaitSeconds()
    {
        Debug.Log("Grid is creating");
        yield return new WaitForSeconds(4);
        CreateGrid();
    }

    /// <summary>
    /// Creates the grid of nodes to be used for pathfinding, and determines if a node is walkable or unwalkable
    /// </summary>
    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY]; // creation of the grid (a 2D array of Nodes)
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2; // calculates the bottom left node as a point of reference

        for (int x = 0; x < gridSizeX; x++)
            for (int z = 0; z < gridSizeX; z++) // iterates through every node
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                int movementPenalty = 0;

                // downwards raycast to calculate A* movement penalty for each node based on layermask or terrain height
                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, 100)) // shoots ray down & outputs hit information
                {
                    worldPoint = new Vector3(worldPoint.x, hit.point.y, worldPoint.z); // sets the Y position of the node to the Y hit point of the ray
                                                                                       // (i.e. maps the node's position to the height of the terrain)
                }
                walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                if (worldPoint.y > 12)
                {
                    int penaltyVal = (int)(((int)worldPoint.y - 12) * 0.4); // sets the node's movement penalty value based off its y position
                                                                            // (i.e. determines higher terrain to be less desirable for pathfinding)
                    movementPenalty += penaltyVal;
                }
                if (worldPoint.y <= 2.6)
                {
                    walkable = false; // makes a node unwalkable if it is below sea level
                }
                
                if (hit.point.y < 2)
                {
                    visible = false; // hides the node gizmo to optimise debugging within the Unity editor
                }
                else
                {
                    visible = true;
                }

                if (!walkable)
                {
                    movementPenalty += obstacleProximityPenalty; // sets the movement penalty of unwalkable nodes
                                                                 // (i.e. nodes that aren't walkable are so undesirable for travel that they are never used)
                }

                grid[x, z] = new Node(walkable, worldPoint, x, Mathf.RoundToInt(worldPoint.y), z, movementPenalty, visible);
            }

        BlurPenaltyMap(3);
    }

    /// <summary>
    /// Uses a box blur algorithm to blur the penalties between adjacent nodes
    /// (i.e. making terrain penalty differences less harsh across terrain)
    /// </summary>
    /// <param name="blurSize"> The intensity of value blurring </param>
    void BlurPenaltyMap(int blurSize) 
    {
        int kernelSize = blurSize * 2 + 1; // kernelSize must always be odd
        int kernelExtents = (kernelSize - 1) / 2; // the number of nodes between the centre and the edge

        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY]; // used to store the results of the horizontal pass through the node grid
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY]; // used to store the results of the vertical pass through the node grid

        for (int y = 0; y < gridSizeY; y++) // horizontal pass to calculate the horizontal total of each node penalty
        {
            for (int x = -kernelExtents; x <= kernelExtents; x++)
            {
                int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;
            }

            for (int x = 1; x < gridSizeX; x++)
            {
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX); // the index to the left of the current node, to be removed
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1); //the index to the right of the current node, to be added

                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
            }
        }

        for (int x = 0; x < gridSizeX; x++) // vertical pass to calculate the vertical total of each node in the horizontal penalties array
        {
            for (int y = -kernelExtents; y <= kernelExtents; y++)
            {
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
            }

            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
            grid[x, 0].movementPenalty = blurredPenalty;

            for (int y = 1; y < gridSizeY; y++)
            {
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY); // the index above the current node, to be removed
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1); // the index to below of the current node, to be added

                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize)); // calculates the final average of both iterations, rounded to an integer
                grid[x, y].movementPenalty = blurredPenalty; // assigns the new penalty value

                if (blurredPenalty > penaltyMax)
                    penaltyMax = blurredPenalty;
                if (blurredPenalty < penaltyMin)
                    penaltyMin = blurredPenalty;
            }
        }
    }

    /// <summary>
    /// Returns a list of all the neighbouring nodes of the parameter node
    /// </summary>
    /// <param name="node"> The node in which its neighbours are being found </param>
    /// <returns></returns>
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++) // iterates through all neighbouring nodes
            {
                if (x == 0 && y == 0) // skip the centre node of the 3x3 area (i.e. the current node)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) // ensures the nodes' coordinates are valid
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }

        return neighbours;
    }

    /// <summary>
    /// Identifies a node in the grid using a world position
    /// </summary>
    /// <param name="worldPosition"> The world position to be located on the grid </param>
    /// <returns> The node in the grid array </returns>
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        // calculates the world position's percentage through the grid size
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        // calculates the grid position using the percentage
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        Debug.Log(grid[x,y].worldPosition);
        return grid[x, y];
    }

    /// <summary>
    /// Displays the grid and how walkable each node is in Unity's scene view, used for debugging and visualising
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        if (grid != null && displayGridGizmos)
        {
            foreach (Node n in grid)
            {
                if (n.visible)
                {
                    Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty)); // the darkness of each node denotes its movement penalty value (white = ideal)

                    Gizmos.color = (n.walkable) ? Gizmos.color : Color.red; // nodes become red if it isn't walkable
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter));
                }
            }
        }
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }
}