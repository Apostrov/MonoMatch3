namespace MonoMatch3.Code;

public static class GameConfig
{
    // draw logic
    public const float SWAP_ANIMATION_TIME = 0.25f;
    public const float SELECTED_ANIMATION_SHRINK = 0.6f;
    public const float SELECTED_ANIMATION_TIME = 0.75f;
    public const float DESTROY_ANIMATION_TIME = 0.35f;
    public const float REARRANGE_ANIMATION_TIME = 0.3f;

    // game logic
    public const int MATCH_COUNT = 3;
    public const int LINE_BONUS_COUNT = 4;
    public const float LINE_DESTROYER_FLY_SPEED = 800f;
    public const int BOMB_BONUS_COUNT = 5;
    public const int BOMB_RADIUS = 1;
    public const float GAME_TIME = 60f;
}