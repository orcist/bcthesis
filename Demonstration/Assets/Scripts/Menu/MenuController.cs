using UnityEngine;

public class MenuController : MonoBehaviour {
	public GameObject AdditionalMenu;
	public GameObject Info;
  public Transform UserHead;
	public GameObject Slider;

  public Animation playback;
	public bool newClip = false;

	private UserController userController;
	private ManipulationController manipulator;
	private float maxHeight;
	private bool playing = false;

	void Start() {
		userController = UserHead.parent.gameObject.GetComponent<UserController>();
		manipulator = GetComponent<ManipulationController>();

		maxHeight = Slider.transform.parent.GetComponent<RectTransform>().sizeDelta.y;
	}
	void Update() {
		adjustSlider();
		handleClipPlayback();
	}

	public void ShowAdditionalMenu(bool visible) {
    if (visible) {
			AdditionalMenu.GetComponent<Animator>().SetTrigger("Display");
      manipulator.SkeletonContainer.transform.parent.gameObject.SetActive(false);
    } else {
			AdditionalMenu.GetComponent<Animator>().SetTrigger("Hide");
      manipulator.SkeletonContainer.transform.parent.gameObject.SetActive(true);
    }
  }
	public void UpdateAdditionalMenuTransform() {
		AdditionalMenu.transform.position = new Vector3(UserHead.position.x, 0f, UserHead.position.z) +
			Quaternion.AngleAxis(UserHead.rotation.eulerAngles.y, Vector3.up) * Vector3.forward * 0.75f;
		AdditionalMenu.transform.rotation = Quaternion.AngleAxis(UserHead.rotation.eulerAngles.y, Vector3.up);
	}
	public void DisplayInfo(string info) {
		Info.GetComponentInChildren<UnityEngine.UI.Text>().text = info;
		Info.GetComponent<Animator>().SetTrigger("Ping");
	}

	private void adjustSlider() {
		float newHeight = userController.GetRecessiveHandAngle() * maxHeight;
		RectTransform sliderTransform = Slider.GetComponent<RectTransform>();
		sliderTransform.sizeDelta = new Vector2(sliderTransform.sizeDelta.x, newHeight);
	}
	private void handleClipPlayback() {
		if (newClip) {
			manipulator.AssignJob(JOB.STANDBY);
			ShowAdditionalMenu(false);
			newClip = false;
			playing = true;
		} else if (playing && !playback.isPlaying) {
			manipulator.AssignJob(JOB.TRACK);
			ShowAdditionalMenu(true);
			playing = false;
		}
	}
}
