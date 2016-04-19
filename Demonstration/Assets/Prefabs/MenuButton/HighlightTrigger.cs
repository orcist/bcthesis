using UnityEngine;

public class HighlightTrigger : StateMachineBehaviour {
	private bool triggered = false;

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (triggered || animator.IsInTransition(0))
			return;

		triggered = true;
		animator.transform.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		triggered = false;
	}
}
