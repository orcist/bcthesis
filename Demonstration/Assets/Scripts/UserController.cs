using UnityEngine;
using System;
using System.Collections.Generic;

public class UserController : MonoBehaviour {
	public bool DrawSkeleton = false;
	public GameObject CursorObject;
	public bool RightHanded = true;
	public GameObject Hip_Center, Spine, Shoulder_Center, Head,
		Shoulder_Left, Elbow_Left, Wrist_Left, Hand_Left,
		Shoulder_Right, Elbow_Right, Wrist_Right, Hand_Right,
		Hip_Left, Knee_Left, Ankle_Left, Foot_Left,
		Hip_Right, Knee_Right, Ankle_Right, Foot_Right;
	public Dictionary<string, GameObject> Joints;
	public LineRenderer SkeletonLine;

	private string[] jointNames = new string[] {
			"Hip_Center", "Spine", "Shoulder_Center", "Head",
			"Shoulder_Left", "Elbow_Left", "Wrist_Left", "Hand_Left",
			"Shoulder_Right", "Elbow_Right", "Wrist_Right", "Hand_Right",
			"Hip_Left", "Knee_Left", "Ankle_Left", "Foot_Left",
			"Hip_Right", "Knee_Right", "Ankle_Right", "Foot_Right"
	};
	private Dictionary<string, LineRenderer> lines;
	private Dictionary<string, string> jointParents;
	private Vector3 initialPosition;
	private Quaternion initialRotation;

	void Start () {
		Joints = new Dictionary<string, GameObject>() {
			{"Hip_Center", Hip_Center},
			{"Spine", Spine},
			{"Shoulder_Center", Shoulder_Center},
			{"Head", Head},

			{"Shoulder_Left", Shoulder_Left},
			{"Elbow_Left", Elbow_Left},
			{"Wrist_Left", Wrist_Left},
			{"Hand_Left", Hand_Left},

			{"Shoulder_Right", Shoulder_Right},
			{"Elbow_Right", Elbow_Right},
			{"Wrist_Right", Wrist_Right},
			{"Hand_Right", Hand_Right},

			{"Hip_Left", Hip_Left},
			{"Knee_Left", Knee_Left},
			{"Ankle_Left", Ankle_Left},
			{"Foot_Left", Foot_Left},

			{"Hip_Right", Hip_Right},
			{"Knee_Right", Knee_Right},
			{"Ankle_Right", Ankle_Right},
			{"Foot_Right", Foot_Right}
		};
		jointParents = new Dictionary<string, string>() {
			{"Hip_Center", "Hip_Center"},
			{"Spine", "Hip_Center"},
			{"Shoulder_Center", "Spine"},
			{"Head", "Shoulder_Center"},

			{"Shoulder_Left", "Shoulder_Center"},
			{"Elbow_Left", "Shoulder_Left"},
			{"Wrist_Left", "Elbow_Left"},
			{"Hand_Left", "Wrist_Left"},

			{"Shoulder_Right", "Shoulder_Center"},
			{"Elbow_Right", "Shoulder_Right"},
			{"Wrist_Right", "Elbow_Right"},
			{"Hand_Right", "Wrist_Right"},

			{"Hip_Left", "Hip_Center"},
			{"Knee_Left", "Hip_Left"},
			{"Ankle_Left", "Knee_Left"},
			{"Foot_Left", "Ankle_Left"},

			{"Hip_Right", "Hip_Center"},
			{"Knee_Right", "Hip_Right"},
			{"Ankle_Right", "Knee_Right"},
			{"Foot_Right", "Ankle_Right"}
		};

		// dictionary holding the skeleton lines
		lines = new Dictionary<string, LineRenderer>();
		if (SkeletonLine)
			foreach (string joint in Joints.Keys) {
				lines[joint] = Instantiate(SkeletonLine) as LineRenderer;
				lines[joint].transform.parent = transform;
			}

		initialPosition = transform.position;
		initialRotation = transform.rotation;
		if (CursorObject)
			mountCursor();
	}

	private void mountCursor() {
		GameObject cursor = Instantiate(CursorObject), parentJoint;

		if (RightHanded)
			parentJoint = Hand_Right;
		else
			parentJoint = Hand_Left;

		cursor.transform.parent = parentJoint.transform;
		cursor.transform.rotation = parentJoint.transform.rotation;
	}

	// Update is called once per frame
	void Update () {
		KinectManager manager = KinectManager.Instance;
		uint userID = (manager != null) ? manager.GetPlayer1ID() : 0;

		if (userID <= 0) {
			resetUser();
			return;
		}
		updateJoints(manager, userID);
		if (DrawSkeleton && SkeletonLine)
			drawSkeleton();
	}

	private void resetUser() {
		if (transform.position != initialPosition)
			transform.position = initialPosition;

		if (transform.rotation != initialRotation)
			transform.rotation = initialRotation;

		foreach (string joint in Joints.Keys) {
			Joints[joint].gameObject.SetActive(true);

			Joints[joint].transform.localPosition = Vector3.zero;
			Joints[joint].transform.localRotation = Quaternion.identity;
		}
	}

	private void updateJoints(KinectManager manager, uint userID) {
		Vector3 userPosition = manager.GetUserPosition(userID), jointPosition;
		Quaternion jointRotation;

		// update the local positions of the joints
		foreach (string joint in Joints.Keys) {
			if (Joints[joint] == null) // TODO can this happen? {0}
				continue;

			int jointIndex = Array.IndexOf(jointNames, joint);
			if (manager.IsJointTracked(userID, jointIndex)) {
				Joints[joint].gameObject.SetActive(true);

				jointPosition = manager.GetJointPosition(userID, jointIndex);
				jointRotation = manager.GetJointOrientation(userID, jointIndex, true);

				jointRotation = initialRotation * jointRotation;
				jointPosition -= userPosition;

				Joints[joint].transform.localPosition = jointPosition;
				Joints[joint].transform.rotation = jointRotation;
			} else {
				Joints[joint].gameObject.SetActive(false);
			}
		}
	}

	private void drawSkeleton() {
		string parentJoint;
		foreach (string joint in Joints.Keys) {
			parentJoint = jointParents[joint];
			if (Joints[joint] == null || !Joints[joint].gameObject.activeSelf || // {0}
					!Joints[parentJoint].gameObject.activeSelf) {
				lines[joint].gameObject.SetActive(false);
				continue;
			}

			lines[joint].gameObject.SetActive(true);
			lines[joint].SetPosition(0, Joints[parentJoint].transform.position);
			lines[joint].SetPosition(1, Joints[joint].transform.position);
		}
	}
}
