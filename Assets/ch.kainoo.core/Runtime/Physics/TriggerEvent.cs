using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    public UnityEvent<Collider> TriggerEnter = new UnityEvent<Collider>();
    public UnityEvent<Collider> TriggerExit = new UnityEvent<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        TriggerEnter.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        TriggerExit.Invoke(other);
    }
}
