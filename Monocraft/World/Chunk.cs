using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


namespace Monocraft
{
    class Chunk
    { 
        //chunk position and dimensions
        private Vector2 _chunkIndex;
        private Vector3 _dimensions; 

        //how far in loading process is chunk
        private ChunkState _chunkState; 

        //noise generators for terrain
        private OpenSimplex2F _perlinGenerator;
        private NoiseMaker _noiseMaker;
        private Random _random; 

        //chunkdata
        private byte[,,,] _chunkData; 

        //handle graphics/models
        private GraphicsBase _graphicsBase;
        

        public Vector2 chunkIndex { get => _chunkIndex; set => _chunkIndex = value; }
        internal GraphicsBase graphicsBase { get => _graphicsBase; set => _graphicsBase = value; }
        public byte[,,,] chunkData { get => _chunkData; set => _chunkData = value; }
        public Vector3 dimensions { get => _dimensions; set => _dimensions = value; }
        public Random random { get => _random; set => _random = value; }
        internal ChunkState chunkState { get => _chunkState; set => _chunkState = value; }

        //new chunk constructor
        public Chunk(Vector3 dimensions, Vector2 chunkIndex, int worldSeed, GraphicsBase graphicsBase, NoiseMaker noiseMaker)
        { 
            //set properties
            _graphicsBase = graphicsBase; 
            _perlinGenerator = new OpenSimplex2F(worldSeed*5);
            _noiseMaker = noiseMaker;
            _chunkIndex = chunkIndex;
            _dimensions = new Vector3(dimensions.X, dimensions.Y, dimensions.Z);

            //generate unique chunk seed (based on world seed)
            int A = (int)chunkIndex.X >= 0 ? 2 * (int)chunkIndex.X : -2 * (int)chunkIndex.X - 1;
            int B = (int)chunkIndex.Y >= 0 ? 2 * (int)chunkIndex.Y : -2 * (int)chunkIndex.Y - 1; 
            //random no from 2 nos
            int seed = ((A + B) * (A + B + 1) / 2 + A) * worldSeed;
            _random = new Random(seed);

            //set data to newly generated data
            _chunkData = GenerateChunk(); 
            chunkState = ChunkState.loaded;
        }

        //constructor for loading chunk from premade chunk data
        public Chunk(Vector3 dimensions, Vector2 chunkIndex, int worldSeed, GraphicsBase graphicsBase, NoiseMaker noiseMaker, byte[,,,] chunkData)
        {
            _graphicsBase = graphicsBase;

            //generate unique seed
            int A = (int)chunkIndex.X >= 0 ? 2 * (int)chunkIndex.X : -2 * (int)chunkIndex.X - 1;
            int B = (int)chunkIndex.Y >= 0 ? 2 * (int)chunkIndex.Y : -2 * (int)chunkIndex.Y - 1;
            //random no from 2 nos
            int seed = ((A + B) * (A + B + 1) / 2 + A) * worldSeed; 

            _random = new Random(seed);
            _perlinGenerator = new OpenSimplex2F(worldSeed * 5);
            _noiseMaker = noiseMaker;
            _chunkIndex = chunkIndex;
            _dimensions = new Vector3(dimensions.X, dimensions.Y, dimensions.Z); 
            //set data to already existing data
            _chunkData = chunkData;
            chunkState = ChunkState.detailed;
        }

        public void Render()
        { 
            graphicsBase.RenderMesh();
        }
         
        //destroy any graphics items
        public void Destroy()
        {
            graphicsBase.Destroy();
            _chunkState = ChunkState.loaded;
        }

        
        public void LoadMesh(World world)
        {
            //create mesh for chunk and load these verticies into graphicsbase
            graphicsBase.LoadVertices(GenerateMesh(chunkData, world));
            chunkState = ChunkState.meshed;
        }

        //search each block for tree marker and build structure at that location
        public void LoadDetail(World world)
        {
            for (int x = 0; x < (int)dimensions.X; x++)
            {
                for (int z = 0; z < (int)dimensions.Z; z++)
                {
                    if (chunkData[x, 0, z, 0] == 20)
                    {
                        world.GenTree(new Vector2(x, z) + GetAbsolutePos2(), chunkData[x, 0, z, 1]);
                    }
                }
            }         
            _chunkState = ChunkState.detailed;
        }

        //generate an array of verticies (the mesh) based on visible block faces in the chunk
        private VertexPositionNormalTexture[] GenerateMesh(byte[,,,] chunkData, World world)
        { 
            //function that will tell if block at location
            Func<Vector3, bool> GetBlock; 
            //array of all block vertices
            ArrayList chunkBlocks = new ArrayList();
            for (int y = 0; y < (int)_dimensions.Y; y++)
            {
                for (int z = 0; z < (int)_dimensions.Z; z++)
                {
                    for (int x = 0; x < (int)_dimensions.X; x++)
                    { 
                        //if block is not air
                        if (chunkData[x, y, z, 0] != 0)
                        { 
                            //run mesh code if block is not air
                            GetBlock = world.IsCollisionBlockAt;
                            if (chunkData[x, y, z, 0] == 5)
                            { 
                                //if block is water, change checkblock to check solid block so adjacent water blocks are not rendered when submerged
                                GetBlock = world.IsBlockAt;
                            }
                            //if air block is above OR water is above (but current block isnt water), add block face to verticies
                            if (y != _dimensions.Y - 1 && (chunkData[x, y + 1, z, 0] == 0 || (chunkData[x, y + 1, z, 0] == 5 && chunkData[x, y, z, 0] != 5)))
                            { 
                                //get coords of textures for specific faces
                                Vector2[] texCoords = Utility.GetTexCoord(chunkData[x, y, z, 0], Face.top, chunkData[x, y, z, 1]);
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y, z + 1), Vector3.UnitY, texCoords[0]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y, z + 1), Vector3.UnitY, texCoords[1]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y, z), Vector3.UnitY, texCoords[2]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y, z + 1), Vector3.UnitY, texCoords[3]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y, z), Vector3.UnitY, texCoords[4]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y, z), Vector3.UnitY, texCoords[5]));
                                //if this block is water, and above is air add water face facing DOWN for when submerged
                                if (chunkData[x, y, z, 0] == 5)
                                { 
                                    chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y, z + 1), -Vector3.UnitY, texCoords[0]));
                                    chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y, z + 1), -Vector3.UnitY, texCoords[1]));
                                    chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y, z), -Vector3.UnitY, texCoords[2]));
                                    chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y, z + 1), -Vector3.UnitY, texCoords[3]));
                                    chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y, z), -Vector3.UnitY, texCoords[4]));
                                    chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y, z), -Vector3.UnitY, texCoords[5]));
                                }

                            }
                            //if air block is below, render bottom face of block
                            if (y != 0 && (chunkData[x, y - 1, z, 0] == 0 || (chunkData[x, y + 1, z, 0] == 5 && chunkData[x, y, z, 0] != 5)))
                            {
                                //get coords of textures for specific faces
                                Vector2[] texCoords = Utility.GetTexCoord(chunkData[x, y, z, 0], Face.down, chunkData[x, y, z, 1]);
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y - 1, z), -Vector3.UnitY, texCoords[0]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y - 1, z), -Vector3.UnitY, texCoords[1]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y - 1, z + 1), -Vector3.UnitY, texCoords[2]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y - 1, z), -Vector3.UnitY, texCoords[3]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y - 1, z + 1), -Vector3.UnitY, texCoords[4]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y - 1, z + 1), -Vector3.UnitY, texCoords[5]));
                            } 

                            //Render corresponding side of block if adjacent block is air
                            if (!GetBlock(GetAbsolutePos() + new Vector3(x + 1, y, z)))
                            {
                                //get coords of textures for specific faces
                                Vector2[] texCoords = Utility.GetTexCoord(chunkData[x, y, z, 0], Face.side, chunkData[x, y, z, 1]);
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y, z + 1), -Vector3.UnitX, texCoords[0]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y, z), -Vector3.UnitX, texCoords[1]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y - 1, z + 1), -Vector3.UnitX, texCoords[2]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y, z), -Vector3.UnitX, texCoords[3]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y - 1, z), -Vector3.UnitX, texCoords[4]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y - 1, z + 1), -Vector3.UnitX, texCoords[5]));
                            }
                            if (!GetBlock(GetAbsolutePos() + new Vector3(x - 1, y, z)))
                            {
                                //get coords of textures for specific faces
                                Vector2[] texCoords = Utility.GetTexCoord(chunkData[x, y, z, 0], Face.side, chunkData[x, y, z, 1]);
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y, z), Vector3.UnitX, texCoords[0]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y, z + 1), Vector3.UnitX, texCoords[1]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y - 1, z), Vector3.UnitX, texCoords[2]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y, z + 1), Vector3.UnitX, texCoords[3]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y - 1, z + 1), Vector3.UnitX, texCoords[4]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y - 1, z), Vector3.UnitX, texCoords[5]));
                            }
                            if (!GetBlock(GetAbsolutePos() + new Vector3(x, y, z + 1)))
                            {
                                //get coords of textures for specific faces
                                Vector2[] texCoords = Utility.GetTexCoord(chunkData[x, y, z, 0], Face.side, chunkData[x, y, z, 1]);
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y, z + 1), -Vector3.UnitZ, texCoords[0]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y, z + 1), -Vector3.UnitZ, texCoords[1]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y - 1, z + 1), -Vector3.UnitZ, texCoords[2]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y, z + 1), -Vector3.UnitZ, texCoords[3]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y - 1, z + 1), -Vector3.UnitZ, texCoords[4]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y - 1, z + 1), -Vector3.UnitZ, texCoords[5]));
                            }
                            if (!GetBlock(GetAbsolutePos() + new Vector3(x, y, z - 1)))
                            {
                                //get coords of textures for specific faces
                                Vector2[] texCoords = Utility.GetTexCoord(chunkData[x, y, z, 0], Face.side, chunkData[x, y, z, 1]);
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y, z), Vector3.UnitZ, texCoords[0]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y, z), Vector3.UnitZ, texCoords[1]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y - 1, z), Vector3.UnitZ, texCoords[2]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y, z), Vector3.UnitZ, texCoords[3]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x, y - 1, z), Vector3.UnitZ, texCoords[4]));
                                chunkBlocks.Add(new VertexPositionNormalTexture(GetAbsolutePos() + new Vector3(x + 1, y - 1, z), Vector3.UnitZ, texCoords[5]));
                            }

                        }
                    }
                }
            } 
            //convert vertex list to fixed array and return
            VertexPositionNormalTexture[] vertices = chunkBlocks.Cast<VertexPositionNormalTexture>().ToArray();
            return vertices;
        }

        private byte[,,,] GenerateChunk()
        { 
            //setup new 4D array of blocks in chunk
            byte[,,,] chunkBlocks = new byte[(int)_dimensions.X, (int)_dimensions.Y, (int)_dimensions.Z, 2]; 
            //set origin of chunk to solid block
            chunkBlocks[0, 0, 0, 0] = 2; 
            //loop through x and z of chunk
            for (int x = 0; x < (int)dimensions.X; x++)
            {
                for (int z = 0; z < (int)dimensions.Z; z++)
                { 
                    //Get biome of current x, z position
                    int code = GetBiomeID(x, z);
                    int waterlevel = 11;
                    //generate differently, according to biome
                    switch (code)
                    {
                        //desert
                        case (1):
                            //get height of this block column
                            int level = 15 + (int)((((_perlinGenerator.Noise2((GetAbsolutePos().X + x) / (double)100, (GetAbsolutePos().Z + z) / (double)100) + 1) * 2)
                                * ((_perlinGenerator.Noise2((GetAbsolutePos().X + x) / (double)100, (GetAbsolutePos().Z + z) / (double)100) + 1) * 2)));
                            //loop through blocks in column
                            for (int y = 0; y < level; y++)
                            { 
                                //if at level, set to sand
                                if (y == level - 1)
                                {
                                    chunkBlocks[x, y, z, 0] = 9;
                                }
                                else
                                { 
                                    //if underground, set to stone
                                    chunkBlocks[x, y, z, 0] = 1; 
                                    //set to random variation of stone (ores)
                                    switch (_random.Next(0, 50))
                                    {
                                        case (1):
                                            chunkBlocks[x, y, z, 1] = 1;
                                            break;
                                        case (2):
                                            chunkBlocks[x, y, z, 1] = 2;
                                            break;
                                        case (3):
                                            chunkBlocks[x, y, z, 1] = 3;
                                            break;
                                        case (4):
                                            chunkBlocks[x, y, z, 1] = 4;
                                            break;
                                    }
                                }
                            }
                            if (level <= waterlevel)
                            { 
                                //if below waterlevel, set above to water
                                for (int i = waterlevel; i >= level; i--)
                                {
                                    chunkBlocks[x, i, z, 0] = 5; 
                                    //set to random variation of water (orientation)
                                    switch (_random.Next(0, 20))
                                    {
                                        case (1):
                                            chunkBlocks[x, i, z, 1] = 1;
                                            break;
                                        case (2):
                                            chunkBlocks[x, i, z, 1] = 2;
                                            break;
                                        case (3):
                                            chunkBlocks[x, i, z, 1] = 3;
                                            break;
                                        case (4):
                                            chunkBlocks[x, i, z, 1] = 4;
                                            break;
                                    }
                                }
                            }
                            else
                            { 
                                //if seeded chance, place tree marker
                                if (_random.Next(0, 100) == 0)
                                {
                                    chunkBlocks[x, 0, z, 0] = 20;
                                    chunkBlocks[x, 0, z, 1] = 1;

                                    chunkBlocks[x, level, z, 0] = 20;
                                }
                            }
                            break;
                        //plains
                        case (2): 
                            //set height of block column
                            int level2 = 11 + (int)(((Math.Abs(_perlinGenerator.Noise2((GetAbsolutePos().X + x) / (double)85, (GetAbsolutePos().Z + z) / (double)85)) * 5)
                                * ((_perlinGenerator.Noise2((GetAbsolutePos().X + x) / (double)100, (GetAbsolutePos().Z + z) / (double)100) + 1) * 2)));
                            //loop through all blocks in column
                            for (int y = 0; y < level2; y++)
                            { 
                                //if at level, set to grass
                                if (y == level2 - 1)
                                {
                                    chunkBlocks[x, y, z, 0] = 4; 
                                    //set to random variation of grass
                                    switch (_random.Next(0, 30))
                                    {
                                        case (1):
                                            chunkBlocks[x, y, z, 1] = 1;
                                            break;
                                        case (2):
                                            chunkBlocks[x, y, z, 1] = 2;
                                            break;
                                        case (3):
                                            chunkBlocks[x, y, z, 1] = 3;
                                            break;
                                        case (4):
                                            chunkBlocks[x, y, z, 1] = 4;
                                            break;
                                    }
                                } 
                                //if just under surface, set to dirt
                                else if (y >= level2 - 3)
                                {
                                    chunkBlocks[x, y, z, 0] = 3;
                                }
                                else
                                {
                                    //otherwise make stone
                                    chunkBlocks[x, y, z, 0] = 1; 
                                    //set to random stone variation (ores)
                                    switch (_random.Next(0, 50))
                                    {
                                        case (1):
                                            chunkBlocks[x, y, z, 1] = 1;
                                            break;
                                        case (2):
                                            chunkBlocks[x, y, z, 1] = 2;
                                            break;
                                        case (3):
                                            chunkBlocks[x, y, z, 1] = 3;
                                            break;
                                        case (4):
                                            chunkBlocks[x, y, z, 1] = 4;
                                            break;
                                    }

                                }
                            } 
                            //if ground level is below the water level, make water
                            if (level2 <= waterlevel)
                            {
                                for (int i = waterlevel; i >= level2; i--)
                                {
                                    chunkBlocks[x, i, z, 0] = 5; 
                                    //set to random water variation (orientation)
                                    switch (_random.Next(0, 30))
                                    {
                                        case (1):
                                            chunkBlocks[x, i, z, 1] = 1;
                                            break;
                                        case (2):
                                            chunkBlocks[x, i, z, 1] = 2;
                                            break;
                                        case (3):
                                            chunkBlocks[x, i, z, 1] = 3;
                                            break;
                                        case (4):
                                            chunkBlocks[x, i, z, 1] = 4;
                                            break;
                                    }
                                }
                            }
                            else
                            { 
                                //if seeded chance is achieved, place tree marker
                                if (_random.Next(0, 100) == 0)
                                {
                                    chunkBlocks[x, 0, z, 0] = 20;
                                    chunkBlocks[x, 0, z, 1] = 2;
                                    chunkBlocks[x, level2, z, 0] = 20;
                                }
                                //if seeded chance is achieved, place house marker
                                else if (_random.Next(0, 25000) == 0)
                                {
                                    chunkBlocks[x, 0, z, 0] = 20;
                                    chunkBlocks[x, 0, z, 1] = 3;
                                    chunkBlocks[x, level2, z, 0] = 20;
                                }
                            }
                            break;
                        //snow hills
                        case (3):
                            //set to height og block column
                            int level3 = (int)((((_perlinGenerator.Noise2((GetAbsolutePos().X + x) / (double)50, (GetAbsolutePos().Z + z) / (double)50) + 1) * 6)
                                * ((_perlinGenerator.Noise2((GetAbsolutePos().X + x) / (double)100, (GetAbsolutePos().Z + z) / (double)100) + 1) * 2)));
                            //if level is below waterlevel, set to water
                            if (level3 <= waterlevel)
                            {
                                for (int i = waterlevel; i >= level3; i--)
                                {
                                    chunkBlocks[x, i, z, 0] = 5; 
                                    //set to random water variation (orientation)
                                    switch (_random.Next(0, 30))
                                    {
                                        case (1):
                                            chunkBlocks[x, i, z, 1] = 1;
                                            break;
                                        case (2):
                                            chunkBlocks[x, i, z, 1] = 2;
                                            break;
                                        case (3):
                                            chunkBlocks[x, i, z, 1] = 3;
                                            break;
                                        case (4):
                                            chunkBlocks[x, i, z, 1] = 4;
                                            break;
                                    }
                                }
                            } 
                            //loop through all blocks in column
                            for (int y = 0; y < level3; y++)
                            { 
                                //if at level, set to snow
                                if (y == level3 - 1)
                                {
                                    chunkBlocks[x, y, z, 0] = 10;
                                } 
                                //if just under surface, set to dirt
                                else if (y >= level3 - 3)
                                {
                                    chunkBlocks[x, y, z, 0] = 3;
                                } 
                                //otherwise, set to stone
                                else
                                {
                                    chunkBlocks[x, y, z, 0] = 1; 
                                    //set to randome stone variation (ores)
                                    switch (_random.Next(0, 50))
                                    {
                                        case (1):
                                            chunkBlocks[x, y, z, 1] = 1;
                                            break;
                                        case (2):
                                            chunkBlocks[x, y, z, 1] = 2;
                                            break;
                                        case (3):
                                            chunkBlocks[x, y, z, 1] = 3;
                                            break;
                                        case (4):
                                            chunkBlocks[x, y, z, 1] = 4;
                                            break;
                                    }
                                }
                            }
                            break;
                        //mountains
                        case (4): 
                            //get height of block column
                            int level4 = 10 + (int)((((_perlinGenerator.Noise2((GetAbsolutePos().X + x) / (double)50, (GetAbsolutePos().Z + z) / (double)50) + 1) * 11)
                                * ((_perlinGenerator.Noise2((GetAbsolutePos().X + x) / (double)100, (GetAbsolutePos().Z + z) / (double)100) + 1) * 2)));
                            //if below water level, set to water
                            if (level4 <= waterlevel)
                            { 
                                for (int i = waterlevel; i >= level4; i--)
                                {
                                    chunkBlocks[x, i, z, 0] = 5; 
                                    //set to random water variation (orientation)
                                    switch (_random.Next(0, 30))
                                    {
                                        case (1):
                                            chunkBlocks[x, i, z, 1] = 1;
                                            break;
                                        case (2):
                                            chunkBlocks[x, i, z, 1] = 2;
                                            break;
                                        case (3):
                                            chunkBlocks[x, i, z, 1] = 3;
                                            break;
                                        case (4):
                                            chunkBlocks[x, i, z, 1] = 4;
                                            break;
                                    }
                                }
                            } 
                            //loop through blocks in column
                            for (int y = 0; y < level4; y++)
                            { 
                                //if above snow level, set to snow
                                if (y >= 50)
                                {
                                    chunkBlocks[x, y, z, 0] = 15;
                                } 
                                //if at top, set to stone
                                else if (y >= level4 - 1)
                                {
                                    chunkBlocks[x, y, z, 0] = 1; 
                                    //set to random stone variation (ores)
                                    switch (_random.Next(0, 50))
                                    {
                                        case (1):
                                            chunkBlocks[x, y, z, 1] = 1;
                                            break;
                                        case (2):
                                            chunkBlocks[x, y, z, 1] = 2;
                                            break;
                                        case (3):
                                            chunkBlocks[x, y, z, 1] = 3;
                                            break;
                                        case (4):
                                            chunkBlocks[x, y, z, 1] = 4;
                                            break;
                                    }
                                }
                                else
                                { 
                                    //set underground to stone
                                    chunkBlocks[x, y, z, 0] = 1; 
                                    //set to random stone variation (ores)
                                    switch (_random.Next(0, 50))
                                    {
                                        case (1):
                                            chunkBlocks[x, y, z, 1] = 1;
                                            break;
                                        case (2):
                                            chunkBlocks[x, y, z, 1] = 2;
                                            break;
                                        case (3):
                                            chunkBlocks[x, y, z, 1] = 3;
                                            break;
                                        case (4):
                                            chunkBlocks[x, y, z, 1] = 4;
                                            break;
                                    }
                                }
                            }
                            break;
                    } 
                    //set unbreakable bedrock layer
                    chunkBlocks[x, 1, z, 0] = 12;
                }
            }           
            return chunkBlocks;
        } 

        //Calculate biome of block based on comparing noise maps
        private int GetBiomeID(int x, int z)
        {
            int code = 0; 
            //if bellow heightmap1
            if (_noiseMaker.Tgen.Noise2((GetAbsolutePos().X + x) / (double)200, (GetAbsolutePos().Z + z) / (double)200) + 1 <= 1.4f)
            {
                //if bellow heightmap2
                if (_noiseMaker.Hgen.Noise2((GetAbsolutePos().X + x) / (double)200, (GetAbsolutePos().Z + z) / (double)200) + 1 <= 1.3f)
                { 
                    //plains biome
                    code = 2;
                }
                else
                { 
                    //desert biome
                    code = 1;
                }
            }
            else
            {
                //if bellow heightmap2 1.5
                if (_noiseMaker.Hgen.Noise2((GetAbsolutePos().X + x) / (double)200, (GetAbsolutePos().Z + z) / (double)200) + 1 <= 1.5)
                { 
                    //snow biome
                    code = 3;
                }
                else
                { 
                    //mountain biome
                    code = 4;
                }
            }
            return code;
        }
        //Get position of origin of chunk
        public Vector3 GetAbsolutePos()
        { 
            //multiply position by chunksize
            Vector3 position = new Vector3(_chunkIndex.X * _dimensions.X, 0, _chunkIndex.Y * _dimensions.Z);
            return position;
        }
        //Get vector 2 position of origin of chunk
        public Vector2 GetAbsolutePos2()
        {
            //multiply position by chunksize
            Vector2 position = new Vector2(_chunkIndex.X * _dimensions.X, _chunkIndex.Y * _dimensions.Z);
            return position;
        }

    }
}
