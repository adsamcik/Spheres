﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectController : MonoBehaviour {
    const float MAX_SPEED = 10;
    const float MAX_SPEED_SQR = MAX_SPEED * MAX_SPEED;

    Rigidbody r;
    SphereStats s;
    Vector3 velocity;
    MeshRenderer mr;

    bool isFrozen = false;

    uint nextIndex = 0;

    public BonusManager bonusManager = new BonusManager();

    void Start() {
        StartCoroutine("IsInside");
        r = GetComponent<Rigidbody>();
        s = GetComponent<SphereStats>();
    }

    IEnumerator IsInside() {
        while (true) {
            if (Mathf.Abs(transform.position.x) > 8 || Mathf.Abs(transform.position.z) > 8 || transform.position.y < -1) {
                transform.position = new Vector3(0, 6, 0);
                r.velocity = Vector3.zero;
            }
            yield return new WaitForSeconds(1);
        }
    }

    public void FixedUpdate() {
        if (r.velocity.sqrMagnitude > MAX_SPEED_SQR)
            r.velocity *= 0.99f;

        s.AbilityUpdate(r);
    }

    public void SetFreeze(bool value) {
        if (value == isFrozen)
            return;

        isFrozen = value;

        if (value) {
            velocity = r.velocity;
            r.velocity = Vector3.zero;
        } else
            r.velocity = velocity;

        r.isKinematic = value;
    }

    void LoadMeshRenderer() {
        if (mr == null)
            mr = gameObject.GetComponent<MeshRenderer>();
    }

    /// <summary>
    /// Adds material on top of all materials
    /// </summary>
    /// <param name="m">Material</param>
    public void AddMaterial(Material m) {
        LoadMeshRenderer();
        Material[] materials = new Material[mr.materials.Length + 1];
        mr.materials.CopyTo(materials, 0);
        materials[mr.materials.Length] = m;
        mr.materials = materials;
    }

    /// <summary>
    /// Set <paramref name="m"/> as the only material
    /// </summary>
    /// <param name="m">Material</param>
    public void SetMaterial(Material m) {
        LoadMeshRenderer();
        mr.materials = new Material[] { m };
    }

    /// <summary>
    /// Set <paramref name="m"/> as base material (first one to render)
    /// </summary>
    /// <param name="m">Material</param>
    public void SetBaseMaterial(Material m) {
        LoadMeshRenderer();
        mr.material = m;
    }

    /// <summary>
    /// Changes model of the sphere
    /// </summary>
    /// <param name="m">Mesh</param>
    public void SetModel(Mesh m) {
        gameObject.GetComponent<MeshFilter>().mesh = m;
    }
}
