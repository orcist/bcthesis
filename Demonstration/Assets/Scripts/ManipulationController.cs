using UnityEngine;
using System;
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
	private MenuStructure menu;
	private GameObject cursor;
	private Animator cursorAnimator;
	private Dictionary<string, Action> manipulators;
	private string todo;
	private Animator highlightedJointAnimator;

	void Start () {
		menu = GetComponent<MenuStructure>();

		cursor = GameObject.FindGameObjectWithTag("User cursor");
		cursorAnimator = cursor.GetComponent<Animator>();

		Transform root = SkeletonContainer.transform.GetChild(0);
		defaultRotations = new Dictionary<GameObject, Quaternion>();
		memorizeTransforms(root);
		attachMarkersRecursively(root, 1);

		manipulators = new Dictionary<string, Action>() {
			{JOB.TRACK, () => {
				Vector3 cursorPosition = cursor.transform.position;
				Collider[] colliders = Physics.OverlapSphere(
					cursorPosition,
					cursor.transform.lossyScale.x/2,
					1 << LayerMask.NameToLayer("Joint marker")
				);

				if (colliders.Length == 0) {
					if (cursorAnimator.isInitialized)
						cursorAnimator.SetTrigger("Normal");

					if (HighlightedJoint != null) {
						highlightedJointAnimator.SetTrigger("Normal");
						HighlightedJoint = null;
						menu.Reset();
					}
					return;
				}

				Transform closest = colliders[0].transform;
				float distance, shortest;
				foreach (Collider collider in colliders) {
					distance = Vector3.Distance(cursorPosition, collider.transform.position);
					shortest = Vector3.Distance(cursorPosition, closest.position);

					if (distance < shortest && !Mathf.Approximately(distance, shortest))
						closest = collider.transform;
				}

				if (closest.parent.gameObject != HighlightedJoint) {
					if (HighlightedJoint != null)
						highlightedJointAnimator.SetTrigger("Normal");
					else
						menu.ActivateOption(OPTION.DOWN);

					HighlightedJoint = closest.parent.gameObject;
					highlightedJointAnimator = closest.GetComponent<Animator>();
					highlightedJointAnimator.SetTrigger("Highlight");
				}
				if (cursorAnimator.isInitialized)
					cursorAnimator.SetTrigger("Hide");
			}},
			{JOB.ROTATE, () => {
				if (cursorAnimator.isInitialized)
					cursorAnimator.SetTrigger("Normal");
				HighlightedJoint.transform.LookAt(cursor.transform);
			}},
			{JOB.STANDBY, () => {}}
		};
	}
	void Update() {
		if (todo != null)
			manipulators[todo].Invoke();
	}

	public void AssignJob(string job) {
		todo = job;
	}
	public void Reset() {
		foreach (GameObject joint in defaultRotations.Keys)
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
