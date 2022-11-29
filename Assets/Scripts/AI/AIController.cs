using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace WIAFN.AI
{
    public class AIController : MonoBehaviour
    {
        private AIStateBase _state;
        private NPCControllerBase _npcController;

        public float maxIdleTime;
        public float maxPatrolDistance;

        public float viewRange;
        public float viewAngle;
        public LayerMask seeCheckObstacles;

        public float searchDuration;

        private void Awake()
        {
            _npcController = GetComponent<NPCControllerBase>();
            Debug.Assert(_npcController != null, $"{gameObject.name} named NPC doesn't have an AI Controller.");
        }

        void Start()
        {
            ChangeState(new IdleState());
        }

        public void ChangeState(AIStateBase targetState)
        {
            Debug.Assert(targetState != null);
            _state?.OnExit(this);
            _state = targetState;
            _state?.OnEnter(this);
        }

        // Update is called once per frame
        void Update()
        {
            _state.OnUpdate(this);
        }

        public bool AttackIfCanSeePlayer()
        {
            return AttackIfCanSeeCharacter(GameManager.instance.mainPlayer);
        }

        public bool AttackIfCanSeeCharacter(Character character)
        {
            bool canSeePlayer = CanSeeCharacter(character);
            if (canSeePlayer)
            {
                ChangeState(new AttackState(character));
            }

            return canSeePlayer;
        }

        public bool CanSeeCharacter(Character character)
        {
            Vector3 directionToTarget = character.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, directionToTarget);
            float distance = directionToTarget.magnitude;
            return CanFeelCharacter(character) && (angle < viewAngle || distance < viewRange / 5f);
        }

        // What kind of a naming convention is this...
        public bool CanFeelCharacter(Character character)
        {
            Vector3 directionToTarget = character.transform.position - transform.position;
            float distance = directionToTarget.magnitude;
            return distance <= viewRange && !Physics.Linecast(transform.position, character.transform.position, seeCheckObstacles, QueryTriggerInteraction.Ignore);
        }

        public AIStateBase State => _state;
        public NPCControllerBase NPCController => _npcController;
        public bool IsStopped => _npcController.IsStopped();
    }
}