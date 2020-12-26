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
    // TODO: Dummy examples, the BulletPrefab(s) and Shoot logic should be defined in subclasses of Emitter
    public Bullet BulletPrefab;
    public List<Bullet> SpawnedBullets = new List<Bullet>();
    void Start() {
        // Shoot(BulletPrefab, null);
    }

    /// <summary>
    /// Shoot a single bullet at the specified position (relative to the emitter) and pass in the specified bullet logic
    /// </summary>
    /// <param name="bullet">Bullet prefab to shoot</param>
    /// <param name="logic">List of BulletLogic to pass to the instantiated bullet</param>
    /// <param name="localPosition">The position relative to this Emitter's transform to shoot the bullet.
    /// Zero vector by default.</param>
    protected void Shoot(Bullet bullet, List<BulletLogic> logic, Vector3 localPosition = default) {
        if (bullet == null)
            throw new Exception("Trying to shoot a null bullet!");
        
        Transform t = transform;
        Bullet spawnedBullet = Instantiate(bullet, t.position + localPosition, t.rotation, GameController.Instance.ShotBucket);
        SpawnedBullets.Add(spawnedBullet);
        spawnedBullet.AssignBulletLogic(logic);
    }

    // TODO: Enforce somewhere that things tagged with [Emission] cannot have any parameters
    [Emission]
    public void ShootBasicShot() {
        Shoot(BulletPrefab, null);
    }

    [Emission]
    public void ShootAFarMoreComplexShot() {
        Shoot(BulletPrefab, null, Vector3.left);
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
