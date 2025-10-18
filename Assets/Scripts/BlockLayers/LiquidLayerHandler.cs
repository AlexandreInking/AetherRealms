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
        // Este manejador se aplica a los bloques que están por encima del terreno pero por debajo del nivel del líquido
        if (y > surfaceHeightNoise && y <= liquidLevel)
        {
            Vector3Int pos = new Vector3Int(x, y, z);
            Chunk.SetBlock(chunkData, pos, liquidType);

            // Si estamos justo un bloque por encima del terreno original, creamos una orilla
            if (y == surfaceHeightNoise + 1 && shoreBlockType != BlockType.Nothing)
            {
                for (int i = 0; i < shoreSpread; i++)
                {
                    pos.y = surfaceHeightNoise - i;
                    // Solo reemplazamos si el bloque no es aire (para no llenar cuevas con arena)
                    if (Chunk.GetBlockFromChunkCoordinates(chunkData, pos) != BlockType.Air)
                    {
                        Chunk.SetBlock(chunkData, pos, shoreBlockType);
                    }
                }
            }
            return true;
        }
        return false;
    }
}