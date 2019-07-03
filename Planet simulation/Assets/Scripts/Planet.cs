using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Planet : MonoBehaviour {
    [HideInInspector]
    public Rigidbody rb;
    public float size;
    public float planetConsumeRatio = 1f;
    public static EventHandler planetsMerged;
    [HideInInspector]
    public Transform tx;
    private ParticleSystem [ ] particleSystems;
    private TrailRenderer trail;
    private Vector3 currentVelocity;
    public float timeToOrbit;
    public Text uiText;

    Material material;
    PlanetSpawner planetSpawner;

    private void OnEnable ()
    {
        tx = transform;
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        trail = GetComponent<TrailRenderer>();
        planetSpawner = FindObjectOfType<PlanetSpawner>();
        material = GetComponent<Renderer>().material;
        rb = GetComponent<Rigidbody>();
        if (planetSpawner.planets == null)
            planetSpawner.planets = new List<Planet>();

        planetSpawner.planets.Add(this);
    }

    private void Start ()
    {
        AudioSource source = GetComponent<AudioSource>();
        source.volume = size;
        source.maxDistance *= size;

        foreach (var particleSystem in particleSystems)
            particleSystem.gameObject.transform.localScale = new Vector3(size * 0.05f, size * 0.05f, size * 0.05f);

        if (uiText != null)
        {
            if (timeToOrbit != 0)
                uiText.text = $"Time to orbit : {timeToOrbit}";

            else
                uiText.text = "";
        }
    }

    public void SetColor (float maxSize, float minSize, float size, Gradient gradient, float colourMultiplier)
    {
        float ColorValue = (((100f / maxSize - minSize) * (size - maxSize) + 100f) / 100f);
        Color color = gradient.Evaluate(ColorValue * colourMultiplier);
        material.SetColor("_EmissionColor", color);
        trail.material = material;
    }

    private void OnDisable ()
    {
        if (planetSpawner.planets == null)
            planetSpawner.planets = new List<Planet>();

        planetSpawner.planets.Remove(this);
    }

    private void OnCollisionEnter (Collision collision)
    {
        if (collision.gameObject.GetComponent<Rigidbody>().mass < rb.mass)
            Merge(collision);
    }

    void PushAway (Collision collision)
    {
        Vector3 dir = collision.GetContact(0).point - transform.position;
        dir = -dir.normalized;
        rb.AddForce(dir * collision.impulse.magnitude * 1.2f, ForceMode.Impulse);
    }

    void Merge (Collision collision)
    {
        GameObject otherObject = collision.gameObject;
        float otherMass = otherObject.GetComponent<Rigidbody>().mass;
        if (otherMass < rb.mass)
        {
            transform.localScale = new Vector3(
                transform.localScale.x + otherObject.transform.localScale.x * planetConsumeRatio,
                transform.localScale.y + otherObject.transform.localScale.y * planetConsumeRatio,
                transform.localScale.z + otherObject.transform.localScale.z * planetConsumeRatio);

            rb.mass += otherMass * planetConsumeRatio;
            size += otherObject.GetComponent<Planet>().size * planetConsumeRatio;

            SetColor(planetSpawner.maximumSize, planetSpawner.minimumSize, size, planetSpawner.gradient, 0.1f);

            foreach (var particleSystem in particleSystems)
                particleSystem.gameObject.transform.localScale = new Vector3(size * 0.05f, size * 0.05f, size * 0.05f);


            GameObject.Destroy(collision.gameObject);

            OnPlanetsMerged();
        }
    }

    protected virtual void OnPlanetsMerged ()
    {
        planetsMerged(this, new EventArgs());
    }

    private void Update ()
    {
        currentVelocity = rb.velocity;
    }

    private void OnDrawGizmos ()
    {
        if (tx != null)
            Gizmos.DrawLine(tx.position, tx.position + currentVelocity);
    }
}
