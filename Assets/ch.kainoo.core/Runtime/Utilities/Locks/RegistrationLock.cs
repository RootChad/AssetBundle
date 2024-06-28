using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ch.kainoo.core
{
    /// <summary>
    /// This lock allows for <typeparamref name="T"/> types to register themselves. 
    /// <para/>
    /// <b>Be careful to always release the elements to the lock. If you don't use Unlock(T) and then </b>
    /// </summary>
    /// <typeparam name="T">Any reference type. In the context of Unity, you will probably use 'MonoBehavior'.</typeparam>
    public class RegistrationLock<T> where T : class
    {
        public bool IsLocked => _registeredElements.Count > 0;

        private HashSet<T> _registeredElements = new HashSet<T>();

        public bool Lock(T elem)
        {
            return _registeredElements.Add(elem);
        }

        public bool Unlock(T elem)
        {
            return _registeredElements.Remove(elem);
        }

        public void Clear()
        {
            _registeredElements.Clear();
        }

    }

}