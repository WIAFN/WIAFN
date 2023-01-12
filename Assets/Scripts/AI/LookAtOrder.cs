using System;
using System.Collections.Generic;
using UnityEngine;

namespace WIAFN.AI
{
    public enum LookAtOrderType
    {
        None,
        Position,
        Character
    }

    public class LookAtOrder
    {
        public LookAtOrderType Type { get; private set; }
        private Character _lookAtCharacter;
        private Vector3 _lookAtPosition;

        private float _additionalHeight;

        public LookAtOrder(Character character, float additionalHeight = 0f)
        {
            _lookAtCharacter = character;
            Type = LookAtOrderType.Character;
            _additionalHeight = additionalHeight;
        }

        public LookAtOrder(Vector3 position, float additionalHeight = 0f)
        {
            _lookAtCharacter = null;
            _lookAtPosition = position;
            Type = LookAtOrderType.Position;
            _additionalHeight = additionalHeight;
        }

        public Vector3 GetLookAtPosition()
        {
            return Position;
        }

        public Character Character => _lookAtCharacter;
        public Vector3 Position 
        {
            get
            {
                Vector3 result = Vector3.zero;
                switch (Type)
                {
                    case LookAtOrderType.Position:
                        result = _lookAtPosition;
                        break;

                    case LookAtOrderType.Character:
                        result = _lookAtCharacter.transform.position;
                        break;

                    default:
                        Debug.LogAssertion("Invalid LookAtOrderType.");
                        break;
                }

                result += Vector3.up * _additionalHeight;
                return result;
            }
        }
    }
}
