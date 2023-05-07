using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WIAFN.LevelGeneration
{
    internal enum ItemPoolGenerationPhase
    {
        None,
        Running,
        Completed
    }

    internal class ItemPoolGenerator
    {
        public ItemPoolGenerator(int itemPoolSize)
        {
            this._itemPoolSize = itemPoolSize;
            Pool = new List<GameObject>();
            Phase = ItemPoolGenerationPhase.None;
        }

        public List<GameObject> Pool { get; private set; }
        public ItemPoolGenerationPhase Phase { get; private set; }

        public bool IsCompleted
        {
            get
            {
                return Phase == ItemPoolGenerationPhase.Completed;
            }
        }

        private int _itemPoolSize;

        /// <param name="operatorMonobehaviour">Any monobehaviour can be passed here. We just use it to run coroutines.</param>
        /// <param name="getObjectFunc">The function that returns any item really.</param>
        /// <param name="relaxed">Should item pool generation take some time?</param>
        /// <param name="parentObj">Item the new generated items will be attached to.</param>
        public void StartGeneration(MonoBehaviour operatorMonobehaviour, Func<GameObject> getObjectFunc, bool relaxed = true, Transform parentObj = null)
        {
            Pool.Clear();
            Phase = ItemPoolGenerationPhase.Running;

            operatorMonobehaviour.StartCoroutine(GenerateImpl(getObjectFunc, relaxed, parentObj));

            Phase = ItemPoolGenerationPhase.Completed;
        }

        private IEnumerator GenerateImpl(Func<GameObject> getObjectFunc, bool relaxed = true, Transform parentObj = null)
        {
            for (int i = 0; i < _itemPoolSize; i++)
            {
                GameObject selectedItem = getObjectFunc();
                Debug.Assert(selectedItem != null);
                GameObject newItem = UnityEngine.Object.Instantiate(selectedItem, new Vector3(0f, 0f, -10000f), Quaternion.identity, parentObj);
                newItem.SetActive(false);
                Pool.Add(newItem);

                //yield return null;
                //if (relaxed)
                //{
                //    yield return new WaitForSeconds(0.1f);
                //}
            }
            yield break;
        }

        public void DestroyItems()
        {
            foreach (GameObject obj in Pool)
            {
                UnityEngine.Object.Destroy(obj);
            }

            Pool.Clear();
        }
    }
}
