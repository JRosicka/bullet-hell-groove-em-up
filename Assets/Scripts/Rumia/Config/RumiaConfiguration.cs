using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rumia {
    /// <summary>
    /// Configuration representing a bullet pattern.
    ///
    /// The <see cref="Pattern"/> field points to a Prefab to be instantiated at the beginning of the game, the
    /// <see cref="MeasuresList"/> field holds a collection of configured <see cref="RumiaMeasure"/>s used to schedule
    /// <see cref="RumiaAction"/>s to be invoked on the <see cref="Pattern"/> field.
    ///
    /// Multiple of these can exist in a scene, a <see cref="Pattern"/> instance will be spawned for each one and fed the
    /// <see cref="MeasuresList"/> data. 
    /// </summary>
    [CreateAssetMenu(fileName = "RumiaConfiguration", menuName = "Resources/Patterns/RumiaConfiguration", order = 3)]
    public class RumiaConfiguration : ScriptableObject {
        public Pattern Pattern;

        // Amount of measures to delay before starting the pattern
        public int StartMeasure;
        // TODO: We should really change the setup of RumiaActions from happening during recompilation to instead happening with a button press somewhere
        /// <summary>
        /// Guaranteed to be invoked right at the start of gameplay before any <see cref="MeasuresList"/> measures are invoked.
        /// Also accounts for <see cref="TimingController.NumberOfMeasuresToSkipOnStart"/>.
        /// Things like spawning are good to include here.
        /// </summary>
        public List<RumiaMeasure> StartMeasures;
        
        // Measures. It's hard to transcribe sheet music to scriptable objects, okay?
        [Header("Measures list")] public List<RumiaMeasureList> MeasuresList;

        public List<RumiaMeasure> GetAllMeasures() {
            List<RumiaMeasure> ret = new List<RumiaMeasure>(StartMeasures);
            foreach (RumiaMeasureList measureList in MeasuresList) {
                ret.AddRange(measureList.Measures);
            }

            return ret;
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(RumiaConfiguration))]
        public class RumiaConfigurationEditor : Editor {
            public override void OnInspectorGUI() {
                // Draw the default inspector
                DrawDefaultInspector();
                RumiaConfiguration config = target as RumiaConfiguration;
                if (config == null) return;
                Pattern pattern = config.Pattern;
                if (!pattern)
                    return;

                foreach (RumiaMeasure measure in config.StartMeasures) {
                    if (measure == null)
                        continue;
                    measure.SetPattern(pattern);
                }
                
                foreach (RumiaMeasureList measures in config.MeasuresList) {
                    foreach (RumiaMeasure measure in measures.Measures) {
                        if (measure == null)
                            continue;
                        measure.SetPattern(pattern);
                    }
                }
            }
        }
#endif
    }

    /// <summary>
    /// Collection used in order to allow configuration in the inspector
    /// </summary>
    [Serializable]
    public class RumiaMeasureList {
        public List<RumiaMeasure> Measures;
    }
}