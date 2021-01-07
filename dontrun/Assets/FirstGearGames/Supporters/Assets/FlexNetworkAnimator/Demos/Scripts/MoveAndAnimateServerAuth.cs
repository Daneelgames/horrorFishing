
using Mirror;
using UnityEngine;

namespace FirstGearGames.Mirrors.Assets.FlexNetworkAnimators.Demos
{

    public class MoveAndAnimateServerAuth : NetworkBehaviour
    {
        private FlexNetworkAnimator _flexNetworkAnimator;
        private Animator _animator;
        private bool _jump = false;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _flexNetworkAnimator = GetComponent<FlexNetworkAnimator>();
        }

        private void Update()
        {
            if (base.hasAuthority)
                CheckJump();
        }

        private void FixedUpdate()
        {
            if (base.hasAuthority)
            {
                float horizontal = Input.GetAxis("Horizontal");
                Move(horizontal);
                CmdUpdateHorizontal(horizontal);
            }

            if (base.isServer)
                Jump();
        }

        /// <summary>
        /// Checks to jump.
        /// </summary>
        private void CheckJump()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                CmdJump();
        }

        /// <summary>
        /// Moves left and right.
        /// </summary>
        /// <param name="horizontal"></param>
        private void Move(float horizontal)
        {
            float moveRate = 1f;
            transform.position += new Vector3(horizontal, 0f, 0f) * moveRate * Time.deltaTime;
        }

        /// <summary>
        /// Updates the aniamtor based on move values.
        /// </summary>
        private void UpdateHorizontal(float horizontal)
        {
            _animator.SetFloat("Horizontal", horizontal);
        }

        /// <summary>
        /// Jumps.
        /// </summary>
        private void Jump()
        {
            if (!_jump)
                return;
            _jump = false;

            string jumpString = "Jump";
            _animator.SetTrigger(jumpString);
            if (_flexNetworkAnimator != null)
                _flexNetworkAnimator.SetTrigger(Animator.StringToHash(jumpString));
        }

        [Command]
        private void CmdJump()
        {
            _jump = true;
        }
        [Command]
        private void CmdUpdateHorizontal(float horizontal)
        {
            UpdateHorizontal(horizontal);
        }


    }


}