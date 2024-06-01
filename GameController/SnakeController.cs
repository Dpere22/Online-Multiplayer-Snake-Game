
// Written by Travis Martin
// Further Implementation by Diego Perez and Christina Le 
// for 3500 Last Update: November 27, 2023
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using World;
using NetworkUtil;

namespace GameController
{
    /// <summary>
    /// A controller class for communicating with the server
    /// and the view
    /// </summary>
    public class SnakeController
    {
        // Controller events that the view can subscribe to
        public delegate void ConnectedHandler();
        public event ConnectedHandler? Connected;

        public delegate void ErrorHandler(string err);
        public event ErrorHandler? Error;

        public delegate void GameUpdateHandler();
        public event GameUpdateHandler? UpdateArrived;

        //keeps track of the current world information
        private SnakeWorld theWorld = new();

        /// <summary>
        /// State representing the connection with the server
        /// </summary>
        SocketState? _theServer;

        /// <summary>
        /// Returns the current snake world
        /// </summary>
        /// <returns></returns>
        public SnakeWorld GetWorld()
        {
            return theWorld;
        }

        /// <summary>
        /// Begins the process of connecting to the server
        /// </summary>
        /// <param name="addr"></param>
        public void Connect(string addr)
        {
            Networking.ConnectToServer(OnConnect, addr, 11000);
        }

        /// <summary>
        /// Method to be invoked by the networking library when a connection is made
        /// </summary>
        /// <param name="state"></param>
        private void OnConnect(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                // inform the view
                Error?.Invoke("Error connecting to server");
                return;
            }

            _theServer = state;

            // inform the view
            Connected?.Invoke();

            // Start an event loop to receive messages from the server
            state.OnNetworkAction = ReceiveMessage;
            Networking.GetData(state);
        }

        /// <summary>
        /// Method to be invoked by the networking library when 
        /// data is available
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveMessage(SocketState state)
        {
            if (state.TheSocket.Connected == false)
            {
                return;
            }
            if (state.ErrorOccurred)
            {
                // inform the view
                Error?.Invoke("Lost connection to server");
                return;
            }
            ProcessMessages(state);

            // Continue the event loop
            // state.OnNetworkAction has not been changed, 
            // so this same method (ReceiveMessage) 
            // will be invoked when more data arrives
            Networking.GetData(state);
        }

        /// <summary>
        /// Process any buffered messages separated by '\n'
        /// Then inform the view
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessages(SocketState state)
        {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.

            List<string> newMessages = new List<string>();

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[^1] != '\n')
                    break;

                // build a list of messages to send to the view
                newMessages.Add(p);

                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }
            foreach (var m in newMessages)
            {
                var doc = JsonDocument.Parse(m);

                //gets the playerId (should always be the first item server sends)
                if (theWorld.playerId == -1)
                {
                    theWorld.playerId = JsonSerializer.Deserialize<int>(doc);
                    continue;
                }
                //gets the world size (should always be the second item the server sends)
                if (theWorld.playerId != -1 && theWorld.givenSize == false)
                {
                    theWorld.Size = JsonSerializer.Deserialize<int>(doc);
                    theWorld.givenSize = true;
                    continue;
                }

                //parses the messages and deserializes base on the string match from parse
                if (doc.RootElement.TryGetProperty("snake", out _))
                {
                    lock (theWorld.Snakes)
                    {
                        //adds the deserialized snake object to the world 
                        Snake s = JsonSerializer.Deserialize<Snake>(m)!;
                        if (!theWorld.Snakes.TryAdd(s.snake, s))
                        {
                            theWorld.Snakes[s.snake] = s;
                        }
                    }
                }
                else if (doc.RootElement.TryGetProperty("wall", out _))
                {
                    lock (theWorld.Walls)
                    {
                        //adds the deserialized wall object to the world 
                        Wall w = JsonSerializer.Deserialize<Wall>(m)!;
                        if (!theWorld.Walls.TryAdd(w.wall, w))
                        {
                            theWorld.Walls[w.wall] = w;
                        }
                    }
                }
                else if (doc.RootElement.TryGetProperty("power", out _))
                {
                    lock (theWorld.Powers)
                    {
                        //adds the deserialized power object to the world 
                        Power pow = JsonSerializer.Deserialize<Power>(m)!;
                        if (!theWorld.Powers.TryAdd(pow.power, pow))
                        {
                            theWorld.Powers[pow.power] = pow;
                        }
                    }
                }
            }
            // inform the view
            UpdateArrived?.Invoke();
        }

        /// <summary>
        /// Closes the connection with the server
        /// </summary>
        public void Close()
        {
            _theServer?.TheSocket.Close();
        }

        /// <summary>
        /// Send a message to the server
        /// </summary>
        /// <param name="message"></param>
        public void MessageEntered(string message)
        {
            if (_theServer is not null)
                Networking.Send(_theServer.TheSocket, message + "\n");
        }
    }
}