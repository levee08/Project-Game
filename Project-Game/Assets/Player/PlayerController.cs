using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed=5f;
    public float gridSize;
    private bool isMoving;
    private Vector2 input;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public LayerMask solidObjectsLayer;
    public LayerMask interactableLayer;
    private void Awake()
    {
       animator = GetComponent<Animator>();
         spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

           
            //Debug.Log("input.x:" + input.x);
            //Debug.Log("input.y:" + input.y);

            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);
                if (input.x < 0)
                {
                    spriteRenderer.flipX = true;
                    animator.SetBool("isFlipped", true);
                }
                else if (input.x > 0)
                {
                    spriteRenderer.flipX = false;
                    animator.SetBool("isFlipped", false);
                }

                var targetPos=transform.position;
                targetPos.x += input.x*gridSize;
                targetPos.y += input.y*gridSize;

                if(isWalkable(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }
               
            }
        }
        animator.SetBool("isMoving", isMoving);
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        while((targetPos-transform.position).sqrMagnitude > Mathf.Epsilon)
        {
           transform.position =Vector3.MoveTowards(transform.position, targetPos, moveSpeed*Time.deltaTime);
            yield return null;
        }
        transform.position= targetPos;
        isMoving = false;
    }

    private bool isWalkable(Vector3 targetPos)
    {
        if(Physics2D.OverlapCircle(targetPos,0.2f,solidObjectsLayer | interactableLayer)!=null)
        {
            return false;
        }
        return true;
    }


}
