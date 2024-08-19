using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private string horizontalInputName = "Horizontal";
    [SerializeField] private string verticalInputName = "Vertical";

    [SerializeField] private float movementSpeed = 2f;

    private CharacterController charController;


    private void Awake()
    {
        charController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        //StartCoroutine(EnableAfterDelay());
    }

    IEnumerator EnableAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        charController.enabled = true;
    }

    private void Update()
    {
        if (!charController.enabled)
        {
            return;
        }
        PlayerMovement();
    }

    private void PlayerMovement()
    {
        float vertInput = Input.GetAxis(verticalInputName) * movementSpeed;     //CharacterController.SimpleMove() applies deltaTime
        float horizInput = Input.GetAxis(horizontalInputName) * movementSpeed;

        Vector3 forwardMovement = transform.forward * vertInput;
        Vector3 rightMovement = transform.right * horizInput;

        //simple move applies delta time automatically
        charController.SimpleMove(forwardMovement + rightMovement);
    }
}
