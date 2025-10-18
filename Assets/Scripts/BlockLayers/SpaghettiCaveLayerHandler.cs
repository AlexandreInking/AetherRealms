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
        // Al igual que con las cuevas de queso, la lógica principal se llamará desde BiomeGenerator.
        return false;
    }

    public void GenerateCaves(ChunkData chunkData, int x, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        caveNoiseSettings1.worldOffset = mapSeedOffset;
        caveNoiseSettings2.worldOffset = mapSeedOffset;

        // Coordenadas mundiales de la columna
        int worldX = chunkData.worldPosition.x + x;
        int worldZ = chunkData.worldPosition.z + z;

        // Iteramos verticalmente a través de la columna, usando coordenadas locales del chunk
        for (int y = 0; y < chunkData.chunkHeight; y++)
        {
            int worldY = chunkData.worldPosition.y + y;

            // Solo generamos cuevas bajo la superficie y por encima de la capa de roca base (bedrock)
            if (worldY < surfaceHeightNoise && worldY > bedrockLayer)
            {
                // Obtenemos el valor de los dos mapas de ruido 3D
                float noiseValue1 = MyNoise.OctavePerlin3D(
                    worldX,
                    worldY,
                    worldZ,
                    caveNoiseSettings1
                );

                float noiseValue2 = MyNoise.OctavePerlin3D(
                    worldX,
                    worldY,
                    worldZ,
                    caveNoiseSettings2
                );

                // Se crea un túnel solo si AMBOS valores de ruido superan el umbral
                if (noiseValue1 > caveThreshold && noiseValue2 > caveThreshold)
                {
                    Vector3Int blockPos = new Vector3Int(x, y, z);
                    Chunk.SetBlock(chunkData, blockPos, BlockType.Air);
                }
            }
        }
    }
}