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
  public static readonly uint BOTTOM = 20002;
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
  public Action Trigger;
  public override string ToString() {
    return String.Format(
      "MenuNode(NodeLabel: \"{0}\", NodeType: {1}, Child count: {2})",
      NodeLabel, NODE_TYPE.Translate(NodeType), ChildNodes.Keys.Count
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
      {"start", () => { Debug.Log("Reset menu."); }},
      {"first", () => { Debug.Log("Activate first callback."); }}
    };

    StartNode = new MenuNode(
      "Start node", NODE_TYPE.LABEL, callbacks["start"], new Dictionary<uint, MenuNode>() {
        { OPTION.UP, new MenuNode("First choice", NODE_TYPE.BUTTON, callbacks["first"], new Dictionary<uint, MenuNode>()) }
      }
    );

    Reset();
    ActivateOption(OPTION.UP);
  }

  public void ActivateOption(uint option) {
    CurrentNode = CurrentNode.ChildNodes[option];

    if (DebugMode) Debug.Log("Entered node: " + CurrentNode);

    CurrentNode.Trigger();
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
