using UnityEngine;
using UnityEngine.EventSystems;

public class CollideClick : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
		ExecuteEvents.Execute(
			other.gameObject,
			new PointerEventData(EventSystem.current),
			ExecuteEvents.pointerEnterHandler
		);
	}
	void OnTriggerExit(Collider other) {
		ExecuteEvents.Execute(
			other.gameObject,
			new PointerEventData(EventSystem.current),
			ExecuteEvents.pointerExitHandler
		);
	}
}
