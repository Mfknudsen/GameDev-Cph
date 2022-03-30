#region Packages

using System.Collections;
using GameDev.Terrain;
using UnityEngine;

#endregion

namespace GameDev.Buildings.Aliens
{
    public class CreepSpreader : MonoBehaviour
    {
        #region Values

        [SerializeField] private int spreadStrength;

        private CreepManager creepManager;
        private CreepPoint creepPoint;

        #endregion

        #region Build In States

        private void Start()
        {
            creepManager = FindObjectOfType<CreepManager>();

            StartCoroutine(WaitForManagerResponse());
        }

        private void OnDestroy()
        {
            creepPoint.spreadStrength.Remove(spreadStrength);
        }

        #endregion

        #region Internal

        private IEnumerator WaitForManagerResponse()
        {
            yield return new WaitWhile(() => !creepManager.GetIsReady());

            creepPoint = creepManager.GetClosestToPosition(transform.position);
            creepPoint.SetSpread(0.1f);
            creepManager.AddUpdatePoint(creepPoint.index,spreadStrength );
        }

        #endregion
    }
}