using UnityEngine;
using UnityEngine.EventSystems;

public class WorldPointerHandler : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
		ExecuteEvents.Execute(
			other.transform.parent.gameObject,
			new PointerEventData(EventSystem.current),
			ExecuteEvents.pointerEnterHandler
		);
	}
	void OnTriggerExit(Collider other) {
		GameObject buttonObject = other.transform.parent.gameObject;
		ExecuteEvents.Execute(
			buttonObject,
			new PointerEventData(EventSystem.current),
			ExecuteEvents.pointerClickHandler
		);
		ExecuteEvents.Execute(
			buttonObject,
			new PointerEventData(EventSystem.current),
			ExecuteEvents.pointerExitHandler
		);
	}
}
