
namespace WIAFN.AI
{
    public interface AIStateBase
    {
        public void OnEnter(AIController ai);

        public void UpdateState(AIController ai);
        public void UpdateNPCBehaviour(AIController ai);
        public void OnExit(AIController ai);
    }

}