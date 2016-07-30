﻿using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using Abilities;
using System.Linq;
using System.IO;
using System.Collections.Generic;

[CustomEditor(typeof(GameController))]
public class GameControllerEditor : Editor {
    ReorderableList abilityList = null;

    void OnEnable() {
        var obj = (GameController)target;

        obj.Initialize();

        if (!Application.isPlaying) {
            if (obj.abilities != null) {
                bool removed = false;
                for (int i = 0; i < obj.abilities.Count; i++) {
                    if (obj.abilities[i].ability == null) {
                        obj.abilities.RemoveAt(i--);
                        removed = true;
                    }
                }
                IEnumerable<Ability> newAbilities;
                newAbilities = GameController.abilityList.Where(x => obj.abilities.FirstOrDefault(y => x.GetType() == y.ability.GetType()) == default(AbilityInfo));
                foreach (var a in newAbilities)
                    obj.abilities.Add(new AbilityInfo(a, 1, true));
                if (newAbilities.Count() > 0 || removed)
                    Save();
            } else {
                foreach (var a in GameController.abilityList)
                    obj.abilities.Add(new AbilityInfo(a, 1, true));
                Save();
            }
        }

        abilityList = new ReorderableList(obj.abilities, typeof(AbilityInfo), true, true, false, false);

        // Add listeners to draw events
        abilityList.drawHeaderCallback += DrawHeader;
        abilityList.drawElementCallback += DrawElement;
    }

    private void OnDisable() {
        // Make sure we don't get memory leaks etc.
        if (abilityList != null) {
            abilityList.drawHeaderCallback -= DrawHeader;
            abilityList.drawElementCallback -= DrawElement;
        }
    }

    private void DrawHeader(Rect rect) {
        GUI.Label(rect, "Abilities");
    }

    private void DrawElement(Rect rect, int index, bool active, bool focused) {
        AbilityInfo item = ((GameController)target).abilities[index];

        EditorGUI.BeginChangeCheck();
        float offset = 0;
        if (!Application.isPlaying) {
            item.enabled = EditorGUI.Toggle(new Rect(rect.x, rect.y, 15, rect.height), item.enabled);
            offset = 15;
        }
        EditorGUI.LabelField(new Rect(rect.x + offset, rect.y, rect.width - 30, rect.height), item.name);
        item.chanceToSpawn = EditorGUI.FloatField(new Rect(rect.width - 30, rect.y, 30, rect.height), item.chanceToSpawn);
        if (EditorGUI.EndChangeCheck()) {
            Save();
        }
    }


    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        var obj = (GameController)target;
        if (Application.isPlaying)
            EditorGUILayout.LabelField("Spawned", obj.spawned.ToString());
        obj._sun = (Light)EditorGUILayout.ObjectField("Sun", obj._sun, typeof(Light), true);
        obj.finalResults = (Text)EditorGUILayout.ObjectField("Final result", obj.finalResults, typeof(Text), true);
        obj.pauseMenu = (GameObject)EditorGUILayout.ObjectField("Pause menu", obj.pauseMenu, typeof(GameObject), true);
        abilityList.DoLayoutList();
        if (GUI.changed) {
            Save();
        }
    }

    void Save() {
        StreamWriter sw = new StreamWriter("Assets/Resources/" + GameController.ABILITY_FILE + ".json");
        sw.Write(JsonUtility.ToJson(new AbilityInfoList(((GameController)target).abilities)));
        sw.Dispose();
        AssetDatabase.Refresh();
    }

}
