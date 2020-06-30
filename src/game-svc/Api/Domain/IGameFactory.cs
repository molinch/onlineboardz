namespace Api.Domain
{
    public interface IGameFactory
    {
        Game Create(GameType type);
    }
}