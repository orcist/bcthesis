﻿using UnityEngine;
using System;
using System.Collections.Generic;

// Define symbols for code readability
class OPTION {
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
      NodeLabel,
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
        {OPTION.DOWN, new MenuNode("Additional actions.", null, new Dictionary<uint, MenuNode>() {
          {OPTION.UP, new MenuNode("Touch any button for more information.", null, new Dictionary<uint, MenuNode>())},
          {OPTION.MIDDLE, new MenuNode("Step back to continue working.", null, new Dictionary<uint, MenuNode>())},
        })},
        {OPTION.UP, new MenuNode("Character animation", null, new Dictionary<uint, MenuNode>() {
          {OPTION.UP, new MenuNode("Touch the joint you wish to manipulate.", null, null)},
          {OPTION.MIDDLE, new MenuNode("Manipulate selected joint.", callbacks["n/a"], new Dictionary<uint, MenuNode>() {
            {OPTION.MIDDLE, new MenuNode("Which do you wish to manipulate?", null, null)},
            {OPTION.UP, new MenuNode("Joint position.", callbacks["n/a"], new Dictionary<uint, MenuNode>() {
              {OPTION.UP, new MenuNode("Adjust position to your liking.", null, null)},
              {OPTION.DOWN, new MenuNode("Discard changes.", callbacks["n/a"], null)},
              {OPTION.MIDDLE, new MenuNode("Save changes.", callbacks["n/a"], new Dictionary<uint, MenuNode>() {
                {OPTION.DOWN, new MenuNode("Continue working.", callbacks["reset"], null)}
              })}
            })},
            {OPTION.DOWN, new MenuNode("Joint rotation.", callbacks["n/a"], new Dictionary<uint, MenuNode>() {
              {OPTION.DOWN, new MenuNode("Adjust rotation to your liking.", null, null)},
              {OPTION.UP, new MenuNode("Discard changes.", callbacks["n/a"], new Dictionary<uint, MenuNode>() {
                {OPTION.DOWN, new MenuNode("Continue working.", callbacks["reset"], null)}
              })},
              {OPTION.MIDDLE, new MenuNode("Save changes.", callbacks["n/a"], new Dictionary<uint, MenuNode>() {
                {OPTION.DOWN, new MenuNode("Continue working.", callbacks["reset"], null)}
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
      Debug.LogError("This option isn't available in this context.");

    CurrentNode = CurrentNode.ChildNodes[option];

    if (DebugMode)
      Debug.Log("Entered node: " + CurrentNode);

    if (CurrentNode.Trigger != null)
      CurrentNode.Trigger();
    rebuildMenu();
  }
  public void Reset() {
    CurrentNode = startNode;
    Debug.Log("Menu reset, now at startNode.");
  }

  private void rebuildMenu() { // TODO manipulate GameObjects and sprites to show new menu {0}
    Debug.Log("Rebuild menu");
  }
}