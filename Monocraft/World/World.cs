using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace Monocraft
{
    [Serializable]
    public class World : IDrawable, IUpdateable
    { 
        //world + chunk dimensions
        private int _chunkSize;
        private int _loadSize;
        private int _chunkHeight;
        private Vector2 _centreChunk;

        //the worlds seed used for all generation
        private int _seed;  

        //holds block to be placed if right-click is pressed
        private byte _blockType = 1;

        //handles all rendering + model work
        private GraphicsBase _graphicsBase; 

        //gets noise from simplex algorithms
        private NoiseMaker _noiseMaker;

        //mouse states for block editing
        private MouseState currentMouseState;
        private MouseState lastMouseState; 

        //the player in the world
        private Player _player;

        //holds all loaded actors;
        private List<Actor> _actors;

        private Model _actorModel;
        private Texture2D _actorTexture;

        //dictionarys of chunks         

        //holds edited chunks that need to be saved & loaded specially
        private Dictionary<Vector2, Chunk> _oldChunks = new Dictionary<Vector2, Chunk>(); 

        //hold all chunks currently loaded around player (all chunks in memory)
        private Dictionary<Vector2, Chunk> _chunks = new Dictionary<Vector2, Chunk>(); 

        //Queue of all chunks that need loading
        private List<Vector2> _chunksToLoad = new List<Vector2>();

        //new world constructor
        public World(int loadSize, int chunkSize, int seed, int worldHeight, GraphicsBase graphicsBase, Player player, Model actorModel, Texture2D actorTexture)
        {
            //set properties
            _chunkSize = chunkSize;
            _loadSize = loadSize;
            _graphicsBase = graphicsBase;
            _chunkHeight = worldHeight;
            _seed = seed;
            _player = player;
            _centreChunk = player.GetCurrentChunk(_chunkSize);
            _noiseMaker = new NoiseMaker(seed);
            _actorModel = actorModel;
            _actorTexture = actorTexture;
            _actors = new List<Actor>();

            //Add chunks in player's load distance to the load queue (in a spiral pattern)
            for (int x = 0; x <= _loadSize * _loadSize * 4; x++)
            {
                Vector2 i = Utility.Spiral(_loadSize * 2, _loadSize * 2, x); 
                //if doent contain chunk, add said chunk
                if (!_chunks.ContainsKey(i + _centreChunk)
                && !_chunksToLoad.Contains(i + _centreChunk))
                { 
                    _chunksToLoad.Add(i + _centreChunk);
                }
            } 
            //load centre chunk of world so player has solid ground
            _chunks.Add(_centreChunk, new Chunk(new Vector3(_chunkSize, _chunkHeight, _chunkSize), _centreChunk, _seed, _graphicsBase.Clone(), _noiseMaker));
            _chunksToLoad.Remove(_centreChunk); 
        }

        //constructor for reloading old world from worldsave
        public World(int loadSize, WorldSave worldSave, GraphicsBase graphicsBase, Player player, Model actorModel, Texture2D actorTexture)
        {
            _chunkSize = (int)worldSave.Dimensions.X;
            _loadSize = loadSize;
            _graphicsBase = graphicsBase;
            _chunkHeight = (int)worldSave.Dimensions.Y;
            _seed = worldSave.Seed;
            _player = player;
            _centreChunk = player.GetCurrentChunk(_chunkSize);
            _noiseMaker = new NoiseMaker(_seed);
            _actorModel = actorModel;
            _actorTexture = actorTexture;
            _actors = new List<Actor>();

            //add each edited chunk from the old world to the new world
            foreach (var data in worldSave.KnownChunks)
            {
                _oldChunks.Add(new Vector2(data.Key.X, data.Key.Y), new Chunk(new Vector3(worldSave.Dimensions.X, worldSave.Dimensions.Y, worldSave.Dimensions.Z), new Vector2(data.Key.X, data.Key.Y), _seed, graphicsBase.Clone(), _noiseMaker, data.Value));
            }

            //load centre chunk so player has ground to stand on while world is loading
            if (_oldChunks.ContainsKey(_centreChunk))
            {
                _chunks.Add(_centreChunk, _oldChunks[_centreChunk]);
            }
            else
            {
                _chunks.Add(_centreChunk, new Chunk(new Vector3(_chunkSize, _chunkHeight, _chunkSize), _centreChunk, _seed, _graphicsBase.Clone(), _noiseMaker));
            }
        }

        //create the worldsave object for this world
        public WorldSave GetWorldSave()
        {
            WorldSave ws = new WorldSave();
            Dictionary<Vector2Ser, byte[,,,]> chunks = new Dictionary<Vector2Ser, byte[,,,]>(); 
            //add all edited chunks to world save
            foreach (var item in _oldChunks)
            {
                chunks.Add(new Vector2Ser(item.Key.X, item.Key.Y), item.Value.chunkData);
            }
            ws.KnownChunks = chunks; 
            //save seed
            ws.Seed = _seed; 
            //save world dimensions
            ws.Dimensions = new Vector3Ser(_chunkSize, _chunkHeight, _chunkSize);
            return ws;
        }

        //check if there is a solid (non-water / air) block at given coords
        public bool IsCollisionBlockAt(Vector3 position)
        { 
            //if chunk not loaded, assume false
            if (!_chunks.ContainsKey(new Vector2((int)Math.Floor((int)position.X / (double)_chunkSize), (int)Math.Floor((int)position.Z / (double)_chunkSize))))
            {
                return false;
            } 
            //if in chunk boundaries
            if (position.Y < _chunkHeight && position.Y >= 0)
            { 
                //get chunk and sub position coordinate refers to
                switch (_chunks[new Vector2((int)Math.Floor((int)position.X / (double)_chunkSize), (int)Math.Floor((int)position.Z / (double)_chunkSize))].chunkData[Utility.Mod((int)position.X, _chunkSize), (int)position.Y, Utility.Mod((int)position.Z, _chunkSize), 0])
                { 
                    //if air, or water return false
                    case (0):
                        return false;
                    case (5):
                        return false;
                    default:
                        return true;
                }
            }
            return false;
        }

        //check if there is any block at give coords
        public bool IsBlockAt(Vector3 position)
        {
            //if chunk not loaded, assume false
            if (!_chunks.ContainsKey(new Vector2((int)Math.Floor((int)position.X / (double)_chunkSize), (int)Math.Floor((int)position.Z / (double)_chunkSize))))
            {
                return false;
            }
            //if in chunk boundaries
            if (position.Y < _chunkHeight && position.Y >= 0)
            {
                //get chunk and sub position coordinate refers to
                if (_chunks[new Vector2((int)Math.Floor((int)position.X / (double)_chunkSize), (int)Math.Floor((int)position.Z / (double)_chunkSize))].chunkData[Utility.Mod((int)position.X, _chunkSize), (int)position.Y, Utility.Mod((int)position.Z, _chunkSize), 0] != 0)
                { 
                    //return true if not 0
                    return true;
                }
            }
            return false;
        }

        //set given coords to certain block
        public void SetBlockAt(Vector3 position, byte ID)
        {
            //if chunk not loaded, do nothing
            if (!_chunks.ContainsKey(new Vector2((int)Math.Floor((int)position.X / (double)_chunkSize), (int)Math.Floor((int)position.Z / (double)_chunkSize))))
            {
                return;
            }
            //get chunk and sub position coordinate refers to
            if (position.Y < _chunkHeight && _chunks[new Vector2((int)Math.Floor((int)position.X / (double)_chunkSize), (int)Math.Floor((int)position.Z / (double)_chunkSize))].chunkData[Utility.Mod((int)position.X, _chunkSize), (int)position.Y, Utility.Mod((int)position.Z, _chunkSize), 0] != 12)
            { 
                //set chunk and sub position coordinate refers to to desired block
                _chunks[new Vector2((int)Math.Floor((int)position.X / (double)_chunkSize), (int)Math.Floor((int)position.Z / (double)_chunkSize))].chunkData[Utility.Mod((int)position.X, _chunkSize), (int)position.Y, Utility.Mod((int)position.Z, _chunkSize), 0] = ID;
            }
        }

        //get block type at certain coords
        public byte GetBlockAt(Vector3 position)
        {
            //if chunk not loaded, assume air
            if (!_chunks.ContainsKey(new Vector2((int)Math.Floor((int)position.X / (double)_chunkSize), (int)Math.Floor((int)position.Z / (double)_chunkSize))))
            {
                return 0;
            } 
            //if in bounds
            if (position.Y < _chunkHeight)
            { 
                //return block id
                return _chunks[new Vector2((int)Math.Floor((int)position.X / (double)_chunkSize), (int)Math.Floor((int)position.Z / (double)_chunkSize))].chunkData[Utility.Mod((int)position.X, _chunkSize), (int)position.Y, Utility.Mod((int)position.Z, _chunkSize), 0];
            }
            return 0;
        }

        //check if given position is underwater
        public bool CheckSubmerged(Vector3 position)
        { 
            //if chunk doesnt exist assume false
            if (!_chunks.ContainsKey(new Vector2((int)Math.Floor((int)position.X / (double)_chunkSize), (int)Math.Floor((int)position.Z / (double)_chunkSize))))
            {
                return false;
            } 
            //check if block == water
            if (position.Y < _chunkHeight && _chunks[new Vector2((int)Math.Floor((int)position.X / (double)_chunkSize), (int)Math.Floor((int)position.Z / (double)_chunkSize))].chunkData[Utility.Mod((int)position.X, _chunkSize), (int)position.Y, Utility.Mod((int)position.Z, _chunkSize), 0] == 5)
            {
                return true;
            }
            return false;
        }

        //render each meshed chunk in memory 
        public void Draw()
        { 
            //loop through chunks
            foreach (var chunk in _chunks)
            {
                if (chunk.Value.chunkState == ChunkState.meshed)
                { 
                    chunk.Value.Render();
                }
            }  
            //loop through actors and draw each
            foreach (Actor actor in _actors)
            {
                actor.Draw();
            }
        }


        public void Update(GameTime gameTime = null)
        {
            //get current centre chunk
            _centreChunk = _player.GetCurrentChunk(_chunkSize);
            //delete far away and load new chunks
            ClearChunks();
            AddChunks();
            LoadChunks(); 
            
            //get blocktype to be placed from numpad
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad1))
            {
                _blockType = 1;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.NumPad2))
            {
                _blockType = 2;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.NumPad3))
            {
                _blockType = 4;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.NumPad4))
            {
                _blockType = 7;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.NumPad5))
            {
                _blockType = 8;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.NumPad6))
            {
                _blockType = 9;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.NumPad7))
            {
                _blockType = 10;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.NumPad8))
            {
                _blockType = 11;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.NumPad9))
            {
                _blockType = 12;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.NumPad0))
            {
                _blockType = 19;
            }


            //logic for detecting click
            lastMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            // Recognize a single click of the left mouse button    
            if (lastMouseState.LeftButton == ButtonState.Released && currentMouseState.LeftButton == ButtonState.Pressed)
            {
                SetBlockAt(_player.blockLookingAt, 0);
                //re-mesh chunk block is in
                _chunks[new Vector2((int)Math.Floor((int)_player.blockLookingAt.X / (double)_chunkSize), (int)Math.Floor((int)_player.blockLookingAt.Z / (double)_chunkSize))].LoadMesh(this);
                switch (Utility.Mod((int)_player.blockLookingAt.X, 16))
                {
                    //re-mesh adjacent chunks if block is on border;
                    case (0):
                        _chunks[new Vector2((int)Math.Floor((int)_player.blockLookingAt.X / (double)_chunkSize) - 1, (int)Math.Floor((int)_player.blockLookingAt.Z / (double)_chunkSize))].LoadMesh(this);
                        break;
                    case (15):
                        _chunks[new Vector2((int)Math.Floor((int)_player.blockLookingAt.X / (double)_chunkSize) + 1, (int)Math.Floor((int)_player.blockLookingAt.Z / (double)_chunkSize))].LoadMesh(this);
                        break;
                }
                switch (Utility.Mod((int)_player.blockLookingAt.Z, 16))
                {
                    //re-mesh adjacent chunks if block is on border;
                    case (0):
                        _chunks[new Vector2((int)Math.Floor((int)_player.blockLookingAt.X / (double)_chunkSize), (int)Math.Floor((int)_player.blockLookingAt.Z / (double)_chunkSize) - 1)].LoadMesh(this);
                        break;
                    case (15):
                        _chunks[new Vector2((int)Math.Floor((int)_player.blockLookingAt.X / (double)_chunkSize), (int)Math.Floor((int)_player.blockLookingAt.Z / (double)_chunkSize) + 1)].LoadMesh(this);
                        break;
                }
                //if chunk already exists in old chunks, update it, if not, add it
                if (_oldChunks.ContainsKey(new Vector2((int)Math.Floor((int)_player.blockLookingAt.X / (double)_chunkSize), (int)Math.Floor((int)_player.blockLookingAt.Z / (double)_chunkSize))))
                {
                    _oldChunks[new Vector2((int)Math.Floor((int)_player.blockLookingAt.X / (double)_chunkSize), (int)Math.Floor((int)_player.blockLookingAt.Z / (double)_chunkSize))] = _chunks[new Vector2((int)Math.Floor((int)_player.blockLookingAt.X / (double)_chunkSize), (int)Math.Floor((int)_player.blockLookingAt.Z / (double)_chunkSize))];
                }
                else
                {
                    _oldChunks.Add(new Vector2((int)Math.Floor((int)_player.blockLookingAt.X / (double)_chunkSize), (int)Math.Floor((int)_player.blockLookingAt.Z / (double)_chunkSize)), _chunks[new Vector2((int)Math.Floor((int)_player.blockLookingAt.X / (double)_chunkSize), (int)Math.Floor((int)_player.blockLookingAt.Z / (double)_chunkSize))]);
                }
            }

            //detect mous right click
            if (lastMouseState.RightButton == ButtonState.Released && currentMouseState.RightButton == ButtonState.Pressed)
            {
                SetBlockAt(_player.blockHover, _blockType);
                //reload chunk new block is in
                _chunks[new Vector2((int)Math.Floor((int)_player.blockHover.X / (double)_chunkSize), (int)Math.Floor((int)_player.blockHover.Z / (double)_chunkSize))].LoadMesh(this);
                //reload adjacent chunks if block is on border
                if (_oldChunks.ContainsKey(new Vector2((int)Math.Floor((int)_player.blockLookingAt.X / (double)_chunkSize), (int)Math.Floor((int)_player.blockLookingAt.Z / (double)_chunkSize))))
                {
                    _oldChunks[new Vector2((int)Math.Floor((int)_player.blockLookingAt.X / (double)_chunkSize), (int)Math.Floor((int)_player.blockLookingAt.Z / (double)_chunkSize))] = _chunks[new Vector2((int)Math.Floor((int)_player.blockLookingAt.X / (double)_chunkSize), (int)Math.Floor((int)_player.blockLookingAt.Z / (double)_chunkSize))];
                }
                else
                {
                    _oldChunks.Add(new Vector2((int)Math.Floor((int)_player.blockLookingAt.X / (double)_chunkSize), (int)Math.Floor((int)_player.blockLookingAt.Z / (double)_chunkSize)), _chunks[new Vector2((int)Math.Floor((int)_player.blockLookingAt.X / (double)_chunkSize), (int)Math.Floor((int)_player.blockLookingAt.Z / (double)_chunkSize))]);
                }
            }

            List<Actor> hold = new List<Actor>(); 
            //check if actors are out of loaded world
            foreach (Actor actor in _actors)
            {
                Vector2 pos = actor.GetCurrentChunk(_chunkSize);
                if (pos.X < _centreChunk.X - _loadSize
                        || pos.X > _centreChunk.X + _loadSize
                        || pos.Y < _centreChunk.Y - _loadSize
                        || pos.Y > _centreChunk.Y + _loadSize)
                { 
                    //add actor to unload list
                    hold.Add(actor);
                    continue;
                }
                    actor.Update(gameTime);
            } 
            //unlaod all actors out of bounds
            foreach (Actor actor in hold)
            {
                _actors.Remove(actor);
            }

        }
        //unload any chunks out of player load range
        private void ClearChunks()
        {
            foreach (var chunk in _chunks)
            { 
                //check if chunks is too far away
                if (chunk.Key.X < _centreChunk.X - _loadSize
                        || chunk.Key.X > _centreChunk.X + _loadSize
                        || chunk.Key.Y < _centreChunk.Y - _loadSize
                        || chunk.Key.Y > _centreChunk.Y + _loadSize)
                { 
                    //remove it and destroy any graphics items
                    chunk.Value.Destroy();
                    _chunks.Remove(chunk.Key);
                }
            }
        }

        //generate tree, cactus or other structure
        public void GenTree(Vector2 position, byte treeType)
        {
            int y = _chunkHeight; 
            //loop through column of chunk and look for tree marker
            for (int i = 1; i < _chunkHeight; i++)
            {
                if (GetBlockAt(new Vector3(position.X, i, position.Y)) == 20)
                {
                    y = i;
                    break;
                }
            } 
            //create approprite "tree" at position of marker
            Vector3 basePos = new Vector3(position.X, y, position.Y);
            switch (treeType)
            { 
                //create cactus 
                case (1):
                    for (int i = 0; i < 3; i++)
                    {
                        SetBlockAt(new Vector3(basePos.X, basePos.Y + i, basePos.Z), 8);
                    }
                    break; 
                    //create regular tree
                case (2): 
                    //make trunk
                    for (int i = 0; i < 5; i++)
                    {
                        SetBlockAt(new Vector3(basePos.X, basePos.Y + i, basePos.Z), 7);
                    } 
                    //do leaves
                    for (int i = -2; i <= 2; i++)
                    {
                        for (int j = -2; j <= 2; j++)
                        {
                            SetBlockAt(new Vector3(basePos.X+i, basePos.Y + 3, basePos.Z+j), 8);
                        }
                    }
                    //do leaves
                    for (int i = -2; i <= 2; i++)
                    {
                        for (int j = -2; j <= 2; j++)
                        {
                            SetBlockAt(new Vector3(basePos.X + i, basePos.Y + 4, basePos.Z + j), 8);
                        }
                    }
                    //do top leaves
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            SetBlockAt(new Vector3(basePos.X + i, basePos.Y + 5, basePos.Z + j), 8);
                        }
                    }
                    //do top leaves
                    for (int i = -1; i <= 1; i++)
                    {
                            SetBlockAt(new Vector3(basePos.X + i, basePos.Y + 6, basePos.Z), 8);
                    }
                    //do top leaves
                    for (int i = -1; i <= 1; i++)
                    {
                        SetBlockAt(new Vector3(basePos.X, basePos.Y + 6, basePos.Z + i), 8);
                    }
                    break; 
                    //create house
                case (3):
                    for (int i = 0; i < 6; i++)
                    { 
                        //make floor
                        for (int j = 0; j < 3; j++)
                        {
                            SetBlockAt(new Vector3(basePos.X + i, basePos.Y + j, basePos.Z), 2);
                            SetBlockAt(new Vector3(basePos.X, basePos.Y + j, basePos.Z + i), 2);
                            SetBlockAt(new Vector3(basePos.X+6, basePos.Y + j, basePos.Z + i), 2);
                            SetBlockAt(new Vector3(basePos.X + i, basePos.Y + j, basePos.Z + 6), 2);
                        }
                    } 
                    //make corners + walls
                    for (int j = 0; j < 7; j++)
                    {
                        SetBlockAt(new Vector3(basePos.X + j, basePos.Y + 3, basePos.Z -1), 11);
                        SetBlockAt(new Vector3(basePos.X + j, basePos.Y + 3, basePos.Z), 11);
                        for (int i = 1; i <=5; i++)
                        {
                            SetBlockAt(new Vector3(basePos.X + j, basePos.Y + 4, basePos.Z + i), 11);
                        }
                        
                        SetBlockAt(new Vector3(basePos.X + j, basePos.Y + 3, basePos.Z + 6), 11);
                        SetBlockAt(new Vector3(basePos.X + j, basePos.Y + 3, basePos.Z + 7), 11);
                    }
                    SetBlockAt(new Vector3(basePos.X, basePos.Y + 3, basePos.Z + 1), 7);
                    SetBlockAt(new Vector3(basePos.X, basePos.Y + 3, basePos.Z + 5), 7);
                    SetBlockAt(new Vector3(basePos.X + 6, basePos.Y + 3, basePos.Z + 1), 7);
                    SetBlockAt(new Vector3(basePos.X + 6, basePos.Y + 3, basePos.Z + 5), 7);
                    SetBlockAt(new Vector3(basePos.X, basePos.Y + 1, basePos.Z + 3), 0);
                    SetBlockAt(new Vector3(basePos.X, basePos.Y + 1, basePos.Z + 2), 0);
                    SetBlockAt(new Vector3(basePos.X, basePos.Y + 1, basePos.Z + 4), 0);
                    SetBlockAt(new Vector3(basePos.X, basePos.Y + 0, basePos.Z + 2), 0);
                    SetBlockAt(new Vector3(basePos.X, basePos.Y + 0, basePos.Z + 3), 0);
                    SetBlockAt(new Vector3(basePos.X, basePos.Y + 0, basePos.Z + 4), 0);
                    break;
            }
        }

        private void AddChunks()
        { 
            //check that all chunks in load area are loaded, if they are not, add them to load queue
            for (int x = -_loadSize; x <= _loadSize; x++)
            {
                for (int y = -_loadSize; y <= _loadSize; y++)
                { 
                    //if chunk does not exist, add chunk
                    if (!_chunks.ContainsKey(new Vector2((int)_centreChunk.X + x, (int)_centreChunk.Y + y))
                    && !_chunksToLoad.Contains(new Vector2((int)_centreChunk.X + x, (int)_centreChunk.Y + y)))
                    {
                        _chunksToLoad.Add(new Vector2((int)_centreChunk.X + x, (int)_centreChunk.Y + y));
                    }

                }
            }
        }

        //load new chunks based on load queue
        private void LoadChunks()
        { 
            //check if queue has any items
            if (_chunksToLoad.Count != 0)
            {
                List<Vector2> hold = new List<Vector2>();
                foreach (Vector2 chunkID in _chunksToLoad)
                { 
                    //check if chunk in load queue has been previously loaded
                    if (_oldChunks.ContainsKey(chunkID))
                    {
                        _chunks.Add(chunkID, _oldChunks[chunkID]);
                        hold.Add(chunkID);
                    } 
                    //create new chunk
                    else
                    {
                        _chunks.Add(chunkID, new Chunk(new Vector3(_chunkSize, _chunkHeight, _chunkSize), chunkID, _seed, _graphicsBase.Clone(), _noiseMaker));
                        hold.Add(chunkID);
                    }
                } 
                //remove loaded chunks from load queue
                foreach (var item in hold)
                {
                    _chunksToLoad.Remove(item);
                }
            }  
            //do this is the queue was empty this frame
            else
            {
                bool found = false; 
                //loop through all loaded chunks, if they have not been "detailed" then detail them
                foreach (var chunk in _chunks)
                { 
                    if (chunk.Value.chunkState == ChunkState.loaded
                        && chunk.Value.chunkIndex.X < _centreChunk.X + _loadSize
                        && chunk.Value.chunkIndex.X > _centreChunk.X - _loadSize
                        && chunk.Value.chunkIndex.Y < _centreChunk.Y + _loadSize
                        && chunk.Value.chunkIndex.Y > _centreChunk.Y - _loadSize)
                    {
                        chunk.Value.LoadDetail(this);
                        found = true;
                    }

                } 
                //if no chunks were detailed this frame, mesh 1 unmeshed chunk (by looping)
                if (!found)
                {
                    foreach (var chunk in _chunks)
                    { 
                        //if chunk is within render borders
                        if (chunk.Value.chunkState == ChunkState.detailed
                            && chunk.Value.chunkIndex.X < _centreChunk.X + _loadSize - 1
                            && chunk.Value.chunkIndex.X > _centreChunk.X - _loadSize + 1
                            && chunk.Value.chunkIndex.Y < _centreChunk.Y + _loadSize - 1
                            && chunk.Value.chunkIndex.Y > _centreChunk.Y - _loadSize + 1)
                        { 
                            //render chunk
                            chunk.Value.LoadMesh(this); 
                            //if 1/4 chance achieved, spawn actor
                            if (new Random().Next(0, 4) == 0)
                            {
                                PhysicsBase physBase = new PhysicsBase(new Vector3(chunk.Value.GetAbsolutePos().X, 60, chunk.Value.GetAbsolutePos().Z), 1, 1);
                                physBase.LoadCollider(IsCollisionBlockAt);
                                _actors.Add(new Actor(physBase, _player.camera, _actorModel, _actorTexture, _graphicsBase.Graphics));
                            }
                            break;
                        }
                    }
                }
            }
        } 
    }
}
