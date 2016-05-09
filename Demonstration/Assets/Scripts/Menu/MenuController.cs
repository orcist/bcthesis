using UnityEngine;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour {
	public UserController userController;
	public Animator AdditionalMenuAnimator;

	public GameObject CrosshairObject;

	private Transform cameraTransform;
	private GameObject lastHit;
	private Animator crosshairAnimator;
	private bool crosshairDisplayed = false;

	void Start() {
		cameraTransform = userController.Head.GetComponentInChildren<Camera>().transform;

		GameObject crosshair = Instantiate(CrosshairObject) as GameObject;
		crosshair.transform.parent = cameraTransform;
		crosshair.transform.localPosition = new Vector3(0f, 0f, 0.5f);
		crosshair.transform.localRotation = Quaternion.identity;

		crosshairAnimator = crosshair.GetComponent<Animator>();

		if (!userController.RightHanded)
			transform.Rotate(Vector3.up, 90f);
	}
	void Update() {
		traceSight();
	}

	public void ShowAdditionalMenu(bool visible) {
		AdditionalMenuAnimator.SetTrigger(visible ? "Display" : "Hide");
  }

	private void traceSight() {
		RaycastHit hit;
		bool success = Physics.Raycast(
			cameraTransform.position,
			cameraTransform.forward,
			out hit,
			20f,
			1 << LayerMask.NameToLayer("UI"),
			QueryTriggerInteraction.Collide
		);

		if (lastHit != null && (!success || lastHit != hit.transform.parent.gameObject)) {
			ExecuteEvents.Execute(
				lastHit,
				new PointerEventData(EventSystem.current),
				ExecuteEvents.pointerExitHandler
			);
		}

		if (success) {
			if (!crosshairDisplayed) {
				crosshairAnimator.SetTrigger("Display");
				crosshairDisplayed = true;
			}

			ExecuteEvents.Execute(
				hit.transform.parent.gameObject,
				new PointerEventData(EventSystem.current),
				ExecuteEvents.pointerEnterHandler
			);
		} else {
			if (crosshairDisplayed) {
				crosshairAnimator.SetTrigger("Hide");
				crosshairDisplayed = false;
			}
		}

		lastHit = success ? hit.transform.parent.gameObject : null;
	}
}
