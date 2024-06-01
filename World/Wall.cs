
// Implementation by Diego Perez and Christina Le 
// for 3500 Last Update: November 27, 2023
using SnakeGame;
using System.Text.Json.Serialization;

namespace World
{
    /// <summary>
    /// Wall Class representing a stretch of a wall
    /// </summary>
    public class Wall
    {
        /// <summary>
        /// Id of the wall
        /// </summary>
        public int wall { get; set; }

        /// <summary>
        /// First point where the wall segment begins
        /// </summary>
        public Vector2D p1 { get;  set; }
        /// <summary>
        /// Last point where the wall segment ends
        /// </summary>
        public Vector2D p2 { get;  set; }

        /// <summary>
        /// Constructs a wall object 
        /// </summary>
        /// <param name="wall">wall id</param>
        /// <param name="p1">begining point of the wall</param>
        /// <param name="p2">ending point of the wall</param>
        [JsonConstructor]
        public Wall(int wall, Vector2D p1, Vector2D p2)
        {
            this.wall = wall;
            this.p1 = p1;
            this.p2 = p2;
        }

        /// <summary>
        /// Empty wall constructor for the XML 
        /// </summary>
        public Wall()
        {
            this.wall = 0;
            this.p1 = new Vector2D(0,0);
            this.p2 = new Vector2D(0, 0);
        }
    }
}