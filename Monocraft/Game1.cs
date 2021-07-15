using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Monocraft
{
    public class Game1 : Game
    {
        //all visual and updateable objects 
        List<IDrawable> visualObjects;
        List<IUpdateable> updateableObjects; 

        //required game assets
        Texture2D textureSheet;
        Texture2D controlsSheet;//t
        private Model actorModel;
        private Texture2D actorTexture; 
        SpriteFont defaultFont;  

        //graphics objects
        SpriteBatch spriteBatch;
        private readonly GraphicsDeviceManager _graphics; 

        //game states
        GameState gameState;
        GameState lastState; 

        //file to load
        string loadFile; 

        //player, world objects
        Player tido;
        World middleWorld; 

        //seed entry and control input
        NumberEntry seedEntry;
        bool backPressedLastFrame;

        public Game1()
        { 
            //set up screen resolution (full screen)
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = (int)(1080),
                PreferredBackBufferWidth = (int)(1920),
            };
            _graphics.IsFullScreen = false;
            IsMouseVisible = true;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        { 
            gameState = GameState.opening; 
            //init lists of game objects for game loop
            visualObjects = new List<IDrawable>();
            updateableObjects = new List<IUpdateable>(); 
            //spritebatch for drawing 2D items (text etc)
            spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);
            backPressedLastFrame = false;
            base.Initialize();
        }

        protected override void LoadContent()
        { 
            //Set up font for all text
            defaultFont = Content.Load<SpriteFont>("defaultFont"); 
            //load chicken's data
            actorModel = Content.Load<Model>("source/chicken");
            actorTexture = Content.Load<Texture2D>("textures/chicken"); 
            //load info screen
            controlsSheet = Content.Load<Texture2D>("ControlSheet");
        }

        protected override void Update(GameTime gameTime)
        { 
            //exit on escape
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            //switch game state process
            SwitchGameState();
            if (CheckGameStateChanged())
            {
                InitState();
            }

            switch (gameState)
            {
                //unique code to run on gamestates
                case GameState.title: 
                    break;
                case GameState.createWorld: 
                    break;
                case GameState.playing:
                    //get file being saved to
                    string fileNo = "0"; 
                    //if save-key pressed, select file
                    if (Keyboard.GetState().IsKeyDown(Keys.F1))
                    {
                        fileNo = "1";
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.F2))
                    {
                        fileNo = "2";
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.F3))
                    {
                        fileNo = "3";
                    } 
                    //check user is saving
                    if (fileNo != "0")
                    { 
                        //delete existing file and replace with empty new one
                        File.Delete("save_file_" + fileNo + "_world.bin");
                        IFormatter formatter2 = new BinaryFormatter();
                        Stream stream2 = new FileStream("save_file_" + fileNo + "_world.bin", FileMode.Create, FileAccess.Write, FileShare.None);

                        //write world save data to file
                        formatter2.Serialize(stream2, middleWorld.GetWorldSave());
                        stream2.Close();
                        visualObjects.Add(new TextBox("Saved to file " + fileNo, new Vector2(_graphics.PreferredBackBufferWidth / 2 - 10 * 15, _graphics.PreferredBackBufferHeight / 2), 1, defaultFont, _graphics.GraphicsDevice, spriteBatch, 180));
                    }
                    
                    break;
                default:
                    break;
            }

            //update each game object
            foreach (var gameObject in updateableObjects)
            {
                gameObject.Update(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //draw each game object;
            foreach (var gameObject in visualObjects)
            {
                gameObject.Draw();
            }

            //run any unique drawing code
            switch (gameState)
            {
                case GameState.title:
                    break;
                case GameState.menu:
                    break;
                case GameState.playing: 
                    break;
                default:
                    break;
            }

            base.Draw(gameTime);
        }


        public void InitState()
        { 
            //deassign all visual / updateable objects
            visualObjects.Clear();
            updateableObjects.Clear();

            switch (gameState)
            {
                //check desire gamestate, load & init all gameobjects required for the game state
                case GameState.title: 
                    IsMouseVisible = true; 
                    //set up menu items
                    TextBox title = new TextBox("Monocraft", new Vector2(_graphics.PreferredBackBufferWidth / 2 - 9 * 10 * 4, _graphics.PreferredBackBufferHeight / 10), 4, defaultFont, _graphics.GraphicsDevice, spriteBatch);
                    Button begin = new Button("Begin", defaultFont, new Vector2(_graphics.PreferredBackBufferWidth / 2 - 100, _graphics.PreferredBackBufferHeight * 4 / 10), new Vector2(200, 60), _graphics.GraphicsDevice, spriteBatch, GoToMenu);

                    //assign visual and updateable objects
                    visualObjects.Add(title);
                    visualObjects.Add(begin);

                    updateableObjects.Add(begin);
                    break;

                case GameState.menu:
                    IsMouseVisible = true; 
                    //set up menu items
                    TextBox menuHeader = new TextBox("Main Menu", new Vector2(_graphics.PreferredBackBufferWidth / 2 - 9 * 10 * 4, _graphics.PreferredBackBufferHeight / 10), 4, defaultFont, _graphics.GraphicsDevice, spriteBatch);
                    Button newGame = new Button("New Game", defaultFont, new Vector2(_graphics.PreferredBackBufferWidth / 2 - 100, _graphics.PreferredBackBufferHeight * 4 / 10), new Vector2(200, 60), _graphics.GraphicsDevice, spriteBatch, NewGame);
                    Button loadGame = new Button("Load Game", defaultFont, new Vector2(_graphics.PreferredBackBufferWidth / 2 - 100, _graphics.PreferredBackBufferHeight * 6 / 10), new Vector2(200, 60), _graphics.GraphicsDevice, spriteBatch, GoToLoadWorld);
                    Button controls = new Button("Controls", defaultFont, new Vector2(_graphics.PreferredBackBufferWidth / 2 - 100, _graphics.PreferredBackBufferHeight * 8 / 10), new Vector2(200, 60), _graphics.GraphicsDevice, spriteBatch, GoToInfo);

                    //assign visual and updateable objects
                    visualObjects.Add(menuHeader);
                    visualObjects.Add(newGame);
                    visualObjects.Add(loadGame);
                    visualObjects.Add(controls);

                    updateableObjects.Add(newGame);
                    updateableObjects.Add(loadGame);
                    updateableObjects.Add(controls);
                    break;

                case GameState.createWorld:
                    //set up menu items
                    TextBox enterSeed = new TextBox("Enter Seed", new Vector2(_graphics.PreferredBackBufferWidth / 2 - 10 * 10 * 4, _graphics.PreferredBackBufferHeight / 10), 4, defaultFont, _graphics.GraphicsDevice, spriteBatch);
                    seedEntry = new NumberEntry(new Vector2(_graphics.PreferredBackBufferWidth / 2 - 400, _graphics.PreferredBackBufferHeight * 4 / 10), 2, defaultFont, GraphicsDevice, spriteBatch);

                    Button BeginGame = new Button("Create World", defaultFont, new Vector2(_graphics.PreferredBackBufferWidth / 2 - 100, _graphics.PreferredBackBufferHeight * 7 / 10), new Vector2(200, 60), _graphics.GraphicsDevice, spriteBatch, CreateWorld);

                    //assign visual and updateable objects
                    visualObjects.Add(enterSeed);
                    visualObjects.Add(seedEntry);
                    visualObjects.Add(BeginGame);

                    updateableObjects.Add(seedEntry);
                    updateableObjects.Add(BeginGame);
                    break;

                case GameState.saveSelect:
                    //set up menu items
                    TextBox loadHeader = new TextBox("Choose File", new Vector2(_graphics.PreferredBackBufferWidth / 2 - 11 * 10 * 4, _graphics.PreferredBackBufferHeight / 10), 4, defaultFont, _graphics.GraphicsDevice, spriteBatch);

                    Button File1 = new Button("File 1", defaultFont, new Vector2(_graphics.PreferredBackBufferWidth / 2 - 100, _graphics.PreferredBackBufferHeight * 4 / 10), new Vector2(200, 60), _graphics.GraphicsDevice, spriteBatch, SelectFile1ToLoad);
                    Button File2 = new Button("File 2", defaultFont, new Vector2(_graphics.PreferredBackBufferWidth / 2 - 100, _graphics.PreferredBackBufferHeight * 6 / 10), new Vector2(200, 60), _graphics.GraphicsDevice, spriteBatch, SelectFile2ToLoad);
                    Button File3 = new Button("File 3", defaultFont, new Vector2(_graphics.PreferredBackBufferWidth / 2 - 100, _graphics.PreferredBackBufferHeight * 8 / 10), new Vector2(200, 60), _graphics.GraphicsDevice, spriteBatch, SelectFile3ToLoad);

                    //assign visual and updateable objects
                    visualObjects.Add(loadHeader);
                    visualObjects.Add(File1);
                    visualObjects.Add(File2);
                    visualObjects.Add(File3);

                    updateableObjects.Add(File1);
                    updateableObjects.Add(File2);
                    updateableObjects.Add(File3);

                    break;

                case GameState.playing:

                    PhysicsBase physicsbase = new PhysicsBase(new Vector3(16, 60, 20), 0.1f, 1.5f, true);
                    tido = new Player(_graphics, physicsbase);
                    
                    //load up texture objects
                    textureSheet = Content.Load<Texture2D>(@"TextureSheet");

                    GraphicsBase graphicsBase = new GraphicsBase(_graphics, tido.camera);
                    graphicsBase.LoadContent(textureSheet);
                    
                    //load world from save, if it exists
                    if ((loadFile != null) && File.Exists(loadFile + "_world" + ".bin"))
                    {
                        IFormatter formatter2 = new BinaryFormatter();
                        Stream stream2 = new FileStream(loadFile + "_world" + ".bin", FileMode.Open, FileAccess.Read, FileShare.Read);
                        WorldSave ws = (WorldSave)formatter2.Deserialize(stream2);
                        stream2.Close();
                        middleWorld = new World(13, ws, graphicsBase, tido, actorModel, actorTexture);
                    } 
                    //create world from scratch if not
                    else
                    {
                        middleWorld = new World(13, 16, seedEntry.Seed, 100, graphicsBase, tido, actorModel, actorTexture);
                    }

                    //load collider functions from world object 
                    tido.physicsBase.LoadCollider(middleWorld.IsCollisionBlockAt);
                    tido.SubmersionCheck = middleWorld.CheckSubmerged;

                    //set up crosshair as shapes
                    Rect crosshair1 = new Rect(new Vector2(_graphics.PreferredBackBufferWidth / 2 - 2, _graphics.PreferredBackBufferHeight / 2 - 15), new Vector2(4, 30), Color.White, _graphics.GraphicsDevice, spriteBatch);
                    Rect crosshair2 = new Rect(new Vector2(_graphics.PreferredBackBufferWidth / 2 - 15, _graphics.PreferredBackBufferHeight / 2 - 2), new Vector2(30, 4), Color.White, _graphics.GraphicsDevice, spriteBatch);

                    //assign all visual and updateable objects
                    visualObjects.Add(middleWorld);
                    visualObjects.Add(tido);
                    visualObjects.Add(crosshair1);
                    visualObjects.Add(crosshair2);

                    updateableObjects.Add(middleWorld);
                    updateableObjects.Add(tido);

                    IsMouseVisible = false;
                    break;

                case GameState.infoScreen:
                    //set up menu items
                    Rect info = new Rect(new Vector2(0,0), new Vector2(1920, 1080), Color.White, _graphics.GraphicsDevice, spriteBatch, controlsSheet);

                    //assign as visual object
                    visualObjects.Add(info);
                    break;
                default:
                    break;
            } 
            //copyright text
            TextBox cc = new TextBox("EZA 2021", new Vector2(5, _graphics.PreferredBackBufferHeight - 40), 1, defaultFont, _graphics.GraphicsDevice, spriteBatch);
            visualObjects.Add(cc);
        }

        //functions to be loaded into buttons:
        public void GoToMenu()
        {
            gameState = GameState.menu;
        }

        public void GoToInfo()
        {
            gameState = GameState.infoScreen;
        }

        public void CreateWorld()
        {
            gameState = GameState.playing;
        }

        public void GoToLoadWorld()
        {
            gameState = GameState.saveSelect;
        }

        public void NewGame()
        {
            gameState = GameState.createWorld;
        } 
        
        public void SelectFile1ToLoad()
        {
            loadFile = "save_file_1";
            gameState = GameState.playing;
        }
        public void SelectFile2ToLoad()
        {
            loadFile = "save_file_2";
            gameState = GameState.playing;
        }
        public void SelectFile3ToLoad()
        {
            loadFile = "save_file_3";
            gameState = GameState.playing;
        }
        //end of button functions

        //perform any state-change logic
        public void SwitchGameState()
        {
            if (gameState == GameState.opening)
            {
                gameState = GameState.title;
                return;
            }
 
            if (!Keyboard.GetState().IsKeyDown(Keys.Back))
            { 
                //return to menu screen (on backspace release)
                if (backPressedLastFrame)
                {
                    gameState = GameState.menu;
                    IsMouseVisible = true;
                }
                backPressedLastFrame = false;
                return;
            }
            else
            {
                backPressedLastFrame = true;
            }
            
        } 

        //return true if gamestate has been changed - for gamestate logic
        public bool CheckGameStateChanged()
        { 
            if (lastState != gameState)
            {
                lastState = gameState;
                return true;
            }
            lastState = gameState;
            return false;
        }
    }
}

// Tom Lewis 2020 - 2021