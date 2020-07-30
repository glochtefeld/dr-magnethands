namespace Magnethands.Game
{
    /// <summary>
    /// Potentially unused: this specifies the global state of the game.
    /// </summary>
    public enum State
    {
        MainMenu,
        CConnectedSending,
        CConnectedAwaiting,
        CInGame,
        CSacrifice,
        CDead,
        CGameEnd,
        HSuggestAllowNew,
        HSuggestNoNew,
        HAwaitAllowNew,
        HAwaitNoNew,
        HInit,
        HInGame,
        HEndGame
    }
}