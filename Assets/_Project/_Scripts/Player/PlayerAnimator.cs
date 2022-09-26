using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starwalker.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Dependencies"), SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        [Header("Properties"), SerializeField] private ParticleSystem jumpParticlePrefab;
        [SerializeField] private ParticleSystem doubleJumpParticlePrefab;
        [SerializeField] private Transform jumpParticleSpawner, leftWallJumpParticleSpawner, rightWallJumpParticleSpawner;

        private const string GROUNDED = "Grounded";
        private const string H_MOTION = "Horizontal Motion";
        private const string CROUCHING = "Crouching";
        private const string SWIMMING = "Swimming";
        private const string SITTING = "Sitting";
        private const string JUMP = "Jump";

        public void Jump(JumpType type)
        {
            animator.SetTrigger(JUMP);
            var origin = jumpParticleSpawner.position;
            ParticleSystem particle;
            if (type == JumpType.Aerial) particle = Instantiate(doubleJumpParticlePrefab);
            else
            {
                particle = Instantiate(jumpParticlePrefab);
                var main = particle.main;
                if (type == JumpType.WallJumpLeft)
                {
                    origin = leftWallJumpParticleSpawner.position;
                    main.startRotation = Mathf.PI / 2;
                }
                if (type == JumpType.WallJumpRight)
                {
                    origin = rightWallJumpParticleSpawner.position;
                    main.startRotation = -Mathf.PI / 2;
                }
            }
            particle.transform.position = origin;
            particle.Play();
        }

        public void SetFacing(bool right) => spriteRenderer.flipX = !right;
        public void SetGrounded(bool grounded) => animator.SetBool(GROUNDED, grounded);
        public void SetCrouching(bool crouching) => animator.SetBool(CROUCHING, crouching);
        public void SetSitting(bool sitting) => animator.SetBool(SITTING, sitting);
        public void SetSwimming(bool swimming) => animator.SetBool(SWIMMING, swimming);
        public void SetHorizontalMotion(float motion) => animator.SetFloat(H_MOTION, motion);
    }
}
