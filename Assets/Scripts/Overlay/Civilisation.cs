using UnityEngine;
using System.Collections.Generic;

public class Civilisation
{
    public string Name { get; set; }
    public Color Colour { get; set; }

    // Make civilisations publicly accessible and initialize it
    public static List<Civilisation> civilisations = new List<Civilisation>
    {
        new Civilisation { Name = "Blutopik", Colour = new Color(0, 0, 0.545f) }, // Dark Blue
        new Civilisation { Name = "Indigon", Colour = new Color(0.294f, 0, 0.510f) }, // Indigo
        new Civilisation { Name = "Purplonia", Colour = new Color(0.502f, 0, 0.502f) }, // Purple
        new Civilisation { Name = "Magentica", Colour = Color.magenta }, // Magenta
        new Civilisation { Name = "Pinktoria", Colour = new Color(1, 0.411f, 0.705f) } // Pink
    };
}
