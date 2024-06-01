
// Implementation by Diego Perez and Christina Le 
// for 3500 Last Update: November 27, 2023
using SnakeGame;

namespace World
{
    /// <summary>
    /// World class containing all the Snakes, Power Ups, Walls,
    /// and festive state of the world
    /// </summary>
    public class SnakeWorld
    {
        /// <summary>
        /// List of all the Snakes in the game
        /// </summary>
        public Dictionary<int, Snake> Snakes { get; set; }

        /// <summary>
        /// List of all the power ups in the game
        /// </summary>
        public Dictionary<int, Power> Powers { get; set; }

        /// <summary>
        /// List of all the wall segments in the game
        /// </summary>
        public Dictionary<int, Wall> Walls { get; set; }

        /// <summary>
        /// Property for the world Size
        /// </summary>
        public int Size { get; set; } = 2000;

        /// <summary>
        /// Property for the playerId
        /// </summary>
        public int playerId { get; set; } = -1;

        /// <summary>
        /// bool signifying whether the server has provided the world size
        /// </summary>
        public bool givenSize { get; set; }

        /// <summary>
        /// bool for the festive state of the world
        /// </summary>
        public bool festive { get; set; } = false;

        public int respawnRate { get; set; } = 24;

        /// <summary>
        /// SnakeWorld constructor with no snakes, power ups, or walls
        /// </summary>
        public SnakeWorld()
        {
            Snakes = new Dictionary<int, Snake>();
            Powers = new Dictionary<int, Power>();
            Walls = new Dictionary<int, Wall>();
        }
    }
}