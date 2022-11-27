using System;
using System.Collections.Generic;
using UnityEngine;

namespace WIAFN.AI
{
    [RequireComponent(typeof(NPCController))]
    public class AIController : MonoBehaviour
    {
        private AIStateBase _state;
        private NPCController _npcController;

        public float maxIdleTime;
        public float maxPatrolDistance;

        public float viewRange;
        public float viewAngle;
        public LayerMask seeCheckObstacles;

        public float searchDuration;

        private void Awake()
        {
            _npcController = GetComponent<NPCController>();
        }

        void Start()
        {
            ChangeState(new IdleState());
        }

        public void ChangeState(AIStateBase targetState)
        {
            _state?.OnExit(this);
            _state = targetState;
            _state?.OnEnter(this);
        }

        // Update is called once per frame
        void Update()
        {
            _state?.OnUpdate(this);
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
        public NPCController NPCController => _npcController;
        public bool IsStopped => _npcController.IsStopped;
    }
}