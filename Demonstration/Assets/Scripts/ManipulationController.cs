using UnityEngine;
using System.Collections.Generic;

public class ManipulationController : MonoBehaviour {
	public GameObject SkeletonContainer;
	public GameObject MarkerObject;
	public float MarkerScaleFactor =  0.85f;
	private List<GameObject> markers;


	void Start () {
		markers = new List<GameObject>();
		attachMarkersRecursively(SkeletonContainer.transform.GetChild(0), 1);
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
