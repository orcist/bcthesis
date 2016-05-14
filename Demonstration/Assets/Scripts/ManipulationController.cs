using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

// Define symbols for code readability
public static class JOB {
	public static readonly string TRACK = "JOB.TRACK";
	public static readonly string ROTATE = "JOB.ROTATE";
	public static readonly string STANDBY = "JOB.STANDBY";
}

public class ManipulationController : MonoBehaviour {
	public GameObject SkeletonContainer;
	public GameObject MarkerObject;
	public float MarkerScaleFactor =  0.85f;

	public GameObject HighlightedJoint;

	private Dictionary<GameObject, Quaternion> defaultRotations;
	private Dictionary<string, Action> jobs;
	private string currentJob;
	private List<GameObject> cursors;
	private Animator highlightedJointAnimator;
	private Animator highlightedCursorAnimator;
	private bool highlightedCursorVisible = false;
	private MenuStructure menu;

	void Start () {
		menu = GetComponent<MenuStructure>();

		cursors = new List<GameObject>(GameObject.FindGameObjectsWithTag("User cursor"));

		Transform root = SkeletonContainer.transform.GetChild(0);
		defaultRotations = new Dictionary<GameObject, Quaternion>();
		memorizeTransforms(root);
		attachMarkersRecursively(root, 1);

		jobs = new Dictionary<string, Action>() {
			{JOB.TRACK, () => {
				foreach (GameObject a in cursors)
					if (!a.activeSelf) {
						return;
					}

				Dictionary<Collider, GameObject> nearJoints = new Dictionary<Collider, GameObject>();
				foreach (GameObject c in cursors) {
					Collider[] collidingJoints = Physics.OverlapSphere(
						c.transform.position,
						c.transform.lossyScale.x/2,
						1 << LayerMask.NameToLayer("Joint marker")
					);
					for (uint i = 0; i < collidingJoints.Length; i++)
						nearJoints.Add(collidingJoints[i], c);
				}

				if (nearJoints.Keys.Count == 0) {
					if (HighlightedJoint != null) {
						highlightedCursorAnimator.SetTrigger("Display");
						highlightedJointAnimator.SetTrigger("Normal");

						HighlightedJoint = null;

						menu.Reset();
					}
					return;
				}

				Collider closest = new List<Collider>(nearJoints.Keys).OrderBy(
					collider => Vector3.Distance(
						nearJoints[collider].transform.position,
						collider.transform.position
					)
				).First();

				if (closest.transform.parent.gameObject == HighlightedJoint)
					return;

				if (HighlightedJoint == null) {
					if (highlightedCursorAnimator != null)
						highlightedCursorAnimator.SetTrigger("Display");
					menu.ActivateOption(OPTION.DOWN);
				} else {
					highlightedJointAnimator.SetTrigger("Normal");
				}

				HighlightedJoint = closest.transform.parent.gameObject;

				highlightedJointAnimator = closest.gameObject.GetComponent<Animator>();
				highlightedJointAnimator.SetTrigger("Highlight");

				highlightedCursorAnimator = nearJoints[closest].GetComponent<Animator>();
				highlightedCursorAnimator.SetTrigger("Hide");

				highlightedCursorVisible = false;
			}},
			{JOB.ROTATE, () => {
				if (!highlightedCursorVisible) {
					highlightedCursorAnimator.SetTrigger("Display");
					highlightedCursorVisible = true;
				}
				HighlightedJoint.transform.LookAt(highlightedCursorAnimator.transform);
				HighlightedJoint.transform.rotation *= Quaternion.Inverse(defaultRotations[HighlightedJoint]);
			}},
			{JOB.STANDBY, () => {}}
		};
	}
	void Update() {
		if (currentJob != null)
			jobs[currentJob].Invoke();
	}

	public void AssignJob(string job) {
		currentJob = job;
	}
	public void Reset() {
		foreach (GameObject joint in defaultRotations.Keys)
			ResetJoint(joint);
	}
	public void ResetJoint(GameObject joint) {
		joint.transform.rotation = defaultRotations[joint];
	}

	private void memorizeTransforms(Transform joint) {
		defaultRotations[joint.gameObject] = joint.rotation;
		for (int i = 0; i < joint.transform.childCount; i++)
			memorizeTransforms(joint.transform.GetChild(i));
	}
	private void attachMarkersRecursively(Transform joint, uint depth) {
		for (int i = 0; i < joint.transform.childCount; i++)
			attachMarkersRecursively(joint.transform.GetChild(i), depth+1);

		GameObject marker = Instantiate(MarkerObject) as GameObject;
		marker.transform.parent = joint.transform;
		marker.transform.localPosition = Vector3.zero;
		marker.transform.localScale *= Mathf.Pow(MarkerScaleFactor, depth);
	}
}
