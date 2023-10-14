using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Adjustable Values")]
    [SerializeField] private float speed;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float jumpHeight;

    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource stepSource;
    [SerializeField] private AudioSource jumpSource;

    [Header("Debugging")]
    [SerializeField] private Vector3 velocity;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isWalking;
    [SerializeField] private bool isJumping;
    [SerializeField] private float x;
    [SerializeField] private float z;

    void Update()
    {
        if (!DialogueManager.instance.dialogueIsPlaying)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
                animator.SetBool("isJumping", false);
            }

            x = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");

            if (x != 0 || z != 0)
            {
                if (!isWalking)
                {
                    isWalking = true;
                    animator.SetBool("isWalking", true);
                }
            }

            else
            {
                isWalking = false;
                animator.SetBool("isWalking", false);
            }

            Vector3 move = transform.right * x + transform.forward * z;

            controller.Move(move * speed * Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                animator.SetBool("isJumping", true);

                jumpSource.Play();
            }

            velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);
        }
    }

    public void ReturnToIdle()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isJumping", false);
    }

    public void StepNoise()
    {
        if (isWalking)
        {
            stepSource.pitch = Random.Range(0.5f, 0.8f);
            stepSource.volume = Random.Range(0.1f, 0.4f);
            stepSource.Play();
        }
    }
}
