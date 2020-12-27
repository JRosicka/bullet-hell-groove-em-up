using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Emits <see cref="Bullet"/>s and gives them behaviors (bezier curves to follow, specific properties, etc)
/// </summary>
public class Emitter : MonoBehaviour {
    public List<Bullet> SpawnedBullets = new List<Bullet>();
    
    /// <summary>
    /// All of the details needed for the firing of a single bullet
    /// </summary>
    protected struct EmissionConfig {
        public Bullet Bullet;
        public List<BulletLogic> Logic;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;

        public float EmissionTime;
    }

    // TODO: Keep this private, we want to control things being added here by subclasses so that we can enforce ordering
    private List<EmissionConfig> queuedEmissions = new List<EmissionConfig>();
    private float timeElapsed;
    
    /// <summary>
    /// Perform any queued NoteActions that have triggered
    /// </summary>
    private void Update() {
        if (GameController.Instance.IsResetting())
            return;
        
        timeElapsed += Time.deltaTime;

        int actionsCompleted = 0;
        for (int i = 0; i < queuedEmissions.Count; i++) {
            EmissionConfig emissionConfig = queuedEmissions[i];
            if (emissionConfig.EmissionTime < timeElapsed) {
                Shoot(emissionConfig);
                actionsCompleted++;
            } else {
                // Since we assume that the queuedShots list is ordered by FireTime, we know that none of the remaining shots should be fired yet
                break;
            }
        }

        queuedEmissions.RemoveRange(0, actionsCompleted);
    }

    protected void ScheduleEmission(EmissionConfig config) {
        ScheduleEmissions(new List<EmissionConfig>() {config});
    }

    protected void ScheduleEmissions(List<EmissionConfig> configs) {
        queuedEmissions.AddRange(configs);
        queuedEmissions = queuedEmissions.OrderBy(e => e.EmissionTime).ToList();
    }

    protected void Shoot(EmissionConfig emissionConfig) {
        Shoot(emissionConfig.Bullet, emissionConfig.Logic, emissionConfig.LocalPosition, emissionConfig.LocalRotation);
    }

    /// <summary>
    /// Shoot a single bullet at the specified position (relative to the emitter) and pass in the specified bullet logic
    /// </summary>
    /// <param name="bullet">Bullet prefab to shoot</param>
    /// <param name="logic">List of BulletLogic to pass to the instantiated bullet</param>
    /// <param name="localPosition">The position relative to this Emitter's transform to shoot the bullet.
    /// Zero vector by default.</param>
    /// <param name="localRotation">The rotation to shoot the bullet relative to the reference frame of the
    /// emitter's rotation. Zero Quaternion by default.</param>
    protected void Shoot(Bullet bullet, List<BulletLogic> logic, Vector3 localPosition = default, Quaternion localRotation = default) {
        if (bullet == null)
            throw new Exception("Trying to shoot a null bullet!");
        
        Transform t = transform;
        Quaternion r = t.rotation;
        
        // Apply the local rotation to the local position vector
        Vector3 rotatedLocalPosition = r * localRotation * localPosition;
        Bullet spawnedBullet = Instantiate(bullet, t.position + rotatedLocalPosition, r * localRotation, GameController.Instance.ShotBucket);
        SpawnedBullets.Add(spawnedBullet);
        spawnedBullet.AssignBulletLogic(logic);
    }

    // protected void ShootGroup(Bullet bullet, List<BulletLogic> logic, int numberOfShots,
    //     Vector3 localPosition = default) {
    //     for (int i = 0; i < numberOfShots; i++) {
    //         Shoot(bullet, logic, localPosition, localRotation);
    //     }
    // }

    protected void ShootGroup(Bullet bullet, List<BulletLogic> logic, int numberOfShots, 
                            float timeBetweenShot, float delayBeforeFirstShot = 0.0f, 
                            float startRotationDegrees = 0, float endRotationDegrees = 0, 
                            Vector3 localPosition = default) {
        if (timeBetweenShot < 0)
            throw new Exception("Cannot have a negative amount of time between each shot, silly");
        if (delayBeforeFirstShot < 0)
            throw new Exception("Cannot have a negative delay before first shot, silly");
        if (numberOfShots < 0)
            throw new Exception("Cannot have a negative number of shots to fire, silly");
        
        // Fire all the shots now if we don't have to wait to fire any shots
        // if (delayBeforeFirstShot <= 0 && timeBetweenShot <= 0) {
        //     ShootGroup(bullet, logic, numberOfShots, localPosition);
        //     return;
        // }

        List<EmissionConfig> emissions = new List<EmissionConfig>();
        for (int i = 0; i < numberOfShots; i++) {
            float lerpValue = (float) i / (numberOfShots - 1);
            emissions.Add(new EmissionConfig {
                Bullet = bullet,
                Logic = logic,
                LocalPosition = localPosition,
                LocalRotation = Quaternion.AngleAxis(Mathf.Lerp(startRotationDegrees, endRotationDegrees, lerpValue), Vector3.forward),
                
                EmissionTime = timeElapsed + delayBeforeFirstShot + i * timeBetweenShot
            });
        }
        ScheduleEmissions(emissions);
    }

    protected void ShootGroupSymmetricalArc(Bullet bullet, List<BulletLogic> logic, int numberOfShots,
                        float timeBetweenShot, float delayBeforeFirstShot = 0.0f,
                        float rotationDegreesRange = 0, float startRotationDegrees = 0,
                        Vector3 localPosition = default) {
        ShootGroup(bullet, logic, numberOfShots, timeBetweenShot, delayBeforeFirstShot, startRotationDegrees - rotationDegreesRange / 2, startRotationDegrees + rotationDegreesRange / 2, localPosition);
    }

    // TODO: Enforce somewhere that things tagged with [Emission] cannot have any parameters
    [Emission]
    public void ShootBasicShot() {
        Shoot(BulletPrefab, null);
    }

    // TODO: Dummy examples, the BulletPrefab(s) and Shoot logic should be defined in subclasses of Emitter
    public Bullet BulletPrefab;
    [Range(0, 100)]
    public int NumberOfShots;
    [Range(0, 5)]
    public float TimeBetweenShot;
    [Range(0, 5)]
    public float DelayBeforeFirstShot;
    [Range(0, 900)]
    public int RotationArc;
    [Range(-900, 900)]
    public int StartRotation;
    [Range(-10, 10)] 
    public float HowFarForward;
    [Emission]
    public void ShootAFarMoreComplexShot() {
        ShootGroupSymmetricalArc(BulletPrefab, null, NumberOfShots, TimeBetweenShot, DelayBeforeFirstShot, RotationArc, StartRotation, new Vector3(HowFarForward, 0));
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(Emitter))]
    public class EmitterEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            Emitter emitter = target as Emitter;

            MethodInfo[] methods = emitter.GetType()
                .GetMethods()
                .Where(t => t.GetCustomAttributes(typeof(Emission), false).Length > 0)
                .ToArray();
            
            // Create a button for each method on this Emitter with the [Emission] attribute
            foreach (MethodInfo method in methods) {
                if (GUILayout.Button(method.Name)) {
                    if (!Application.IsPlaying(emitter))
                        Debug.LogWarning("Must be in play mode in order to test bullets");
                    else
                        method.Invoke(emitter, null);
                }
            }
        }
    }
    #endif
}
