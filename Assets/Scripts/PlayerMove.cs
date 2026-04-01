using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private CharacterController controller;
    [SerializeField] private float speed = 5f;

    [SerializeField] private Vector3 inputVector;
    [SerializeField] private Vector3 movementVector;
    [SerializeField] private float gravity = -10f; 

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update() 
    {
        GetInput();
        MovePlayer();
    }

    private void GetInput()
    {
        inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        inputVector.Normalize();
        inputVector = transform.TransformDirection(inputVector);

        movementVector = (inputVector * speed) + (Vector3.up * gravity);
    }

    private void MovePlayer()
    {
        controller.Move(movementVector * Time.deltaTime);
        //AudioManager.instance.Play("Footsteps");
    }
}
