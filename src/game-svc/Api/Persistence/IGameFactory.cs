namespace Api.Persistence
{
    public interface IGameFactory
    {
        Game Create(GameType type);
    }
}