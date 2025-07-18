using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 0.001f;
    public float panBorderThickness = 1f;
    public float zoomSpeed = 0.001f;

    void Update()
    {
        Vector3 pos = transform.position;

        if ((Input.GetKey("w") || (Input.mousePosition.y >= Screen.height - panBorderThickness && Input.mousePosition.y <= Screen.height)) &&
            (Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width))
        {
            pos.y += panSpeed * Time.deltaTime;
        }
        if ((Input.GetKey("s") || (Input.mousePosition.y <= panBorderThickness && Input.mousePosition.y >= 0)) &&
            (Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width))
        {
            pos.y -= panSpeed * Time.deltaTime;
        }
        if ((Input.GetKey("d") || (Input.mousePosition.x >= Screen.width - panBorderThickness && Input.mousePosition.x <= Screen.width)) &&
            (Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height))
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        if ((Input.GetKey("a") || (Input.mousePosition.x <= panBorderThickness && Input.mousePosition.x >= 0)) &&
            (Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height))
        {
            pos.x -= panSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        {
            Camera.main.orthographicSize = Mathf.Min(Camera.main.orthographicSize + (zoomSpeed * Time.deltaTime), 40);
        }
        if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Equals)) // Equals is for the + key without Shift
        {
            Camera.main.orthographicSize = Mathf.Max(Camera.main.orthographicSize - (zoomSpeed * Time.deltaTime), 4);
        }

        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                Debug.Log("Key initially pressed: " + keyCode);
            }
        }

        transform.position = pos;
    }
}