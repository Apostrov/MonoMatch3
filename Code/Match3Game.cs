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

        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private EcsWorld _world;
        private IEcsSystems _gameSystems;
        private IEcsSystems _drawSystems;
        private readonly SharedData _sharedData;

        public Match3Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1366;
            _graphics.PreferredBackBufferHeight = 900;
            _graphics.ApplyChanges();

            _sharedData = new SharedData
            {
                GraphicsDevice = GraphicsDevice,
                Tweener = new Tweener(),
                BoardSize = BOARD_SIZE
            };
            EcsInit();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            CreateGameSystems();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _sharedData.SpriteBatch = _spriteBatch;
            _sharedData.Background = Content.Load<Texture2D>("background_blur");
            _sharedData.TilesAtlas = Content.Load<Texture2D>("assets_candy");
            _sharedData.Font = Content.Load<SpriteFont>("Debug");

            CreateDrawSystems();
        }

        protected override void UnloadContent()
        {
            EcsClean();
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            _sharedData.GameTime = gameTime;
            _gameSystems?.Run();
            _sharedData.Tweener.Update(gameTime.GetElapsedSeconds());

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            _sharedData.GameTime = gameTime;
            _drawSystems?.Run();

            base.Draw(gameTime);
        }

        private void EcsInit()
        {
            _world = new EcsWorld();
            _gameSystems = new EcsSystems(_world, _sharedData);
            _drawSystems = new EcsSystems(_world, _sharedData);
        }

        private void CreateGameSystems()
        {
            _gameSystems
                // game logic
                .Add(new GameLogic.Systems.GameInit())
                .Add(new GameLogic.Systems.Match3Solver())
                .Add(new GameLogic.Systems.SwapWithoutMatchTracker())
                .Add(new GameLogic.Systems.SwapPiecesProcessing())
                .Add(new GameLogic.Systems.BonusSpawner())
                .Add(new GameLogic.Systems.PlayerClickedProcessing())
                .Add(new GameLogic.Systems.RearrangeBoardProcessing())
                .Add(new GameLogic.Systems.FillBoardProcessing())
                .Add(new GameLogic.Systems.GamePieceDestroyer())
                .Add(new GameLogic.Systems.LineMatchProcessing())
                .Add(new GameLogic.Systems.LineDestroyerFlyProcessing())
                .Add(new GameLogic.Systems.BombMatchProcessing())
                .Add(new GameLogic.Systems.GameTimeDecreaser())

                // ui
                .Add(new UI.Systems.PlayButtonClickedTracker())
                .Add(new UI.Systems.RestartButtonClickedTracker(EcsRestart))

                // init
                .Inject()
                .Init();
        }

        private void CreateDrawSystems()
        {
            _drawSystems
                // game logic
                .Add(new GameLogic.Systems.GameBoardInit()) // in draw systems, because it depend on content

                // draw logic
                .Add(new DrawLogic.Systems.BackgroundDrawer())
                .Add(new DrawLogic.Systems.GameBoardDrawer())
                .Add(new DrawLogic.Systems.GamePieceDrawer())
                .Add(new DrawLogic.Systems.LineDestroyerDrawer())

                // ui
                .Add(new UI.Systems.MenuUIDrawer())
                .Add(new UI.Systems.GameplayUIDrawer())
                .Add(new UI.Systems.GameEndUIDrawer())

                // init
                .Inject()
                .Init();
        }

        private void EcsClean()
        {
            _gameSystems?.Destroy();
            _gameSystems = null;
            _drawSystems?.Destroy();
            _drawSystems = null;
            _world?.Destroy();
            _world = null;
        }

        private void EcsRestart()
        {
            _sharedData.Tweener.CancelAll();
            EcsClean();
            EcsInit();
            CreateGameSystems();
            CreateDrawSystems();
        }
    }
}