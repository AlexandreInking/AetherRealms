// RUTA: Assets/Scripts/BlockLayers/CheeseCaveLayerHandler.cs

using UnityEngine;

public class CheeseCaveLayerHandler : BlockLayerHandler
{
    public NoiseSettings caveNoiseSettings;
    
    [Range(0, 1)]
    public float caveThreshold = 0.6f; // Umbral para crear una cueva. Valores más bajos = más cuevas.

    public int bedrockLayer = 5; // Altura a la que no se generarán cuevas para dejar una base.

    // La lógica de este manejador se llama manualmente desde BiomeGenerator, así que este método no hace nada.
    protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (y < surfaceHeightNoise && y > bedrockLayer)
        {
            caveNoiseSettings.worldOffset = mapSeedOffset;
            int worldX = chunkData.worldPosition.x + x;
            int worldZ = chunkData.worldPosition.z + z;

            float noiseValue = MyNoise.OctavePerlin3D(worldX, y, worldZ, caveNoiseSettings);

            if (noiseValue > caveThreshold)
            {
                int localY = y - chunkData.worldPosition.y;
                Chunk.SetBlock(chunkData, new Vector3Int(x, localY, z), BlockType.Air);
                return true;
            }
        }
        return false;
    }

}
