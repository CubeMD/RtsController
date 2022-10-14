using System;
using UnityEngine;

namespace Systems.Interfaces
{
    public interface IDestroyable
    {
        event Action<IDestroyable> OnDestroyableDestroy;

        GameObject GetGameObject();
    }
}