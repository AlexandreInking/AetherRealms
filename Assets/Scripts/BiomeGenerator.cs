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

    // -- AÑADIR ESTAS DOS LÍNEAS --
    public bool useNoodleCaves = true; // Parámetro para activar/desactivar
    public NoodleCaveLayerHandler noodleCaveLayerHandler; // Referencia al nuevo script

    [Header("Liquid & Features")]
    public LiquidLayerHandler waterLayer; // Referencia para el agua
    public LiquidLayerHandler lavaLayer;  // Referencia para la lava
    public LiquidLayerHandler oilLayer;   // Referencia para el petróleo
    public SubterraneanFeatureLayerHandler featureHandler; // Referencia para huesos/cristales

    public ChunkData ProcessChunkColumn(ChunkData data, int x, int z, Vector2Int mapSeedOffset, int? terrainHeightNoise)
    {
        biomeNoiseSettings.worldOffset = mapSeedOffset;

        int groundPosition;
        if (terrainHeightNoise.HasValue == false)
            groundPosition = GetSurfaceHeightNoise(data.worldPosition.x + x, data.worldPosition.z + z, data.chunkHeight);
        else
            groundPosition = terrainHeightNoise.Value;

        // Step 1: Generate base terrain
        for (int y = data.worldPosition.y; y < data.worldPosition.y + data.chunkHeight; y++)
        {
            startLayerHandler.Handle(data, x, y, z, groundPosition, mapSeedOffset);
        }

        // Step 2: Carve all caves (cheese, spaghetti, noodle)
        if (useCheeseCaves && cheeseCaveLayerHandler != null)
        {
            cheeseCaveLayerHandler.GenerateCaveColumn(data, x, z, groundPosition, mapSeedOffset);
        }

        if (useSpaghettiCaves && spaghettiCaveLayerHandler != null)
        {
            spaghettiCaveLayerHandler.GenerateCaves(data, x, z, groundPosition, mapSeedOffset);
        }

        if (useNoodleCaves && noodleCaveLayerHandler != null)
        {
            noodleCaveLayerHandler.GenerateCaves(data, groundPosition, mapSeedOffset);
        }

        // Step 3: Place liquids (water, lava, oil) and subterranean features
        if (waterLayer != null)
        {
            waterLayer.Handle(data, x, data.worldPosition.y, z, groundPosition, mapSeedOffset);
        }

        if (lavaLayer != null)
        {
            lavaLayer.Handle(data, x, data.worldPosition.y, z, groundPosition, mapSeedOffset);
        }

        if (oilLayer != null)
        {
            oilLayer.Handle(data, x, data.worldPosition.y, z, groundPosition, mapSeedOffset);
        }

        if (featureHandler != null)
        {
            featureHandler.Handle(data, x, data.worldPosition.y, z, groundPosition, mapSeedOffset);
        }

        // Step 4: Generate features that depend on previous steps (bones, crystals)
        // Additional dependent feature generation can be added here

        // Step 5: Apply final surface decoration layers
        foreach (var layer in additionalLayerHandlers)
        {
            layer.Handle(data, x, data.worldPosition.y, z, groundPosition, mapSeedOffset);
        }

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
