using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class KainooEvent<T0> : UnityEvent<T0>
{
    private HashSet<UnityAction<T0>> actions = new HashSet<UnityAction<T0>>();

    public new void AddListener(UnityAction<T0> action)
    {
        actions.Add(action);
    }

    public new void RemoveListener(UnityAction<T0> action)
    {
        actions.Remove(action);
    }

    // Safely invokes all actions
    public new void Invoke(T0 instance)
    {
        foreach (var action in actions)
        {
            try
            {
                action.Invoke(instance);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        base.Invoke(instance);
    }
}
