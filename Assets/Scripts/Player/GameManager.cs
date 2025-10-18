using UnityEngine;

public class GameManager : MonoBehaviour
{
    public World world;

    // Define el tamaño del mapa en chunks. 
    // Por ejemplo, un valor de 10 creará un mapa de 10x10 chunks.
    public int mapSizeInChunks = 16; 

    void Start()
    {
        // Asegúrate de que la referencia al mundo esté asignada en el Inspector.
        if (world != null)
        {
            // Llama a la función para generar el mundo completo.
            world.GenerateWorld(mapSizeInChunks);
        }
        else
        {
            Debug.LogError("La referencia al World no está asignada en el GameManager.");
        }
    }
}