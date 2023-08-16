using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class threadedDataRequester : MonoBehaviour
{
    Queue<MapThreadInfo<HeightMap>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<HeightMap>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void RequestHeightMap(Vector2 centre, Action<HeightMap> callback)
    {
        ThreadStart threadStart = delegate
        {
            HeightMapThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    void HeightMapThread(Vector2 centre, Action<HeightMap> callback)
    {
        HeightMap heightMap = heightMapGenerator.generateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, centre);
        lock (mapDataThreadInfoQueue) //thread can only be executed one at a time
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<HeightMap>(callback, heightMap)); //add new threading task to queue
        }

    }

    public void RequestMeshData(HeightMap heightMap, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(heightMap, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(HeightMap heightMap, int lod, Action<MeshData> callback)
    {
        MeshData meshData = meshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod);
        lock (meshDataThreadInfoQueue) //thread can only be executed one at a time
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData)); //add new threading task to queue
        }
    }

    private void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<HeightMap> threadInfo = mapDataThreadInfoQueue.Dequeue(); //next thing in queue
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
