using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIAFN.AI
{
    public class IdleState : AIStateBase
    {
        private float _chosenIdleTime;
        private float _passedTime;

        private float _canSeeCheckTime;

        private const float canSeeCheckDeltaTime = 0.1f;
        public void OnEnter(AIController ai)
        {
            ai.NPCController.StopMoving();
            _passedTime = 0f;
            _canSeeCheckTime = 0f;
            _chosenIdleTime = Random.Range(0f, ai.maxIdleTime);
        }

        public void OnUpdate(AIController ai)
        {
            _passedTime += Time.deltaTime;
            _canSeeCheckTime += Time.deltaTime;

            if (_canSeeCheckTime > canSeeCheckDeltaTime)
            {
                _canSeeCheckTime = 0f;
                if (ai.AttackIfCanSeePlayer())
                {
                    return;
                }
            }

            if (_passedTime >= _chosenIdleTime)
            {
                ai.ChangeState(new PatrolState());
            }
        }

        public void OnExit(AIController ai)
        {

        }
    }
}
