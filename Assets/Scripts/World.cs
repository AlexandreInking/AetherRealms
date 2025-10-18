using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class World : MonoBehaviour
{
    // mapSizeInChunks y chunkDrawingRange ya no se usan aquí, los controla el GameManager
    public int chunkSize = 16, chunkHeight = 100;
    public GameObject chunkPrefab;
    public WorldRenderer worldRenderer;
    public TerrainGenerator terrainGenerator;
    public Vector2Int mapSeedOffset;
    
    CancellationTokenSource taskTokenSource = new CancellationTokenSource();
    
    public UnityEvent OnWorldCreated; // Ya no necesitamos OnNewChunksGenerated
    public WorldData worldData { get; private set; }
    public bool IsWorldCreated { get; private set; }

    private void Awake()
    {
        worldData = new WorldData
        {
            chunkHeight = this.chunkHeight,
            chunkSize = this.chunkSize,
            chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>(),
            chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>()
        };
    }

    // NUEVO MÉTODO DE GENERACIÓN PARA RTS
    public async void GenerateWorld(int mapSize)
    {
        // Genera los puntos de bioma basados en el centro del mapa
        terrainGenerator.GenerateBiomePoints(Vector3.zero, mapSize, chunkSize, mapSeedOffset);

        // Calcula todas las posiciones de los chunks para el mapa completo
        List<Vector3Int> chunkPositionsToCreate = new List<Vector3Int>();
        for (int x = -mapSize / 2; x < mapSize / 2; x++)
        {
            for (int z = -mapSize / 2; z < mapSize / 2; z++)
            {
                chunkPositionsToCreate.Add(new Vector3Int(x * chunkSize, 0, z * chunkSize));
            }
        }
        
        // --- El resto del proceso es similar pero simplificado ---

        ConcurrentDictionary<Vector3Int, ChunkData> dataDictionary = null;
        try
        {
            dataDictionary = await CalculateWorldChunkData(chunkPositionsToCreate);
        }
        catch (Exception)
        {
            Debug.Log("Task canceled");
            return;
        }

        foreach (var calculatedData in dataDictionary)
        {
            worldData.chunkDataDictionary.Add(calculatedData.Key, calculatedData.Value);
        }
        
        ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary = new ConcurrentDictionary<Vector3Int, MeshData>();
        List<ChunkData> dataToRender = worldData.chunkDataDictionary.Values.ToList();

        try
        {
            meshDataDictionary = await CreateMeshDataAsync(dataToRender);
        }
        catch (Exception)
        {
            Debug.Log("Task canceled");
            return;
        }

        StartCoroutine(ChunkCreationCoroutine(meshDataDictionary));
    }

    private Task<ConcurrentDictionary<Vector3Int, MeshData>> CreateMeshDataAsync(List<ChunkData> dataToRender)
    {
        ConcurrentDictionary<Vector3Int, MeshData> dictionary = new ConcurrentDictionary<Vector3Int, MeshData>();
        return Task.Run(() =>
        {

            foreach (ChunkData data in dataToRender)
            {
                if (taskTokenSource.Token.IsCancellationRequested)
                {
                    taskTokenSource.Token.ThrowIfCancellationRequested();
                }
                MeshData meshData = Chunk.GetChunkMeshData(data);
                dictionary.TryAdd(data.worldPosition, meshData);
            }

            return dictionary;
        }, taskTokenSource.Token
        );
    }

    private Task<ConcurrentDictionary<Vector3Int, ChunkData>> CalculateWorldChunkData(List<Vector3Int> chunkDataPositionsToCreate)
    {
        ConcurrentDictionary<Vector3Int, ChunkData> dictionary = new ConcurrentDictionary<Vector3Int, ChunkData>();

        return Task.Run(() => 
        {
            foreach (Vector3Int pos in chunkDataPositionsToCreate)
            {
                if (taskTokenSource.Token.IsCancellationRequested)
                {
                    taskTokenSource.Token.ThrowIfCancellationRequested();
                }
                ChunkData data = new ChunkData(chunkSize, chunkHeight, this, pos);
                ChunkData newData = terrainGenerator.GenerateChunkData(data, mapSeedOffset);
                dictionary.TryAdd(pos, newData);
            }
            return dictionary;
        },
        taskTokenSource.Token
        );
        
        
    }

    IEnumerator ChunkCreationCoroutine(ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary) 
    {
        foreach (var item in meshDataDictionary)
        {
            CreateChunk(worldData, item.Key, item.Value);
            yield return new WaitForEndOfFrame();
        }
        if (IsWorldCreated == false)
        {
            IsWorldCreated = true;
            OnWorldCreated?.Invoke();
        }
    }

    private void CreateChunk(WorldData worldData, Vector3Int position, MeshData meshData)
    {
        ChunkRenderer chunkRenderer = worldRenderer.RenderChunk(worldData, position, meshData);
        worldData.chunkDictionary.Add(position, chunkRenderer);

    }

    internal bool SetBlock(RaycastHit hit, BlockType blockType)
    {
        ChunkRenderer chunk = hit.collider.GetComponent<ChunkRenderer>();
        if (chunk == null)
            return false;

        Vector3Int pos = GetBlockPos(hit);

        WorldDataHelper.SetBlock(chunk.ChunkData.worldReference, pos, blockType);
        chunk.ModifiedByThePlayer = true;

        if (Chunk.IsOnEdge(chunk.ChunkData, pos))
        {
            List<ChunkData> neighbourDataList = Chunk.GetEdgeNeighbourChunk(chunk.ChunkData, pos);
            foreach (ChunkData neighbourData in neighbourDataList)
            {
                //neighbourData.modifiedByThePlayer = true;
                ChunkRenderer chunkToUpdate = WorldDataHelper.GetChunk(neighbourData.worldReference, neighbourData.worldPosition);
                if (chunkToUpdate != null)
                    chunkToUpdate.UpdateChunk();
            }

        }

        chunk.UpdateChunk();
        return true;
    }

    private Vector3Int GetBlockPos(RaycastHit hit)
    {
        Vector3 pos = new Vector3(
             GetBlockPositionIn(hit.point.x, hit.normal.x),
             GetBlockPositionIn(hit.point.y, hit.normal.y),
             GetBlockPositionIn(hit.point.z, hit.normal.z)
             );

        return Vector3Int.RoundToInt(pos);
    }

    private float GetBlockPositionIn(float pos, float normal)
    {
        if (Mathf.Abs(pos % 1) == 0.5f)
        {
            pos -= (normal / 2);
        }

        return (float)pos;
    }

    internal BlockType GetBlockFromChunkCoordinates(int x, int y, int z)
    {
        Vector3Int pos = new Vector3Int(x, y, z);
        ChunkData containerChunk = WorldDataHelper.GetChunkData(this, pos);

        if (containerChunk == null)
            return BlockType.Nothing;

        Vector3Int blockInChunkCoordinates = Chunk.GetBlockInChunkCoordinates(containerChunk, pos);
        return Chunk.GetBlockFromChunkCoordinates(containerChunk, blockInChunkCoordinates);
    }
}
