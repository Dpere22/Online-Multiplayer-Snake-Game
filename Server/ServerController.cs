//Implementation by Diego Perez and Christina Le 
// for 3500 Last Update: December 7, 2023
using System.Text.RegularExpressions;
using NetworkUtil;
using SnakeGame;
using System.Collections.Generic;
using World;

namespace Server;
/// <summary>
/// 
/// </summary>
public class ServerController
{
    //used to randomize spawn of snake/power ups
    Random _random = new Random();

    //keeps track of all the connected clients
    private Dictionary<long, SocketState> _clients;
    private SnakeWorld _theWorld = new();

    //represents the frame wait time of the snakes after events
    public Dictionary<Snake, int> Dead = new();
    public Dictionary<Snake, int> growth = new();
    public Dictionary<Snake, int> turn = new();


    /// <summary>
    /// Returns the current snake world
    /// </summary>
    /// <returns></returns>
    public SnakeWorld GetWorld()
    {
        return _theWorld;
    }

    /// <summary>
    /// Initialized the server's state
    /// </summary>
    public ServerController()
    {
        _clients = new Dictionary<long, SocketState>();
    }

    /// <summary>
    /// Start accepting Tcp sockets connections from clients
    /// </summary>
    public void StartServer()
    {
        // This begins an "event loop"
        Networking.StartServer(NewClientConnected, 11000);

        Console.WriteLine("Server is running");
    }

    /// <summary>
    /// Method to be invoked by the networking library
    /// when a new client connects (see line 41)
    /// </summary>
    /// <param name="state">The SocketState representing the new client</param>
    private void NewClientConnected(SocketState state)
    {
        if (state.ErrorOccurred)
            return;

        // Save the client state
        // Need to lock here because clients can disconnect at any time

        // change the state's network action to the 
        // receive handler so we can process data when something
        // happens on the network
        state.OnNetworkAction = ReceiveMessage;

        Networking.GetData(state);
        Console.WriteLine("Client " + state.ID + " connected");
    }

    /// <summary>
    /// Method to be invoked by the networking library
    /// when a network action occurs (see lines 64-66)
    /// </summary>
    /// <param name="state"></param>
    private void ReceiveMessage(SocketState state)
    {
        // Remove the client if they aren't still connected
        if (state.ErrorOccurred)
        {
            RemoveClient(state.ID);
            return;
        }

        ProcessMessage(state);
        // Continue the event loop that receives messages from this client
        Networking.GetData(state);
    }


    /// <summary>
    /// Given the data that has arrived so far, 
    /// potentially from multiple receive operations, 
    /// determine if we have enough to make a complete message,
    /// and process it (print it and broadcast it to other clients).
    /// </summary>
    /// <param name="state"></param>
    private void ProcessMessage(SocketState state)
    {
        string totalData = state.GetData();

        string[] parts = Regex.Split(totalData, @"(?<=[\n])");

        // Loop until we have processed all messages.
        // We may have received more than one.
        foreach (string p in parts)
        {
            // Ignore empty strings added by the regex splitter
            if (p.Length == 0)
                continue;
            // The regex splitter will include the last string even if it doesn't end with a '\n',
            // So we need to ignore it if this happens. 
            if (p[p.Length - 1] != '\n')
                break;

            //if the client is not already connected, sets up a new connection
            if (!_clients.TryGetValue(state.ID, out _))
            {
                lock (_theWorld.Snakes)
                {
                    Networking.Send(state.TheSocket, state.ID + "\n" + _theWorld.Size + "\n");

                    Snake s = new Snake((int)state.ID, p.Substring(0, p.Length - 1), new List<Vector2D>(), new Vector2D(), 0, false, true, false, true);
                    RandomSpawn(s);
                }
                
                lock (_clients)
                {
                    _clients[state.ID] = state;
                }
            }
            //processes message if the client is connected with a snake
            else
            {
                if (_theWorld.Snakes.TryGetValue((int)state.ID, out Snake? snake))
                {
                    //if given the proper movement commands, and the snake has moved it width after a turn;
                    //changes the direction of the snake 
                    if (p.Contains("moving") && !p.Contains("none") && !turn.ContainsKey(snake))
                    {
                        lock (_theWorld.Snakes)
                        {
                            //makes sure that the snake only moves when it is alive
                            if (snake.alive && !snake.died)
                            {
                                //makes sure the X,Y values for the direction is only -1,0, or 1
                                snake.dir.Clamp();

                                if (p.Contains("up"))
                                {
                                    //checks to see if the snake is not already travelling the Y axis
                                    if (Math.Abs(snake.dir.Y) != 1)
                                    {
                                        snake.dir = new Vector2D(0, -1);
                                        //notes the direction change
                                        snake.body.Add(snake.body.Last());
                                    }
                                }
                                if (p.Contains("down"))
                                {
                                    //checks to see if the snake is not already travelling the Y axis
                                    if (Math.Abs(snake.dir.Y) != 1)
                                    {
                                        snake.dir = new Vector2D(0, 1);
                                        //notes the direction change
                                        snake.body.Add(snake.body.Last());
                                    }
                                }
                                if (p.Contains("left"))
                                {
                                    //checks to see if the snake is not already travelling the X axis
                                    if (Math.Abs(snake.dir.X) != 1)
                                    {
                                        snake.dir = new Vector2D(-1, 0);
                                        //notes the direction change
                                        snake.body.Add(snake.body.Last());
                                    }
                                }

                                if (p.Contains("right"))
                                {
                                    //checks to see if the snake is not already travelling the X axis
                                    if (Math.Abs(snake.dir.X) != 1)
                                    {
                                        snake.dir = new Vector2D(1, 0);
                                        //notes the direction change
                                        snake.body.Add(snake.body.Last());
                                    }
                                }
                                //ensures that the snake moves it's width before the next movement input
                                lock (turn)
                                {
                                    if (!turn.TryAdd(snake, 2))
                                    {
                                        turn[snake] += 2;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Remove it from the SocketState's growable buffer
            state.RemoveData(0, p.Length);
        }
    }

    /// <summary>
    /// Randomly Spawn either a snake or a power up. Guarentees that the spawn does not collide with anything.
    /// To randomly spawn a snake/power up object parameter should be a snake/power up.
    /// </summary>
    /// <param name="o"></param>
    public void RandomSpawn(object o)
    {
        Vector2D spawn = new Vector2D(_random.Next(-800,800), _random.Next(-800, 800));
        
        if(o is Power && _theWorld.Powers.Count < 20)
        {
            //creates a power up if the spawn does not cause a collision 
            if(!CheckCollision(spawn, out object? _))
            {
                Power p = new Power(_theWorld.Powers.Count, spawn, false);
                lock (_theWorld.Powers)
                {
                    if (!_theWorld.Powers.TryAdd(p.power, p) && p.died)
                        _theWorld.Powers[p.power] = p;
                }
            }
        }

        if (o is Snake)
        {
            //constantly finds a new spawn location until it doesn't cause a collision
            while (CheckCollision(spawn, out object? _))
            {
                spawn.X = _random.Next(-800, 800);
                spawn.Y = _random.Next(-800, 800);
            }

            //gets the snake value from the given object
            Snake s = (Snake)o;

            //sets up for the while loop
            bool safe = false;
            Vector2D dir = new();

            //Constantly tried to find a direction where drawing a 120 unit snake doesn't cause collisions
            while (!safe)
            {
                //randomly chooses a direction
                switch (_random.Next() % 4)
                {
                    //left
                    case 0:
                        dir = new Vector2D(-1, 0);
                        break;
                    //right
                    case 1:
                        dir = new Vector2D(1, 0);
                        break;
                    //up
                    case 2:
                        dir = new Vector2D(0, 1);
                        break;
                    //down 
                    case 3:
                        dir = new Vector2D(0, -1);
                        break;
                }

                object? collision = null;

                //checks if the entire section of the snake is not colliding with anything
                for (int i = 0; i <= 120; i ++)
                {
                    CheckCollision(spawn + dir * i, out collision);
                }

                if (collision is null)
                    safe = true;
            }

            //Creates a representation of the first segment of the snake
            List<Vector2D> body = new List<Vector2D>{
                spawn ,
                spawn + dir * 120
                };

            //creates the new snake (under the same client) and adds it into the world
            s = new Snake(s.snake, s.name, body, dir, 0, false, true, false, true);
            lock (_theWorld.Snakes)
            {
                if (!_theWorld.Snakes.TryAdd(s.snake, s))
                    _theWorld.Snakes[s.snake] = s;
            }
        }
    }

    /// <summary>
    /// Moves the snake towards its given location while the tail follows the body.
    /// Also checks for snake collisions from the head of the snake.
    /// </summary>
    public void SnakeUpdate()
    {
        foreach (Snake s in _theWorld.Snakes.Values)
        {
            //doesn't need to check anything if the snake is dead
            if (Dead.ContainsKey(s))
                continue;

            lock (_theWorld.Snakes)
            {
                //checks to see if a client has disconnected
                if (s.dc || !_clients.ContainsKey(s.snake))
                    _theWorld.Snakes.Remove(s.snake);
            }

            //If the snake hasn't recently collected a power up, its tail follows the body 
            if (!growth.TryGetValue(s, out int _))
            {
                if (s.body[0].X > s.body[1].X)
                {
                    s.body[0].X -= 6;
                }
                else if (s.body[0].X < s.body[1].X)
                {
                    s.body[0].X += 6;
                }
                else if (s.body[0].Y > s.body[1].Y)
                {
                    s.body[0].Y -= 6;
                }
                else if (s.body[0].Y < s.body[1].Y)
                {
                    s.body[0].Y += 6;
                }

                //if the tail reaches an end of a segment it removes that point, so that
                //it can go onto the segment
                if (s.body[0].Equals(s.body[1]))
                {
                    s.body.Remove(s.body[1]);
                }
            }

            //Moves the head of the snake in its given direction by 5 units
            Vector2D v = s.dir * 6.0;
            s.body[^1] += v;

            //checks if the snake is at the edge of the map and makes it wrap around to the other side
            if (Math.Abs(s.body[^1].X) >= 1000)
            {
                WrapAroundX(s);
            }
            else if (Math.Abs(s.body[^1].Y) >= 1000)
            {
                WrapAroundY(s);
            }

            //checks if the snake has collided with anything
            if (CheckCollision(s.body[^1], out object? collidedWith))
            {
                //"grows" the snake if it has collied with a power up
                if (collidedWith is Power p)
                {
                    s.score += 1;
                    p.died = true;

                    lock (growth)
                    {
                        if (!growth.TryAdd(s, 24))
                        {
                            growth[s] += 24;
                        }
                    }
                }

                //kills the snake if it collided into a wall or a snake
                if (collidedWith is Wall || collidedWith is Snake)
                {
                    s.died = true;

                    lock (Dead)
                    {
                        Dead.TryAdd(s, _theWorld.respawnRate);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Wraps the snake around the map once it hits an edge of the map.
    /// Snake at the bottom of the map will move to the top and vice versa.
    /// </summary>
    /// <param name="snake"></param>
    private void WrapAroundY(Snake snake)
    {
        foreach (var segment in snake.body)
        {
            if (segment.Y > 0)
                segment.Y -= 975*2;
            else
                segment.Y += 975*2;
        }
    }

    /// <summary>
    /// Wraps the snake around the map once it hits an edge of the map.
    /// Snake at the left of the map will move to the right and vice versa.
    /// </summary>
    /// <param name="snake"></param>
    private void WrapAroundX(Snake snake)
    {
        foreach (var segment in snake.body)
        {
            if (segment.X > 0)
                segment.X -= 975 * 2;
            else
                segment.X += 975 * 2;
        }
    }

    /// <summary>
    /// Check if the given head vector will collide with any object
    /// </summary>
    /// <param name="head"></param>
    /// <param name="collidedWith"></param>
    /// <returns></returns>
    public bool CheckCollision(Vector2D head, out object? collidedWith)
    {
        collidedWith = null;

        while(collidedWith is null)
        {
            foreach (Power p in _theWorld.Powers.Values)
            {
                if ((head - p.loc).Length() <= 20)
                {
                    collidedWith = p;
                }
            }
            foreach (Wall w in _theWorld.Walls.Values)
            {
                if (SegmentCollision(w.p1, w.p2, head, "w"))
                {
                    collidedWith = w;
                }
            }
            foreach (Snake s in _theWorld.Snakes.Values)
            {
                //Doesn't check for collision where the snake is dead
                if (Dead.ContainsKey(s) || s.died)
                    continue;

                
                int segCheck = s.body.Count - 1;

                //prevents the comparisons of head vector and head vector if it is the same snake
                if (s.body.Last().Equals(head))
                {
                    if (turn.ContainsKey(s))
                        continue;
                    segCheck -= 1;
                }

                //checks every segment to see if the head collides
                for (int i = 0; i < segCheck; i++)
                {
                    if (SegmentCollision(s.body[i], s.body[i + 1], head, "s"))
                    {
                        collidedWith = s;
                    }
                }
            }
            //prevents and infinite loop if there is no collisions
            break;
        }

        //a collision was found
        if (collidedWith is not null)
            return true;
        
        return false;
    }

    /// <summary>
    /// Creates a segment out of the first two given vector and checks if the head vector collides with it.
    /// string obj represents what object we are checking the head with, it will change the margins used to
    /// check for collisions.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="head"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private bool SegmentCollision(Vector2D first, Vector2D second, Vector2D head, string obj)
    {
        //margin for wall collision check
        double margin = 30;

        //margin for snake collision check
        if (obj.Equals("s"))
            margin = 10;

        double segX1;
        double segX2;
        double segY1;
        double segY2;

        //if the segment is along the X axis
        if (first.X < second.X)
        {
            segX1 = first.X - margin;
            segX2 = second.X + margin;
        }
        else
        {
            segX1 = second.X - margin;
            segX2 = first.X + margin;
        }

        //if the segment is along the Y axis
        if (first.Y < second.Y)
        {
            segY1 = first.Y - margin;
            segY2 = second.Y + margin;
        }
        else
        {
            segY1 = second.Y - margin;
            segY2 = first.Y + margin;
        }

        //checks if the head is inside the segment 
        bool withinX = segX1 <= head.X && segX2 >= head.X  ; // x1' < x < x2'
        bool withinY = segY1 <= head.Y && segY2 >= head.Y; //y1' < y < y2'

        if (withinX && withinY)
            return true;

        return false;
    }

    /// <summary>
    /// Send a message to the server
    /// </summary>
    /// <param name="message"></param>
    public void MessageEntered(string message)
    {
        HashSet<long> disconnectedClients = new HashSet<long>();

        lock (_clients)
        {
            foreach (SocketState client in _clients.Values)
            {
                //if a message cannot reach a client then they are flagged as disconnected
                if (!Networking.Send(client.TheSocket, message + "\n"))
                    disconnectedClients.Add(client.ID);
            }
        }

        foreach (long id in disconnectedClients)
            RemoveClient(id);
    }

    /// <summary>
    /// Removes a client from the clients dictionary and sets the state of the client's snake
    /// </summary>
    /// <param name="id">The ID of the client</param>
    private void RemoveClient(long id)
    {
        Console.WriteLine("Client " + id + " disconnected");

        lock (_clients)
        {
            _clients.Remove(id);
        }

        //sends a "dead" version of the snake to the clients
        _theWorld.Snakes[(int)id].dc = true;
        _theWorld.Snakes[(int)id].died = true;

        lock (Dead)
        {
            Dead.TryAdd(_theWorld.Snakes[(int)id], _theWorld.respawnRate);
        }
    }

}
