using System.Threading;
using UnityEngine;

public class GridMap: MonoBehaviour
{

    [SerializeField]
    int numThreads = 2;

    object gridMapIsDirtyLock = new Object();
    object gridMapLock = new Object();
    object updateGridLock = new Object();
    object threadsDoneLock = new object();

    bool[] threadsIsDone;

    bool gridMapIsDirty = false;

    int[,] gridMap;
    int[,] newGridMap;
    int[,] oldGridMap;

    Thread[] gridUpdateThreads;

    bool running = true;

    private void Start()
    {
        GetInitialMap();
        StartThreads();
    }

    void StartThreads()
    {
        gridUpdateThreads = new Thread[numThreads + 1];
        threadsIsDone = new bool[numThreads];

        int width = gridMap.GetLength(0);
        int height = gridMap.GetLength(1);

        int widthPerThread = width / numThreads;

        for (int i = 0; i < numThreads; i++)
        {
            int i_min = i * widthPerThread;
            int i_max;
            if (i != numThreads - 1)
                i_max = (i + 1) * widthPerThread;
            else
                i_max = width;

            int threadNum = i;

            gridUpdateThreads[i] = new Thread(() => UpdateGrid(i_min, i_max, threadNum));
            gridUpdateThreads[i].IsBackground = true;
     
        }

        gridUpdateThreads[numThreads] = new Thread(() => GridMapUpdater());
        gridUpdateThreads[numThreads].IsBackground = true;

        foreach (Thread thread in gridUpdateThreads)
            thread.Start();

    }

    // Create a map of the grid
    void GetInitialMap()
    { 
        Texture2D initialMap = (Texture2D)Resources.Load("maps/InitialMap");

        // 0 is dead
        // 1 is alive
        gridMap = new int[initialMap.width, initialMap.height];
        newGridMap = new int[initialMap.width, initialMap.height];
        oldGridMap = new int[initialMap.width, initialMap.height];

        for (int i = 0; i < gridMap.GetLength(0); i++)
        {
            for (int j = 0; j < gridMap.GetLength(1); j++)
            {
                Color color = initialMap.GetPixel(i, j);
                if (color.r < 0.01 || color.g < 0.01 || color.b < 0.01)
                    gridMap[i, j] = 1;

                else gridMap[i, j] = 0;
            }
        }
        EventManager.CreateMesh?.Invoke(gridMap);
        EventManager.UpdateMesh?.Invoke(gridMap);;
        
    }

    private void Update()
    {
        lock (gridMapIsDirtyLock)
        {
            if (gridMapIsDirty)
            {
                EventManager.UpdateMesh(gridMap);
                gridMapIsDirty = false;
            }

        }

    }

    void UpdateGrid(int i_min, int i_max, int threadNum)
    {

        int width = gridMap.GetLength(0);
        int height = gridMap.GetLength(1);

        // Rules
        // Live if 2 <= live neighbours <= 3
        // Spawn if live neighbours == 3
        while (running)
        {
            bool shouldContinue = false;

            // Wait for other threads to complete
            lock (threadsDoneLock)
            {
                if (threadsIsDone[threadNum])
                {
                    shouldContinue = true;

                }
            }

            if (shouldContinue)
            {
                Thread.Sleep(1);
                continue;
            }

            // Update part of the grid
            for (int i = i_min; i < i_max; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    bool isAlive = false;

                    if (gridMap[i, j] > 0)
                        isAlive = true;

                    int aliveNeighbours = CountAliveNeighbours(i, j, width, height);

                    // Case when alive
                    if (isAlive && !(aliveNeighbours >= 2 && aliveNeighbours <= 3))
                        isAlive = false;

                    else if (!isAlive && (aliveNeighbours == 3))
                        isAlive = true;

                    if (isAlive)
                    {
                        newGridMap[i, j] = 1;
                    }
                    else
                    {
                        newGridMap[i, j] = 0;
                    }
                }
            }

            // Set this thread to done.
            lock (threadsDoneLock)
            {
                threadsIsDone[threadNum] = true;
            }
        }
    }

    void GridMapUpdater()
    {
        while (running)
        {
            bool shouldContinue = false;

            lock (threadsDoneLock)
            {
                for (int i = 0; i < numThreads; i++)
                {
                    if (!threadsIsDone[i])
                    {
                        shouldContinue = true;
                        break;
                    }

                }
            }
            if (shouldContinue)
            {
                //Thread.Sleep(1);
                continue;
            }


            lock (gridMapIsDirtyLock)
            {
                oldGridMap = gridMap;
                gridMap = newGridMap;
                gridMapIsDirty = true;
                newGridMap = oldGridMap;
                Thread.Sleep(1);
            }

            lock (threadsDoneLock)
            {
                for (int i = 0; i < numThreads; i++)
                    threadsIsDone[i] = false;
            }

        }
    }


    int CountAliveNeighbours(int i, int j, int width, int height)
    {
        int aliveNeighbours = 0;

        // Right Neighbour
        if (i + 1 < width && gridMap[i + 1, j] > 0)
            aliveNeighbours++;

        // Left Neighbour
        if (i - 1 >= 0 && gridMap[i - 1, j] > 0)
            aliveNeighbours++;

        // Up Neighbour
        if (j + 1 < height && gridMap[i, j + 1] > 0)
            aliveNeighbours++;

        // Right Neighbour
        if (j - 1 >= 0 && gridMap[i, j - 1] > 0)
            aliveNeighbours++;

        // Top Right Neighbour
        if (i + 1 < width && j + 1 < height && gridMap[i + 1, j + 1] > 0)
            aliveNeighbours++;

        // Top Left Neighbour
        if (i - 1 >= 0 && j + 1 < height && gridMap[i - 1, j + 1] > 0)
            aliveNeighbours++;

        // Bottem Left Neighbour
        if (i - 1 >= 0 && j - 1 >= 0 && gridMap[i - 1, j - 1] > 0)
            aliveNeighbours++;

        // Bottem Right Left Neighbour
        if (i + 1 < width && j - 1 >= 0 && gridMap[i + 1, j - 1] > 0)
            aliveNeighbours++;

        return aliveNeighbours;
    }


    private void OnApplicationQuit()
    {
        running = false;
    }
}
