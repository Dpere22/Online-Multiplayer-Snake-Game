// Implementation by Diego Perez and Christina Le 
// for 3500 Last Update: November 27, 2023
using SnakeGame;
using System.Text.Json.Serialization;

namespace World
{
    public class Power
    {
        /// <summary>
        /// Id of the power up
        /// </summary>
        public int power { get; private set; }

        /// <summary>
        /// x,y location of the power up
        /// </summary>
        public Vector2D loc { get; private set; }

        /// <summary>
        /// If the power up is still active or not
        /// </summary>
        public bool died { get; set; }

        /// <summary>
        /// Creates a power up object
        /// </summary>
        /// <param name="power">power up id</param>
        /// <param name="loc">location of the power up</param>
        /// <param name="died">state of the power up</param>
        [JsonConstructor]
        public Power(int power, Vector2D loc, bool died)
        {
            this.power = power;
            this.loc = loc;
            this.died = died;
        }
    }
}