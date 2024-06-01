
// Written by Travis Martin
// Further Implementation by Diego Perez and Christina Le 
// for 3500 Last Update: November 27, 2023
using IImage = Microsoft.Maui.Graphics.IImage;
#if MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#else
using Microsoft.Maui.Graphics.Win2D;
#endif
using System.Reflection;
using World;



namespace SnakeGame;
public class WorldPanel : ScrollView, IDrawable
{
    //saves loaded image so it doesn't have to be loaded everytime we draw
    private IImage wall;
    private IImage background;
    private IImage powerUp1;
    private IImage powerUp2;
    private IImage hat;

    //indicates if the images are properly initialized for drawing
    private bool initializedForDrawing;

    //object drawer delegate for the DrawObjectWithTransform method
    public delegate void ObjectDrawer(object o, ICanvas canvas);

    //initializes the view and the world 
    private GraphicsView graphicsView = new();
    private SnakeWorld theWorld;
    private int viewSize = 900;

    /// <summary>
    /// Loads the image given by the string name of the image
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeClient.Resources.Images";
        using (Stream stream = assembly.GetManifestResourceStream($"{path}.{name}"))
        {
#if MACCATALYST
            return PlatformImage.FromStream(stream);
#else
            return new W2DImageLoadingService().FromStream(stream);
#endif
        }
    }

    /// <summary>
    /// Initializes the world panel 
    /// </summary>
    public WorldPanel()
    {
        graphicsView.Drawable = this;
        graphicsView.HeightRequest = 900;
        graphicsView.WidthRequest = 900;
        Content = graphicsView;
        hat = loadImage("emptyhat.png");
    }

    /// <summary>
    /// Sets this SnakeWorld to the given SnakeWorld
    /// </summary>
    /// <param name="w"></param>
    public void SetWorld(SnakeWorld w)
    {
        theWorld = w;
    }

    /// <summary>
    /// Loads the world map's images
    /// Images are based on if the world's festive is true or not
    /// </summary>
    private void InitializeDrawing()
    {
        if (theWorld.festive)
        {
            wall = loadImage("snakewinterwall.png");
            background = loadImage("snakewintermap.png");
            powerUp1 = loadImage("redpowerup.png");
            powerUp2 = loadImage("greenpowerup.png");
        }
        else
        {
            wall = loadImage("wallsprite.png");
            background = loadImage("background.png");
        }

        initializedForDrawing = true;
    }
    /// <summary>
    /// This method performs a translation and rotation to draw an object.
    /// </summary>
    /// <param name="canvas">The canvas object for drawing onto</param>
    /// <param name="o">The object to draw</param>
    /// <param name="worldX">The X component of the object's position in world space</param>
    /// <param name="worldY">The Y component of the object's position in world space</param>
    /// <param name="angle">The orientation of the object, measured in degrees clockwise from "up"</param>
    /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
    private void DrawObjectWithTransform(ICanvas canvas, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
    {
        // "push" the current transform
        canvas.SaveState();

        canvas.Translate((float)worldX, (float)worldY);
        canvas.Rotate((float)angle);
        drawer(o, canvas);

        // "pop" the transform
        canvas.RestoreState();
    }

    /// <summary>
    /// Sets the festivity state of the world
    /// if festive is true then the map is winter
    /// else default world map
    /// </summary>
    /// <param name="change"></param>
    public void Festive(bool change)
    {
        //reloads the wall and background images based on festive setting
        initializedForDrawing = false;

        //changes the festive bool to the oposite of it is currently
        if (change)
        {
            theWorld.festive = !(theWorld.festive);
        }
        //reverts back to the default map
        else
        {
            theWorld.festive = false;
        }
    }

    /// <summary>
    /// loads and sets a hat image based on the given number between 0-9
    /// </summary>
    /// <param name="currHat"></param>
    /// <exception cref="ArgumentOutOfRangeException"> given number was not between 0-9</exception>
    public void SetHat(int currHat)
    {
        hat = currHat switch
        {
            0 => loadImage("emptyhat.png"),
            1 => loadImage("tophat.png"),
            2 => loadImage("chefhat.png"),
            3 => loadImage("winterhat.png"),
            4 => loadImage("greenwinterhat.png"),
            5 => loadImage("army.png"),
            6 => loadImage("sprouthat.png"),
            7 => loadImage("earmuffs.png"),
            8 => loadImage("yankeewbrimhat.png"),
            9 => loadImage("crownhat.png"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Returns a color based on the given snake's id
    /// </summary>
    /// <param name="o"></param>
    /// <param name="canvas"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void Color(object o, ICanvas canvas)
    {
        if (o is Snake p)
            canvas.StrokeColor = (p.snake % 8) switch
            {
                0 => Colors.Red,
                1 => Colors.Orange,
                2 => Colors.DarkSlateGrey,
                3 => Colors.Yellow,
                4 => Colors.Green,
                5 => Colors.Purple,
                6 => Colors.Brown,
                7 => Colors.Pink,
                _ => throw new ArgumentOutOfRangeException()
            };
    }

    /////////////////////////////////////////////////////////////////////////////////////////
    // Object Drawers
    /////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Draws the given snake segment
    /// </summary>
    /// <param name="o">The player to draw</param>
    /// <param name="canvas"></param>
    private void SnakeDrawer(object o, ICanvas canvas)
    {
        int snakeSegmentLength = (int)o;
        canvas.DrawLine(0, 0, 0, -snakeSegmentLength - 5);
    }

    /// <summary>
    /// Draws the snake's death state
    /// </summary>
    /// <param name="o"></param>
    /// <param name="canvas"></param>
    private void SnakeDeathDrawer(object o, ICanvas canvas)
    {
        canvas.StrokeSize = 5;
        canvas.StrokeDashPattern = new float[] { 1, 7 };
        canvas.DrawCircle(0, 0, 50);
        canvas.StrokeDashPattern = null;
    }

    /// <summary>
    /// Draws the wall image
    /// </summary>
    /// <param name="o"></param>
    /// <param name="canvas"></param>
    private void WallDrawer(object o, ICanvas canvas)
    {
        float w = wall.Width;
        float h = wall.Height;
        canvas.DrawImage(wall, -w / 2, -h / 2, w, h);
    }

    /// <summary>
    /// Draws two visually different power ups
    /// </summary>
    /// <param name="o"></param>
    /// <param name="canvas"></param>
    private void PowerDrawer(object o, ICanvas canvas)
    {
        Power p = o as Power;
        int width = 16;

        if (p.power % 2 == 0)
        {
            //draws presents for the power up if the map is festive
            if (theWorld.festive)
            {
                canvas.DrawImage(powerUp1, -(width / 2), -(width / 2), width, width);
                return;
            }

            //draws round orange power ups
            canvas.FillColor = Colors.Orange;
            canvas.FillEllipse(-(width / 2), -(width / 2), width, width);
        }
        else
        {
            //draws presents for the power up if the map is festive
            if (theWorld.festive)
            {
                canvas.DrawImage(powerUp2, -(width / 2), -(width / 2), width, width);
                return;
            }

            // draws green square power ups
            canvas.FillColor = Colors.Green;
            canvas.FillRoundedRectangle(-(width / 2), -(width / 2), 10, 10, 1);
        }

    }

    /// <summary>
    /// Draws the client's view
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="dirtyRect"></param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // makes sure the images are properly loaded
        if (!initializedForDrawing)
            InitializeDrawing();

        //reset the state of the canvas so we're not drawing over the old one
        canvas.ResetState();

        // draws only if the player exists and if the server has provided the world size
        if (theWorld.Snakes.TryGetValue(theWorld.playerId, out Snake s) && theWorld.givenSize)
        {
            //translates the view so that it follows the player
            float playerX = (float)s.body.Last().X;
            float playerY = (float)s.body.Last().Y;
            canvas.Translate(-playerX + (viewSize / 2), -playerY + (viewSize / 2) - (viewSize / 4));
            canvas.DrawImage(background, (-theWorld.Size / 2), (-theWorld.Size / 2), theWorld.Size, theWorld.Size);

            lock (theWorld.Snakes)
            {
                //draws each snake in the world
                foreach (var snake in theWorld.Snakes.Values)
                {
                    if (!snake.dc)
                    {
                        //sets the color and stroke size for drawing the current snake
                        canvas.StrokeSize = 10;
                        Color(snake, canvas);

                        //draws the death state of the snake if it is dead
                        if (snake.died || !snake.alive)
                        {
                            DrawObjectWithTransform(canvas, 0, snake.body.Last().X, snake.body.Last().Y, 0, SnakeDeathDrawer);
                        }
                        else if (snake.alive)
                        {
                            //keeps track of where the previous vector point of the snakes section was
                            var prev = snake.body.First();
                            double segmentX = prev.GetX();
                            double segmentY = prev.GetY();

                            //draws each section of the snake
                            foreach (var vec in snake.body)
                            {
                                //finds the segment between the previous Vector2D in the body and the current one
                                int segmentLength = (int)Math.Abs((vec - prev).Length());
                                double segmentDirection = Vector2D.AngleBetweenPoints(vec, prev);

                                //makes the head of the snake round
                                if (vec.Equals(snake.body.Last()) || prev.Equals(snake.body.First()))
                                {
                                    canvas.StrokeLineCap = LineCap.Round;
                                    segmentLength -= 5;
                                }

                                //draws the segment
                                DrawObjectWithTransform(canvas, segmentLength, segmentX, segmentY, segmentDirection, SnakeDrawer);

                                //draws a hat and player name on top of the snake's head
                                if (vec.Equals(snake.body.Last()))
                                {
                                    string snakeName = snake.name + " : " + snake.score;
                                    canvas.DrawImage(hat, (float)vec.X - 7f, (float)vec.Y - 20f, hat.Width, hat.Height);
                                    canvas.DrawString(snakeName, (float)vec.X - 7f, (float)vec.Y - 20f, HorizontalAlignment.Center);
                                }

                                //resets the stroke line cap and set the current vector as the previous
                                canvas.StrokeLineCap = LineCap.Butt;
                                prev = vec;
                                segmentX = prev.GetX();
                                segmentY = prev.GetY();
                            }
                        }
                    }
                }
            }

            lock (theWorld.Walls)
            {
                //draws each wall segment in the world
                foreach (var w in theWorld.Walls.Values)
                {
                    //sees if the walls is drawn along x axis or y axis
                    if (w.p1.X == w.p2.X)
                    {
                        //calculates the length between the given y axis
                        double seg = Math.Abs(w.p1.Y - w.p2.Y) + 50;

                        //draws the walls along the y axis staring at where it should end (p1.x, p2.y) to where it starts (p1.x, p1.y)
                        while (seg > 0)
                        {
                            //determins if the walls are drawn up or down the y axis depending on if the p1's y is larger or p2's y
                            if (w.p1.Y > w.p2.Y)
                            {
                                DrawObjectWithTransform(canvas, w, w.p1.X, w.p1.Y - seg + 50, 0, WallDrawer);
                            }
                            else
                            {
                                DrawObjectWithTransform(canvas, w, w.p1.X, w.p1.Y + seg - 50, 0, WallDrawer);
                            }

                            //increments the segment length signaling that we drew that part of the segment
                            seg -= 50;
                        }
                    }
                    else if (w.p1.Y == w.p2.Y)
                    {
                        //calculates the length between the given x axis
                        double seg = Math.Abs(w.p1.X - w.p2.X) + 50;

                        //draws the walls along the x axis staring at where it should end (p2.x, p1.y) to where it starts (p1.x, p1.y)
                        while (seg > 0)
                        {
                            //determins if the walls are drawn left or right on the x axis depending on if the p1's x is larger or p2's x
                            if (w.p1.X > w.p2.X)
                            {
                                DrawObjectWithTransform(canvas, w, w.p1.X - seg + 50, w.p1.Y, 0, WallDrawer);
                            }
                            else
                            {
                                DrawObjectWithTransform(canvas, w, w.p1.X + seg - 50, w.p1.Y, 0, WallDrawer);
                            }

                            //increments the segment length signaling that we drew that part of the segment
                            seg -= 50;
                        }
                    }
                }
            }

            lock (theWorld.Powers)
            {
                //Draws the power ups in the world
                foreach (var power in theWorld.Powers.Values)
                {
                    //makes sure the power up is still active
                    if (!power.died)
                    {
                        DrawObjectWithTransform(canvas, power, power.loc.X, power.loc.Y, 0, PowerDrawer);
                    }
                }
            }
        }

    }
}