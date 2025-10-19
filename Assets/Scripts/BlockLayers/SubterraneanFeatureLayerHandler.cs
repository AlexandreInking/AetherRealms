// RUTA: Assets/Scripts/BlockLayers/SubterraneanFeatureLayerHandler.cs

using UnityEngine;

public class SubterraneanFeatureLayerHandler : BlockLayerHandler
{
    [Header("Bone -> Oil Indicator")]
    public bool generateBoneIndicators = true;
    public NoiseSettings boneNoiseSettings;
    [Range(0, 1)]
    public float bonePlacementThreshold = 0.8f; // Umbral de ruido para colocar un grupo de huesos

    [Header("Lava -> Crystal Indicator")]
    public bool generateCrystalIndicators = true;
    public int crystalDepthThreshold = 20; // Profundidad máxima a la que pueden aparecer cristales
    public int lavaSearchRadius = 3; // Radio de búsqueda de lava alrededor de un bloque de piedra
    [Range(0, 1)]
    public float crystalPlacementChance = 0.1f; // Probabilidad de que aparezca un cristal si hay lava cerca

    protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        Vector3Int worldPosition = new Vector3Int(chunkData.worldPosition.x + x, y, chunkData.worldPosition.z + z);

        if (generateBoneIndicators)
        {
            if (y == surfaceHeightNoise) // Solo comprobar en la superficie
            {
                boneNoiseSettings.worldOffset = mapSeedOffset;
                float boneNoise = MyNoise.OctavePerlin(worldPosition.x, worldPosition.z, boneNoiseSettings);
                if (boneNoise > bonePlacementThreshold)
                {
                    Chunk.SetBlock(chunkData, new Vector3Int(x, y, z), BlockType.BoneStone);
                    return true; // Se ha manejado
                }
            }
        }

        if (generateCrystalIndicators)
        {
            if (y < crystalDepthThreshold && Chunk.GetBlockFromChunkCoordinates(chunkData, new Vector3Int(x, y, z)) == BlockType.Stone)
            {
                if (IsLavaNearby(chunkData, new Vector3Int(x, y, z)))
                {
                    if (UnityEngine.Random.value < crystalPlacementChance)
                    {
                        Chunk.SetBlock(chunkData, new Vector3Int(x, y, z), BlockType.CrystalStone);
                        return true; // Se ha manejado
                    }
                }
            }
        }

        return false; // No se hizo nada, pasar al siguiente.
    }


    private bool IsLavaNearby(ChunkData chunkData, Vector3Int localPosition)
    {
        for (int i = -lavaSearchRadius; i <= lavaSearchRadius; i++)
        {
            for (int j = -lavaSearchRadius; j <= lavaSearchRadius; j++)
            {
                for (int k = -lavaSearchRadius; k <= lavaSearchRadius; k++)
                {
                    if (i == 0 && j == 0 && k == 0) continue;

                    Vector3Int checkPos = localPosition + new Vector3Int(i, j, k);
                    if (Chunk.GetBlockFromChunkCoordinates(chunkData, checkPos) == BlockType.Lava)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}