using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// Define symbols for code readability
public class OPTION {
  public static readonly string UP = "OPTION.UP";
  public static readonly string MIDDLE = "OPTION.MIDDLE";
  public static readonly string DOWN = "OPTION.DOWN";
}

public class MenuNode {
  public readonly string NodeLabel;
  public readonly Dictionary<string, MenuNode> ChildNodes;
  public readonly Action Trigger;

  public MenuNode(string nodeLabel, Action trigger, Dictionary<string, MenuNode> childNodes) {
    NodeLabel = nodeLabel;
    Trigger = trigger;
    ChildNodes = childNodes;
  }
  public override string ToString() {
    return String.Format(
      "MenuNode(NodeLabel: \"{0}\", NodeType: {1}, Child count: {2})",
      NodeLabel.Substring(0, Math.Min(NodeLabel.Length, 25)),
      (Trigger == null) ? "NODE.LABEL" : "NODE.BUTTON",
      (ChildNodes == null ? 0 : ChildNodes.Keys.Count)
    );
  }
}

public class MenuStructure : MonoBehaviour {
  public Button[] OptionButtons;
  public GameObject AdditionalActions;
  public Transform UserHead;
  public bool DebugMode = false;

  private MenuNode startNode, currentNode, emptyNode = new MenuNode("", null, null);
  private Dictionary<string, Action> callbacks;
  private ManipulationController manipulator;

  void Start() {
    manipulator = GetComponent<ManipulationController>();

    callbacks = new Dictionary<string, Action>() {
      {"empty", () => {}},
      {"reset", () => Reset()},
      {"rotate joint", () => {
        manipulator.assignJob("rotate highlighted joint");
      }},
      {"open additional", () => {
        manipulator.assignJob("standby");

        AdditionalActions.transform.position = new Vector3(UserHead.position.x, 0f, UserHead.position.z) +
          (Quaternion.AngleAxis(UserHead.rotation.eulerAngles.y, Vector3.up) * Vector3.forward);
        AdditionalActions.transform.rotation = Quaternion.AngleAxis(UserHead.rotation.eulerAngles.y, Vector3.up);

        AdditionalActions.GetComponent<Animator>().SetTrigger("Display");
      }},
      {"close additional", () => {
        AdditionalActions.GetComponent<Animator>().SetTrigger("Hide");
        Reset();
      }},
      {"n/a", () => { Debug.Log("This functionality is not yet implemented."); }},
    };

    startNode = new MenuNode(
      "Start", null, new Dictionary<string, MenuNode>() {
        {OPTION.UP, new MenuNode("Undo the last change.", callbacks["n/a"], null)},
        {OPTION.MIDDLE, new MenuNode("Open additional actions menu.", callbacks["open additional"], new Dictionary<string, MenuNode>() {
          {OPTION.MIDDLE, new MenuNode("Touch a button to execute action.", null, null)},
          {OPTION.DOWN, new MenuNode("Exit additional actions menu.", callbacks["close additional"], null)}
        })},
        {OPTION.DOWN, new MenuNode("Touch the characters joint to start animating.", null, new Dictionary<string, MenuNode>() {
          {OPTION.UP, new MenuNode("Do you want to work with this joint?.", null, null)},
          {OPTION.MIDDLE, new MenuNode("Rotate selected joint.", callbacks["rotate joint"], new Dictionary<string, MenuNode>() {
            {OPTION.UP, new MenuNode("Discard changes.", callbacks["n/a"], new Dictionary<string, MenuNode>() {
              {OPTION.UP, new MenuNode("Are you sure about that?", null, null)},
              {OPTION.MIDDLE, new MenuNode("Yes, discard the changes.", callbacks["reset"], null)},
              {OPTION.DOWN, new MenuNode("I changed my mind, save the changes.", callbacks["n/a"], null)}
            })},
            {OPTION.MIDDLE, new MenuNode("Adjust joint rotation to your liking.", null, null)},
            {OPTION.DOWN, new MenuNode("Save changes.", callbacks["n/a"], null)}
          })}
        })
      }}
    );

    Reset();
  }

  public void ActivateOption(string option) {
    if (currentNode.ChildNodes == null || !currentNode.ChildNodes.ContainsKey(option))
      Debug.LogError("This option isn't available in this context.");

    currentNode = currentNode.ChildNodes[option];
    if (DebugMode) Debug.Log("Entered node: " + currentNode);

    if (currentNode.Trigger != null)
      currentNode.Trigger();
    else if (currentNode.ChildNodes == null && DebugMode)
      Debug.LogWarning("Activated node without trigger or child nodes: " + currentNode);

    rebuildMenu(currentNode.ChildNodes);
  }
  public void Reset() {
    currentNode = startNode;
    if (DebugMode) Debug.Log("Menu reset, now at startNode.");
    manipulator.assignJob("track cursor-model collisions");
    rebuildMenu(currentNode.ChildNodes);
  }

  private void rebuildMenu(Dictionary<string, MenuNode> newNodes) {
    foreach (Button option in OptionButtons)
      buildOption(option, !newNodes.ContainsKey(option.name) ? emptyNode : newNodes[option.name]);
  }
  private void buildOption(Button button, MenuNode node) {
    button.GetComponentInChildren<Text>().text = node.NodeLabel;
    button.interactable = (node.Trigger != null);
  }
}
