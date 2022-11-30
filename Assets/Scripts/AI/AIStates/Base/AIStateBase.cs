
namespace WIAFN.AI
{
    public interface AIStateBase
    {
        public void OnEnter(AIController ai);
        public void OnUpdate(AIController ai);
        public void OnExit(AIController ai);
    }

}