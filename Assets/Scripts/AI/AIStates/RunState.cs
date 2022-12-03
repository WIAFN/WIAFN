using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIAFN.AI
{
    public class RunState : ConditionalAIStateBase
    {
        public Character fromCharacter;

        private float _runTargetPosUpdateTimer;
        private float _runTargetPosUpdateCurrentDeltaTime;

        private const float runDistance = 60f;
        private const float runDirectionRandomize = 40f;
        private const float runTargetPosUpdateMaxDeltaTime = 0.6f;

        public RunState(Character fromCharacter)
        {
            this.fromCharacter = fromCharacter;
        }

        public void OnEnter(AIController ai)
        {
            //NPCControllerBase npc = ai.NPCController;
            SetRunTargetPos(ai);
            _runTargetPosUpdateTimer = 0f;
            _runTargetPosUpdateCurrentDeltaTime = Random.Range(runTargetPosUpdateMaxDeltaTime / 10f, runTargetPosUpdateMaxDeltaTime);
        }

        public void UpdateState(AIController ai)
        {
            if (!ai.CanFeelCharacter(fromCharacter))
            {
                ai.ChangeState(new IdleState());
            }
        }

        public void UpdateNPCBehaviour(AIController ai)
        {
            _runTargetPosUpdateTimer += Time.deltaTime;

            if (_runTargetPosUpdateTimer > _runTargetPosUpdateCurrentDeltaTime)
            {
                SetRunTargetPos(ai);
                _runTargetPosUpdateTimer = 0f;
                _runTargetPosUpdateCurrentDeltaTime = Random.Range(runTargetPosUpdateMaxDeltaTime / 10f, runTargetPosUpdateMaxDeltaTime);
            }
        }

        public void OnExit(AIController ai)
        {

        }

        public static bool CheckCondition(AIController ai)
        {
            NPCControllerBase npc = ai.NPCController;
            return npc.character.health < npc.character.BaseStats.maxHealth / 3f;
        }

        private void SetRunTargetPos(AIController ai)
        {
            NPCControllerBase npc = ai.NPCController;
            Vector3 runDirection = (npc.transform.position - fromCharacter.transform.position).normalized;
            runDirection = GetRandomizedRunDirectionQuaternion() * runDirection * Random.Range(runDistance / 10f, runDistance);

            npc.MoveToRelative(runDirection);
        }

        private static Quaternion GetRandomizedRunDirectionQuaternion()
        {
            return Quaternion.Euler(GetRandomizedRunDirectionAngle(), GetRandomizedRunDirectionAngle(), GetRandomizedRunDirectionAngle());
        }

        private static float GetRandomizedRunDirectionAngle()
        {
            return Random.Range(-runDirectionRandomize, runDirectionRandomize);
        }
    }
}