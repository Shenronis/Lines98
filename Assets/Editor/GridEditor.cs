using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class GridEditor : Editor
{
    bool isTracking;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GridManager grid = (GridManager)target;        
        
        if (GUILayout.Button("Randomize Selected Unit"))
        {
            var selectedUnit = BallUnitManager.Instance.selectedBallUnit;
            if (selectedUnit == null) return;

            var selectedTile = selectedUnit.OccupiedTile;
            var color = BallUnitManager.Instance.GetRandomBallUnit();
            var special = BallUnitManager.Instance.GetRandomSpecial();
            BallUnitManager.Instance.SetBallUnitAt(selectedTile, color, special);
            BallUnitManager.Instance.selectedBallUnit = selectedTile.OccupiedBallUnit;
            selectedTile.isSelected = true;
            selectedTile.SetHighlight();
            selectedTile.FloatSelected();            
        }        

        if (GUILayout.Button("Next Turn"))
        {            
            GameManager.Instance.ChangeState(GameState.SpawnAndQueue);
        }

        if (GUILayout.Button("Clear Board"))
        {
            grid.RestartAllTiles();
        }
        
        if (GUILayout.Button("Shake Camera"))
        {
            CameraShake.Instance.Shake();
        }

        if (GUILayout.Button("???"))
        {
            grid.TACTICALNUKE();
            CameraShake.Instance.Shake(1f, 3f);
        }
    }
}
