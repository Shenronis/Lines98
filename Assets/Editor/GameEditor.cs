using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GameManager game = (GameManager)target;

        GUILayout.BeginHorizontal();        

        if (GUILayout.Button("Save Game"))
        {
            game.SaveGame();
        }
        
        if (GUILayout.Button("Load Game"))
        {
            game.LoadGame();
        }

        GUILayout.EndHorizontal();
    }    
}
