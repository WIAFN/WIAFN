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

    public Transform head;
    public float eyeHeight;
    public float angularSpeed;
    private const float headAngleLimit = 100f;

    protected Character followCharacter;

    private float _targetCharacterPositionUpdateDelta;
    private const float targetPosUpdateInterval = 0f;

    private LookAtOrder _lookAtOrder;

    public virtual Vector3 CurrentTargetPosition { get; }

    // TODO - Safa: Maybe we should move all constants to a single file.
    protected const float reachCheckSqr = 7f;

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

    private void UpdateLookAt()
    {
        if (head == null) return;
        Transform toRotate = head;

        Vector3 forward = toRotate.parent.forward;

        if (!LookAtSpecified)
        {
            toRotate.forward = forward;
            return;
        }

        Vector3 lookAtPosition = _lookAtOrder.Position;

        if (toRotate.position == lookAtPosition)
        {
            return;
        }

        Quaternion newRotation = Quaternion.FromToRotation(forward, lookAtPosition - toRotate.position);

        newRotation = Quaternion.RotateTowards(toRotate.localRotation, newRotation, Time.deltaTime * angularSpeed);
        newRotation = Quaternion.RotateTowards(Quaternion.identity, newRotation, headAngleLimit);

        toRotate.localRotation = newRotation;
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
