using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WIAFN.AI;

[RequireComponent(typeof(Character))]
public abstract class NPCControllerBase : MonoBehaviour
{
    [HideInInspector]
    public Character character;

    public float eyeHeight;
    public float angularSpeed;

    protected Character followCharacter;

    private float _targetCharacterPositionUpdateDelta;
    private const float targetPosUpdateInterval = 0f;

    private LookAtOrder _lookAtOrder;

    public virtual Vector3 CurrentTargetPosition { get; }

    // TODO - Safa: Maybe we should move all constants to a single file.
    protected const float reachCheckSqr = 7f;


    //stuck control
    protected const float stuckThreshold = 1f; // The time in seconds the NPC has to be stuck to be considered stuck.
    private float timeStuck = 0f;
    public bool isStuck = false;

    public virtual void Awake()
    {
        
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        character = GetComponent<Character>();
        _targetCharacterPositionUpdateDelta = 0f;

        ClearLookAt();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (followCharacter != null)
        {
            _targetCharacterPositionUpdateDelta += Time.deltaTime;
            if (_targetCharacterPositionUpdateDelta >= targetPosUpdateInterval)
            {
                MoveTo(followCharacter);
                _targetCharacterPositionUpdateDelta = 0f;
            }
        }
        else
        {
            _targetCharacterPositionUpdateDelta = 0f;
        }

        UpdateLookAt();

        // Check if NPC is stuck
        if (IsStopped())
        {
            timeStuck += Time.deltaTime;
            if (timeStuck > stuckThreshold && !isStuck)
            {
                isStuck = true;
                Debug.Log("NPC is stuck!");
            }
        }
        else
        {
            timeStuck = 0f;
            isStuck = false;
        }
    }
        if (_lookAtOrder != null && !_lookAtOrder.IsValid())
        {
            ClearLookAt();
        }
    }

    public void RotateBodyTowardsPosition(Vector3 facePosition)
    {
        RotateBody(facePosition - transform.position);
    }

    public void RotateBody(Vector3 direction)
    {
        Plane xzPlane = new Plane(Vector3.up, 0f);
        Vector3 closestPoint = xzPlane.ClosestPointOnPlane(direction.normalized);

        if (DebugManager.instance.generalDebug)
        {
            Debug.DrawRay(transform.position, closestPoint * 3f, Color.yellow);
        }

        if (closestPoint == Vector3.zero) return;

        Vector3 parentForward;
        if (transform.parent != null)
        {
            parentForward = transform.parent.forward;
        }
        else
        {
            parentForward = Vector3.forward;
        }

        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.FromToRotation(parentForward, closestPoint), Time.deltaTime * angularSpeed);
        //transform.forward = closestPoint;
    }

    public void LookAt(Vector3 lookAtPosition, bool addEyeHeight = false)
    {
        float additionalHeight = 0f;
        if (addEyeHeight)
        {
            additionalHeight += eyeHeight;
        }

        _lookAtOrder = new LookAtOrder(lookAtPosition, additionalHeight);
    }

    public void LookAt(Character lookAt, bool addEyeHeight = false)
    {
        float additionalHeight = 0f;
        if (addEyeHeight)
        {
            additionalHeight += eyeHeight;
        }

        _lookAtOrder = new LookAtOrder(lookAt, additionalHeight);
    }

    public void ClearLookAt()
    {
        _lookAtOrder = null;
    }

    public abstract bool IsStopped();

    public virtual bool MoveTo(Vector3 position)
    {
        return true;
    }

    public bool MoveTo(Character targetCharacter)
    {
        return MoveTo(targetCharacter.transform.position);
    }

    public bool MoveToRelative(Vector3 globalVector)
    {
        return MoveTo(transform.position + globalVector);
    }

    public virtual bool StopMoving()
    {
        ClearFollow();
        return true;
    }

    public void Follow(Character character)
    {
        followCharacter = character;
    }

    public void ClearFollow()
    {
        followCharacter = null;
    }

    public abstract bool TryAttack(Character character);

    public virtual bool CheckIfReached(Vector3 targetPosition)
    {
        return (transform.position - targetPosition).sqrMagnitude <= reachCheckSqr;
    }

    public bool IsFollowing => followCharacter != null;

    public bool LookAtSpecified => _lookAtOrder != null;
    public LookAtOrder LookAtOrder => _lookAtOrder;
}
