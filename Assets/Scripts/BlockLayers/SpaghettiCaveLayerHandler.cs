// RUTA: Assets/Scripts/BlockLayers/SpaghettiCaveLayerHandler.cs

using UnityEngine;

public class SpaghettiCaveLayerHandler : BlockLayerHandler
{
    // Necesitamos dos configuraciones de ruido para crear los túneles
    public NoiseSettings caveNoiseSettings1;
    public NoiseSettings caveNoiseSettings2;

    [Range(0, 1)]
    public float caveThreshold = 0.8f; // Umbral para los túneles. Debe ser alto para que sean delgados.

    public int bedrockLayer = 5;

    protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (y < surfaceHeightNoise && y > bedrockLayer)
        {
            caveNoiseSettings1.worldOffset = mapSeedOffset;
            caveNoiseSettings2.worldOffset = mapSeedOffset;

            int worldX = chunkData.worldPosition.x + x;
            int worldZ = chunkData.worldPosition.z + z;

            float noiseValue1 = MyNoise.OctavePerlin3D(
                worldX,
                y,
                worldZ,
                caveNoiseSettings1
            );

            float noiseValue2 = MyNoise.OctavePerlin3D(
                worldX,
                y,
                worldZ,
                caveNoiseSettings2
            );

            if (noiseValue1 > caveThreshold && noiseValue2 > caveThreshold)
            {
                int localY = y - chunkData.worldPosition.y;
                Vector3Int blockPos = new Vector3Int(x, localY, z);
                Chunk.SetBlock(chunkData, blockPos, BlockType.Air);
                return true;
            }
        }
        return false;
    }
}