// RUTA: Assets/Scripts/BlockLayers/LiquidLayerHandler.cs

using UnityEngine;

public class LiquidLayerHandler : BlockLayerHandler
{
    [Header("Liquid Settings")]
    public BlockType liquidType; // El tipo de bloque líquido a colocar (Agua, Lava, Petróleo)
    public int liquidLevel; // La altura máxima a la que aparecerá este líquido

    [Header("Shoreline Settings")]
    public BlockType shoreBlockType; // El bloque que se colocará en la orilla (ej. Arena para Agua)
    public int shoreSpread = 1; // Cuántos bloques de orilla se colocarán

    protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        int localY = y - chunkData.worldPosition.y;

        if (y > surfaceHeightNoise && y <= liquidLevel)
        {
            Vector3Int pos = new Vector3Int(x, localY, z);
            Chunk.SetBlock(chunkData, pos, liquidType);

            if (y == surfaceHeightNoise + 1 && shoreBlockType != BlockType.Nothing)
            {
                for (int i = 0; i < shoreSpread; i++)
                {
                    int shoreLocalY = (surfaceHeightNoise - i) - chunkData.worldPosition.y;
                    // Comprobar que la coordenada de la orilla esté dentro del chunk actual
                    if (shoreLocalY >= 0 && shoreLocalY < chunkData.chunkHeight) 
                    {
                        Vector3Int shorePos = new Vector3Int(x, shoreLocalY, z);
                        if (Chunk.GetBlockFromChunkCoordinates(chunkData, shorePos) != BlockType.Air)
                        {
                            Chunk.SetBlock(chunkData, shorePos, shoreBlockType);
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }
}