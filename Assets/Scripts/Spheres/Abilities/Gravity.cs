﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Abilities {
    public class Gravity : Ability {
        const int BONUS = (int)(FORCE * 50);
        const float MAX_DIST = 3.5f;
        const float FORCE = 4;
        const string GRAVITY_EFFECT_NAME = "gravity";

        List<Rigidbody> inRange = new List<Rigidbody>();

        Transform distortion;

        public override int GetValue() {
            return 10;
        }

        public override void Initialize(SphereStats s) {
            base.Initialize(s);
            distortion = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("GravityDistortion")).transform;
            distortion.parent = gameObject.transform;
            distortion.localPosition = distortion.position;
            distortion.localScale = new Vector3(2 * MAX_DIST, 2 * MAX_DIST, 2 * MAX_DIST);
            distortion.name = GRAVITY_EFFECT_NAME;

            AddSphereTrigger(MAX_DIST);
        }

        public override void FixedUpdate(Rigidbody rigidbody) {
            for (int i = 0; i < inRange.Count; i++) {
                var rb = inRange[i];
                if (rb == null)
                    inRange.RemoveAt(i--);
                else {
                    float force = Vector3.Distance(gameObject.transform.position, rb.transform.position) / MAX_DIST * FORCE;
                    Vector3 forceVector = (gameObject.transform.position - rb.transform.position) * force;
                    rb.AddForce(forceVector, ForceMode.Force);
                }
            }
        }

        public override void OnFieldEnter(SphereStats stats) {
            inRange.Add(stats.GetComponent<Rigidbody>());
            stats.bonusManager.AddBonus(this, new Bonus(stats, (source, target) => { return Mathf.RoundToInt((1 - (Vector3.Distance(source.transform.position, target.transform.position) / MAX_DIST)) * BONUS); }));
        }

        public override void OnFieldExit(SphereStats stats) {
            stats.bonusManager.RemoveBonus(this);
        }

        public override IEnumerator Pop() {
            Transform t = gameObject.transform.Find(GRAVITY_EFFECT_NAME);
            float val = 0;
            while (val < 1) {
                t.localScale = Vector3.Lerp(t.localScale, Vector3.zero, val);
                val += Time.deltaTime * 4;
                yield return new WaitForEndOfFrame();
            }
            t.transform.localScale = Vector3.zero;
        }

        public override Ability Clone() {
            return new Gravity();
        }

        public override void OnRemove() {
            UnityEngine.Object.Destroy(distortion.gameObject);
        }
    }
}