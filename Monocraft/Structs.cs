using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monocraft
{ 
    //faces of a block
    enum Face
    {
        top, 
        side, 
        down
    }
     
    //stages of chunk loading
    enum ChunkState
    {
        loaded, 
        detailed, 
        meshed
    } 

    //menu hierarchy
    enum GameState
    { 
        opening,
        title, 
        menu,   
        infoScreen,
        saveSelect,
        createWorld,
        playing, 
    }
}

