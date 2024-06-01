Authors: Diego Perez & Christina Le
Last Updated 12/7/2023

Design Choices:
    Program.cs is the entry point for the server. It uses top level statements. It's only goal is to start 
    the server's game loop and to make the serverController. 

    ServerController.cs is used for both connections and game logic. It has various logic methods that check 
    for collisions and update world. The world is the same world code that is used for Snake Client. Goal is 
    that the server has the master world that it sends out to all clients on each frame. MsPerFrame, Respawn 
    rate, and universe size is adjustable from the XML. Wrap-around works as expected. 
    
    GameSettings.cs is simply a class for deserializing the XML file into at server start and then is not 
    re-used. 

    The velocity is hard coded at 6 units per frame.
