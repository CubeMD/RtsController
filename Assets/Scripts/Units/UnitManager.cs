using System;
using System.Collections.Generic;
using System.Linq;
using Players;
using UnityEngine;
using Utilities;

namespace Units
{
    public class UnitManager : MonoBehaviour
    {
        [Serializable]
        private class UnitPrefabGroup
        {
            public UnitType unitType;
            public Unit unitPrefab;
        }
        
        [SerializeField] private Player player;
        [SerializeField] private List<UnitPrefabGroup> unitPrefabGroups = new List<UnitPrefabGroup>();
        
        public List<Unit> OwnedUnits { get; } = new List<Unit>();
        public List<Unit> SelectedUnits { get; } = new List<Unit>();
        public List<Unit> GhostUnits { get; } = new List<Unit>();

        private readonly Dictionary<UnitType, Unit> unitPrefabTable = new Dictionary<UnitType, Unit>();

        private void Awake()
        {
            unitPrefabTable.Clear();

            foreach (UnitPrefabGroup group in unitPrefabGroups)
            {
                if (unitPrefabTable.ContainsKey(group.unitType))
                {
                    Debug.Log("Unit with the same type already exists in the table");
                    continue;
                }
               
                unitPrefabTable.Add(group.unitType, group.unitPrefab);
            }
        }

        public void AddSelectedUnit(Unit unit)
        {
            if (!unit.IsConstructed || SelectedUnits.Contains(unit))
            {
                return;
            }
            
            SelectedUnits.Add(unit);
        }
        
        public void AddSelectedUnits(List<Unit> units)
        {
            foreach (Unit unit in units)
            {
                AddSelectedUnit(unit);
            }
        }
        
        public List<Unit> GetSelectedUnitsByOrder(OrderType orderType)
        {
            return SelectedUnits.Where(x => x.CanExecuteOrderType(orderType)).ToList();
        }

        public void ResetUnitManager()
        {
            ClearOwnedUnits();
        }

        private void ClearOwnedUnits()
        {
            int numUnits = OwnedUnits.Count;
            
            for (int i = 0; i < numUnits; i++)
            {
                OwnedUnits[0].DestroyUnit();
            }
        }

        public Unit PlaceUnitGhost(UnitType unitType, Vector3 position)
        {
            if (TryGetUnitPrefabByUnitType(unitType, out Unit prefab))
            {
                Unit unit = ObjectPooler.InstantiateGameObject(prefab, position, Quaternion.identity, transform);
                GhostUnits.Add(unit);
                unit.DisableUnit();
                return unit;
            }

            return null;
        }

        public void EnableUnit(Unit unit, bool forceConstructed = false)
        {
            if (GhostUnits.Contains(unit))
            {
                unit.OnUnitDestroyed += HandleUnitDestroyed;
                unit.EnableUnit();
                unit.SetOwner(player);
            
                if (forceConstructed)
                {
                    unit.ForceConstructed();
                }
                else
                {
                    unit.StartConstruction();
                }
            
                OwnedUnits.Add(unit);
                GhostUnits.Remove(unit);
            }
        }
        
        public Unit SpawnUnit(UnitType unitType, Vector3 position, bool forceConstructed = false)
        {
            if (TryGetUnitPrefabByUnitType(unitType, out Unit prefab))
            {
                Unit unit = ObjectPooler.InstantiateGameObject(prefab, position, Quaternion.identity, transform);
                unit.OnUnitDestroyed += HandleUnitDestroyed;
                unit.SetOwner(player);
            
                if (forceConstructed)
                {
                    unit.ForceConstructed();
                }
                else
                {
                    unit.StartConstruction();
                }
            
                OwnedUnits.Add(unit);
                return unit;
            }

            return null;
        }

        public void SpawnCommander()
        {
            SpawnUnit(UnitType.Commander, player.startingPosition, true);
        }

        private void HandleUnitDestroyed(Unit unit)
        {
            if (unit == null)
            {
                return;
            }
            
            unit.OnUnitDestroyed -= HandleUnitDestroyed;
            OwnedUnits.Remove(unit);

            if (SelectedUnits.Contains(unit))
            {
                SelectedUnits.Remove(unit);
            }
        }

        public bool CanBuildUnitTypeAtPosition(UnitType unitType, Vector3 position)
        {
            if (TryGetUnitPrefabByUnitType(unitType, out Unit unit))
            {
                return Physics.OverlapBox(
                    position, 
                    Vector3.one * unit.Size, 
                    Quaternion.identity,
                    player.matchManager.SpaceLayerMask).Length < 1;
            }

            return false;
        }

        public bool TryGetUnitPrefabByUnitType(UnitType unitType, out Unit unit)
        {
            unit = null;
            
            if (unitPrefabTable.TryGetValue(unitType, out unit))
            {
                return true;
            }

            Debug.LogError($"Requested prefab for non-registered unit type {unitType}");
            return false;
        }
    }
}