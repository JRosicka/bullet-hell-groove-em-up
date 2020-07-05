using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ParticleEffectsController : MonoBehaviour {
	private readonly List<ParticleSystem> particleSystems = new List<ParticleSystem>();

	[Header("Play")]
	public bool StartFresh;
	public bool Play;

	[Header("Stop")]
	public bool StopMakingNew;
	public bool Pause;
	public bool Clear;

	private void Start() {
		foreach (ParticleSystem system in GetComponentsInChildren<ParticleSystem>()) {
			particleSystems.Add(system);
		}

	}

	private void Update() {
		if (StartFresh) {
			StartFresh = false;
			ClearAll();
			Trigger();
		}

		if (StopMakingNew) {
			StopMakingNew = false;
			StopAll();
		}

		if (Pause) {
			Pause = false;
			PauseAll();
		}

		if (Clear) {
			Clear = false;
			ClearAll();
		}

		if (Play) {
			Play = false;
			Trigger();
		}
	}

	private void Trigger() {
		foreach (ParticleSystem system in particleSystems) {
			system.Play();
		}
	}

	private void ClearAll() {
		foreach (ParticleSystem system in particleSystems) {
			system.Clear();
		}
	}

	private void StopAll() {
		foreach (ParticleSystem system in particleSystems) {
			system.Stop();
		}
	}

	private void PauseAll() {
		foreach (ParticleSystem system in particleSystems) {
			system.Pause();
		}
	}


	// TODO: I don't think I really need any of this right now
#if UNITY_EDITOR
	[CanEditMultipleObjects]
	[CustomEditor(typeof(ParticleEffectsController))]
	public class ParticleEffectsEditor : Editor {
		override public void OnInspectorGUI() {

			var myScript = target as ParticleEffectsController;
			List<string> excludedProperties = new List<string>();
			serializedObject.Update();

//			if (myScript.Style != FxParticles.ParticleStyle.Canvas) {
//				excludedProperties.Add("RendersUnderAnchor");
//				excludedProperties.Add("AddsCanvas");
//				excludedProperties.Add("SortingOrder");
//			}

			DrawPropertiesExcluding(serializedObject, excludedProperties.ToArray());
			serializedObject.ApplyModifiedProperties();
		}
	}
#endif
}
