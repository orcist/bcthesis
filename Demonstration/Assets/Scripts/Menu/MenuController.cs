using UnityEngine;

public class MenuController : MonoBehaviour {
	public UserController userController;
	public GameObject Slider;

	private float maxHeight;

	void Start() {
		maxHeight = Slider.transform.parent.GetComponent<RectTransform>().sizeDelta.y;
	}
	void Update() {
		adjustSlider();
	}
	private void adjustSlider() {
		float newHeight = userController.GetRecessiveHandAngle() * maxHeight;
		RectTransform sliderTransform = Slider.GetComponent<RectTransform>();
		sliderTransform.sizeDelta = new Vector2(sliderTransform.sizeDelta.x, newHeight);
	}
}
