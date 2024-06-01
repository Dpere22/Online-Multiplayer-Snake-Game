// Implementation by Diego Perez and Christina Le 
// for 3500 Last Update: November 27, 2023
using SnakeGame;
using System.Text.Json.Serialization;

namespace World
{
    public class Snake
    {
        /// <summary>
        /// snake id
        /// </summary>
        public int snake { get; private set; }

        /// <summary>
        /// snake player name
        /// </summary>
        public string name { get; private set; }

        /// <summary>
        /// list of snake body segments
        /// </summary>
        public List<Vector2D> body { get; private set; }

        /// <summary>
        /// Vector 2D of direction snake is currently moving
        /// </summary>
        public Vector2D dir { get; set; }

        public int score { get; set; }
        public bool died { get; set; }
        public bool alive { get; set; }
        public bool dc { get; set; }
        public bool join { get; private set; }

        /// <summary>
        /// Constructs a snake object
        /// </summary>
        /// <param name="snake">Snake's id</param>
        /// <param name="name">Snake's player's name</param>
        /// <param name="body">Vector2D list of the body segments</param>
        /// <param name="dir">Vector2D of the direction the head is going</param>
        /// <param name="score">Current Score</param>
        /// <param name="died">Death state</param>
        /// <param name="alive">Alive state</param>
        /// <param name="dc">Disconnection state</param>
        /// <param name="join">Connected state</param>
        [JsonConstructor]
        public Snake(int snake, string name, List<Vector2D> body, Vector2D dir,
        int score, bool died, bool alive, bool dc, bool join)
        {
            this.snake = snake;
            this.name = name;
            this.body = body;
            this.dir = dir;
            this.score = score;
            this.died = died;
            this.alive = alive;
            this.dc = dc;
            this.join = join;
        }
    }
}