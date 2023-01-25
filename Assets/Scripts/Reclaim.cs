using System;
using Interfaces;
using UnityEngine;
using Utilities;

public class Reclaim : MonoBehaviour, IDestroyable
{
    public event Action<IDestroyable> OnDestroyableDestroy;
    
    //[SerializeField] private Renderer ren;
    [SerializeField] private float amount = 1;

    public float Amount
    {
        get => amount;
        set
        {
            if (value <= 0)
            {
                DestroyReclaim();
            }
            else
            {
                amount = value;
            }

            //ren.material.color = gradient.Evaluate(Mathf.InverseLerp(reclaimMinMax.x, reclaimMinMax.y, amount));
        }
    }
    
    public void DestroyReclaim()
    {
        OnDestroyableDestroy?.Invoke(this);
        ObjectPooler.PoolGameObject(gameObject);
    }
    
    public GameObject GetGameObject()
    {
        return gameObject;
    }
}