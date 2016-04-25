using UnityEngine;
using System;
using System.Collections.Generic;

public class ManipulationController : MonoBehaviour {
	public GameObject SkeletonContainer;
	public GameObject MarkerObject;
	public float MarkerScaleFactor =  0.85f;

	private GameObject cursor;
	private Dictionary<string, Action> manipulators;
	private Action job;
	private List<GameObject> markers;

	private GameObject highlightedJoint;

	void Start () {
		markers = new List<GameObject>();
		attachMarkersRecursively(SkeletonContainer.transform.GetChild(0), 1);
		cursor = GameObject.FindGameObjectWithTag("User cursor");

		manipulators = new Dictionary<string, Action>() {
			{"track cursor-model collisions", () => {
				Vector3 cursorPosition = cursor.transform.position;
				Collider[] colliders = Physics.OverlapSphere(
					cursorPosition,
					cursor.GetComponent<SphereCollider>().radius,
					1 << LayerMask.NameToLayer("Joint marker")
				);

				if (colliders.Length == 0) {
					highlightedJoint = null;
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

				if (closest.parent.gameObject != highlightedJoint) {
					highlightedJoint = closest.parent.gameObject;
					Debug.Log(highlightedJoint.name);
				}
			}}
		};

		job = manipulators["track cursor-model collisions"];
	}
	void Update() {
		job.Invoke();
	}

	public void assignJob(string jobLabel) {
		job = manipulators[jobLabel];
	}

	private void attachMarkersRecursively(Transform joint, uint depth) {
		for (int i = 0; i < joint.transform.childCount; i++)
			attachMarkersRecursively(joint.transform.GetChild(i), depth+1);

		GameObject marker = Instantiate(MarkerObject) as GameObject;
		marker.transform.parent = joint.transform;
		marker.transform.localPosition = Vector3.zero;
		marker.transform.localScale *= Mathf.Pow(MarkerScaleFactor, depth);
		marker.GetComponent<SphereCollider>().radius = marker.transform.lossyScale.x/2;
		marker.layer = LayerMask.NameToLayer("Joint marker");

		markers.Add(marker);
	}
}
