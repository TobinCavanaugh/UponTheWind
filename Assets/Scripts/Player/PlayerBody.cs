﻿using System.Collections;
using DG.Tweening;
using Map;
using MilkShake;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerBody : MonoBehaviour
    {
        public SplineBasedBirdController splineBasedBirdController;
        public float timeBeforeReturn = 2f;
        public float lerpSpeed = 1f;
        public float boostMult = 15f;
        private float curTime = 0;
        public AnimationCurve boostCurve;
        public ShakePreset boostShake;

        [Space]
        public float slowedSpeed = 35;
        public float slowTime = .5f;
        public float slowDelay = 3f;
        
        public float slowedLerpSpeed = 1.5f;

        public AnimationCurve slowCurve = AnimationCurve.Linear(0,0, 1,1);

        public float returnTime = 2f;
        public AnimationCurve returnCurve = AnimationCurve.Linear(0,0, 1,1);

        private void Update()
        {
            curTime += Time.deltaTime;

            if (curTime >= timeBeforeReturn)
            {
                splineBasedBirdController.curOffset = Vector3.Lerp(splineBasedBirdController.curOffset, Vector3.zero,
                    Time.deltaTime * lerpSpeed);
            }
        }

        private Tweener _tweener;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out AirCurrent airCurrent))
            {
                curTime = 0;
                
                _tweener?.Kill();
                _tweener = DOVirtual.Vector3(splineBasedBirdController.curOffset,
                    airCurrent.GetBoostDirection() * boostMult,
                    timeBeforeReturn,
                    x =>
                    {
                        splineBasedBirdController.curOffset = x;
                    })
                    .SetEase(boostCurve);

                Shaker.GlobalShakers[0].Shake(boostShake);
            }
            
            if (other.TryGetComponent(out FeatherPickup featherPickup))
            {
                featherPickup.Pickup(this.transform);
            }
        }

        private Tweener _slowTweener;
        private void OnCollisionEnter(Collision collision)
        {
            _slowTweener?.Kill();

            StopAllCoroutines();
            DOVirtual.Float(splineBasedBirdController.slowMult, slowedSpeed, slowTime, value =>
            {
                splineBasedBirdController.slowMult = value;
                splineBasedBirdController.posLerpMult = slowedLerpSpeed;
            }).SetEase(slowCurve).OnComplete(() => StartCoroutine(SlowReturn()));
        }

        private Tweener _returnTweener;
        private IEnumerator SlowReturn()
        {
            _returnTweener?.Kill();

            yield return new WaitForSeconds(slowDelay);

            DOVirtual.Float(splineBasedBirdController.slowMult, 1, returnTime, value =>
            {
                splineBasedBirdController.slowMult = value;
            }).SetEase(returnCurve).OnComplete(() =>
            {
                splineBasedBirdController.posLerpMult = 1;
            });
        }
    }
}