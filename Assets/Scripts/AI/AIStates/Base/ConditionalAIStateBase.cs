
using System.Diagnostics;

namespace WIAFN.AI
{
    public interface ConditionalAIStateBase: AIStateBase
    {
        public static bool CheckCondition(AIController ai)
        {
            Debug.Fail("ConditionalAIStateBase.CheckCondition is called.");
            return true;
        }
    }

}