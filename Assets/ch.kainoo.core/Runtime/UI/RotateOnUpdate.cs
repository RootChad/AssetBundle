using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ch.kainoo.core
{

    public class RotateOnUpdate : MonoBehaviour
    {
        [Tooltip("Rotation speed expressed in degrees per second")]
        [SerializeField] 
        private float m_rotationSpeed = 1f;
        [Tooltip("World space vector around which the object should rotate. The rotation center is the object's pivot.")]
        [SerializeField] 
        private Vector3 m_rotationAxis = Vector3.up;

        void Update()
        {
            transform.Rotate(m_rotationAxis, m_rotationSpeed * Time.deltaTime);
        }
    }

}