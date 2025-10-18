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
        return false;
    }

    // Este método talla las cuevas en una única columna de bloques dentro de un chunk.
    public void GenerateCaveColumn(ChunkData chunkData, int x, int z, int surfaceHeight, Vector2Int mapSeedOffset)
    {
        caveNoiseSettings.worldOffset = mapSeedOffset;

        // Coordenadas mundiales de la columna
        int worldX = chunkData.worldPosition.x + x;
        int worldZ = chunkData.worldPosition.z + z;

        // Iteramos verticalmente a través de la columna, usando coordenadas locales del chunk
        for (int y = 0; y < chunkData.chunkHeight; y++)
        {
            int worldY = chunkData.worldPosition.y + y;

            // Solo generamos cuevas bajo la superficie y por encima de la capa de roca base (bedrock)
            if (worldY < surfaceHeight && worldY > bedrockLayer)
            {
                // Obtenemos el valor de ruido 3D para la posición mundial del bloque actual
                float noiseValue = MyNoise.OctavePerlin3D(worldX, worldY, worldZ, caveNoiseSettings);

                // Si el valor de ruido supera el umbral, tallamos una cueva (convertimos el bloque en Aire)
                if (noiseValue > caveThreshold)
                {
                    Vector3Int blockPos = new Vector3Int(x, y, z);
                    Chunk.SetBlock(chunkData, blockPos, BlockType.Air);
                }
            }
        }
    }
}
