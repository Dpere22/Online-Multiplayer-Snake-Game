//Implementation by Diego Perez and Christina Le 
// for 3500 Last Update: December 7, 2023
//
//Program initializes the world's state, maintains the frames, and sends update
//messages to the ServerController every frame.
using System.Diagnostics;
using System.Text.Json;
using System.Xml.Serialization;
using Server;
using SnakeGame;
using World;

//Start the controller and get the world from the controller
ServerController serverController = new();
SnakeWorld _theWorld;

//Open the XML file and deserialize in GameSettings object
XmlSerializer serializer = new XmlSerializer(typeof(GameSettings));
GameSettings gameSettings;
using (Stream reader = new FileStream("../../../settings.xml", FileMode.Open))
{
    gameSettings = (GameSettings)serializer.Deserialize(reader)!;
}

//sets the given setting into the world
_theWorld= serverController.GetWorld();
_theWorld.Size = gameSettings.UniverseSize;
_theWorld.respawnRate = gameSettings.RespawnRate;
        

//One time loop to transfer walls from game settings to the world
lock (_theWorld.Walls)
{
    foreach (Wall wall in gameSettings.Walls)
    {
        _theWorld.Walls.Add(wall.wall, wall);
    }
}

//server can now be started
serverController.StartServer();
//clock for ms
Stopwatch watch = new Stopwatch();
watch.Start();
//clock for frames
int frameCount = 0;
Stopwatch watchSec = new Stopwatch();
watchSec.Start();

//loop restarts every frame
while (true)
{
    //simulates the time between each frame update
    while (watch.ElapsedMilliseconds < gameSettings.MSPerFrame)
    {
        //do nothing
    }
    watch.Restart();
    Update();
    frameCount++;

    //decrease the frames a snake needs to wait after their death/grown/movement turn
    lock (serverController.Dead)
    {
        foreach (Snake s in serverController.Dead.Keys)
        {
            //changes the death state 1 frame after the snake dies, but prevents instant respawn
            if (serverController.Dead[s] == _theWorld.respawnRate -1)
            {
                s.alive = false;
                s.died = false;
            }

            serverController.Dead[s]--;

            //snake can now respawn
            if (serverController.Dead[s] <= 0)
            {
                serverController.Dead.Remove(s);
                serverController.RandomSpawn(s);
            }
        }
    }
    lock (serverController.growth)
    {
        foreach (Snake s in serverController.growth.Keys)
        {
            serverController.growth[s]--;

            //snakes stops growing after a power up
            if (serverController.growth[s] <= 0)
            {
                serverController.growth.Remove(s);
            }
        }
    }
    lock (serverController.turn)
    {
        foreach (Snake s in serverController.turn.Keys)
        {
            serverController.turn[s]--;

            //snake can now give another movement command
            if (serverController.turn[s] <= 0)
            {
                serverController.turn.Remove(s);
            }
        }
    }

    //keeps counts of how many frame updates there has been in 1 second
    if (watchSec.ElapsedMilliseconds >= 1000)
    {
        serverController.RandomSpawn(new Power(0, new Vector2D(0, 0), true));
        Console.WriteLine("FPS: " + frameCount);
        watchSec.Restart();
        frameCount = 0;
    }
}

/// Updates the Power ups, Walls, and Snakes. Sends the JSON serialization of the new state of each item to the client.
void Update()
{
    lock (_theWorld.Walls)
    {
        //Walls never move so no update
        foreach(Wall wall in _theWorld.Walls.Values)
        {
            serverController.MessageEntered(JsonSerializer.Serialize(wall));
        }
    }
    lock (_theWorld.Powers)
    {
        foreach (Power power in _theWorld.Powers.Values)
        {
            serverController.MessageEntered(JsonSerializer.Serialize(power));

            //removes the power up from the client if it has been interacted with 
            if(power.died)
            {
                _theWorld.Powers.Remove(power.power);
            }
        }
    }
    lock (_theWorld.Snakes)
    {
        //moves each snake and checks for collisions
        serverController.SnakeUpdate();

        foreach (Snake snake in _theWorld.Snakes.Values)
        {
            serverController.MessageEntered(JsonSerializer.Serialize(snake));
        }
    }
}




