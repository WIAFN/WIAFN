using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIAFN.AI
{
    public class AttackState : AIStateBase
    {
        public Character target;

        private float _canSeeCheckTime;

        private const float canSeeCheckDeltaTime = 0.5f;

        public AttackState(Character target)
        {
            this.target = target;
        }

        public void OnEnter(AIController ai)
        {
            ai.NPCController.SetTarget(target);
            _canSeeCheckTime = 0f;
        }

        public void OnUpdate(AIController ai)
        {
            NPCController npc = ai.NPCController;

            if (RunState.CheckCondition(ai))
            {
                ai.ChangeState(new RunState(target));
                return;
            }

            _canSeeCheckTime += Time.deltaTime;
            if (_canSeeCheckTime > canSeeCheckDeltaTime)
            {
                if (!ai.CanSeeCharacter(target))
                {
                    ai.ChangeState(new SearchState(target.transform.position, target));
                }
                _canSeeCheckTime = 0f;
            }
        }

        public void OnExit(AIController ai)
        {

        }
    }
}