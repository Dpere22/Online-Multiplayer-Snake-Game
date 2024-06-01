
// Written by Travis Martin
// Further Implementation by Diego Perez and Christina Le 
// for 3500 Last Update: November 27, 2023
using GameController;

namespace SnakeGame;

public partial class MainPage
{
    //for client/server communication
    private SnakeController _snakeController;
    public MainPage()
    {
        InitializeComponent();

        _snakeController = new SnakeController();
        _snakeController.UpdateArrived += OnFrame;
        _snakeController.Connected += HandleConnected;
        _snakeController.Error += NetworkErrorHandler;
        worldPanel.SetWorld(_snakeController.GetWorld());

        //inform the view
        OnFrame();
    }

    /// <summary>
    /// When the screen is tapped the key entry is focused for 
    /// snake control inputs
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void OnTapped(object sender, EventArgs args)
    {
        keyboardHack.Focus();
    }

    /// <summary>
    /// Sends a message to the server to move the snake based on entered keys
    /// w : up
    /// s : down
    /// a : left
    /// d : right
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void OnTextChanged(object sender, TextChangedEventArgs args)
    {
        Entry entry = (Entry)sender;
        String text = entry.Text.ToLower();

        //sends the movement to the server (does nothing if not w,a,s,d)
        if (text == "w")
        {
            string message = "{\"moving\":\"up\"}";
            _snakeController.MessageEntered(message);
        }
        else if (text == "a")
        {
            string message = "{\"moving\":\"left\"}";
            _snakeController.MessageEntered(message);
        }
        else if (text == "s")
        {
            string message = "{\"moving\":\"down\"}";
            _snakeController.MessageEntered(message);
        }
        else if (text == "d")
        {
            string message = "{\"moving\":\"right\"}";
            _snakeController.MessageEntered(message);
        }

        //prepares the entry box for the next input
        entry.Text = "";
    }

    /// <summary>
    /// Error handler for the SnakeController
    /// </summary>
    /// <param name="err"></param>
    private void NetworkErrorHandler(string err)
    {
        // Show the error
        Dispatcher.Dispatch(() => DisplayAlert("Error", err, "OK"));

        // Then re-enable the controls so the user can reconnect
        Dispatcher.Dispatch(
          () =>
          {
              connectButton.IsEnabled = true;
              serverText.IsEnabled = true;
          });

        //Create a new client all together
        ResetConnection();
    }

    /// <summary>
    /// Reconnects to the server as a new player if the client disconnects
    /// </summary>
    private void ResetConnection()
    {
        _snakeController.Close();

        _snakeController = new SnakeController();
        _snakeController.UpdateArrived += OnFrame;
        _snakeController.Connected += HandleConnected;
        _snakeController.Error += NetworkErrorHandler;
        worldPanel.SetWorld(_snakeController.GetWorld());
        worldPanel.Festive(false);

        //inform the view
        OnFrame();
    }

    /// <summary>
    /// Event handler for the connect button
    /// We will put the connection attempt interface here in the view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ConnectClick(object sender, EventArgs args)
    {
        //reconencts if the client is disconnected
        if (connectButton.Text == "Disconnect")
        {
            connectButton.Text = "Connect";
            ResetConnection();
        }
        //connects to the server
        else
        {
            connectButton.Text = "Disconnect";
            if (serverText.Text == "")
            {
                DisplayAlert("Error", "Please enter a server address", "OK");
                return;
            }
            if (nameText.Text == "")
            {
                DisplayAlert("Error", "Please enter a name", "OK");
                return;
            }
            if (nameText.Text.Length > 16)
            {
                DisplayAlert("Error", "Name must be less than 16 characters", "OK");
                return;
            }

            _snakeController.Connect(serverText.Text);
        }

        //anticipates movement inputs
        keyboardHack.Focus();
    }

    /// <summary>
    /// Handler for the controller's Connected event
    /// </summary>
    private void HandleConnected()
    {
        _snakeController.MessageEntered(nameText.Text);
    }

    /// <summary>
    /// Use this method as an event handler for when the controller has updated the world
    /// </summary>
    private void OnFrame()
    {
        Dispatcher.Dispatch(() => graphicsView.Invalidate());
    }

    /// <summary>
    /// Method for when the Control button is clicked.
    /// Displays window with control information
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ControlsButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Controls",
                     "W: Move up\n" +
                     "A: Move left\n" +
                     "S: Move down\n" +
                     "D: Move right\n" +
                     "Surprise!: change the map\n" +
                     "Hat: use Next and Back to change which hat your snake wears\n",
                     "OK");
        //anticipates movement inputs
        keyboardHack.Focus();
    }

    /// <summary>
    /// Method for when the About button is clicked.
    /// Displays window with about information
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AboutButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("About",
      "SnakeGame solution\nArtwork by Jolie Uk, Alex Smith, Diego Perez, and Christina Le\nGame design by Daniel Kopta and Travis Martin\n" +
      "Implementation by Diego Perez and Christina Le\n" +
        "CS 3500 Fall 2023, University of Utah", "OK");

        //anticipates movement inputs
        keyboardHack.Focus();
    }

    /// <summary>
    /// Method for when the Surprise button is clicked.
    /// Changes the map to and from the winter and default maps
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SurpriseButton_Clicked(object sender, EventArgs e)
    {
        worldPanel.Festive(true);

        //anticipates movement inputs
        keyboardHack.Focus();
    }

    /// <summary>
    /// Method for when the Next button is clicked.
    /// Sets the snakes hats to the next hat on the list
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NextButton_Clicked(object sender, EventArgs e)
    {
        int.TryParse(CurrentHat.Text, out int curr);
        if (curr == 9)
        {
            worldPanel.SetHat(0);
            CurrentHat.Text = "0";
        }
        else
        {
            worldPanel.SetHat(curr + 1);
            CurrentHat.Text = "" + (curr + 1);
        }

        //anticipates movement inputs
        keyboardHack.Focus();
    }

    /// <summary>
    /// Method for when the Back button is clicked.
    /// Sets the snakes hats to the previous hat on the list
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BackButton_Clicked(object sender, EventArgs e)
    {
        int.TryParse(CurrentHat.Text, out int curr);

        if (curr == 0)
        {
            worldPanel.SetHat(9);
            CurrentHat.Text = "9";
        }
        else
        {
            worldPanel.SetHat(curr - 1);
            CurrentHat.Text = "" + (curr - 1);
        }

        //anticipates movement inputs
        keyboardHack.Focus();
    }

    /// <summary>
    /// Focuses on the next key enter for snake movement control
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContentPage_Focused(object sender, FocusEventArgs e)
    {
        if (!connectButton.IsEnabled)
            keyboardHack.Focus();
    }
}