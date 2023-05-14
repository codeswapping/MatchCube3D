using System;
using System.Collections;
using UnityEngine;

namespace MatchCube.Scripts.Animations
{
    public class UIMotionAnimation : MonoBehaviour
    {
        public Vector3 fromScale, toScale;
        public AnimationCurve AnimationCurve;
        public Vector3 fromPosition, toPosition;
        public float animationTime;

        private void OnEnable()
        {
            StartCoroutine(AnimatePanelIn());
        }


        private IEnumerator AnimatePanelIn()
        {
            yield return 0;
            var timeScale = animationTime;
            while (animationTime > 0)
            {
                yield return 0;
                animationTime -= Time.deltaTime;
                
            }
        }
        private IEnumerator AnimatePanelOut()
        {
            yield return 0;
        }
        
    }
}
