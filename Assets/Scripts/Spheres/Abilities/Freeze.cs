﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Abilities {
    public class Freeze : Ability {
        const float FREEZE_RANGE = 3;
        const float FREEZE_TIME = 1.5f;

        Material material;

        public override int GetValue() {
            return 50;
        }

        public override void Initialize(SphereStats s) {
            base.Initialize(s);
            material = Resources.Load<Material>("Materials/Ice");
            controller.AddMaterial(material);
            controller.SetModel(Resources.Load<Mesh>("Models/MedIce"));
            AddSphereTrigger(FREEZE_RANGE);
        }

        public override IEnumerator Pop() {
            transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            List<SphereStats> inRange = stats.FindSpheresInRange(FREEZE_RANGE);
            //gameObject.GetComponent<SphereController>().SetMaterial(material);
            //gameObject.transform.localScale = new Vector3(FREEZE_RANGE * 2, FREEZE_RANGE * 2, FREEZE_RANGE * 2);
            gameObject.GetComponent<Rigidbody>().isKinematic = true;

            GameObject ice = Resources.Load<GameObject>("Models/FreezeBridge");

            List<GameObject> iceInstances = new List<GameObject>();
            foreach (var sphere in inRange) {
                sphere.GetComponent<SphereController>().SetFreeze(true);
                sphere.bonusManager.AddBonus(this, new Bonus(stats, 25, false, FREEZE_TIME));
                GameObject iceInst = UnityEngine.Object.Instantiate(ice);
                iceInst.transform.position = transform.position;
                iceInst.transform.LookAt(sphere.transform, Vector3.right);
                iceInst.transform.localScale = new Vector3(1, 1, Mathf.Abs((sphere.transform.position - transform.position).magnitude));
                iceInstances.Add(iceInst);
            }

            yield return new WaitForSeconds(FREEZE_TIME);

            foreach (var sphere in inRange)
                if (sphere) sphere.GetComponent<SphereController>().SetFreeze(false);

            foreach (var item in iceInstances)
                UnityEngine.Object.Destroy(item);
        }

        public override Ability Clone() {
            return new Freeze();
        }
    }
}