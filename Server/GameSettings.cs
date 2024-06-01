//Implementation by Diego Perez and Christina Le 
// for 3500 Last Update: December 7, 2023
using System.Runtime.Serialization;
using World;

namespace Server;
using System.Xml;

/// <summary>
/// Class representing the chosen game settings provided by the settings.xml
/// </summary>
[DataContract(Namespace = "")]
public class GameSettings
{
    //milliseconds between one frame and the next
    [DataMember] public int MSPerFrame;
    //how many frames the server waits before respawning the snake
    [DataMember] public int RespawnRate;
    [DataMember] public int UniverseSize;
    [DataMember] public List<Wall> Walls;

    public GameSettings()
    {
        Walls = new List<Wall>();
    }
    
}