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
  public bool DebugMode = false;
  public MenuNode CurrentNode;

  private MenuNode startNode, blankNode = new MenuNode("", null, null);
  private Dictionary<string, Action> callbacks;

  void Awake() {
    callbacks = new Dictionary<string, Action>() {
      {"empty", () => {}},
      {"reset", () => Reset()},
      {"n/a", () => { Debug.Log("This functionality is not yet implemented."); }},
    };

    startNode = new MenuNode(
      "Start", null, new Dictionary<string, MenuNode>() {
        {OPTION.UP, new MenuNode("Additional actions.", null, new Dictionary<string, MenuNode>() {
          {OPTION.UP, new MenuNode("Undo the last change.", callbacks["n/a"], null)},
          {OPTION.MIDDLE, new MenuNode("Open additional actions menu.", null, new Dictionary<string, MenuNode>())},
          {OPTION.DOWN, new MenuNode("Touch the characters joint to start animating.", null, new Dictionary<string, MenuNode>())},
        })},
        {OPTION.DOWN, new MenuNode("Character animation.", null, new Dictionary<string, MenuNode>() {
          {OPTION.UP, new MenuNode("Do you want to work with this joint?.", null, null)},
          {OPTION.MIDDLE, new MenuNode("Rotate selected joint.", callbacks["n/a"], new Dictionary<string, MenuNode>() {
            {OPTION.UP, new MenuNode("Discard changes.", callbacks["n/a"], new Dictionary<string, MenuNode>() {
              {OPTION.UP, new MenuNode("Are you sure about that?", null, null)},
              {OPTION.MIDDLE, new MenuNode("Yes, discard the changes.", callbacks["reset"], null)},
              {OPTION.DOWN, new MenuNode("I changed my mind, save the changes.", callbacks["n/a"], null)}
            })},
            {OPTION.MIDDLE, new MenuNode("Adjust joint rotation to your liking.", null, null)},
            {OPTION.DOWN, new MenuNode("Save changes.", callbacks["n/a"], null)}
          })},
          {OPTION.DOWN, new MenuNode("To access additional actions break contact.", null, null)}
        })}
      }
    );

    Reset();
    ActivateOption(OPTION.DOWN);
  }

  public void ActivateOption(string option) {
    if (CurrentNode.ChildNodes == null || !CurrentNode.ChildNodes.ContainsKey(option))
      Debug.LogError("This option isn't available in this context.");

    CurrentNode = CurrentNode.ChildNodes[option];
    if (DebugMode) Debug.Log("Entered node: " + CurrentNode);

    if (CurrentNode.Trigger != null)
      CurrentNode.Trigger();
    else if (CurrentNode.ChildNodes == null && DebugMode)
      Debug.LogWarning("Activated node without trigger or child nodes: " + CurrentNode);

    rebuildMenu(CurrentNode.ChildNodes);
  }
  public void Reset() {
    CurrentNode = startNode;
    if (DebugMode) Debug.Log("Menu reset, now at startNode.");
    rebuildMenu(CurrentNode.ChildNodes);
  }

  private void rebuildMenu(Dictionary<string, MenuNode> newNodes) {
    foreach (Button option in OptionButtons)
      buildOption(option, !newNodes.ContainsKey(option.name) ? blankNode : newNodes[option.name]);
  }
  private void buildOption(Button button, MenuNode node) {
    button.GetComponentInChildren<Text>().text = node.NodeLabel;
    button.interactable = (node.Trigger != null);
  }
}
