using UnityEngine;
using System;
using System.Collections.Generic;

public class UserController : MonoBehaviour {
  public bool RightHanded = true;
  public bool Calibrated = false;
	public bool DrawSkeleton = false;

	public GameObject CursorObject;
  public GameObject BoundaryObject;

  public float JointAdjustmentSpeed = 4.0f;
  public GameObject Hip_Center, Spine, Shoulder_Center, Head,
    Shoulder_Left, Elbow_Left, Wrist_Left, Hand_Left,
    Shoulder_Right, Elbow_Right, Wrist_Right, Hand_Right,
    Hip_Left, Knee_Left, Ankle_Left, Foot_Left,
    Hip_Right, Knee_Right, Ankle_Right, Foot_Right;
	public LineRenderer SkeletonLine;

	private string[] jointNames = {
			"Hip_Center", "Spine", "Shoulder_Center", "Head",
			"Shoulder_Left", "Elbow_Left", "Wrist_Left", "Hand_Left",
			"Shoulder_Right", "Elbow_Right", "Wrist_Right", "Hand_Right",
			"Hip_Left", "Knee_Left", "Ankle_Left", "Foot_Left",
			"Hip_Right", "Knee_Right", "Ankle_Right", "Foot_Right"
	};
	private Dictionary<string, GameObject> Joints;
	private Dictionary<string, string> jointParents;
  private bool allJointsVisible = false;

  private Vector3[] bindingTrapezoid;
  /* (-+),(++)
   * (--),(+-)
   * in this order
   */
  private Func<Vector3, Vector3, Vector3>[] trapezoidFunctions = new Func<Vector3, Vector3, Vector3>[] {
    (Vector3 currentPosition, Vector3 scanned) => {
      return new Vector3(
        Mathf.Min(scanned.x, currentPosition.x),
        0f,
        Mathf.Max(scanned.z, currentPosition.z)
    );}, // (-+)
    (Vector3 currentPosition, Vector3 scanned) => {
      return new Vector3(
        Mathf.Max(scanned.x, currentPosition.x),
        0f,
        Mathf.Max(scanned.z, currentPosition.z)
    );}, // (++)
    (Vector3 currentPosition, Vector3 scanned) => {
      return new Vector3(
        Mathf.Min(scanned.x, currentPosition.x),
        0f,
        Mathf.Min(scanned.z, currentPosition.z)
    );}, // (--)
    (Vector3 currentPosition, Vector3 scanned) => {
      return new Vector3(
        Mathf.Max(scanned.x, currentPosition.x),
        0f,
        Mathf.Min(scanned.z, currentPosition.z)
    );} // (+-)
  };
  private GameObject[] bindingObjects;
	private Dictionary<string, LineRenderer> lines;

  private float minimumPositionDelta = 0.2f; // epsilon for joint and user position interpolation (in unity units ~ meters)
  private float minimumRotationDelta = 5f; // epsilon for joint and user rotation interpolation (in degrees)

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
        lines[joint].transform.parent = Joints[joint].transform;
			}

		if (CursorObject)
			mountCursor();
	}
	void Update () {
		KinectManager manager = KinectManager.Instance;
		uint userID = (manager != null) ? manager.GetPlayer1ID() : 0;

    if (Calibrated && bindingObjects == null)
      buildBoundaries();

		if (userID <= 0) {
			resetUser();
			return;
    }

    updateUserPosition(manager, userID);
		updateJoints(manager, userID);

    if (!Calibrated)
      calibrateSpace();

		if (DrawSkeleton && SkeletonLine)
			drawSkeleton();
	}

  public float GetRecessiveHandAngle() {
    GameObject recessiveHand = getRecessiveHand();
    if (!recessiveHand.gameObject.activeSelf)
      return -1.0f;

    Vector3 armVector = recessiveHand.transform.position - getRecessiveShoulder().transform.position;
    return Vector3.Angle(-Vector3.up, armVector.normalized) / 180.0f;
  }

	private void resetUser() {
    foreach (string joint in Joints.Keys) {
      Joints[joint].gameObject.SetActive(false);

			Joints[joint].transform.localPosition = Vector3.zero;
      Joints[joint].transform.localRotation = Quaternion.identity;

			lines[joint].gameObject.SetActive(false);
    }
    allJointsVisible = false;
	}
	private void mountCursor() {
		GameObject cursor = Instantiate(CursorObject) as GameObject,
      parentJoint = getDominantHand();

    cursor.transform.parent = parentJoint.transform;
    cursor.transform.localPosition = Vector3.zero;
    cursor.transform.localRotation = Quaternion.Euler(0, 180f, 0);
	}
  private void updateUserPosition(KinectManager manager, uint userID) {
    Vector3 userPosition = manager.GetUserPosition(userID);
    userPosition.x *= -1;
    if (Vector3.Distance(transform.position, userPosition) < minimumPositionDelta)
      transform.position = userPosition;
    else
      transform.position = Vector3.Lerp(transform.position, userPosition, Time.deltaTime * JointAdjustmentSpeed);
  }
	private void updateJoints(KinectManager manager, uint userID) {
    allJointsVisible = true;

		Vector3 userPosition = transform.position, jointPosition;
		Quaternion userRotation = transform.rotation, jointRotation;
    int jointIndex;
    // update the local positions of the joints
    foreach (string joint in Joints.Keys) {
			jointIndex = Array.IndexOf(jointNames, joint);

			if (!manager.IsJointTracked(userID, jointIndex)) {
        allJointsVisible = false;
        Joints[joint].gameObject.SetActive(false);
        continue;
      }

      jointPosition = manager.GetJointPosition(userID, jointIndex) - userPosition;
      jointPosition.z *= -1;
      jointRotation = manager.GetJointOrientation(userID, jointIndex, true) * userRotation;

      if (!Joints[joint].gameObject.activeSelf) {
        Joints[joint].transform.localPosition = jointPosition;
        Joints[joint].transform.rotation = jointRotation;
      } else {
        if ((Joints[joint].transform.localPosition - jointPosition).magnitude < minimumPositionDelta)
          Joints[joint].transform.localPosition = jointPosition;
        else {
          Joints[joint].transform.localPosition = Vector3.Lerp(
            Joints[joint].transform.localPosition,
            jointPosition,
            Time.deltaTime * JointAdjustmentSpeed);
        }

        if (Quaternion.Angle(Joints[joint].transform.rotation, jointRotation) < minimumRotationDelta)
          Joints[joint].transform.rotation = jointRotation;
        else {
          Joints[joint].transform.rotation = Quaternion.Slerp(
          Joints[joint].transform.rotation,
          jointRotation,
          Time.deltaTime * JointAdjustmentSpeed);
        }
      }
      Joints[joint].gameObject.SetActive(true);
    }
	}
  private void calibrateSpace() {
    if (!allJointsVisible)
      return;

    Vector3 userPosition = transform.position;

    if (bindingTrapezoid == null) { // first scan of user
      bindingTrapezoid = new Vector3[4];
      for (uint i = 0; i < 4; i++) {
        bindingTrapezoid[i] = userPosition;
        bindingTrapezoid[i].y = 0f;
      }
    } else {
      int j = (userPosition.x < 0f ? 0 : 1) + (userPosition.z < 0f ? 2 : 0);
      bindingTrapezoid[j] = trapezoidFunctions[j](bindingTrapezoid[j], userPosition);
    }
  }
  private void buildBoundaries() {
    bindingObjects = new GameObject[4];
    int[] trapezoidPaths = {0, 1, 3, 2, 0};

    Vector3 fromPoint, toPoint, boundaryVector;
    Transform boundary;
    for (uint i = 0; i < 4; i++) {
      fromPoint = bindingTrapezoid[trapezoidPaths[i]];
      toPoint = bindingTrapezoid[trapezoidPaths[i+1]];
      boundaryVector = toPoint-fromPoint;

      bindingObjects[i] = Instantiate(BoundaryObject) as GameObject;
      boundary = bindingObjects[i].transform;

      boundary.localScale = new Vector3(boundary.localScale.x, boundary.localScale.y, boundaryVector.magnitude);
      boundary.position += fromPoint + boundaryVector/2;
      boundary.LookAt(toPoint + new Vector3(0f, boundary.localScale.y, 0f));
    }
  }
	private void drawSkeleton() {
		string parentJoint;
		foreach (string joint in Joints.Keys) {
			parentJoint = jointParents[joint];
			if (!Joints[joint].gameObject.activeSelf || !Joints[parentJoint].gameObject.activeSelf) {
				lines[joint].gameObject.SetActive(false);
				continue;
			}

			lines[joint].gameObject.SetActive(true);
			lines[joint].SetPosition(0, Joints[parentJoint].transform.position);
			lines[joint].SetPosition(1, Joints[joint].transform.position);
		}
	}
  private GameObject getDominantHand() {
    return RightHanded ? Hand_Right : Hand_Left;
  }

  private GameObject getRecessiveHand() {
    return RightHanded ? Hand_Left : Hand_Right;
  }

  private GameObject getRecessiveShoulder() {
    return RightHanded ? Shoulder_Left : Shoulder_Right;
  }
}
