
using Mirror;
using System.Collections;
using UnityEngine;

namespace FirstGearGames.Mirrors.Assets.FlexNetworkAnimators.Demos
{

    public class AnimateServerObject : NetworkBehaviour
    {
        private FlexNetworkAnimator _flexNetworkAnimator;
        private Animator _animator;
        private float _horizontalTarget = 0f;

        private const float HORIZONTAL_MOVE_RATE = 2f;
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _flexNetworkAnimator = GetComponent<FlexNetworkAnimator>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            StartCoroutine(__RandomAnimate());
        }

        private void Update()
        {
            if (!base.isServer)
                return;


            string horizontal = "Horizontal";
            float current = _animator.GetFloat(horizontal);
            if (current != _horizontalTarget)
            {
                float next = Mathf.MoveTowards(current, _horizontalTarget, HORIZONTAL_MOVE_RATE * Time.deltaTime);
                _animator.SetFloat(horizontal, next);
            }
        }

        private IEnumerator __RandomAnimate()
        {
            
            string jump = "Jump";
            WaitForSeconds delay = new WaitForSeconds(2f);
            while (true)
            {
                int r = Random.Range(0, 3);
                if (r == 0)
                {
                    _horizontalTarget = -1f;
                }
                else if (r == 1)
                {
                    _horizontalTarget = 1f;
                }
                else
                {
                    _horizontalTarget = 0f;
                    _animator.SetTrigger(jump);
                        _flexNetworkAnimator.SetTrigger(Animator.StringToHash(jump));
                }

                yield return delay;
            }
        }


    }


}