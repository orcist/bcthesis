using UnityEngine;
using System;
using System.Collections.Generic;

// Define symbols for code readability
public class OPTION {
  public static readonly uint UP = 20001;
  public static readonly uint MIDDLE = 20002;
  public static readonly uint DOWN = 20003;
}

public class MenuNode {
  public readonly string NodeLabel;
  public readonly Dictionary<uint, MenuNode> ChildNodes;
  public readonly Action Trigger;

  public MenuNode(string nodeLabel, Action trigger, Dictionary<uint, MenuNode> childNodes) {
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
  public bool DebugMode = false;
  public MenuNode CurrentNode;

  private MenuNode startNode;
  private Dictionary<string, Action> callbacks;

  void Awake() {
    callbacks = new Dictionary<string, Action>() {
      {"reset", () => Reset()},
      {"n/a", () => { Debug.Log("This functionality is not yet implemented."); }},
    };

    startNode = new MenuNode(
      "Start", null, new Dictionary<uint, MenuNode>() {
        {OPTION.UP, new MenuNode("Additional actions.", null, new Dictionary<uint, MenuNode>() {
          {OPTION.UP, new MenuNode("Undo the last change. There won't be any additional confirmation.", callbacks["n/a"], null)},
          {OPTION.MIDDLE, new MenuNode("Open additional actions menu.", null, new Dictionary<uint, MenuNode>())},
          {OPTION.DOWN, new MenuNode("Touch the characters joint to start animating.", null, new Dictionary<uint, MenuNode>())},
        })},
        {OPTION.DOWN, new MenuNode("Character animation.", null, new Dictionary<uint, MenuNode>() {
          {OPTION.UP, new MenuNode("Do you want to work with this joint?.", null, null)},
          {OPTION.MIDDLE, new MenuNode("Rotate selected joint.", callbacks["n/a"], new Dictionary<uint, MenuNode>() {
            {OPTION.UP, new MenuNode("Discard changes.", null, new Dictionary<uint, MenuNode>() {
              {OPTION.UP, new MenuNode("Are you sure about that?", null, null)},
              {OPTION.MIDDLE, new MenuNode("Yes, discard the changes.", callbacks["reset"], null)},
              {OPTION.DOWN, new MenuNode("I changed my mind, save the changes.", callbacks["n/a"], null)}
            })},
            {OPTION.MIDDLE, new MenuNode("Adjust joint rotation to your liking.", null, null)},
            {OPTION.DOWN, new MenuNode("Save changes.", callbacks["n/a"], null)}
          })},
          {OPTION.DOWN, new MenuNode("To access additional actions menu break contact with model.", null, null)}
        })}
      }
    );

    Reset();
  }

  public Dictionary<uint, MenuNode> ActivateOption(uint option) {
    if (CurrentNode.ChildNodes == null || !CurrentNode.ChildNodes.ContainsKey(option))
      Debug.LogError("This option isn't available in this context.");

    CurrentNode = CurrentNode.ChildNodes[option];

    if (DebugMode) Debug.Log("Entered node: " + CurrentNode);

    if (CurrentNode.Trigger != null)
      CurrentNode.Trigger();
    else if (CurrentNode.ChildNodes == null && DebugMode)
      Debug.LogWarning("Activated node without trigger or child nodes: " + CurrentNode);

    rebuildMenu();

    return CurrentNode.ChildNodes;
  }
  public void Reset() {
    CurrentNode = startNode;
    if (DebugMode) Debug.Log("Menu reset, now at startNode.");
  }

  private void rebuildMenu() { // TODO manipulate GameObjects and sprites to show new menu {0}
    Debug.Log("Rebuild menu");
  }
}
