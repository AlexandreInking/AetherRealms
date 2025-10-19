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

        int groundPosition = terrainHeightNoise.HasValue
            ? terrainHeightNoise.Value
            : GetSurfaceHeightNoise(data.worldPosition.x + x, data.worldPosition.z + z, data.chunkHeight);

        // --- NUEVA LÓGICA DE PROCESAMIENTO POR BLOQUE ---

        for (int y = data.worldPosition.y; y < data.worldPosition.y + data.chunkHeight; y++)
        {
            // 1. Generar terreno base (piedra, tierra, etc.)
            startLayerHandler.Handle(data, x, y, z, groundPosition, mapSeedOffset);

            // 2. Tallar cuevas (todas ellas)
            if (useCheeseCaves && cheeseCaveLayerHandler != null)
            {
                cheeseCaveLayerHandler.Handle(data, x, y, z, groundPosition, mapSeedOffset);
            }
            if (useSpaghettiCaves && spaghettiCaveLayerHandler != null)
            {
                spaghettiCaveLayerHandler.Handle(data, x, y, z, groundPosition, mapSeedOffset);
            }
            if (useNoodleCaves && noodleCaveLayerHandler != null)
            {
                noodleCaveLayerHandler.Handle(data, x, y, z, groundPosition, mapSeedOffset);
            }

            // 3. Colocar líquidos
            if (waterLayer != null) waterLayer.Handle(data, x, y, z, groundPosition, mapSeedOffset);
            if (lavaLayer != null) lavaLayer.Handle(data, x, y, z, groundPosition, mapSeedOffset);
            if (oilLayer != null) oilLayer.Handle(data, x, y, z, groundPosition, mapSeedOffset);

            // 4. Colocar características como huesos o cristales
            if (featureHandler != null) featureHandler.Handle(data, x, y, z, groundPosition, mapSeedOffset);

            // 5. Aplicar capas de decoración de superficie
            foreach (var layer in additionalLayerHandlers)
            {
                layer.Handle(data, x, y, z, groundPosition, mapSeedOffset);
            }
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
