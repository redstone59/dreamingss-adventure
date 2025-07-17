using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MuffinCredits.JumpKingPlayer
{
    [Serializable]
    public class JumpKingPlayer
    {
        public JumpKingPlayer(Rigidbody2D dreaming, SpriteRenderer sprite, TilemapCollider2D ladder, TilemapCollider2D ground,
                              float maxJumpLength, float maxJumpVelocity, float playerGravity,
                              float acceleration, float maxHorizontalVelocity, float friction)
        {
            dreamingBody = dreaming;
            dreamingSprite = sprite;
            ladderCollider = ladder;
            groundCollider = ground;

            this.maxJumpLength = maxJumpLength;
            this.maxJumpVelocity = maxJumpVelocity;

            this.acceleration = acceleration;
            this.maxHorizontalVelocity = maxHorizontalVelocity;
            this.friction = friction;

            this.gravityScale = playerGravity;
        }

        public Rigidbody2D dreamingBody;
        public SpriteRenderer dreamingSprite;
        public TilemapCollider2D ladderCollider;
        public TilemapCollider2D groundCollider;

        private float secondsHeld = 0;
        public float maxJumpLength;
        public float maxJumpVelocity;
        private Func<bool> pressingUp;
        public float directionRotate;

        public float acceleration;
        public float maxHorizontalVelocity;
        public float friction;

        public float gravityScale;
        private bool hasSwitched = false;
        public bool onGround = true;

        public void Initialise()
        {
            dreamingBody.gravityScale = gravityScale;
            pressingUp = () => Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        }

        public void Update()
        {
            if (!hasSwitched)
            {
                Initialise();
                hasSwitched = true;
            }

            CheckIfInAir();

            bool pressingLeft = GetKeys(KeyCode.A, KeyCode.LeftArrow);
            bool pressingRight = GetKeys(KeyCode.D, KeyCode.RightArrow);
            if (onGround && pressingUp())
            {
                if (!dreamingBody.IsTouching(ladderCollider))
                    dreamingBody.velocity = new(0, dreamingBody.velocity.y);
                secondsHeld += Time.deltaTime;
                secondsHeld = Mathf.Min(secondsHeld, maxJumpLength);
                float percentHeld = secondsHeld / maxJumpLength;
                dreamingSprite.color = Color.Lerp(Color.white, Color.red, percentHeld);
            }
            else if (secondsHeld > 0)
            {
                float percentJump = secondsHeld / maxJumpLength;
                secondsHeld = 0;

                Vector2 jumpVelocity = Vector2.up * maxJumpVelocity * percentJump;

                // Rotate jump vector based on direction held
                if (pressingLeft) jumpVelocity = Quaternion.Euler(0, 0, directionRotate) * jumpVelocity;
                else if (pressingRight) jumpVelocity = Quaternion.Euler(0, 0, -directionRotate) * jumpVelocity;

                dreamingBody.velocity += jumpVelocity;
                dreamingSprite.color = Color.white;
            }

            Vector2 nextVelocity = dreamingBody.velocity;

            if (pressingLeft)
            {
                if (nextVelocity.x > 0) nextVelocity.x *= -1;
                dreamingSprite.flipX = true;
                nextVelocity.x -= acceleration * Time.deltaTime;
                nextVelocity.x = Mathf.Max(nextVelocity.x, -maxHorizontalVelocity);
            }
            else if (pressingRight)
            {
                if (nextVelocity.x < 0) nextVelocity.x *= 1;
                dreamingSprite.flipX = false;
                nextVelocity.x += acceleration * Time.deltaTime;
                nextVelocity.x = Mathf.Min(nextVelocity.x, maxHorizontalVelocity);
            }
            else
            {
                nextVelocity.x = Mathf.Lerp(nextVelocity.x, 0, friction * Time.deltaTime);
                if (Mathf.Abs(nextVelocity.x) <= 1e-4f) nextVelocity.x = 0;
            }

            dreamingBody.velocity = nextVelocity;
        }

        public void FixedUpdate()
        {

        }

        private bool GetKeys(params KeyCode[] keyCodes)
        {
            foreach (KeyCode keyCode in keyCodes)
                if (Input.GetKey(keyCode)) return true;
            
            return false;
        }

        private void CheckIfInAir()
        {
            bool touchingGround = false;

            if (dreamingBody.IsTouching(groundCollider))
            {
                List<ContactPoint2D> contactPoints = new();
                groundCollider.GetContacts(contactPoints);
                
                foreach (ContactPoint2D contactPoint in contactPoints)
                {
                    if (contactPoint.rigidbody != dreamingBody) continue;
                    if (CloseEnough(contactPoint.normal, Vector2.down))
                    {
                        touchingGround = true;
                        break;
                    }
                }
            }

            bool onLadder = dreamingBody.IsTouching(ladderCollider);
            onGround = touchingGround || onLadder;
            bool dreamingMovingDownward = dreamingBody.velocity.y <= 0;

            if (onGround)
                dreamingBody.gravityScale = 1;    
            else if (onLadder && dreamingMovingDownward)
                dreamingBody.gravityScale = gravityScale / 5f;
            else
                dreamingBody.gravityScale = gravityScale;
        }

        private bool CloseEnough(Vector2 a, Vector2 b, float tolerance = 1e-2f)
        {
            return (a - b).magnitude <= tolerance;
        }
    }
}