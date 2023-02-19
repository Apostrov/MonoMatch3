using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoMatch3.Code
{
    public class Match3Game : Game
    {
        private const int BOARD_SIZE = 8;
        
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private readonly EcsWorld _world;
        private readonly IEcsSystems _gameSystems;
        private readonly IEcsSystems _drawSystems;
        private readonly SharedData _sharedData;

        public Match3Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            _world = new EcsWorld();
            _sharedData = new SharedData
            {
                GraphicsDevice = GraphicsDevice
            };
            _gameSystems = new EcsSystems(_world, _sharedData);
            _drawSystems = new EcsSystems(_world, _sharedData);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            _gameSystems
                .Add(new GameLogic.Systems.GameBoardInit(BOARD_SIZE))
                .Init();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _sharedData.SpriteBatch = _spriteBatch;
            _sharedData.Background = Content.Load<Texture2D>("background_blur");
            _sharedData.TilesAtlas = Content.Load<Texture2D>("assets_candy");
            
            _drawSystems
                .Add(new DrawLogic.Systems.BackgroundDrawer())
                .Add(new DrawLogic.Systems.GameBoardDrawer())
                .Init();
        }

        protected override void UnloadContent()
        {
            _gameSystems.Destroy();
            _drawSystems.Destroy();
            _world.Destroy();

            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            _gameSystems.Run();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            _drawSystems.Run();

            base.Draw(gameTime);
        }
    }
}