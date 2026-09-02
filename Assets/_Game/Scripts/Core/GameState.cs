namespace SmashFest.Core
{
    public enum GameState
    {
        /// <summary>
        /// The main menu. Every level starts here and every level returns here.
        /// </summary>
        Home = 0,

        Playing = 1,
        LevelComplete = 2,

        /// <summary>
        /// Balls are gone but the level is not lost yet, the player is offered a paid continue.
        /// </summary>
        OutOfBalls = 3,

        /// <summary>
        /// The continue was declined. This is the real loss, a life is spent here.
        /// </summary>
        LevelFailed = 4
    }
}
