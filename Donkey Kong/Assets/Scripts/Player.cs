using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class Player : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite[] runSprites;
    public Sprite climbSprite;
    public Sprite standSprite;
    private int spriteIndex;

    private new Rigidbody2D rigidbody;
    private new Collider2D collider;

    private Collider2D[] results;
    private Collider2D[] nearbyObjects;
    private Vector2 direction;
    public float nearbyRadius = 5f;

    public float moveSpeed = 1f;
    public float jumpStrength = 1f;
    public float holdSpaceGravityScale = 0.5f;
    public float fastFallGravityScale = 2f;
    private float defaultGravityScale = 1f;

    private Vector2 bouncingStartPos;
    private GameObject bounceTarget;
    private Vector2 bounceDirection;
    public float bounceForce;
    public float bouncingTimeScale;

    public bool grounded;
    private bool climbing;
    private bool jumping;
    public bool bouncing;

    public GameObject lastRoom;
    public GameObject nextRoom;
    public GameObject player;

    private void Start()
    {
        (lastRoom) = GameObject.Find("PrevRoom");
        (nextRoom) = GameObject.Find("EndGoal");
        (player) = GameObject.Find("Mario");

        if (FindObjectOfType<GameManager>().roomEnter)
        {
            player.transform.position = lastRoom.transform.position;
        }
        else
        {
            player.transform.position = nextRoom.transform.position;
        }
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        results = new Collider2D[4];
    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(AnimateSprite), 1f / 12f, 1f / 12f);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void CheckCollision()
    {
        grounded = false;
        climbing = false;

        Vector2 size = collider.bounds.size;
        size.y += 0.1f;
        size.x /= 2f;

        int amount = Physics2D.OverlapBoxNonAlloc(transform.position, size, 0f, results);
        for (int i = 0; i < amount; i++)
        {
            if (results[i].transform.gameObject.GetComponent<TagScript>() != null)
            {
                if (results[i].transform.gameObject.GetComponent<TagScript>().tags.Contains("Solid"))
                {
                    grounded = true;
                }
            }
        }

    }

    private void CheckBounce()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            float minDist = 999f;
            bounceTarget = null;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, nearbyRadius);
            foreach (Collider2D col in hits)
            {
                if (col.transform.gameObject.GetComponent<TagScript>())
                {
                    if (col.transform.gameObject.GetComponent<TagScript>().tags.Contains("Bounceable")) {
                        float dist = Vector2.Distance(transform.position, col.transform.position);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            bounceTarget = col.gameObject;
                        }
                    }
                }
            }
            if (bounceTarget)
            {
                bouncing = true;
                bouncingStartPos = Input.mousePosition;
            }
        }

        if (bouncing && Input.GetKey(KeyCode.Mouse1))
        {
            Time.timeScale = bouncingTimeScale;
            bounceDirection = (new Vector2(Input.mousePosition.x, Input.mousePosition.y) - bouncingStartPos).normalized;
        }

        if (bouncing && Input.GetKeyUp(KeyCode.Mouse1))
        {
            bouncing = false;
            rigidbody.velocity = new Vector2(0, 0);
            rigidbody.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);
            if (bounceTarget.GetComponent<TagScript>().tags.Contains("MoveableByBounce") && bounceTarget.GetComponent<Rigidbody2D>())
            {
                rigidbody.AddForce(bounceDirection * -1 * bounceForce, ForceMode2D.Impulse);
            }
            Time.timeScale = 1f;
        }


    }

    private void Update()
    {
        DebugClass.DrawCircle(transform.position, nearbyRadius, 12, Color.green);
        CheckCollision();
        CheckBounce();


        if(climbing)
        {
            direction.y = Input.GetAxis("Vertical") * moveSpeed;
        }
        else if(Input.GetButtonDown("Jump") && grounded)
        {
            rigidbody.AddForce(new Vector2(0, jumpStrength), ForceMode2D.Force);
            jumping = true;
        }

        if (Input.GetButton("Jump") && jumping)
        {
            rigidbody.gravityScale = holdSpaceGravityScale;
        }
        else if (!grounded && Input.GetAxis("Vertical") < 0)
        {
            rigidbody.gravityScale = fastFallGravityScale;
        }
        else
        {
            rigidbody.gravityScale = defaultGravityScale;
        }

        direction.x = Input.GetAxis("Horizontal") * moveSpeed;
    }

    private void FixedUpdate()
    {
        rigidbody.transform.Translate(direction*Time.deltaTime);
    }

    private void AnimateSprite()
    {
        if(climbing)
        {
            spriteRenderer.sprite = climbSprite;
        }
        else if(direction.x != 0f)
        {
            spriteIndex++;

            if(spriteIndex >= runSprites.Length)
            {
                spriteIndex = 0;
            }
            spriteRenderer.sprite = runSprites[spriteIndex];
        }
        else
        {
            spriteRenderer.sprite = standSprite;
        }

        if(direction.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if(direction.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Objective"))
        {
            FindObjectOfType<GameManager>().roomEnter = true;
            enabled = false;
            FindObjectOfType<GameManager>().LevelComplete();
        }
        else if (collision.gameObject.CompareTag("PreviousRoom"))
        {
            FindObjectOfType<GameManager>().roomEnter = false;
            enabled = false;
            FindObjectOfType<GameManager>().LastRoom();
        }
        else if(collision.gameObject.CompareTag("Obstacle"))
        {
            enabled = false;
            FindObjectOfType<GameManager>().LevelFailed();
        }
    }
}
