using System;
using UnityEngine;

namespace Interfaces
{
    public interface IDestroyable
    {
        event Action<IDestroyable> OnDestroyableDestroy;

        GameObject GetGameObject();
    }
}