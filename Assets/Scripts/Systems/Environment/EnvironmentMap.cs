using System;
using UnityEngine;

namespace Systems.Environment
{
    public class EnvironmentMap : MonoBehaviour
    {
        [SerializeField]
        private Transform mapGround;

        private void Awake()
        {
            mapGround.localScale = new Vector3(
                EnvironmentGlobalSettings.GroundSize.x, 
                1,
                EnvironmentGlobalSettings.GroundSize.y);
        }
    }
}