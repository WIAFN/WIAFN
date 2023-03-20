using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;
using WIAFN.AI;
using WIAFN.Constants;

public class RangedEnemyAnimation : MonoBehaviour
{
    [Header("Arms")]
    public Transform rightArmClavicle;

    [Header("Procedural Animation")]
    public Rig aimRig;
    public Rig lookAtRig;
    public Rig legsRig;
    public MultiAimConstraint headConstraint;
    public MultiAimConstraint bodyConstraint;
    public ChainIKConstraint rightArmIK;

    private Character _character;
    private NPCControllerBase _npc;
    private Animator _animator;

    private bool _lastAttacking;

    private Transform _headAimObject;
    private Transform _bodyAimObject;

    private Transform _rightArmIKTarget;

    private int _bodyIdleMovementDirection;
    private float _bodyOffset;

    #region Leg IK Variables
    [Header("LegIK")]
    public TwoBoneIKConstraint rightLegIK;
    public TwoBoneIKConstraint leftLegIK;

    public Transform leftLegRootIndicator, rightLegRootIndicator;

    public float legRootTargetDistThresholdSqr, legResetDistanceThresholdSqr;

    public LayerMask legsRaycastLayerMask;

    public float stepHeight, stepLength;

    private Transform _leftLegIKTarget, _rightLegIKTarget;

    private Vector3 _leftLegOldPos, _rightLegOldPos;
    private Vector3 _leftLegCurrentPos, _rightLegCurrentPos;
    private Vector3 _leftLegTargetPos, _rightLegTargetPos;

    private Vector3 _leftLegOldNormal, _rightLegOldNormal;
    private Vector3 _leftLegCurrentNormal, _rightLegCurrentNormal;
    private Vector3 _leftLegTargetNormal, _rightLegTargetNormal;

    private float _leftLegLerp, _rightLegLerp;

    private bool _leftLegOnGround => _leftLegLerp >= 1f;
    private bool _rightLegOnGround => _rightLegLerp >= 1f;
    #endregion Leg IK Variables


    // Start is called before the first frame update
    void Start()
    {
        _character = GetComponentInParent<Character>();
        _npc = _character.GetComponent<NPCControllerBase>();
        _animator = GetComponent<Animator>();

        //DisableAimConstraints();
        SetLookAt(false);
        SetAim(false);

        _bodyIdleMovementDirection = 1;
        _bodyOffset = 0f;

        _headAimObject = headConstraint.data.sourceObjects.GetTransform(0);
        _bodyAimObject = bodyConstraint.data.sourceObjects.GetTransform(0);
        InitializeLegIK();
    }

    private void InitializeLegIK()
    {
        _rightArmIKTarget = rightArmIK.data.target;

        _rightLegIKTarget = rightLegIK.data.target;
        _leftLegIKTarget = leftLegIK.data.target;

        _leftLegTargetPos = _leftLegIKTarget.position;
        ResetLeftLegIK();
        _rightLegTargetPos = _rightLegIKTarget.position;
        ResetRightLegIK();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //UpdateGeneralBody();

        // For body.
        bool lookAtSpecified = _npc.LookAtSpecified;
        if (_lastAttacking != lookAtSpecified)
        {
            if (lookAtSpecified)
            {
                //if (_animator != null)
                //{
                //    _animator.SetBool(WIAFNAnimatorParams.InFight, true);
                //}
                SetLookAt(true);
                SetAim(true);

            }
            else
            {
                //if (_animator != null)
                //{
                //    _animator.SetBool(WIAFNAnimatorParams.InFight, false);
                //}
                SetLookAt(false);
                SetAim(false);

            }

            _lastAttacking = lookAtSpecified;
        }

        if (lookAtSpecified)
        {
            LookAtOrder lookAtOrder = _npc.LookAtOrder;

            UpdateGeneralBodyConstraints(lookAtOrder);
            UpdateAimConstraints(lookAtOrder);
        }

        // For legs.
        //float speed = _character.CharacterMovement.Speed;
        //if (speed > 0f)
        //{
        //    if (speed < 1f)
        //    {

        //        speed = 0f;
        //    }
        //    else if (speed < 6f)
        //    {
        //        speed = 6f;
        //    }
        //}

        //if (_animator != null)
        //{
        //    _animator.SetFloat(WIAFNAnimatorParams.Speed, speed);
        //}
    }

    private void Update()
    {
        UpdateLegs();
    }

    private void UpdateGeneralBody()
    {
        // Should be called on LateUpdate.
        // Doesn't work well with legs.
        _bodyOffset += _bodyIdleMovementDirection * Time.deltaTime * 0.1f;
        if (Mathf.Abs(_bodyOffset) > 0.05f && _bodyOffset * _bodyIdleMovementDirection > 0f)
        {
            _bodyIdleMovementDirection *= -1;
        }

        Vector3 bodyPos = transform.position;
        bodyPos.y = _bodyOffset;
        transform.position = bodyPos;
    }

    #region Leg Operations
    private void UpdateLegs()
    {
        // Interpolate Leg Position.
        CalculateLegInterpolation(ref _leftLegOldPos, ref _leftLegCurrentPos, _leftLegTargetPos, ref _leftLegLerp, stepHeight);
        CalculateLegInterpolation(ref _rightLegOldPos, ref _rightLegCurrentPos, _rightLegTargetPos, ref _rightLegLerp, stepHeight);

        _leftLegIKTarget.position = _leftLegCurrentPos;
        _rightLegIKTarget.position = _rightLegCurrentPos;

        if (DebugManager.instance.debugProceduralAnims)
        {
            Debug.DrawLine(_leftLegOldPos, _leftLegCurrentPos, Color.green);
            Debug.DrawLine(_leftLegCurrentPos, _leftLegTargetPos, Color.yellow);
            Debug.DrawLine(leftLegRootIndicator.position, _leftLegTargetPos);

            Debug.DrawLine(_rightLegOldPos, _rightLegCurrentPos, Color.green);
            Debug.DrawLine(_rightLegCurrentPos, _rightLegTargetPos, Color.yellow);
            Debug.DrawLine(rightLegRootIndicator.position, _rightLegTargetPos);
        }

        CalculateLegTargetPos(leftLegIK, leftLegRootIndicator, _rightLegOnGround, ref _leftLegLerp, ref _leftLegOldPos, ref _leftLegTargetPos);
        CalculateLegTargetPos(rightLegIK, rightLegRootIndicator, _leftLegOnGround, ref _rightLegLerp, ref _rightLegOldPos, ref _rightLegTargetPos);

        Vector3 footDiff = _leftLegTargetPos - leftLegRootIndicator.position;
        if (footDiff.sqrMagnitude > legResetDistanceThresholdSqr)
        {
            ResetLeftLegIK();
        }

        footDiff = _rightLegTargetPos - rightLegRootIndicator.position;
        if (footDiff.sqrMagnitude > legResetDistanceThresholdSqr)
        {
            ResetRightLegIK();
        }
    }

    private void CalculateLegInterpolation(ref Vector3 legOldPos, ref Vector3 legCurrentPos, Vector3 legTargetPos, ref float legLerp, float stepHeight)
    {
        if (legLerp < 1)
        {
            legCurrentPos = Vector3.Lerp(legOldPos, legTargetPos, legLerp);
            legCurrentPos.y += Mathf.Sin(legLerp * Mathf.PI) * stepHeight;
            legLerp += Time.deltaTime * Mathf.Max(1f, _character.BaseStats.Speed * 0.45f);
        }
        else
        {
            legOldPos = legTargetPos;
        }
    }

    private void CalculateLegTargetPos(TwoBoneIKConstraint legIK, Transform legRoot, bool otherLegOnGround, ref float legLerp, ref Vector3 legOldPos, ref Vector3 legTargetPos)
    {
        Vector3 footDiff = legTargetPos - legRoot.position;
        if (legIK != null && footDiff.sqrMagnitude > legRootTargetDistThresholdSqr && otherLegOnGround && legLerp >= 1f)
        {
            // Shoots raycast always down. Should not look ok when moving on walls.
            bool hit = Physics.Raycast(legRoot.position, Vector3.down, out RaycastHit hitInfo, 10f, legsRaycastLayerMask, QueryTriggerInteraction.UseGlobal);
            if (hit)
            {
                legLerp = 0f;
                Vector3 footOffset = Vector3.up * 0.9f;
                Transform body = _character.transform;
                int direction = body.InverseTransformPoint(hitInfo.point).z > body.InverseTransformPoint(legTargetPos).z ? 1 : -1;
                float characterSpeed = _character.CharacterMovement.Speed;

                // Can be unnecessary just in case.
                if (characterSpeed < 0.2f)
                {
                    direction = 0;
                }

                legTargetPos = hitInfo.point + (body.forward * stepLength * direction * Mathf.Max(1f, characterSpeed / 7f)) + footOffset;
                legTargetPos = legTargetPos + new Vector3(Random.value, 0f, Random.value) * 0.05f; // To create a minor randomness.
                //legTargetNormal = hitInfo.normal;

            }
        }
    }

    private void ResetLeftLegIK()
    {
        _leftLegCurrentPos = _leftLegTargetPos;
        _leftLegLerp = 1f;

        // TODO: If normals going to be implemented move the both legs operations to their own classes.
        //_leftLegCurrentNormal = _leftLegTargetNormal = _leftLegOldNormal = Vector3.up;
    }

    private void ResetRightLegIK()
    {
        _rightLegCurrentPos = _rightLegTargetPos;
        _rightLegLerp = 1f;

        //_rightLegCurrentNormal = _rightLegTargetNormal = _rightLegOldNormal = Vector3.up;
    }

    #endregion Leg Operations

    private LookAtOrder GetEmptyLookAt()
    {
        Vector3 bodyPos = bodyConstraint.data.constrainedObject.position;
        return new LookAtOrder(bodyPos + _npc.transform.forward * 2f, _npc.eyeHeight - (bodyPos - _npc.transform.position).magnitude);
    }

    private void SetLookAt(bool toSet)
    {
        lookAtRig.weight = toSet ? 1f: 0f;
    }


    private void SetAim(bool toSet)
    {
        aimRig.weight = toSet ? 1f: 0f;
    }

    private void SetLegs(bool toSet)
    {
        legsRig.weight = toSet ? 1f: 0f;
    }

    private void UpdateGeneralBodyConstraints(LookAtOrder lookAtOrder)
    {
        _headAimObject.position = Vector3.Lerp(_headAimObject.position, lookAtOrder.Position, 40f * Time.deltaTime);
        _bodyAimObject.position = Vector3.Lerp(_bodyAimObject.position, lookAtOrder.PositionRaw, 40f * Time.deltaTime);
    }

    private void UpdateAimConstraints(LookAtOrder lookAtOrder)
    {
        Vector3 targetDir = (lookAtOrder.PositionRaw - rightArmClavicle.position).normalized;

        float angleToLookAt = Vector3.SignedAngle(_npc.transform.forward, lookAtOrder.Position - _npc.transform.position, _npc.transform.up);
        float targetDistance = 1.6f;
        if (angleToLookAt < -70f)
        {
            targetDir = (targetDir + _npc.transform.forward * 0.8f).normalized;
        }
        if (angleToLookAt > 120f || angleToLookAt < -120f)
        {
            targetDistance *= 3f;
        }

        _rightArmIKTarget.position = rightArmClavicle.position + targetDir * targetDistance;
        //_rightArmIKTarget.up = Vector3.up;
        _rightArmIKTarget.LookAt(lookAtOrder.PositionRaw, Vector3.up);
    }

    private void UpdateLookAt(Vector3 lookAtPosition, Transform toRotate, float angleLimit, float angularSpeed, Quaternion restQuat = default, bool useYAxis = true)
    {
        if (toRotate == null) return;

        Vector3 currentPosition = toRotate.position;

        if (!useYAxis)
        {
            Plane xzPlane = new Plane(Vector3.up, 0f);
            Vector3 lookAtLocal = toRotate.InverseTransformPoint(lookAtPosition);
            Vector3 closestPoint = xzPlane.ClosestPointOnPlane(lookAtLocal);

            lookAtPosition = toRotate.TransformPoint(closestPoint);
        }

        if (currentPosition == lookAtPosition)
        {
            return;
        }

        Vector3 parentForward = toRotate.parent.rotation * -Vector3.up; // Because of the skeleton of the mesh.
        Vector3 targetDir = lookAtPosition - currentPosition;

        //Quaternion changeQuaternion = Quaternion.FromToRotation(parentForward, targetDir);
        //Debug.DrawRay(currentPosition, changeQuaternion * parentForward * 0.75f, Color.black);

        //Quaternion newRotation = Quaternion.Euler(0f, 90f, -90f) * changeQuaternion * restQuat;
        //Quaternion newRotation = changeQuaternion;
        //Quaternion newRotation = changeQuaternion * Quaternion.Euler(0f, 90f, -90f);
        //Quaternion newRotation = (Quaternion.Inverse(toRotate.parent.rotation) * changeQuaternion) * Quaternion.Euler(0f, 90f, -90f);
        Quaternion newRotation = Quaternion.LookRotation(toRotate.parent.InverseTransformDirection(targetDir), toRotate.parent.InverseTransformDirection(Vector3.up)) * Quaternion.Euler(0f, 90f, -90f);


        if (DebugManager.instance.generalDebug)
        {
            Debug.DrawRay(currentPosition, parentForward, Color.green);
            Debug.DrawRay(currentPosition, (toRotate.parent.rotation * newRotation) * -Vector3.up * 1.5f, Color.blue);
        }

        //newRotation = Quaternion.RotateTowards(toRotate.localRotation, newRotation, Time.deltaTime * angularSpeed);

        newRotation = Quaternion.RotateTowards(restQuat, newRotation, angleLimit);

        if (DebugManager.instance.generalDebug)
        {
            Debug.DrawRay(currentPosition, (toRotate.parent.rotation * newRotation) * -Vector3.up, Color.red);
        }

        toRotate.localRotation = newRotation;
    }

}
