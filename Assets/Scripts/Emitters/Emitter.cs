using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rumia;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using SplineMesh;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Random = System.Random;

/// <summary>
/// Emits <see cref="Bullet"/>s and gives them behaviors (bezier curves to follow, specific properties, etc)
/// </summary>
public class Emitter : MonoBehaviour {
    private List<Bullet> SpawnedBullets;

    [Serializable]
    public struct SpeedChange {
        public float StartingSpeed;
        public AnimationCurve SpeedCurve;
    }

    [Serializable]
    public struct AimChange {
        public float RotationDegrees;
        // If true, will ignore RotationDegrees value TODO: Change to disallow RotationDegrees value if true
        public bool AimTowardsPlayer;
    }

    /// <summary>
    /// All of the details needed for firing a group of bullets, to be configured in the inspector. These configuration
    /// details are used to set up individual <see cref="BulletConfig"/>s.
    /// </summary>
    [Serializable]
    public struct EmissionConfiguration {
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
        // TODO: If true, you should probably have RotateFromCenter be true as well
        public bool AimAtPlayer;
        /// <summary>
        /// The start rotation, in degrees, to initially aim at
        /// </summary>
        [Range(-360, 360)]
        public int StartRotation;
        /// <summary>
        /// How wide of a range of variance for the start rotation. A value of 10 gives each individual shot
        /// a start range of StartRotation +- 5.
        /// </summary>
        public int StartRotationVariance;
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
        // [Range(-10, 10)] 
        // public float Radius;    // TODO: This isn't useful anymore since we now have StartPositionOffset
        public Vector2 StartPositionOffset;
        /// <summary>
        /// How wide of a range of variance for the start position. A value of (10, 10) give each individual shot a
        /// start position offset of (StartPositionOffset.x +- 5, StartPositionOffset.y +- 5).
        /// </summary>
        public Vector2 StartPositionVariance;
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
        [Range(0, 30)]
        public float BulletStartSpeed;
        public AnimationCurve BulletSpeedOverTime;
        public bool UseSpline;
        public Spline Spline;
        public bool AdjustColorOverTime;
        public bool OnlyAdjustAlpha;
        public Color InitialColor;
        public Color EndColor;
        [Range(0, 10)]
        public float ColorAdjustmentDuration;
        public AnimationCurve BulletColorOverTime;
        public bool DestroyBulletAfterDuration;
        [Range(0, 30)]
        public float DestructionTime;
        public bool UseTrail;
        public TrailRendererWith2DCollider Trail;
        // TODO: Make a way to mirror across y axis, not just x axis
        public bool Reverse;

        public SpeedChange[] SpeedChanges;
        public AimChange[] AimChanges;
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

    private bool hasScheduledAnyEmissions;
    private List<SpeedSubscriptionObject> speedSubscriptionObjects = new List<SpeedSubscriptionObject>();
    private List<AimSubscriptionObject> aimSubscriptionObjects = new List<AimSubscriptionObject>();

    [Button]
    // TODO: Currently this depends on the speed subscriptions and aim subscriptions being synchronized by index, that's probably not good to assume
    public void TriggerSpeedAndAimChange(int index) {
        TriggerSpeedChange(index);
        TriggerAimChange(index);
    }
    public void TriggerSpeedChange(int index) {
        Assert.IsTrue(speedSubscriptionObjects.Count > index, $"{name}: {nameof(speedSubscriptionObjects)} count ({speedSubscriptionObjects.Count}) greater than index ({index})");
        speedSubscriptionObjects[index].TriggerSpeedCurve();
    }
    public void TriggerAimChange(int index) {
        Assert.IsTrue(aimSubscriptionObjects.Count > index, $"{name}: {nameof(aimSubscriptionObjects)} count ({aimSubscriptionObjects.Count}) greater than index ({index})");
        aimSubscriptionObjects[index].TriggerRotation();
    }

    public void UnsubscribeAllSpeedAndAimChangeObjects() {
        speedSubscriptionObjects.ForEach(o => o.UnsubscribeAllBullets());
        aimSubscriptionObjects.ForEach(o => o.UnsubscribeAllBullets());
    }

    public void Awake() {
        SpawnedBullets = new List<Bullet>();
    }

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
    /// Create and schedule <see cref="BulletConfig"/>s for the given <see cref="EmissionConfiguration"/>, setting up the
    /// fire positions, overall rotations and timings, passing in bullet logic, etc.\
    ///
    /// TODO: Document the following somewhere better
    /// Subscription objects can be configured here (type subscription object structs) in an array, and so we can define them
    /// in the config instead of having to do so in the code. Then when scheduling the emission here, we can add matching bullet logic.
    /// The order in the array should probably be roughly chronological for convenience, but doesn't necessarily have to be. 
    /// </summary>
    /// <param name="configuration"></param>
    protected void ScheduleEmission(EmissionConfiguration configuration, int startBulletIndex = 0, int endBulletIndex = -1, float overrideAimOffset = 0) {
        // Determine rotations
        float startRotationDegrees;
        float endRotationDegrees;
        int direction = configuration.RotateClockwise ? -1 : 1;
        float startRotation = configuration.StartRotation;
        if (configuration.Reverse) {
            direction *= -1;
            startRotation *= -1;
        }

        if (configuration.RotateFromCenter) {
            startRotationDegrees = startRotation - direction * configuration.RotationArc / 2.0f;
            endRotationDegrees = startRotation + direction * configuration.RotationArc / 2.0f;
        } else {
            startRotationDegrees = startRotation;
            endRotationDegrees = startRotation + direction * configuration.RotationArc;
        }

        if (overrideAimOffset != 0) {
            startRotationDegrees += overrideAimOffset;
            endRotationDegrees += overrideAimOffset;
        }
        
        foreach (SpeedChange change in configuration.SpeedChanges) {
            speedSubscriptionObjects.Add(new SpeedSubscriptionObject(change.StartingSpeed, change.SpeedCurve));
        }
        foreach (AimChange change in configuration.AimChanges) {
            aimSubscriptionObjects.Add(new AimSubscriptionObject(change.AimTowardsPlayer, change.RotationDegrees));
        }
        
        List<BulletConfig> bullets = new List<BulletConfig>();
        if (startBulletIndex == -1) {
            startBulletIndex = 0;
        }
        if (endBulletIndex == -1) {
            endBulletIndex = configuration.NumberOfShots - 1;
        }
        for (int i = startBulletIndex; i <= endBulletIndex; i++) {
            float lerpValue;
            if (configuration.NumberOfShots == 1)
                lerpValue = 0;
            else
                lerpValue = (float) i / (configuration.NumberOfShots - 1);
            
            // Add BulletLogic
            List<BulletLogic> logic = new List<BulletLogic>();
            if (configuration.AdjustColorOverTime) {
                logic.Add(new ColorOverTimeBulletLogic(configuration.InitialColor, configuration.EndColor, 
                    configuration.BulletColorOverTime, configuration.ColorAdjustmentDuration, configuration.OnlyAdjustAlpha));
            }
            if (configuration.AnimationPrefab != null)
                logic.Add(new AnimateInBulletLogic(configuration.AnimationPrefab, configuration.UseWhiteShaderForAnimateIn));
            logic.Add(new SpeedOverTimeBulletLogic(configuration.BulletStartSpeed, configuration.BulletSpeedOverTime));
            if (configuration.UseSpline) {
                logic.Add(new MoveAlongSplineBulletLogic(configuration.Spline, false, configuration.Reverse));
            }

            if (configuration.DestroyBulletAfterDuration)
                logic.Add(new DestroyAfterDurationBulletLogic(configuration.DestructionTime));
            if (configuration.UseTrail)
                logic.Add(new TrailBulletLogic(configuration.Trail));

            Vector2 startPositionOffset = configuration.StartPositionOffset;
            startPositionOffset.x +=
                UnityEngine.Random.Range(-configuration.StartPositionVariance.x / 2, configuration.StartPositionVariance.x / 2);
            startPositionOffset.y +=
                UnityEngine.Random.Range(-configuration.StartPositionVariance.y / 2, configuration.StartPositionVariance.y / 2);
            if (configuration.Reverse)
                startPositionOffset.y *= -1;

            Quaternion localRotation =
                Quaternion.AngleAxis(Mathf.Lerp(startRotationDegrees, endRotationDegrees, lerpValue), Vector3.forward);
            Vector3 eulerAngles = localRotation.eulerAngles;
            eulerAngles.z +=
                UnityEngine.Random.Range(-configuration.StartRotationVariance / 2, configuration.StartRotationVariance / 2);
            localRotation.eulerAngles = eulerAngles;

            logic.AddRange(speedSubscriptionObjects.Select(speed => new SubscribeToSpeedControllerBulletLogic(speed)));
            logic.AddRange(aimSubscriptionObjects.Select(aim => new SubscribeToAimControllerBulletLogic(aim)));
            
            bullets.Add(new BulletConfig {
                Bullet = configuration.BulletPrefab,
                Logic = logic,
                LocalPosition = startPositionOffset,
                LocalRotation = localRotation,
                
                EmissionTime = timeElapsed + configuration.DelayBeforeFirstShot + i * configuration.TimeBetweenShot
            });
        }
        ScheduleBullets(bullets);

        hasScheduledAnyEmissions = true;
    }

    // public void AssignSpeedSubscriptionObject(SpeedSubscriptionObject speedSubscriptionObject) {
    //     // if (hasScheduledAnyEmissions)
    //     //     throw new Exception("We assigned a SpeedSubscriptionObject too late, emissions were already scheduled! Gotta get on that AP train!");
    //     
    //     this.speedSubscriptionObjects = speedSubscriptionObject;
    // }
    //
    // public void AssignAimSubscriptionObject(AimSubscriptionObject aimSubscriptionObject) {
    //     this.aimSubscriptionObjects = aimSubscriptionObject;
    // }
    
    // TODO: Enforce somewhere that things tagged with [Emission] cannot have any parameters
    // TODO: Dummy examples, the BulletPrefab(s) and Shoot logic should be defined in subclasses of Emitter
    public EmissionConfiguration Config;
    [Button]
    // ReSharper disable once UnusedMember.Global
    public void Emit() {
        EmitDetailed(0, -1);
    }
    
    [Button]
    public void EmitStepwise(int bulletIndex) {
        EmitDetailed(0, bulletIndex);
    }

    public void EmitDetailed(float rotationOffset, int bulletIndex) {
        if (!Application.IsPlaying(this)) {
            Debug.LogWarning("Must be in play mode in order to test bullets");
            return;
        }
        Assert.IsTrue(Config.NumberOfShots > bulletIndex, $"{name}: Tried to emit with an invalid index:{bulletIndex} for emitter with number of shots:{Config.NumberOfShots}");
        ScheduleEmission(Config, bulletIndex, bulletIndex, rotationOffset);
    }

#if UNITY_EDITOR
    // [CustomEditor(typeof(Emitter))]
    // public class EmitterEditor : Editor {
    //     public override void OnInspectorGUI() {
    //         DrawDefaultInspector();
    //         Emitter emitter = target as Emitter;
    //         // TODO: Maybe just use button attributes for emit instead of doing all of this
    //         MethodInfo[] methods = emitter.GetType()
    //             .GetMethods()
    //             .Where(t => t.GetCustomAttributes(typeof(Emission), false).Length > 0)
    //             .ToArray();
    //         
    //         // Create a button for each method on this Emitter with the [Emission] attribute
    //         foreach (MethodInfo method in methods) {
    //             if (GUILayout.Button(method.Name)) {
    //                 if (!Application.IsPlaying(emitter))
    //                     Debug.LogWarning("Must be in play mode in order to test bullets");
    //                 else
    //                     method.Invoke(emitter, null);
    //             }
    //         }
    //     }
    // }
    #endif
}
