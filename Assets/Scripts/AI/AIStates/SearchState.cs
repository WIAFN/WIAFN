using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIAFN.AI
{
    public class SearchState : AIStateBase
    {
        public Vector3 lastSeenPosition;
        public Character searchTarget;

        public bool stopping;

        private float _updateSearchAtTime;

        private float _searchStartTime;

        private float _canSeeCheckTime;

        private const float canSeeCheckDeltaTime = 0.5f;

        public float speedMultiplier = 0.7f;

        public AudioManager audioManager;

        private void Awake()
        {
            //   audioManager = Object.FindObjectOfType<AudioManager>();
            audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        }


        public SearchState(Vector3 lastSeenPosition, Character searchTarget)
        {
            this.lastSeenPosition = lastSeenPosition;
            this.searchTarget = searchTarget;
            _updateSearchAtTime = Time.realtimeSinceStartup;
        }

        public void OnEnter(AIController ai)
        {
            stopping = false;
            ai.NPCController.MoveTo(lastSeenPosition);
            _searchStartTime = Time.realtimeSinceStartup;

            _canSeeCheckTime = 0f;

            var _baseStats = ai.GetComponent<CharacterBaseStats>();
            _baseStats.speedCoefficient = 7000f;

            AudioManager.instance.PlayEnemyRandom(ai.transform);

        }

        public void UpdateState(AIController ai)
        {
            _canSeeCheckTime += Time.deltaTime;
            if (_canSeeCheckTime > canSeeCheckDeltaTime)
            {
                _canSeeCheckTime = 0f;
                if (ai.SwitchToAttackIfCanSeeCharacter(searchTarget)) { return; }
            }

            if (Time.realtimeSinceStartup - _searchStartTime > ai.searchDuration)
            {
                ai.ChangeState(new IdleState());
            }

        }

        public void UpdateNPCBehaviour(AIController ai)
        {
            NPCControllerBase npc = ai.NPCController;

            // Wait and look around.
            if (!stopping && ai.IsStopped)
            {
                npc.StopMoving();
                stopping = true;
                _updateSearchAtTime = Time.realtimeSinceStartup + Random.Range(0f, 3f);
            }

            // Continue searching.
            if (stopping && Time.realtimeSinceStartup > _updateSearchAtTime)
            {
                npc.MoveTo(lastSeenPosition + (Random.insideUnitSphere * ai.maxPatrolDistance));
                stopping = false;
            }
        }

        public void OnExit(AIController ai)
        {
            ai.NPCController.StopMoving();
        }

    }
}