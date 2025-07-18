using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 10f;
    public float panBorderThickness = 10f;
    public float zoomSpeed = 2f;

    void Update()
    {
        Vector3 pos = transform.position;

        //Direction 

        if (Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow))
        {
            pos.y += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow))
        {
            pos.y -= panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow))
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow))
        {
            pos.x -= panSpeed * Time.deltaTime;
        }
        //Zoom

        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        {
            Camera.main.orthographicSize = Mathf.Min(Camera.main.orthographicSize + (zoomSpeed * Time.deltaTime), 40);
        }
        if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Equals)) // Equals is for the + key without Shift
        {
            Camera.main.orthographicSize = Mathf.Max(Camera.main.orthographicSize - (zoomSpeed * Time.deltaTime), 4);
        }

        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta != 0)
        {
            // Calculate the world position of the mouse cursor before zooming
            Vector3 beforeZoom = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Adjust the camera's orthographic size
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - scrollDelta * (zoomSpeed * 10) * Time.deltaTime, 4, 40);

            // Calculate the world position of the mouse cursor after zooming
            Vector3 afterZoom = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Move the camera by the difference
            Camera.main.transform.position += beforeZoom - afterZoom;
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