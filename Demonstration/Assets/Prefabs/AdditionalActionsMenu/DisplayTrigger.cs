using UnityEngine;

public class DisplayTrigger : StateMachineBehaviour {
	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.gameObject.GetComponent<UnityEngine.UI.Image>().enabled = true;
	}
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.gameObject.GetComponent<UnityEngine.UI.Image>().enabled = false;
	}
}
