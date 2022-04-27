namespace PathMaker.ngo
{
    /// <summary>
    /// Something that will handle player input while in the game.
    /// </summary>
    public interface IInGameInputHandler : IProvidable<IInGameInputHandler>
    {
        void OnPlayerInput(ulong playerId, TeamState state, ScoreType score, ulong shooterId);
    }

    public class InGameInputHandlerNoop : IInGameInputHandler
    {
        public void OnPlayerInput(ulong playerId, TeamState state, ScoreType score, ulong shooterId) { }
        public void OnReProvided(IInGameInputHandler previousProvider) { }
    }
}
