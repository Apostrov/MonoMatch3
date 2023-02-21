using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tweening;

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
            _graphics.PreferredBackBufferWidth = 1366;
            _graphics.PreferredBackBufferHeight = 900;
            _graphics.ApplyChanges();

            _world = new EcsWorld();
            _sharedData = new SharedData
            {
                GraphicsDevice = GraphicsDevice,
                Tweener = new Tweener(),
                BoardSize = BOARD_SIZE
            };
            _gameSystems = new EcsSystems(_world, _sharedData);
            _drawSystems = new EcsSystems(_world, _sharedData);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            _gameSystems
                .Add(new GameLogic.Systems.Match3Solver())
                .Add(new GameLogic.Systems.PlayerClickedProcessing())
                .Add(new GameLogic.Systems.SwapPiecesProcessing())
                .Add(new GameLogic.Systems.RearrangeBoardProcessing())
                .Add(new GameLogic.Systems.FillBoardProcessing())
                .Add(new GameLogic.Systems.GamePieceDestroyer())
                .Inject()
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
            _sharedData.DebugFont = Content.Load<SpriteFont>("Debug");

            _drawSystems
                .Add(new GameLogic.Systems.GameBoardInit()) // in draw systems, because it depend on content
                .Add(new DrawLogic.Systems.BackgroundDrawer())
                .Add(new DrawLogic.Systems.GameBoardDrawer())
                .Add(new DrawLogic.Systems.GamePieceDrawer())
                .Inject()
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
            _sharedData.GameTime = gameTime;
            _gameSystems.Run();
            _sharedData.Tweener.Update(gameTime.GetElapsedSeconds());

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            _sharedData.GameTime = gameTime;
            _drawSystems.Run();

            base.Draw(gameTime);
        }
    }
}