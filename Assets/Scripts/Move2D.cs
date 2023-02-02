using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class Move2D : MonoBehaviour
{
    private Rigidbody2D rb;
    public float speed = 8;
    public float jumpForce = 28;
    private bool grounded = true;
    private bool leftWallCollision = false;
    private bool rightWallCollision = false;
    private bool hasDiedAnimation = false;
    private bool isDying = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Jump();
        GravityShift();
        Tilt();
    }

    void Move()
    {
        // Stop player from sticking to wall
        float x = Input.GetAxisRaw("Horizontal");
        if ((x < 0 && leftWallCollision) || (x > 0 && rightWallCollision)) {
            return;
        }
        float moveBy = x * speed;
        rb.velocity = new Vector2(moveBy, rb.velocity.y);

    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded == true || hasDiedAnimation)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    void GravityShift() 
    {
        if (Input.GetKeyDown(KeyCode.E) && grounded)
        {
            rb.gravityScale = -rb.gravityScale;
            jumpForce = -jumpForce;
        }
    }
    
    void Tilt() 
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            float currentRotation = rb.rotation;

            if (currentRotation == 0f)
            {
                rb.SetRotation(270f);
            }
            if ((currentRotation + 360f) % 360f == 270f)
            {
                float deltaY = Math.Abs(rb.gravityScale) / rb.gravityScale * 3;
                rb.SetRotation(0f);
                                    
                rb.position = new Vector2(rb.position.x, rb.position.y + deltaY);
            }
        }
    }

    async void OnCollisionEnter2D(Collision2D obj)
    {
        if (obj.gameObject.tag == "Ground")
        {
            grounded = true;
        }

        // Check which side collision occured with a wall.
        if (obj.gameObject.CompareTag("Wall"))
        {
            if (obj.GetContact(0).point.x < rb.position.x)
            {
                leftWallCollision = true;
            }
            if (obj.GetContact(0).point.x > rb.position.x)
            {
                rightWallCollision = true;
            }
        }

        if (obj.gameObject.tag == "MovingPlatform")
        {
            rb.transform.SetParent(obj.transform);
            grounded = true;    
        }

        if (obj.gameObject.tag == "Spikes" && !isDying)
        {
            // Boolean to prevent async function from running multiple times
            isDying = true;
            await DeathAnimation();

            Vector2 rebirthWaypointPosition = GameObject.FindGameObjectsWithTag("RebirthWaypoint")[0].
            transform.position;
            rb.position = rebirthWaypointPosition;
            rb.SetRotation(0f);
            isDying = false;
        }
    }

    void OnCollisionExit2D(Collision2D obj)
    {
        if (obj.gameObject.tag == "Ground")
        {
            grounded = false;
        }

        if (obj.gameObject.tag == "Wall")
        {
            leftWallCollision = false;
            rightWallCollision = false;
        }

        if (obj.gameObject.tag == "MovingPlatform") 
        {
            rb.transform.SetParent(null);
            grounded = false;
            leftWallCollision = false;
            rightWallCollision = false;
        }
    }

    private IEnumerator Blink()
    {
        SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();

        Color defaultColor = playerSprite.color;

        playerSprite.color = new Color(1, 0, 0, 1);

        yield return new WaitForSeconds(0.05f);

        playerSprite.color = new Color(1, 1, 1, 1);
    }

    private async Task DeathAnimation()
    {
        StartCoroutine(Blink());
        hasDiedAnimation = true;
        Jump();
        hasDiedAnimation = false;
        await Task.Delay(TimeSpan.FromSeconds(0.6));
    }
}
