
namespace OneCog.Io.Owl.Intuition.Device.State
{
    public interface IState
    {
        void OnEnter();

        void OnExit();

        Name Name { get; }
    }
}
