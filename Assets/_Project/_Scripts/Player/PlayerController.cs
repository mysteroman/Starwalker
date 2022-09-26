using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starwalker.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField, Range(0, 10)] private float acceleration;
        [SerializeField, Range(0, 10)] private float jumpPower, walkSpeed, runSpeed;
        [SerializeField, Range(0, 1)] private float airJumpFactor, wallJumpFactor, airSpeedFactor;

        [Header("Dependencies")]
        [SerializeField] private Collider2D collision;
        [SerializeField] private Rigidbody2D rigidBody2D;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private PlayerAnimator animator;
        [SerializeField] private Collider2D leftWallJumpHitbox, rightWallJumpHitbox;

        private const float JUMP_COOLDOWN = .1f;
        private const float GROUNDED_RADIUS = .01f;

        private InputActions _actions;
        private bool _canJump = true, _running, _swimming, _sitting, _rightFacing = true;
        private int _jumps;

        private float _lastDirection;
        private bool _directionConflict;

        private bool Grounded
        {
            get
            {
                var point = collision.bounds.min;
                point.y -= GROUNDED_RADIUS;
                Vector2 size = new(collision.bounds.size.x, GROUNDED_RADIUS);

                Collider2D[] colliders = Physics2D.OverlapBoxAll(point, size, 0, groundLayer);
                foreach (var collider in colliders)
                {
                    if (collider.gameObject != gameObject)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        private float MovementDirection
        {
            get
            {
                bool left = _actions.Default.Left.IsPressed(), right = _actions.Default.Right.IsPressed();
                bool conflict = left && right;
                if (conflict && !_directionConflict) _lastDirection = -_lastDirection;
                if (_directionConflict = conflict) return _lastDirection;
                return _lastDirection = right ? 1 : left ? -1 : 0;
            }
        }
        private bool CanWallJump
        {
            get
            {
                if (Grounded) return false;
                float direction = MovementDirection;
                if (direction == 0) return false;
                var collider = direction > 0 ? rightWallJumpHitbox : leftWallJumpHitbox;
                return collider.IsTouchingLayers(groundLayer);
            }
        }
        private bool Crouching => MovementDirection == 0 && Grounded && _actions.Default.Crouch.IsPressed();

        private void Awake()
        {
            _actions = new();
        }

        private void OnEnable()
        {
            _actions.Enable();
        }

        private void OnDisable()
        {
            _actions.Disable();
        }

        private void Start()
        {
            _canJump = true;
        }

        private void Update()
        {
            HandleMovement();
            Animate();
        }

        private void HandleMovement()
        {
            Jump();
            Strafe();
        }

        private void Jump()
        {
            IEnumerator Cooldown()
            {
                yield return new WaitForSeconds(JUMP_COOLDOWN);
                _canJump = true;
            }

            if (!_canJump) return;
            if (Grounded)
            {
                _jumps = 2;
            }
            if (_actions.Default.Jump.IsPressed())
            {
                if (!_actions.Default.Jump.WasPressedThisFrame() && rigidBody2D.velocity.y > 0.1) return;

                if (CanWallJump)
                {
                    var direction = MovementDirection;
                    var motion = rigidBody2D.velocity;
                    motion.y = jumpPower;
                    motion.x = -direction * jumpPower * wallJumpFactor;
                    _rightFacing = direction < 0;
                    rigidBody2D.velocity = motion;
                    _canJump = false;
                    animator.Jump(direction > 0 ? JumpType.WallJumpRight : JumpType.WallJumpLeft);
                    StartCoroutine(Cooldown());
                    return;
                }

                if (_jumps > 0)
                {
                    float power = jumpPower;
                    if (!Grounded) power *= airJumpFactor;
                    var motion = rigidBody2D.velocity;
                    motion.y = MathF.Max(motion.y + power, power);
                    if (Grounded) motion.y += MathF.Abs(motion.x) * .05f;
                    rigidBody2D.velocity = motion;
                    --_jumps;
                    _canJump = false;
                    animator.Jump(Grounded ? JumpType.Grounded : JumpType.Aerial);
                    StartCoroutine(Cooldown());
                }
            }
        }

        private void Strafe()
        {
            var direction = MovementDirection;
            if (direction == 0) return;

            var motion = rigidBody2D.velocity;

            if (Grounded)
            {
                _running = _actions.Default.Run.IsPressed();
                _rightFacing = direction > 0;
            }

            var (Min, Max) = (ValueTuple<Func<float, float, float>, Func<float, float, float>>)(direction > 0 ? (MathF.Min, MathF.Max) : (MathF.Max, MathF.Min));

            float topSpeed = walkSpeed * direction;
            float delta = acceleration * topSpeed * Time.fixedDeltaTime;
            if (_running) topSpeed *= runSpeed;
            if (!Grounded) delta *= airSpeedFactor;

            if (direction > 0 ? motion.x < topSpeed : motion.x > topSpeed) motion.x = Min(motion.x + delta, topSpeed);
            else if ((direction > 0 ? motion.x > topSpeed : motion.x < topSpeed) && Grounded) motion.x = Max(topSpeed, motion.x - delta);

            rigidBody2D.velocity = motion;
        }

        private void Animate()
        {
            animator.SetFacing(_rightFacing);
            animator.SetGrounded(Grounded);
            animator.SetCrouching(Crouching);
            animator.SetSwimming(_swimming);
            animator.SetSitting(_sitting);
            animator.SetHorizontalMotion(MathF.Abs(rigidBody2D.velocity.x));
        }
    }
}