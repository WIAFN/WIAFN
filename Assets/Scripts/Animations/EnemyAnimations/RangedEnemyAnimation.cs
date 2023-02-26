using System;
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

    [Header("LegIK")]
    public TwoBoneIKConstraint rightLegIK;
    public TwoBoneIKConstraint leftLegIK;

    public float legRootTargetDistThreshold;

    public LayerMask legsRaycastLayerMask;

    private Character _character;
    private NPCControllerBase _npc;
    private Animator _animator;

    private bool _lastAttacking;

    private Transform _headAimObject;
    private Transform _bodyAimObject;

    private Transform _rightArmIKTarget;

    private Transform _rightLegIKTarget;
    private Vector3 _rightLegIKTargetPos;
    private Transform _leftLegIKTarget;
    private Vector3 _leftLegIKTargetPos;

    private Transform _rightLegRoot;
    private Transform _leftLegRoot;


    // Start is called before the first frame update
    void Start()
    {
        _character = GetComponentInParent<Character>();
        _npc = _character.GetComponent<NPCControllerBase>();
        _animator = GetComponent<Animator>();

        //DisableAimConstraints();
        SetLookAt(false);
        SetAim(false);

        _headAimObject = headConstraint.data.sourceObjects.GetTransform(0);
        _bodyAimObject = bodyConstraint.data.sourceObjects.GetTransform(0);

        _rightArmIKTarget = rightArmIK.data.target;

        _rightLegIKTarget = rightLegIK.data.target;
        _leftLegIKTarget = leftLegIK.data.target;

        _rightLegRoot = rightLegIK.data.root;
        _leftLegRoot = leftLegIK.data.root;
    }

    // Update is called once per frame
    void LateUpdate()
    {

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
            //UpdateLookAt(_npc.LookAtOrder.PositionRaw, body, bodyAngleLimit, bodyAngularSpeed, restQuat: _bodyRest, useYAxis: true);
            //UpdateLookAt(_npc.LookAtOrder.Position, head, headAngleLimit, headAngularSpeed, restQuat: _headRest);
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
        _leftLegIKTarget.position = _leftLegIKTargetPos;
        _rightLegIKTarget.position = _rightLegIKTargetPos;

        Vector3 leftFootDiff = _leftLegIKTarget.position - _leftLegRoot.position;
        if (leftLegIK!= null && leftFootDiff.sqrMagnitude > legRootTargetDistThreshold)
        {
            bool hit = Physics.Raycast(_leftLegRoot.position, Vector3.down - leftFootDiff.normalized / 2f, out RaycastHit hitInfo, 10f, legsRaycastLayerMask, QueryTriggerInteraction.UseGlobal);
            if (hit && (hitInfo.point - _rightLegIKTargetPos).sqrMagnitude > _character.CharacterMovement.Speed / 5f)
            {
                _leftLegIKTargetPos = hitInfo.point + Vector3.up * 1f;
            }
        }

        Vector3 rightFootDiff = _rightLegIKTarget.position - _rightLegRoot.position;
        if (rightLegIK != null && rightFootDiff.sqrMagnitude > legRootTargetDistThreshold)
        {
            bool hit = Physics.Raycast(_rightLegRoot.position, Vector3.down - rightFootDiff.normalized / 2f, out RaycastHit hitInfo, 10f, legsRaycastLayerMask, QueryTriggerInteraction.UseGlobal);
            if (hit && (hitInfo.point - _leftLegIKTargetPos).sqrMagnitude > _character.CharacterMovement.Speed / 5f)
            {
                _rightLegIKTargetPos = hitInfo.point + Vector3.up * 0.95f;
            }

        }
    }

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
            targetDir = (targetDir + _npc.transform.forward * 1.3f).normalized;
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
