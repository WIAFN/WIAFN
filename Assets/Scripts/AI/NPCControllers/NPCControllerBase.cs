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

    protected Character followCharacter;

    private float _targetCharacterPositionUpdateDelta;
    private const float targetPosUpdateInterval = 0.5f;

    private Character _lookAtCharacter;

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

    public void LookAt(Vector3 lookAtPosition, bool addEyeHeight = false)
    {
        bool isHead = true;
        Transform toRotate = head;
        if (toRotate == null)
        {
            isHead = false;
            toRotate = transform;
        }
        Vector3 eulerAngles;

        if (addEyeHeight)
        {
            lookAtPosition += Vector3.up * eyeHeight;
        }
        Vector3 forward;
        if (isHead)
        {
            forward = toRotate.parent.forward;
        }
        else
        {
            forward = Vector3.forward;
        }

        if (toRotate.position == lookAtPosition)
        {
            return;
        }

        Quaternion newRotation = Quaternion.FromToRotation(forward, lookAtPosition - toRotate.position);
        newRotation = Quaternion.RotateTowards(toRotate.localRotation, newRotation, Time.deltaTime * angularSpeed);
        eulerAngles = newRotation.eulerAngles;

        if (isHead)
        {
            eulerAngles.z = Mathf.Clamp(eulerAngles.z, -5f, 5f);
            eulerAngles.y = Mathf.Clamp(eulerAngles.y, -60f, 60f);
            eulerAngles.x = Mathf.Clamp(eulerAngles.x, -40f, 40f);
        }
        else
        {
            eulerAngles.z = 0f;
            eulerAngles.x = 0f;
        }

        toRotate.localRotation = Quaternion.Euler(eulerAngles);
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

    public abstract bool MoveTo(Vector3 position);

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
        followCharacter = null;
        return true;
    }

    public void Follow(Character character)
    {
        followCharacter = character;
    }

    public abstract bool TryAttack(Character character);

    public bool IsFollowing => followCharacter != null;
    public bool LookAtSpecified => _lookAtCharacter != null;
    public Character LookAtCharacter => _lookAtCharacter;
}
