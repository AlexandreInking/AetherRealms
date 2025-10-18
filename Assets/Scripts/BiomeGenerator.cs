using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeGenerator : MonoBehaviour
{
    public int waterThreshold = 50;

    public NoiseSettings biomeNoiseSettings;

    public DomainWarping domainWarping;

    public bool useDomainWarping = true;

    public BlockLayerHandler startLayerHandler;

    public List<BlockLayerHandler> additionalLayerHandlers;

    [Header("Cave Settings")]
    public bool useCheeseCaves = true; // Parámetro para activar/desactivar
    public CheeseCaveLayerHandler cheeseCaveLayerHandler; // Referencia al script

    // -- AÑADIR ESTAS DOS LÍNEAS --
    public bool useSpaghettiCaves = true; // Parámetro para activar/desactivar
    public SpaghettiCaveLayerHandler spaghettiCaveLayerHandler; // Referencia al nuevo script

    public ChunkData ProcessChunkColumn(ChunkData data, int x, int z, Vector2Int mapSeedOffset, int? terrainHeightNoise)
    {
        biomeNoiseSettings.worldOffset = mapSeedOffset;

        int groundPosition;
        if (terrainHeightNoise.HasValue == false)
            groundPosition = GetSurfaceHeightNoise(data.worldPosition.x + x, data.worldPosition.z + z, data.chunkHeight);
        else
            groundPosition = terrainHeightNoise.Value;

        // 1. Generar el terreno base (piedra, tierra, etc.)
        for (int y = data.worldPosition.y; y < data.worldPosition.y + data.chunkHeight; y++)
        {
            startLayerHandler.Handle(data, x, y, z, groundPosition, mapSeedOffset);
        }

        // 2. Añadir capas adicionales (superficie, agua, etc.)
        foreach (var layer in additionalLayerHandlers)
        {
            layer.Handle(data, x, data.worldPosition.y, z, groundPosition, mapSeedOffset);
        }

        // 3. ¡NUEVO! Tallar las cuevas si están activadas
        if (useCheeseCaves && cheeseCaveLayerHandler != null)
        {
            cheeseCaveLayerHandler.GenerateCaveColumn(data, x, z, groundPosition, mapSeedOffset);
        }

        // -- AÑADIR ESTE BLOQUE --
        // 4. Tallar las cuevas de espagueti
        if (useSpaghettiCaves && spaghettiCaveLayerHandler != null)
        {
            spaghettiCaveLayerHandler.GenerateCaves(data, x, z, groundPosition, mapSeedOffset);
        }
        // ------------------------

        return data;
    }

    public int GetSurfaceHeightNoise(int x, int z, int chunkHeight)
    {
        float terrainHeight;
        if(useDomainWarping == false)
        {
            terrainHeight = MyNoise.OctavePerlin(x, z, biomeNoiseSettings);
        }
        else
        {
            terrainHeight = domainWarping.GenerateDomainNoise(x, z, biomeNoiseSettings);
        }

        terrainHeight = MyNoise.Redistribution(terrainHeight, biomeNoiseSettings);
        int surfaceHeight = MyNoise.RemapValue01ToInt(terrainHeight, 0, chunkHeight);
        return surfaceHeight;
    }
}
