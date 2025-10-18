using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class WorldDataHelper
{
    public static Vector3Int ChunkPositionFromBlockCoords(World world, Vector3Int worldBlockPosition)
    {
        return new Vector3Int
        {
            x = Mathf.FloorToInt(worldBlockPosition.x / (float)world.chunkSize) * world.chunkSize,
            y = Mathf.FloorToInt(worldBlockPosition.y / (float)world.chunkHeight) * world.chunkHeight,
            z = Mathf.FloorToInt(worldBlockPosition.z / (float)world.chunkSize) * world.chunkSize
        };
    }

    internal static void RemoveChunkData(World world, Vector3Int pos)
    {
        world.worldData.chunkDataDictionary.Remove(pos);
    }

    internal static void RemoveChunk(World world, Vector3Int pos)
    {
        ChunkRenderer chunk = null;
        if (world.worldData.chunkDictionary.TryGetValue(pos, out chunk))
        {
            world.worldRenderer.RemoveChunk(chunk);
            world.worldData.chunkDictionary.Remove(pos);
        }
    }

    public static void SetBlock(World world, Vector3Int worldPosition, BlockType block)
    {
        Vector3Int chunkPosition = ChunkPositionFromBlockCoords(world, worldPosition);
        ChunkData chunkData = GetChunkData(world, chunkPosition);

        if (chunkData == null)
        {
            // Create new chunk data if it doesn't exist
            chunkData = new ChunkData(world.chunkSize, world.chunkHeight, world, chunkPosition);
            world.worldData.chunkDataDictionary.Add(chunkPosition, chunkData);
        }

        Vector3Int localPosition = Chunk.GetBlockInChunkCoordinates(chunkData, worldPosition);
        Chunk.SetBlock(chunkData, localPosition, block);
    }

    public static ChunkRenderer GetChunk(World world, Vector3Int chunkPosition)
    {
        ChunkRenderer chunk = null;
        world.worldData.chunkDictionary.TryGetValue(chunkPosition, out chunk);
        return chunk;
    }

    public static ChunkData GetChunkData(World world, Vector3Int chunkPosition)
    {
        ChunkData chunkData = null;
        world.worldData.chunkDataDictionary.TryGetValue(chunkPosition, out chunkData);
        return chunkData;
    }
}
