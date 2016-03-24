using UnityEngine;
using System;
using System.Collections.Generic;

// Define symbols for code readability
class NODE_TYPE {
  public static readonly uint BUTTON = 10001;
  public static readonly uint LABEL = 10002;

  public static string Translate(uint symbol) {
    if (symbol == NODE_TYPE.BUTTON)
      return "NODE_TYPE.BUTTON";
    else if (symbol == NODE_TYPE.LABEL)
      return "NODE_TYPE.LABEL";
    else
      return "NODE_TYPE.UNKNOWN";
  }
}

class OPTION {
  public static readonly uint UP = 20001;
  public static readonly uint MIDDLE = 20002;
  public static readonly uint DOWN = 20003;
}

public class MenuNode {
  public readonly string NodeLabel;
  public readonly uint NodeType;
  public readonly Dictionary<uint, MenuNode> ChildNodes;

  public MenuNode(string nodeLabel, uint nodeType, Action trigger, Dictionary<uint, MenuNode> childNodes) {
    NodeLabel = nodeLabel;
    NodeType = nodeType;
    Trigger = trigger;
    ChildNodes = childNodes;
  }
  public readonly Action Trigger;
  public override string ToString() {
    return String.Format(
      "MenuNode(NodeLabel: \"{0}\", NodeType: {1}, Child count: {2})",
      NodeLabel, NODE_TYPE.Translate(NodeType), (ChildNodes == null ? 0 : ChildNodes.Keys.Count)
    );
  }
}

public class MenuStructure : MonoBehaviour {
  public bool DebugMode = false;
  public MenuNode CurrentNode;

  private MenuNode StartNode;
  private Dictionary<string, Action> callbacks;

  void Awake() {
    callbacks = new Dictionary<string, Action>() {
      {"start", () => { Debug.Log("Start."); }},
      {"reset", () => { Reset(); }},
      {"n/a", () => { Debug.Log("This functionality is not yet implemented."); }},
    };

    StartNode = new MenuNode(
      "Start", NODE_TYPE.LABEL, callbacks["start"], new Dictionary<uint, MenuNode>() {
        {OPTION.DOWN, new MenuNode("Additional actions.", NODE_TYPE.LABEL, null, new Dictionary<uint, MenuNode>() {
          {OPTION.UP, new MenuNode("Touch any button for more information.", NODE_TYPE.LABEL, null, new Dictionary<uint, MenuNode>())},
          {OPTION.MIDDLE, new MenuNode("Step back to continue working.", NODE_TYPE.LABEL, null, new Dictionary<uint, MenuNode>())},
        })},
        {OPTION.UP, new MenuNode("Character animation", NODE_TYPE.LABEL, null, new Dictionary<uint, MenuNode>() {
          {OPTION.UP, new MenuNode("Touch the joint you wish to manipulate.", NODE_TYPE.LABEL, null, null)},
          {OPTION.MIDDLE, new MenuNode("Manipulate selected joint.", NODE_TYPE.BUTTON, callbacks["n/a"], new Dictionary<uint, MenuNode>() {
            {OPTION.MIDDLE, new MenuNode("Which do you wish to manipulate?", NODE_TYPE.LABEL, null, null)},
            {OPTION.UP, new MenuNode("Joint position.", NODE_TYPE.BUTTON, callbacks["n/a"], new Dictionary<uint, MenuNode>() {
              {OPTION.UP, new MenuNode("Adjust position to your liking.", NODE_TYPE.LABEL, null, null)},
              {OPTION.DOWN, new MenuNode("Discard changes.", NODE_TYPE.BUTTON, callbacks["n/a"], null)},
              {OPTION.MIDDLE, new MenuNode("Save changes.", NODE_TYPE.BUTTON, callbacks["n/a"], new Dictionary<uint, MenuNode>() {
                {OPTION.DOWN, new MenuNode("Continue working.", NODE_TYPE.BUTTON, callbacks["reset"], null)}
              })}
            })},
            {OPTION.DOWN, new MenuNode("Joint rotation.", NODE_TYPE.BUTTON, callbacks["n/a"], new Dictionary<uint, MenuNode>() {
              {OPTION.DOWN, new MenuNode("Adjust rotation to your liking.", NODE_TYPE.LABEL, null, null)},
              {OPTION.UP, new MenuNode("Discard changes.", NODE_TYPE.BUTTON, callbacks["n/a"], new Dictionary<uint, MenuNode>() {
                {OPTION.DOWN, new MenuNode("Continue working.", NODE_TYPE.BUTTON, callbacks["reset"], null)}
              })},
              {OPTION.MIDDLE, new MenuNode("Save changes.", NODE_TYPE.BUTTON, callbacks["n/a"], new Dictionary<uint, MenuNode>() {
                {OPTION.DOWN, new MenuNode("Continue working.", NODE_TYPE.BUTTON, callbacks["reset"], null)}
              })}
            })}
          })}
        })}
      }
    );

    Reset();
  }

  public void ActivateOption(uint option) {
    if (CurrentNode.ChildNodes == null || !CurrentNode.ChildNodes.ContainsKey(option))
      Debug.LogError("This option isn't available in this context."); // TODO remove this ?? {1}

    CurrentNode = CurrentNode.ChildNodes[option];

    if (DebugMode) Debug.Log("Entered node: " + CurrentNode);

    if (CurrentNode.Trigger != null) CurrentNode.Trigger();
    rebuildMenu();
  }
  public void Reset() {
    CurrentNode = StartNode;
    CurrentNode.Trigger();
  }

  private void rebuildMenu() { // TODO manipulate GameObjects and sprites to show new menu {0}
    Debug.Log("Rebuild menu");
  }
}
