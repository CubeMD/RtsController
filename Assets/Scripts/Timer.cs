﻿using System;
using System.Collections;
using UnityEngine;

public class Timer
{
    private readonly float duration;
    private readonly MonoBehaviour owner;
    private readonly Action callback;
    public float TimerPercentage => timeLeft / duration;
    
    private float timeLeft;
    private Coroutine timerRoutine;

    public Timer(MonoBehaviour owner, float duration, Action callback)
    {
        this.owner = owner;
        this.duration = duration;
        this.callback = callback;
    }

    public void StartTimer()
    {
        StopTimer();
        timerRoutine = owner.StartCoroutine(TimerRoutine());
    }

    public void StopTimer()
    {
        if (timerRoutine != null)
        {
            owner.StopCoroutine(timerRoutine);
        }
    }

    private IEnumerator TimerRoutine()
    {
        timeLeft = duration;
        
        while (timeLeft > 0)
        {
            timeLeft -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        
        callback?.Invoke();
        timerRoutine = null;
    }
}