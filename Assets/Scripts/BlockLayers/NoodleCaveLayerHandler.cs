// RUTA: Assets/Scripts/BlockLayers/NoodleCaveLayerHandler.cs

using UnityEngine;

public class NoodleCaveLayerHandler : BlockLayerHandler
{
    public NoiseSettings caveNoiseSettings1;
    public NoiseSettings caveNoiseSettings2;

    [Range(0, 1)]
    public float caveThreshold = 0.85f; // Umbral aún más alto para túneles más finos.

    public int bedrockLayer = 5;

    protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        return false;
    }

    public void GenerateCaves(ChunkData chunkData, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        caveNoiseSettings1.worldOffset = mapSeedOffset;
        caveNoiseSettings2.worldOffset = mapSeedOffset;

        // Coordenadas mundiales del chunk
        int worldX = chunkData.worldPosition.x;
        int worldZ = chunkData.worldPosition.z;

        for (int y = chunkData.worldPosition.y; y < chunkData.worldPosition.y + chunkData.chunkHeight; y++)
        {
            if (y < surfaceHeightNoise && y > bedrockLayer)
            {
                // Llamamos a la nueva función de ruido específica para "noodle caves"
                float noiseValue1 = MyNoise.OctavePerlin3D_Noodle(
                    worldX,
                    y,
                    worldZ,
                    caveNoiseSettings1
                );

                float noiseValue2 = MyNoise.OctavePerlin3D_Noodle(
                    worldX,
                    y,
                    worldZ,
                    caveNoiseSettings2
                );

                // La lógica sigue siendo la misma: crear un hueco si ambos ruidos superan el umbral
                if (noiseValue1 > caveThreshold && noiseValue2 > caveThreshold)
                {
                    // Convertir coordenadas mundiales a locales del chunk
                    int localY = y - chunkData.worldPosition.y;
                    Vector3Int blockPos = new Vector3Int(worldX - chunkData.worldPosition.x, localY, worldZ - chunkData.worldPosition.z);
                    Chunk.SetBlock(chunkData, blockPos, BlockType.Air);
                }
            }
        }
    }
}