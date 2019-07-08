using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Planet : MonoBehaviour {
    [HideInInspector]
    public Rigidbody rb;
    [HideInInspector]
    public Transform tx;
    public static EventHandler planetsMerged;
    private ParticleSystem [ ] particleSystems;
    private TrailRenderer trail;
    private Vector3 currentVelocity;
    public float radius;
    public float timeToOrbit;
    public Text uiText;
    public static bool ToggleUI = false;


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
        foreach (var particleSystem in particleSystems)
            particleSystem.gameObject.transform.localScale = new Vector3(radius * 0.05f, radius * 0.05f, radius * 0.05f);

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
        Planet otherPlanet = otherObject.GetComponent<Planet>();
        if (otherMass < rb.mass)
        {
            //Calculate new values
            float newVolume = GetVolume(radius) + GetVolume(otherPlanet.radius);
            float newRadius = GetRadiusFromVolume(newVolume);
            float newDiameter = newRadius * 2.0f;

            //Set values
            transform.localScale = new Vector3(newDiameter, newDiameter, newDiameter);
            rb.mass += otherMass;
            radius = newRadius;

            SetColor(planetSpawner.maximumSize, planetSpawner.minimumSize, radius, planetSpawner.gradient, 0.1f);
            foreach (var particleSystem in particleSystems)
                particleSystem.gameObject.transform.localScale = new Vector3(radius * 0.05f, radius * 0.05f, radius * 0.05f);

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
        if (ToggleUI)
            UpdateUI();
    }

    void UpdateUI ()
    {
        Debug.Log("updating ui");
        uiText.text =
           $"Radius : {radius}" + $"  Mass : {rb.mass}\n" +
           $"Speed : {rb.velocity.magnitude}";
    }

    private void OnDrawGizmos ()
    {
        if (tx != null)
            Gizmos.DrawLine(tx.position, tx.position + currentVelocity);
    }

    /// <summary>
    /// Returns volume of a sphere in terms of radius
    /// </summary>
    /// <param name="radius"></param>
    /// <returns></returns>
    static float GetVolume (float radius)
    {
        return (4.0f / 3.0f) * Mathf.PI * Mathf.Pow(radius, 3.0f);
    }

    /// <summary>
    /// Returns radius of sphere in terms of volume
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    static float GetRadiusFromVolume (float volume)
    {
        return Mathf.Pow((3.0f * volume) / (4.0f * Mathf.PI), 1.0f / 3.0f);
    }


}
