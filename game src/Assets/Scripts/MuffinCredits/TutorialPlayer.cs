using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MuffinCredits.TutorialPlayer
{
    [Serializable]
    public class TutorialPlayer
    {
        public TutorialPlayer(Rigidbody2D dreaming, SpriteRenderer sprite, TilemapCollider2D ladder, TilemapCollider2D ground,
                              float friction, float ladderSpeed, float jumpSpeed, float jumpGravity, Vector2 max, Vector2 step)
        {
            dreamingBody = dreaming;
            dreamingSprite = sprite;
            ladderCollider = ladder;
            groundCollider = ground;

            this.friction = friction;
            this.ladderSpeed = ladderSpeed;
            this.jumpSpeed = jumpSpeed;
            this.jumpGravity = jumpGravity;
            maximumVelocity = max;
            velocityStep = step;
        }

        public Rigidbody2D dreamingBody;
        public SpriteRenderer dreamingSprite;
        public TilemapCollider2D ladderCollider;
        public TilemapCollider2D groundCollider;
        
        public float friction;
        public float ladderSpeed;
        public float jumpSpeed;
        public float jumpGravity;
        public Vector2 maximumVelocity;
        public Vector2 velocityStep;

        private bool facingRight = true;
        public bool inAir = false;

        public void Update()
        {
            bool pressingJump = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
            if (!inAir && !dreamingBody.IsTouching(ladderCollider) && pressingJump)
            {
                dreamingBody.gravityScale = 15f;
                Vector2 jumpVelocity = dreamingBody.velocity;
                jumpVelocity.y = jumpSpeed;
                dreamingBody.velocity = jumpVelocity;
            }

            if (inAir && dreamingBody.IsTouching(groundCollider))
            {
                dreamingBody.gravityScale = 1f;
            }
        }

        public void FixedUpdate()
        {
            CheckIfInAir();
            AddToVelocity();
            ClimbLadder();
        
            float frictionalForce = friction;
            frictionalForce *= dreamingBody.velocity.magnitude;
            frictionalForce *= Time.fixedDeltaTime;
            dreamingBody.velocity = Vector2.MoveTowards(dreamingBody.velocity, Vector2.zero, frictionalForce);
        }


        private void AddToVelocity()
        {
            bool changedDirection;
            bool pressingLeft = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
            bool pressingRight = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
            if (facingRight) changedDirection = pressingLeft;
            else changedDirection = pressingRight;
            
            if (changedDirection && !(pressingLeft && pressingRight))
            {
                dreamingSprite.flipX = facingRight;
                facingRight = !facingRight;
                Vector2 flippedVelocity = dreamingBody.velocity;
                flippedVelocity.x *= -1;
                dreamingBody.velocity = flippedVelocity;
            }

            if (pressingLeft || pressingRight) 
            {
                Vector2 addedVelocity = dreamingBody.velocity;
                Vector2 realMaximumVelocity = maximumVelocity * (facingRight ? 1 : -1);
                addedVelocity += (facingRight ? 1 : -1) * Time.deltaTime * velocityStep;

                if (facingRight) addedVelocity.x = Mathf.Min(addedVelocity.x, realMaximumVelocity.x);
                else addedVelocity.x = Mathf.Max(addedVelocity.x, realMaximumVelocity.x);

                dreamingBody.velocity = addedVelocity;
            }
        }

        private void ClimbLadder()
        {
            if (!dreamingBody.IsTouching(ladderCollider))
            {
                dreamingBody.gravityScale = inAir ? jumpGravity : 1;
                return;
            }

            dreamingBody.gravityScale = 0f;
            Vector2 climbVelocity = dreamingBody.velocity;
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) climbVelocity.y = ladderSpeed;
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) climbVelocity.y = -ladderSpeed;

            dreamingBody.velocity = climbVelocity;
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

            inAir = !(touchingGround || dreamingBody.IsTouching(ladderCollider));
        }

        private bool CloseEnough(Vector2 a, Vector2 b, float tolerance = 1e-2f)
        {
            return (a - b).magnitude <= tolerance;
        }
    }
}