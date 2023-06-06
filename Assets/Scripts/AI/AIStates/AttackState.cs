using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIAFN.AI
{
    public class AttackState : AIStateBase
    {
        public Character target;

        private float _canSeeCheckTimer;
        private float _updateTargetPositionTimer;

        private float _rotationDirectionMultiplier;
        private float _cantSeeForSecondsStartTime;

        private const float canSeeCheckDeltaTime = 0.2f;
        private const float updateTargetPositionDeltaTime = 0.3f;
        private const float cancelAttackingIfCantSeeForSeconds = 0.8f;

        public AttackState(Character target)
        {
            this.target = target;
        }

        public void OnEnter(AIController ai)
        {
            NPCControllerBase npc = ai.NPCController;
            _canSeeCheckTimer = 0f;
            _updateTargetPositionTimer = 0f;
            _rotationDirectionMultiplier = 0f;
            _cantSeeForSecondsStartTime = 0f;


            ai.SetInCombat(true);
            npc.LookAt(target, true);

            var _baseStats = ai.GetComponent<CharacterBaseStats>();
            _baseStats.speedCoefficient = 1.1f;

        }

        public void UpdateState(AIController ai)
        {
            if (RunState.CheckCondition(ai))
            {
                ai.ChangeState(new RunState(target));
                return;
            }

            if (ai.NPCController.IsStuck)
            {
                Debug.Log("NPC is stuck!");
                ai.ChangeState(new SearchState(target.transform.position, target));
                return;
            }

            _canSeeCheckTimer += Time.deltaTime;
            if (_canSeeCheckTimer > canSeeCheckDeltaTime)
            {
                if (ai.CanSeeCharacter(target))
                {
                    _cantSeeForSecondsStartTime = -1f;
                }
                else
                {
                    if (_cantSeeForSecondsStartTime < 0f)
                    {
                        _cantSeeForSecondsStartTime = Time.realtimeSinceStartup;
                    }

                    if (Time.realtimeSinceStartup - _cantSeeForSecondsStartTime >= cancelAttackingIfCantSeeForSeconds)
                    {
                        ai.ChangeState(new SearchState(target.transform.position, target));
                    }
                }
                _canSeeCheckTimer = 0f;
            }
        }

        public void UpdateNPCBehaviour(AIController ai)
        {
            NPCControllerBase npc = ai.NPCController;
            float distanceToTarget = Vector3.Distance(npc.transform.position, target.transform.position);

            if (distanceToTarget < 20f) //buraya bir de health condition yazmak istiyurum
            {
                npc.TryAttack(target);

            }

            _updateTargetPositionTimer += Time.deltaTime;
            if (ai.IsStopped || 
                (_updateTargetPositionTimer >= updateTargetPositionDeltaTime && 
                    (distanceToTarget < ai.viewRange * 0.4f || distanceToTarget > ai.viewRange * 0.8f)))
            {
                _updateTargetPositionTimer = 0f;
                UpdateMovementTargetPosition(ai);
            }

            if (DebugManager.instance.generalDebug)
            {
                Debug.DrawLine(ai.transform.position, ai.NPCController.CurrentTargetPosition, Color.blue);
            }
        }

        public void OnExit(AIController ai)
        {
            NPCControllerBase npc = ai.NPCController;
            npc.ClearFollow();
            npc.ClearLookAt();

            var _baseStats = ai.GetComponent<CharacterBaseStats>();
            _baseStats.speedCoefficient = 1f;
        }

        private void UpdateMovementTargetPosition(AIController ai)
        {
            if (_rotationDirectionMultiplier == 0f)
            {
                _rotationDirectionMultiplier = Random.Range(0f, 1f) < 0.5f ? 1f : -1f;
            }

            if (Random.Range(0f, 1f) < 0.1f)
            {
                _rotationDirectionMultiplier *= -1;
            }

            Vector3 diffFromTarget = ai.transform.position - target.transform.position;
            float distance = diffFromTarget.magnitude + Random.Range(-0.2f, 0.2f);
            Vector3 dirFromTarget = diffFromTarget.normalized;

            Quaternion rotationQuaternion = Quaternion.Euler(0f, _rotationDirectionMultiplier * Random.Range(10f, 30f), 0f);
            Vector3 targetPosition = target.transform.position + rotationQuaternion * dirFromTarget * Mathf.Clamp(distance, ai.viewRange * 0.4f, ai.viewRange * 0.8f);

            ai.NPCController.MoveTo(targetPosition);
        }
    }
}