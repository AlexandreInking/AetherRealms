using UnityEngine;

public class RTSCamera : MonoBehaviour
{
    public float panSpeed = 30f;
    public float panBorderThickness = 15f;
    public float scrollSpeed = 5f;
    public float minY = 10f;
    public float maxY = 120f;

    public bool useScreenEdgeInput = true;

    void Update()
    {
        Vector3 pos = transform.position;

        // Movimiento con WASD
        if (Input.GetKey("w"))
        {
            pos.z += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("s"))
        {
            pos.z -= panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("d"))
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("a"))
        {
            pos.x -= panSpeed * Time.deltaTime;
        }

        // Movimiento con el borde de la pantalla
        if (useScreenEdgeInput)
        {
            if (Input.mousePosition.y >= Screen.height - panBorderThickness)
            {
                pos.z += panSpeed * Time.deltaTime;
            }
            if (Input.mousePosition.y <= panBorderThickness)
            {
                pos.z -= panSpeed * Time.deltaTime;
            }
            if (Input.mousePosition.x >= Screen.width - panBorderThickness)
            {
                pos.x += panSpeed * Time.deltaTime;
            }
            if (Input.mousePosition.x <= panBorderThickness)
            {
                pos.x -= panSpeed * Time.deltaTime;
            }
        }
        
        // Zoom con la rueda del ratón
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * 1000 * scrollSpeed * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }
}