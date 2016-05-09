using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// Define symbols for code readability
public static class OPTION {
  public static readonly string UP = "OPTION.UP";
  public static readonly string MIDDLE = "OPTION.MIDDLE";
  public static readonly string DOWN = "OPTION.DOWN";
}

public static class CALLBACK {
  public static readonly string STANDBY = "CALLBACK.STANDBY";
  public static readonly string ROTATE_JOINT = "CALLBACK.ROTATE_JOINT";
  public static readonly string OPEN_ADDITIONAL = "CALLBACK.OPEN_ADDITIONAL";
  public static readonly string CLOSE_ADDITIONAL = "CALLBACK.CLOSE_ADDITIONAL";
  public static readonly string DISCARD_CHANGES = "CALLBACK.DISCARD_CHANGES";
  public static readonly string SAVE_CHANGES = "CALLBACK.SAVE_CHANGES";
  public static readonly string UNDO = "CALLBACK.UNDO";
}

public class MenuNode {
  public readonly string NodeLabel;
  public readonly Dictionary<string, MenuNode> ChildNodes;
  public readonly string Callback;

  public MenuNode(string nodeLabel, string callback, Dictionary<string, MenuNode> childNodes) {
    NodeLabel = nodeLabel;
    Callback = callback;
    ChildNodes = childNodes;
  }
  public override string ToString() {
    return String.Format(
      "MenuNode(NodeLabel: \"{0}\", Callback: {1}, Child count: {2})",
      NodeLabel.Substring(0, Math.Min(NodeLabel.Length, 15)),
      (Callback == null ? "NONE (label)" : Callback),
      (ChildNodes == null ? 0 : ChildNodes.Keys.Count)
    );
  }
}

public class MenuStructure : MonoBehaviour {
  public Button[] OptionButtons;
  public bool DebugMode = false;

  private MenuNode startNode, currentNode, emptyNode = new MenuNode("", null, null);
  private Dictionary<string, Action> callbacks;
  private MenuController controller;
  private ManipulationController manipulator;
  private ClipManager clipManager;

  void Start() {
    controller = GetComponent<MenuController>();
    manipulator = GetComponent<ManipulationController>();
    clipManager = GetComponent<ClipManager>();

    callbacks = new Dictionary<string, Action>() {
      {CALLBACK.STANDBY, () => {
        manipulator.AssignJob(JOB.STANDBY);
      }},
      {CALLBACK.ROTATE_JOINT, () => {
        manipulator.AssignJob(JOB.ROTATE);
      }},
      {CALLBACK.OPEN_ADDITIONAL, () => {
        controller.ShowAdditionalMenu(true);
      }},
      {CALLBACK.CLOSE_ADDITIONAL, () => {
        controller.ShowAdditionalMenu(false);
        Reset();
      }},
      {CALLBACK.DISCARD_CHANGES, () => {
        clipManager.Undo(0);
        Reset();
      }},
      {CALLBACK.SAVE_CHANGES, () => {
        clipManager.SaveRotation(manipulator.HighlightedJoint);
        Reset();
      }},
      {CALLBACK.UNDO, () => {
        clipManager.Undo(1);
        Reset();
      }},
    };

    startNode = new MenuNode("Start", null, new Dictionary<string, MenuNode>() {
      {OPTION.UP, new MenuNode("Undo the last change.", CALLBACK.UNDO, null)},
      {OPTION.MIDDLE, new MenuNode("Open additional actions menu.", CALLBACK.OPEN_ADDITIONAL, new Dictionary<string, MenuNode>() {
        {OPTION.MIDDLE, new MenuNode("Gaze at a button to execute action.", null, null)},
        {OPTION.DOWN, new MenuNode("Exit additional actions menu.", CALLBACK.CLOSE_ADDITIONAL, null)}
      })},
      {OPTION.DOWN, new MenuNode("Touch the joint you wish to animate.", null, new Dictionary<string, MenuNode>() {
        {OPTION.UP, new MenuNode("Do you want to work with this joint?.", null, null)},
        {OPTION.MIDDLE, new MenuNode("Rotate selected joint.", CALLBACK.ROTATE_JOINT, new Dictionary<string, MenuNode>() {
          {OPTION.UP, new MenuNode("Discard changes.", CALLBACK.STANDBY, new Dictionary<string, MenuNode>() {
            {OPTION.UP, new MenuNode("Are you sure about that?", null, null)},
            {OPTION.MIDDLE, new MenuNode("Yes, discard the changes.", CALLBACK.DISCARD_CHANGES, null)},
            {OPTION.DOWN, new MenuNode("I changed my mind, save the changes.", CALLBACK.SAVE_CHANGES, null)}
          })},
          {OPTION.MIDDLE, new MenuNode("Adjust joint rotation to your liking.", null, null)},
          {OPTION.DOWN, new MenuNode("Save changes.", CALLBACK.SAVE_CHANGES, null)}
        })}
      })}
    });

    Reset();
  }

  public void ActivateOption(string option) {
    if (currentNode.ChildNodes == null || !currentNode.ChildNodes.ContainsKey(option))
      Debug.LogError("This option isn't available in this context.");

    currentNode = currentNode.ChildNodes[option];
    if (DebugMode) Debug.Log("Entered node: " + currentNode);

    if (currentNode.Callback != null)
      callbacks[currentNode.Callback].Invoke();
    else if (currentNode.ChildNodes == null && DebugMode)
      Debug.LogWarning("Activated node without Callback or child nodes: " + currentNode);

    rebuildMenu(currentNode.ChildNodes);
  }
  public void Reset() {
    currentNode = startNode;
    if (DebugMode) Debug.Log("Menu reset, now at startNode.");
    manipulator.AssignJob(JOB.TRACK);
    rebuildMenu(currentNode.ChildNodes);
  }

  private void rebuildMenu(Dictionary<string, MenuNode> newNodes) {
    foreach (Button option in OptionButtons)
      setOption(option, newNodes.ContainsKey(option.name) ? newNodes[option.name] : emptyNode);
  }
  private void setOption(Button button, MenuNode node) {
    button.GetComponentInChildren<Text>().text = node.NodeLabel;
    button.interactable = (node.Callback != null);
  }
}
