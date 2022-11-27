using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIAFN.AI
{
    public class PatrolState : AIStateBase
    {
        private float _canSeeCheckTime;

        private const float canSeeCheckDeltaTime = 0.1f;
        public void OnEnter(AIController ai)
        {
            ai.NPCController.SetRelativeTarget(Random.insideUnitSphere * ai.maxPatrolDistance);
            _canSeeCheckTime = 0f;
        }

        public void OnUpdate(AIController ai)
        {
            NPCController npc = ai.NPCController;

            _canSeeCheckTime += Time.deltaTime;

            if (_canSeeCheckTime > canSeeCheckDeltaTime)
            {
                _canSeeCheckTime = 0f;
                if (ai.AttackIfCanSeePlayer())
                {
                    return;
                }
            }

            if (npc.IsStopped)
            {
                ai.ChangeState(new IdleState());
            }
        }

        public void OnExit(AIController ai)
        {

        }
    }
}