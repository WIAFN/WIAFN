using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIAFN.AI
{
    public class PatrolState : AIStateBase
    {
        private float _canSeeCheckTime;

        private const float canSeeCheckDeltaTime = 0.1f;

        public AudioManager audioManager;

        private void Awake()
        {
            // audioManager = Object.FindObjectOfType<AudioManager>();
            //audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        }

        public void OnEnter(AIController ai)
        {
            ai.NPCController.MoveToRelative(Random.insideUnitSphere * ai.maxPatrolDistance);
            _canSeeCheckTime = 0f;

            var _baseStats = ai.GetComponent<CharacterBaseStats>();
            _baseStats.speedCoefficient = 7000f;

            //AudioManager.instance.PlayEnemyWalk(ai.transform);

        }

        public void UpdateState(AIController ai)
        {
            NPCControllerBase npc = ai.NPCController;

            _canSeeCheckTime += Time.deltaTime;

            if (_canSeeCheckTime > canSeeCheckDeltaTime)
            {
                _canSeeCheckTime = 0f;
                if (ai.SwitchToAttackIfCanSeePlayer())
                {
                    return;
                }
            }

            if (npc.IsStopped())
            {
                ai.ChangeState(new IdleState());
            }
        }

        public void UpdateNPCBehaviour(AIController ai)
        {
            
        }

        public void OnExit(AIController ai)
        {

        }

    }
}