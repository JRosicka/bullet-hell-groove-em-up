using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SplineMesh;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Emits <see cref="Bullet"/>s and gives them behaviors (bezier curves to follow, specific properties, etc)
/// </summary>
public class Emitter : MonoBehaviour {
    public List<Bullet> SpawnedBullets = new List<Bullet>();

    /// <summary>
    /// All of the details needed for firing a group of bullets, to be configured in the inspector. These config
    /// details are used to set up individual <see cref="BulletConfig"/>s.
    /// </summary>
    [Serializable]
    public struct EmissionConfig {
        /// <summary>
        /// The Bullet prefab to spawn
        /// </summary>
        public Bullet BulletPrefab;
        /// <summary>
        /// How many bullets to fire
        /// </summary>
        [Range(0, 100)]
        public int NumberOfShots;
        /// <summary>
        /// How much time between each bullet
        /// </summary>
        [Range(0, 2)]
        public float TimeBetweenShot;
        /// <summary>
        /// How long to wait before firing the first bullet
        /// </summary>
        [Range(0, 2)]
        public float DelayBeforeFirstShot;
        /// <summary>
        /// The start rotation, in degrees, to initially aim at
        /// </summary>
        [Range(-360, 360)]
        public int StartRotation;
        /// <summary>
        /// How wide of a rotation, in degrees, to fire bullets
        /// </summary>
        [Range(0, 1800)]
        public int RotationArc;
        /// <summary>
        /// If true, rotate clockwise. If False, rotate counterclockwise.
        /// </summary>
        public bool RotateClockwise; 
        /// <summary>
        /// If true, the rotation arc is based around the StartRotation symmetricall around each side.
        /// If false, the rotation arc starts at the StartRotation and continues clockwise or counterclockwise.
        /// </summary>
        public bool RotateFromCenter;
        /// <summary>
        /// How far from the emitter's location to fire the bullets
        /// </summary>
        [Range(-10, 10)] 
        public float Radius;
        /// <summary>
        /// If not null, adds an AnimateInBulletLogic script with this particular animation prefab to each bullet
        /// once it spawns
        /// </summary>
        public AnimateInBulletView AnimationPrefab;
        /// <summary>
        /// If true, then the shader for the animate-in prefab will be solid white. If false, then it will use the
        /// original sprite colors of the bullet sprite.
        /// </summary>
        public bool UseWhiteShaderForAnimateIn;
        /// <summary>
        /// The start speed for each bullet spawned
        /// </summary>
        [Range(0, 10)]
        public float BulletStartSpeed;
        public bool UseSpline;
        public Spline Spline;
        public float SplineTravelDurationSeconds;
    }
    
    /// <summary>
    /// All of the details needed for the firing of a single bullet
    /// </summary>
    protected struct BulletConfig {
        public Bullet Bullet;
        public List<BulletLogic> Logic;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;

        public float EmissionTime;
    }

    // TODO: Keep this private, we want to control things being added here by subclasses so that we can enforce ordering
    private List<BulletConfig> queuedBullets = new List<BulletConfig>();
    private float timeElapsed;
    
    /// <summary>
    /// Perform any queued NoteActions that have triggered
    /// </summary>
    private void Update() {
        if (GameController.Instance.IsResetting())
            return;
        
        timeElapsed += Time.deltaTime;

        int actionsCompleted = 0;
        for (int i = 0; i < queuedBullets.Count; i++) {
            BulletConfig bulletConfig = queuedBullets[i];
            if (bulletConfig.EmissionTime <= timeElapsed) {
                Shoot(bulletConfig);
                actionsCompleted++;
            } else {
                // Since we assume that the queuedShots list is ordered by FireTime, we know that none of the remaining shots should be fired yet
                break;
            }
        }

        queuedBullets.RemoveRange(0, actionsCompleted);
    }
    
    private void ScheduleBullets(List<BulletConfig> configs) {
        queuedBullets.AddRange(configs);
        queuedBullets = queuedBullets.OrderBy(e => e.EmissionTime).ToList();
    }

    protected void Shoot(BulletConfig bulletConfig) {
        Shoot(bulletConfig.Bullet, bulletConfig.Logic, bulletConfig.LocalPosition, bulletConfig.LocalRotation);
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
    
    /// <summary>
    /// Create and schedule <see cref="BulletConfig"/>s for the given <see cref="EmissionConfig"/>, setting up the
    /// fire positions, overall rotations and timings, passing in bullet logic, etc.
    /// </summary>
    /// <param name="config"></param>
    protected void ScheduleEmission(EmissionConfig config) {
        // Determine rotations
        float startRotationDegrees;
        float endRotationDegrees;
        int direction = config.RotateClockwise ? -1 : 1;
        if (config.RotateFromCenter) {
            startRotationDegrees = config.StartRotation - direction * config.RotationArc / 2.0f;
            endRotationDegrees = config.StartRotation + direction * config.RotationArc / 2.0f;
        } else {
            startRotationDegrees = config.StartRotation;
            endRotationDegrees = config.StartRotation + direction * config.RotationArc;
        }
        
        List<BulletConfig> bullets = new List<BulletConfig>();
        for (int i = 0; i < config.NumberOfShots; i++) {
            float lerpValue;
            if (config.NumberOfShots == 1)
                lerpValue = 0;
            else
                lerpValue = (float) i / (config.NumberOfShots - 1);
            
            // Add BulletLogic
            List<BulletLogic> logic = new List<BulletLogic>();
            if (config.AnimationPrefab != null)
                logic.Add(new AnimateInBulletLogic(config.AnimationPrefab, config.UseWhiteShaderForAnimateIn));
            logic.Add(new StartingSpeedBulletLogic(config.BulletStartSpeed));
            if (config.UseSpline)
                logic.Add(new MoveAlongSplineBulletLogic(config.Spline, config.SplineTravelDurationSeconds, false));

            bullets.Add(new BulletConfig {
                Bullet = config.BulletPrefab,
                Logic = logic,
                LocalPosition = new Vector3(config.Radius, 0),
                LocalRotation = Quaternion.AngleAxis(Mathf.Lerp(startRotationDegrees, endRotationDegrees, lerpValue), Vector3.forward),
                
                EmissionTime = timeElapsed + config.DelayBeforeFirstShot + i * config.TimeBetweenShot
            });
        }
        ScheduleBullets(bullets);
    }
    
    // TODO: Enforce somewhere that things tagged with [Emission] cannot have any parameters
    // TODO: Dummy examples, the BulletPrefab(s) and Shoot logic should be defined in subclasses of Emitter
    public EmissionConfig FarMoreComplexConfig;
    [Emission]
    // ReSharper disable once UnusedMember.Global
    public void ShootAFarMoreComplexShot() {
        ScheduleEmission(FarMoreComplexConfig);
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
