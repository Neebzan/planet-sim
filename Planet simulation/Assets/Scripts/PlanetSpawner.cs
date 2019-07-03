using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum Direction { Forwards, Backwards, Left, Right }

public class PlanetSpawner : MonoBehaviour {
    public bool enablePresetOrbit = true;
    public bool enableRandomSpawns = false;
    public int planetsToSpawn;
    public int maximumSpawnRadius;
    public float minimumSize = 0.1f;
    public float maximumSize = 5f;
    public float scaleFactor = 1.0f;

    public GameObject planetPrefab;
    public List<Planet> planets;
    public float gravitationConstant = 0.000557408f;
    public Gradient gradient;
    private Vector3 forceDirection;
    private float r;
    private float m1;
    private float m2;
    private float forceMagnitude;
    private Vector3 force;
    int merges;
    public Text uiText;


    private float sunRadius = 100;
    private float sunMass = 32294600;

    private float earthSunDistance = 1496f;
    private float earthRadius = 0.91602109537201585028497156035588f;
    private float earthMass = 100;

    private float moonEarthDistance = 3.844f;
    private float moonRadius = 0.24974550975689711692748322089753f;
    private float moonMass = 1.23f;

    private float marsMass = 10.7f;
    private float marsRadius = 0.48741351645128452871857692506772f;
    private float marsSunDistance = 2279f;

    private float jupiterMass = 31780f;
    private float jupiterRadius = 10.051789483370428521311041713395f;
    private float jupiterSunDistance = 7785f;

    private float mercuryMass = 5.53f;
    private float mercuryRadius = 0.35082270800623429205702881922278f;
    private float mercurySunDistance = 579.1f;

    private float venusMass = 81.5f;
    private float venusRadius = 0.87015533969415161292177803849848f;
    private float venusSunDistance = 1082;


    private float saturnMass = 9520;
    private float saturnRadius = 8.3725852182864898750265992626972f;
    private float saturnSunDistance = 14340f;
    internal bool useRealisticGravitationConstant;

    private void Start ()
    {
        Planet.planetsMerged += OnPlanetsMerged;

        DefineColorGradient();
        if (planetPrefab != null)
        {
            if (enablePresetOrbit) SetupSolarSystem(scaleFactor);
            if (enableRandomSpawns)
            {
                for (int i = 0; i < planetsToSpawn; i++)
                {
                    float size = UnityEngine.Random.Range(minimumSize, maximumSize);
                    SetupPlanet(size);
                }
            }
        }
        SetUIText();
    }

    void SetupSolarSystem (float scaleFactor)
    {
        if (planetPrefab != null)
        {
            earthSunDistance *= scaleFactor;
            moonEarthDistance *= scaleFactor;
            marsSunDistance *= scaleFactor;
            jupiterSunDistance *= scaleFactor;
            mercurySunDistance *= scaleFactor;
            venusSunDistance *= scaleFactor;
            saturnSunDistance *= scaleFactor;

            sunMass *= scaleFactor;
            earthMass *= scaleFactor;
            moonMass *= scaleFactor;
            marsMass *= scaleFactor;
            jupiterMass *= scaleFactor;
            mercuryMass *= scaleFactor;
            venusMass *= scaleFactor;
            saturnMass *= scaleFactor;

            sunRadius *= scaleFactor;
            earthRadius *= scaleFactor;
            moonRadius *= scaleFactor;
            marsRadius *= scaleFactor;
            jupiterRadius *= scaleFactor;
            mercuryRadius *= scaleFactor;
            venusRadius *= scaleFactor;
            saturnRadius *= scaleFactor;



            var sun = SetupPlanet(sunRadius, sunMass, Vector3.zero);
            sun.name = "sun";

            var earth = SetupPlanet(earthRadius, earthMass, sun.tx.right * earthSunDistance);
            earth.name = "earth";

            var moon = SetupPlanet(moonRadius, moonMass, sun.tx.right * earthSunDistance + (earth.tx.up * moonEarthDistance));
            moon.name = "moon";

            var mars = SetupPlanet(marsRadius, marsMass, sun.tx.right * marsSunDistance);
            mars.name = "mars";

            var jupiter = SetupPlanet(jupiterRadius, jupiterMass, sun.tx.right * jupiterSunDistance);
            jupiter.name = "jupiter";

            var mercury = SetupPlanet(mercuryRadius, mercuryMass, sun.tx.right * mercurySunDistance);
            mercury.name = "mercury";

            var venus = SetupPlanet(venusRadius, venusMass, sun.tx.right * venusSunDistance);
            venus.name = "venus";

            var saturn = SetupPlanet(saturnRadius, saturnMass, sun.tx.right * saturnSunDistance);
            saturn.name = "saturn";


            SetupOrbit(sun, earth, earthSunDistance, Direction.Forwards);
            SetupOrbit(earth, moon, moonEarthDistance, Direction.Right);
            SetupOrbit(sun, mars, marsSunDistance, Direction.Forwards);
            SetupOrbit(sun, jupiter, jupiterSunDistance, Direction.Forwards);
            SetupOrbit(sun, mercury, mercurySunDistance, Direction.Forwards);
            SetupOrbit(sun, venus, venusSunDistance, Direction.Forwards);
            SetupOrbit(sun, saturn, saturnSunDistance, Direction.Forwards);
        }
    }

    void SetupOrbit (Planet centerOfOrbit, Planet orbitingPlanet, float distanceBetween, Direction orbitDirection)
    {
        Vector3 orbitalVelocityDirection;

        switch (orbitDirection)
        {
            case Direction.Forwards:
                orbitalVelocityDirection = Vector3.Cross(centerOfOrbit.tx.up, centerOfOrbit.tx.right);
                break;
            case Direction.Backwards:
                orbitalVelocityDirection = Vector3.Cross(-centerOfOrbit.tx.up, centerOfOrbit.tx.right);
                break;
            case Direction.Left:
                orbitalVelocityDirection = Vector3.Cross(-centerOfOrbit.tx.up, -centerOfOrbit.tx.forward);
                break;
            case Direction.Right:
                orbitalVelocityDirection = Vector3.Cross(centerOfOrbit.tx.up, -centerOfOrbit.tx.forward);
                break;
            default:
                orbitalVelocityDirection = Vector3.Cross(centerOfOrbit.tx.up, centerOfOrbit.tx.right);
                break;
        }
        float orbitalVelocity = Mathf.Sqrt((gravitationConstant * centerOfOrbit.rb.mass) / distanceBetween);
        orbitingPlanet.rb.velocity = orbitalVelocityDirection * orbitalVelocity + centerOfOrbit.rb.velocity;
        orbitingPlanet.tx.LookAt(orbitingPlanet.tx.position + centerOfOrbit.tx.forward * (orbitingPlanet.size + 100));
        orbitingPlanet.timeToOrbit = ((2 * Mathf.PI * distanceBetween) / orbitalVelocity) / 60;
    }

    void SetupPlanet (float size)
    {
        var planet = Instantiate(planetPrefab);
        var controller = planet.GetComponent<Planet>();

        controller.size = size;

        planet.transform.localPosition = new Vector3(
            transform.localPosition.x + UnityEngine.Random.Range(-maximumSpawnRadius, maximumSpawnRadius),
            transform.localPosition.y + UnityEngine.Random.Range(-maximumSpawnRadius, maximumSpawnRadius),
            transform.localPosition.z + UnityEngine.Random.Range(-maximumSpawnRadius, maximumSpawnRadius));
        planet.transform.localScale = new Vector3(size, size, size);
        planet.GetComponent<Rigidbody>().mass *= size * 20;

        controller.SetColor(maximumSize, minimumSize, size, gradient, 0.5f);


        Vector3 randomStartForceDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
        float randomStartForceMagnitude = UnityEngine.Random.Range(0, 100);
        controller.rb.velocity = randomStartForceDirection * randomStartForceMagnitude;
    }

    Planet SetupPlanet (float size, float mass, Vector3 position)
    {
        var planet = Instantiate(planetPrefab);
        var controller = planet.GetComponent<Planet>();

        controller.size = size;
        planet.transform.localScale = new Vector3(size, size, size);
        planet.GetComponent<Rigidbody>().mass = mass;
        controller.tx.position = position;
        controller.SetColor(maximumSize, minimumSize, size, gradient, 0.5f);

        return controller;
    }

    public void OnPlanetsMerged (object sender, EventArgs e)
    {
        merges++;
        SetUIText();
    }

    void SetUIText ()
    {
        uiText.text = $"Planets : { planets.Count.ToString()}\nMerges : {merges.ToString()}";
    }

    private void DefineColorGradient ()
    {
        gradient = new Gradient();

        GradientColorKey [ ] gck = new GradientColorKey [ 2 ];
        gck [ 0 ].color = new Color(1, 1, 1, 1);
        gck [ 0 ].time = 0;
        gck [ 1 ].color = new Color(1, 0.4739144f, 0, 1);
        gck [ 1 ].time = 1f;

        GradientAlphaKey [ ] gak = new GradientAlphaKey [ 2 ];
        gak [ 0 ].alpha = 1f;
        gak [ 0 ].time = 0;
        gak [ 1 ].alpha = 1f;
        gak [ 1 ].time = 1f;
        gradient.SetKeys(gck, gak);
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        for (int i = 0; i < planets.Count; i++)
        {
            for (int x = i + 1; x < planets.Count; x++)
            {
                if (i == x) continue;
                Attract(planets [ i ], planets [ x ]);
            }
        }
    }

    void Attract (Planet attractor, Planet attracted)
    {
        forceDirection = attractor.tx.position - attracted.tx.position;
        r = forceDirection.sqrMagnitude;
        m1 = attractor.rb.mass;
        m2 = attracted.rb.mass;
        forceMagnitude = gravitationConstant * (m1 * m2) / r;
        force = forceMagnitude * forceDirection.normalized;
        attracted.rb.AddForce(force);
        attractor.rb.AddForce(-force);
    }
}
