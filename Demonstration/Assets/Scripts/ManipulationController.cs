using UnityEngine;
using System;
using System.Collections.Generic;

public class ManipulationController : MonoBehaviour {
	public GameObject SkeletonContainer;
	public GameObject MarkerObject;
	public float MarkerScaleFactor =  0.85f;

	public GameObject HighlightedJoint;

	private MenuStructure menu;
	private GameObject cursor;
	private Animator cursorAnimator;
	private Dictionary<string, Action> manipulators;
	private string jobLabel;
	private List<GameObject> markers;

	private Animator highlightedJointAnimator;

	void Start () {
		markers = new List<GameObject>();
		attachMarkersRecursively(SkeletonContainer.transform.GetChild(0), 1);

		menu = GetComponent<MenuStructure>();

		cursor = GameObject.FindGameObjectWithTag("User cursor");
		cursorAnimator = cursor.GetComponent<Animator>();

		manipulators = new Dictionary<string, Action>() {
			{"track cursor-model collisions", () => {
				Vector3 cursorPosition = cursor.transform.position;
				Collider[] colliders = Physics.OverlapSphere(
					cursorPosition,
					cursor.transform.lossyScale.x/2,
					1 << LayerMask.NameToLayer("Joint marker")
				);

				if (colliders.Length == 0) {
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
					highlightedJointAnimator.SetTrigger("Highlighted");
				}
				cursorAnimator.SetTrigger("Hidden");
			}},
			{"rotate highlighted joint", () => {
				cursorAnimator.SetTrigger("Normal");
				HighlightedJoint.transform.LookAt(cursor.transform);
			}}
		};
	}
	void Update() {
		if (jobLabel.Length > 0)
			manipulators[jobLabel].Invoke();
	}

	public void assignJob(string job) {
		jobLabel = job;
	}

	private void attachMarkersRecursively(Transform joint, uint depth) {
		for (int i = 0; i < joint.transform.childCount; i++)
			attachMarkersRecursively(joint.transform.GetChild(i), depth+1);

		GameObject marker = Instantiate(MarkerObject) as GameObject;
		marker.transform.parent = joint.transform;
		marker.transform.localPosition = Vector3.zero;
		marker.transform.localScale *= Mathf.Pow(MarkerScaleFactor, depth);

		markers.Add(marker);
	}
}
