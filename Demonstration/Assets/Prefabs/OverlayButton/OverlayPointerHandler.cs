using UnityEngine;
using UnityEngine.EventSystems;

public class OverlayPointerHandler : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
		ExecuteEvents.Execute(
			other.transform.parent.gameObject,
			new PointerEventData(EventSystem.current),
			ExecuteEvents.pointerEnterHandler
		);
	}
	void OnTriggerExit(Collider other) {
		ExecuteEvents.Execute(
			other.transform.parent.gameObject,
			new PointerEventData(EventSystem.current),
			ExecuteEvents.pointerExitHandler
		);
	}
}
