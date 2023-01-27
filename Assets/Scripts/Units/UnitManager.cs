using System.Collections.Generic;
using Interfaces;
using Orders;
using Players;
using Units.Buildings;
using Units.LandUnits;
using UnityEngine;
using Utilities;

namespace Units
{
    public class UnitManager : MonoBehaviour
    {
        public Player player;

        public readonly List<Unit> ownedUnits = new List<Unit>();
        public readonly List<Unit> selectedUnits = new List<Unit>();
        
        [SerializeField] private Commander commanderPrefab;
        [SerializeField] private Tank tankPrefab;
        [SerializeField] private Engineer engineerPrefab;
        public Factory factoryPrefab;
        
        public void ResetUnitManager()
        {
            ClearOwnedUnits();
        }
        
        public void ClearOwnedUnits()
        {
            int numUnits = ownedUnits.Count;

            for (int i = 0; i < numUnits; i++)
            {
                ownedUnits[0].DestroyUnit();
            }
        }
        
        public void SpawnUnit(Unit unitPrefab, Vector3 position)
        {
            Unit unit = ObjectPooler.InstantiateGameObject(unitPrefab, position, Quaternion.identity, transform);
            unit.OnDestroyableDestroy += HandleUnitDestroyed;
            unit.owner = player;
            ownedUnits.Add(unit);
        }

        public void SpawnCommander()
        {
            SpawnUnit(commanderPrefab, player.startingPosition);
        }

        public void HandleUnitDestroyed(IDestroyable destroyable)
        {
            Unit unit = destroyable.GetGameObject().GetComponent<Unit>();
            unit.OnDestroyableDestroy -= HandleUnitDestroyed;
            ownedUnits.Remove(unit);

            if (selectedUnits.Contains(unit))
            {
                selectedUnits.Remove(unit);
            }
        }
    }
}