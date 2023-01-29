using Units;
using UnityEngine;

namespace Interfaces
{
    public interface IMove
    {
        void Move(Vector3 target, bool queue);
        void Move(Transform target, bool queue);
    }

    public interface IReclaim
    {
        void Reclaim(Reclaim reclaim, bool queue);
    }

    public interface IAttack
    {
        void Attack(Transform target, bool queue);
    }

    public interface IBuild
    {
        void Build(UnitType unitType, Vector3 position, bool queue);
    }
}