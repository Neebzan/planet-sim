using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteStarfield : MonoBehaviour {
    private Transform tx;
    private ParticleSystem.Particle [ ] points;
    private new ParticleSystem particleSystem;
    private float starDistanceSqr;
    private float startClipDistanceSqr;


    public int maxStars = 100;
    public float starSize = 1.0f;
    public float starDistance = 10f;
    public float startClipDistance = 1;


    void Start ()
    {
        starDistanceSqr = starDistance * starDistance;
        startClipDistanceSqr = startClipDistance * startClipDistance;
        tx = transform;
        particleSystem = tx.GetComponent<ParticleSystem>();
    }

    private void CreateStars ()
    {
        points = new ParticleSystem.Particle [ maxStars ];
        for (int i = 0; i < maxStars; i++)
        {
            points [ i ].position = Random.insideUnitSphere * starDistance + tx.position;
            points [ i ].startColor = new Color(1, 1, 1, 1);
            points [ i ].startSize = starSize;
        }
    }

    void Update ()
    {
        if (points == null) CreateStars();

        for (int i = 0; i < maxStars; i++)
        {
            if (starDistanceSqr < (points [ i ].position - tx.position).sqrMagnitude)
            {
                points [ i ].position = Random.insideUnitSphere * starDistance + tx.position;
            }
            if (startClipDistanceSqr >= (points [ i ].position - tx.position).sqrMagnitude)
            {
                float percent = ((points [ i ].position - tx.position).sqrMagnitude / startClipDistanceSqr);
                points [ i ].startColor = new Color(1, 1, 1, percent);
                points [ i ].startSize = starSize * percent;
            }
        }

        particleSystem.SetParticles(points, points.Length);
    }
}
