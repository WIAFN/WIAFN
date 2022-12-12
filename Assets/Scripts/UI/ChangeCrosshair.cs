using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WIAFN.Constants;

namespace WIAFN.UI
{
    public class ChangeCrosshair : MonoBehaviour
    {
        public float crosshairFiringErrorMaxOffset;
        public float crosshairFiringErrorSpeed;

        private GameObject[] _crosshairLines;
        private Dictionary<GameObject, Vector3> _crosshairLinesStartingPositions;

        private void Start()
        {
            _crosshairLines = GameObject.FindGameObjectsWithTag(WIAFNTags.Cross);

            _crosshairLinesStartingPositions = new Dictionary<GameObject, Vector3>();

            foreach(GameObject crosshairLine in _crosshairLines)
            { 
                _crosshairLinesStartingPositions.Add(crosshairLine, crosshairLine.transform.localPosition);
            }
        }

        private void Update()
        {
            UpdateFiringError();
        }

        private void UpdateFiringError()
        {
            Character character = GameManager.instance.mainPlayer;
            float firingErrorRate = character.GetFiringErrorRate();

            foreach (GameObject crosshairLine in _crosshairLines)
            {
                Vector3 startPosition = _crosshairLinesStartingPositions[crosshairLine];
                crosshairLine.transform.localPosition = Vector3.Lerp(crosshairLine.transform.localPosition, startPosition + startPosition * firingErrorRate * crosshairFiringErrorMaxOffset, Time.deltaTime * crosshairFiringErrorSpeed);
            }
        }
    }
}
