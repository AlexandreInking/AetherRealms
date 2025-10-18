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
        // La lógica se ejecutará en un método separado.
        return false;
    }

    public void GenerateFeatures(ChunkData chunkData, int x, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (generateBoneIndicators)
        {
            // --- Lógica para Huesos como indicador de Petróleo ---
            // Generamos ruido para determinar si en esta columna (x, z) hay un "punto caliente" para huesos.
            boneNoiseSettings.worldOffset = mapSeedOffset;
            float boneNoise = MyNoise.OctavePerlin(chunkData.worldPosition.x + x, chunkData.worldPosition.z + z, boneNoiseSettings);
            
            // Si el ruido es alto y estamos cerca de la superficie, colocamos un bloque de hueso.
            if (boneNoise > bonePlacementThreshold)
            {
                // Colocamos el hueso en la superficie del terreno, reemplazando lo que haya.
                Vector3Int pos = new Vector3Int(x, surfaceHeightNoise, z);
                Chunk.SetBlock(chunkData, pos, BlockType.BoneStone);
            }
        }

        if (generateCrystalIndicators)
        {
            // --- Lógica para Cristales cerca de la Lava ---
            // Iteramos solo en las profundidades de la columna actual.
            for (int y = chunkData.worldPosition.y; y < crystalDepthThreshold; y++)
            {
                Vector3Int currentPos = new Vector3Int(x, y, z);
                BlockType blockType = Chunk.GetBlockFromChunkCoordinates(chunkData, currentPos);

                // Solo intentamos colocar cristales si el bloque actual es de piedra.
                if (blockType == BlockType.Stone)
                {
                    // Comprobamos si hay lava cerca
                    if (IsLavaNearby(chunkData, currentPos))
                    {
                        // Si hay lava, tenemos una pequeña probabilidad de colocar un cristal.
                        if (Random.value < crystalPlacementChance)
                        {
                            Chunk.SetBlock(chunkData, currentPos, BlockType.CrystalStone);
                        }
                    }
                }
            }
        }
    }

    private bool IsLavaNearby(ChunkData chunkData, Vector3Int position)
    {
        // Revisa un cubo de 3x3x3 alrededor de la posición para ver si hay lava.
        for (int i = -lavaSearchRadius; i <= lavaSearchRadius; i++)
        {
            for (int j = -lavaSearchRadius; j <= lavaSearchRadius; j++)
            {
                for (int k = -lavaSearchRadius; k <= lavaSearchRadius; k++)
                {
                    if (i == 0 && j == 0 && k == 0) continue; // No revisar el bloque central

                    Vector3Int checkPos = position + new Vector3Int(i, j, k);
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