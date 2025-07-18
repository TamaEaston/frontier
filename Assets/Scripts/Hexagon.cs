using UnityEngine;
using UnityEngine.UI;

public class Hexagon : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;

    public float PosX;
    public float PosY;
    public float Altitude;
    public float AltitudeNew;
    public float MagmaIntensity;
    public float MagmaDirection;
    public float WindIntensity;
    public float WindDirection;
    public char BelongsToPlate;
    public Hexagon[] Neighbours = new Hexagon[6];
}