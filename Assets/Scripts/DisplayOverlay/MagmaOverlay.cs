using UnityEngine;

public class MagmaOverlay : DisplayOverlay
{
    public MagmaOverlay(string name, bool isVisible)
    {
        this.Name = name;
        this.IsVisible = isVisible;
    }

    public new void Render(Hexagon hexagon)
    {
        if (this.IsVisible)
        {
            // Render the overlay based on the MagmaIntensity and MagmaDirection of the hexagon
            // For example:
            // this.ChangeColor(Color.Lerp(Color.blue, Color.red, hexagon.MagmaIntensity));
            // this.DrawArrow(hexagon.MagmaDirection);
        }
    }
}