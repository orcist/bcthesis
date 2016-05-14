using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

public class ClipManager : MonoBehaviour {
	private GameObject model, root;
	private Animation playback;
	private List<AnimationClip> cache;
	private Dictionary<AnimationClip, Dictionary<GameObject, AnimationCurve[]>> curves;

	private int currentKeyframe;
	private ManipulationController manipulator;

	void Start() {
		GameObject skeleton = GetComponent<ManipulationController>().SkeletonContainer;
		root = skeleton.transform.GetChild(0).gameObject;
		model = skeleton.transform.parent.gameObject;

		manipulator = GetComponent<ManipulationController>();

		playback = model.AddComponent<Animation>();

		reset();
	}

	public void SaveRotation(GameObject joint) {
		AnimationClip clip = new AnimationClip();
		cache.Add(clip);
		curves[clip] = new Dictionary<GameObject, AnimationCurve[]>();

		if (cache.Count > 1) {
			EditorUtility.CopySerialized(cache[cache.Count-2], cache[cache.Count-1]);
			Dictionary<GameObject, AnimationCurve[]> pastCurves = curves[cache[cache.Count-2]];
			foreach (GameObject j in pastCurves.Keys) {
				curves[clip][j] = new AnimationCurve[3]; // x, y, z curves
				for (uint i = 0; i < pastCurves[j].Length; i++)
					curves[clip][j][i] = new AnimationCurve(pastCurves[j][i].keys);
			}
		} else
			curves[clip] = new Dictionary<GameObject, AnimationCurve[]>();

		if (!curves[clip].ContainsKey(joint)) {
			curves[clip][joint] = new AnimationCurve[3];
			for (uint i = 0; i < 3; i++)
				curves[clip][joint][i] = new AnimationCurve();

			Quaternion rotation = joint.transform.rotation;
			manipulator.ResetJoint(joint);
			setRotationCurves(clip, joint, (float)(currentKeyframe-1)/2);
			joint.transform.rotation = rotation;
		}

		setRotationCurves(clip, joint, (float)currentKeyframe/2);
		saveCache();
	}
	public void ExportClip() {
		string animationID = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
		AssetDatabase.MoveAsset(
			"Assets/Animations/cache.temp/cache_"+(cache.Count-1)+".anim",
			"Assets/Animations/animation_"+animationID+".anim");
		AssetDatabase.DeleteAsset("Assets/Animations/cache.temp");

		manipulator.Reset();
		reset();
	}
	public void Undo(int steps) {
		int n = cache.Count - Mathf.Min(steps, cache.Count);
		foreach (AnimationClip c in cache.GetRange(n, cache.Count-n))
			curves.Remove(c);

		cache = cache.GetRange(0, n);
		currentKeyframe = 1;
		manipulator.Reset();

		if (cache.Count == 0)
			return;

		AnimationClip clip = cache[cache.Count-1];
		foreach (GameObject j in curves[clip].Keys)
			for (uint i = 0; i < curves[clip][j].Length; i++)
				currentKeyframe = Math.Max(
					(int)(new List<Keyframe>(curves[clip][j][i].keys)
						.OrderByDescending(keyframe => keyframe.time).First().time),
					currentKeyframe
				);

		clip.SampleAnimation(model, currentKeyframe);
	}
	public void PlayAnimation() {
		if (cache.Count == 0)
			return;

		foreach (AnimationState state in playback)
			playback.RemoveClip(state.name);

		AnimationClip clip = new AnimationClip();
		EditorUtility.CopySerialized(cache[cache.Count-1], clip);
		clip.legacy = true;

		playback.AddClip(clip, clip.name);
		playback.Play(clip.name);
	}
	public void CreateKeyframe() {
		currentKeyframe += 1;
	}

	private void reset() {
		cache = new List<AnimationClip>();
		curves = new Dictionary<AnimationClip, Dictionary<GameObject, AnimationCurve[]>>();
		currentKeyframe = 1;
	}
	private string getRootPath(GameObject joint) {
		return (
			joint == root.transform.parent.gameObject ?
				"" : getRootPath(joint.transform.parent.gameObject)+"/"
			) + joint.name;
	}
	private void saveCache() {
		int i = cache.Count-1;
		if (!AssetDatabase.IsValidFolder("Assets/Animations/cache.temp"))
			AssetDatabase.CreateFolder("Assets/Animations", "cache.temp");

		string cachePath = "Assets/Animations/cache.temp/cache_"+i+".anim";
		AssetDatabase.CreateAsset(cache[i], cachePath);
		AssetDatabase.SaveAssets();
	}
	private void setRotationCurves(AnimationClip clip, GameObject joint, float time) {
		Vector3 rotation = joint.transform.localEulerAngles;
		AnimationCurve[] cs = curves[clip][joint];

		cs[0].AddKey(time, rotation.x);
		cs[0].SmoothTangents(cs[0].keys.Length-1, 0);

		cs[1].AddKey(time, rotation.y);
		cs[1].SmoothTangents(cs[1].keys.Length-1, 0);

		cs[2].AddKey(time, rotation.z);
		cs[2].SmoothTangents(cs[2].keys.Length-1, 0);

		string path = getRootPath(joint);
		clip.SetCurve(path, typeof(Transform), "localEuler.x", cs[0]);
		clip.SetCurve(path, typeof(Transform), "localEuler.y", cs[1]);
		clip.SetCurve(path, typeof(Transform), "localEuler.z", cs[2]);
	}
}
