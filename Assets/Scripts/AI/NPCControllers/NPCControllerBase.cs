using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Character))]
public abstract class NPCControllerBase : MonoBehaviour
{
    [HideInInspector]
    public Character character;

    public Transform head;
    public float eyeHeight;
    public float angularSpeed;
    private const float headAngleLimit = 60f;

    protected Character followCharacter;

    private float _targetCharacterPositionUpdateDelta;
    private const float targetPosUpdateInterval = 0f;

    private Character _lookAtCharacter;

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

        if (LookAtSpecified)
        {
            LookAt(_lookAtCharacter.transform.position, true);
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

        transform.forward = closestPoint;
    }

    public void LookAt(Vector3 lookAtPosition, bool addEyeHeight = false)
    {
        if (head == null) return;

        Transform toRotate = head;

        Vector3 forward = toRotate.parent.forward;

        if (toRotate.position == lookAtPosition)
        {
            return;
        }

        if (addEyeHeight)
        {
            lookAtPosition += Vector3.up * eyeHeight;
        }

        Quaternion newRotation = Quaternion.FromToRotation(forward, lookAtPosition - toRotate.position);

        newRotation = Quaternion.RotateTowards(toRotate.localRotation, newRotation, Time.deltaTime * angularSpeed);
        newRotation = Quaternion.RotateTowards(Quaternion.identity, newRotation, headAngleLimit);

        toRotate.localRotation = newRotation;
    }

    public void LookAt(Character lookAt)
    {
        _lookAtCharacter = lookAt;
    }

    public void ClearLookAt()
    {
        _lookAtCharacter = null;
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
    public bool LookAtSpecified => _lookAtCharacter != null;
    public Character LookAtCharacter => _lookAtCharacter;
}
