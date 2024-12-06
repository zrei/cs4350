using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BattleManager))]
public class BattleSetupTool : MonoBehaviour
{
    [SerializeField] private GridSetupHelper m_EnemyGrid;
    [SerializeField] private GridSetupHelper m_PlayerGrid;

    /*
    [HideInInspector]
    [SerializeField] 
    */

    [Header("Just ignore these two thanks :')")]
    public BattleSetupToolEditor.BattleToolTileDataEnemy m_CurrTileEnemy;
    public BattleSetupToolEditor.BattleToolTileDataPlayer m_CurrTilePlayer;

    #if UNITY_EDITOR
    private void Reset()
    {
        foreach (GridLogic gridLogic in GetComponentsInChildren<GridLogic>())
        {
            GridSetupHelper gridSetupHelper = gridLogic.gameObject.AddComponent<GridSetupHelper>();
            if (gridLogic.GridType == GridType.ENEMY)
                m_EnemyGrid = gridSetupHelper;
            else
                m_PlayerGrid = gridSetupHelper;
        }
        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }
    #endif

    public void ClearBattle(CoordPair coordPair)
    {
        m_EnemyGrid.ClearTileEffects(coordPair);
        m_EnemyGrid.ClearUnit(coordPair);
        m_EnemyGrid.ResetTileColor(coordPair);
        m_EnemyGrid.ToggleTileSelected(coordPair, false);

        m_PlayerGrid.ClearTileEffects(coordPair);
        m_PlayerGrid.ClearUnit(coordPair);
        m_PlayerGrid.ResetTileColor(coordPair);
        m_PlayerGrid.ToggleTileSelected(coordPair, false);

        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void SpawnEnemy(CoordPair coordPair, UnitModelData unitModelData, WeaponInstanceSO weaponSO, ClassSO classSO)
    {
        m_EnemyGrid.SpawnUnit(coordPair, unitModelData, weaponSO, classSO);

        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void SpawnTileEffects(GridType gridType, CoordPair coordPair, InflictedTileEffect inflictedTileEffect)
    {
        if (gridType == GridType.ENEMY)
            m_EnemyGrid.SetupTileEffects(coordPair, inflictedTileEffect.TileEffectObjs);
        else
            m_PlayerGrid.SetupTileEffects(coordPair, inflictedTileEffect.TileEffectObjs);

        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void SetSetupTile(CoordPair coordPair)
    {
        m_PlayerGrid.SetTileAsSetupColor(coordPair);

        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void UpdateTile(CoordPair coordPair, BattleSetupToolEditor.BattleToolTileDataEnemy data)
    {
        m_EnemyGrid.ClearTileEffects(coordPair);
        m_EnemyGrid.ClearUnit(coordPair);
        m_EnemyGrid.ResetTileColor(coordPair);

        if (data.m_BattleEnemyUnit.m_EnemyCharacter != null)
        {
            SpawnEnemy(coordPair, data.m_BattleEnemyUnit.m_EnemyCharacter.GetUnitModelData(), data.m_BattleEnemyUnit.m_EnemyCharacter.m_EquippedWeapon, data.m_BattleEnemyUnit.m_EnemyCharacter.m_EnemyClass);
        }

        if (data.m_InflictedTileEffect.m_TileEffect != null)
        {
            SpawnTileEffects(GridType.ENEMY, coordPair, data.m_InflictedTileEffect);
        }

        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void UpdateTile(CoordPair coordPair, BattleSetupToolEditor.BattleToolTileDataPlayer data)
    {
        m_PlayerGrid.ClearTileEffects(coordPair);
        m_PlayerGrid.ClearUnit(coordPair);
        m_PlayerGrid.ResetTileColor(coordPair);

        if (data.m_IsSetupTile)
        {
            SetSetupTile(coordPair);
        }

        if (data.m_InflictedTileEffect.m_TileEffect != null)
        {
            SpawnTileEffects(GridType.PLAYER, coordPair, data.m_InflictedTileEffect);
        }

        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void ToggleTileSelected(GridType gridType, CoordPair coordPair, bool isSelected)
    {
        if (gridType == GridType.ENEMY)
        {
            m_EnemyGrid.ToggleTileSelected(coordPair, isSelected);
        }
        else
        {
            m_PlayerGrid.ToggleTileSelected(coordPair, isSelected);
        }

        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BattleSetupTool))]
public class BattleSetupToolEditor : Editor
{
    [System.Serializable]
    public struct BattleEnemyUnit
    {
        public EnemyCharacterSO m_EnemyCharacter;
        public Stats m_StatAugmnets;
        public EnemyTag m_EnemyTag;
        
        public EnemyUnitPlacement GetEnemyUnitPlacement(CoordPair coordPair)
        {
            return new() {m_Coordinates = coordPair, m_EnemyCharacterData = m_EnemyCharacter, m_EnemyTags = m_EnemyTag, m_StatAugments = m_StatAugmnets};
        }
    }

    [System.Serializable]
    public struct BattleToolTileDataEnemy
    {
        public InflictedTileEffect m_InflictedTileEffect;
        public BattleEnemyUnit m_BattleEnemyUnit;

        public void Clear()
        {
            m_InflictedTileEffect = new InflictedTileEffect {m_TileEffect = null, m_InitialTime = 0};
            m_BattleEnemyUnit = new BattleEnemyUnit {m_EnemyCharacter = null, m_StatAugmnets = new Stats(), m_EnemyTag = EnemyTag.Default};
        }
    }

    [System.Serializable]
    public struct BattleToolTileDataPlayer
    {
        public InflictedTileEffect m_InflictedTileEffect;
        public bool m_IsSetupTile;

        public void Clear()
        {
            m_InflictedTileEffect = new InflictedTileEffect {m_TileEffect = null, m_InitialTime = 0};
            m_IsSetupTile = false;
        }
    }

    private BattleSetupTool m_Target;

    private bool m_ShowAdditionalInstructions = false;

    #region BattleSo Data
    private BattleSO m_BattleSo;
    private BattleToolTileDataEnemy[,] m_BattleDataEnemy = new BattleToolTileDataEnemy[MapData.NUM_ROWS, MapData.NUM_COLS];
    private BattleToolTileDataPlayer[,] m_BattleDataPlayer = new BattleToolTileDataPlayer[MapData.NUM_ROWS, MapData.NUM_COLS];
    #endregion

    #region Selected Tile
    private CoordPair? m_SelectedCoordinates;
    private GridType m_SelectedGrid;
    #endregion

    #region Serialising current tile data
    SerializedProperty m_EnemyTileDataProp;
    SerializedProperty m_PlayerTileDataProp;
    #endregion
    
    #region Tile Colors
    private static Color ENEMY_COLOR = Color.red;
    private static Color PLAYER_COLOR = Color.blue;
    private static Color SELECTED_COLOR = Color.green;
    #endregion

    private void OnEnable()
    {
        m_Target = (BattleSetupTool) target;
        m_EnemyTileDataProp = serializedObject.FindProperty("m_CurrTileEnemy");
        m_PlayerTileDataProp = serializedObject.FindProperty("m_CurrTilePlayer");
        ClearBattle();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10f);

        m_ShowAdditionalInstructions = EditorGUILayout.BeginFoldoutHeaderGroup(m_ShowAdditionalInstructions, "Additional Instructions");

        if (m_ShowAdditionalInstructions)
        {
            GUILayout.Label("DO NOT click off this game object or your changes will reset!!");
            GUILayout.Label("Select a battle SO you want to edit, and use the setup map button to pre-input existing data.");
            GUILayout.Label("If you'd like to get a top-down view, right-click the DownwardView object in the hierarchy and select\nalign view to selected. You can also change the scene view camera from perspective to iso.");
            GUILayout.Label("Use the tile selection below to select a tile, inspect its current data, and edit it.");
            GUILayout.Label("Once done, save to SO to register your changes.");
        }

        GUILayout.Space(10f);

        if (GUILayout.Button("Clear map"))
        {
            ClearBattle();
        }

        GUILayout.Space(10f);

        m_BattleSo = EditorGUILayout.ObjectField("", m_BattleSo, typeof(BattleSO), false) as BattleSO;
        if (m_BattleSo != null)
        {
            if (GUILayout.Button("Setup map using SO"))
            {
                SetMapData();
            }

            if (GUILayout.Button("Save to SO") && EditorUtility.DisplayDialog("Save data?", $"Save the data to the battleSO {m_BattleSo.name}? This can be undone immediately after",
                "Save", "Do not save"))
            {
                SaveToSo();
            }
        }

        RenderMap();

        RenderTileData();

        serializedObject.ApplyModifiedProperties();
    }

    private void SetMapData()
    {
        ClearBattle();
        foreach (EnemyUnitPlacement enemyUnitPlacement in m_BattleSo.m_EnemyUnitsToSpawn)
        {
            CoordPair coordinates = enemyUnitPlacement.m_Coordinates;
            m_BattleDataEnemy[coordinates.m_Row, coordinates.m_Col].m_BattleEnemyUnit = new BattleEnemyUnit{m_EnemyCharacter = enemyUnitPlacement.m_EnemyCharacterData, m_StatAugmnets = enemyUnitPlacement.m_StatAugments, m_EnemyTag = enemyUnitPlacement.m_EnemyTags};
            m_Target.SpawnEnemy(coordinates, enemyUnitPlacement.GetUnitModelData(), enemyUnitPlacement.m_EnemyCharacterData.m_EquippedWeapon, enemyUnitPlacement.m_EnemyCharacterData.m_EnemyClass);
        }

        foreach (StartingTileEffect startingTileEffect in m_BattleSo.m_StartingTileEffects)
        {
            foreach (CoordPair coordPair in startingTileEffect.m_Coordinates)
            {
                if (startingTileEffect.m_GridType == GridType.ENEMY)
                {
                    m_BattleDataEnemy[coordPair.m_Row, coordPair.m_Col].m_InflictedTileEffect = new InflictedTileEffect {m_TileEffect = startingTileEffect.m_InflictedTileEffect.m_TileEffect, m_InitialTime = startingTileEffect.m_InflictedTileEffect.m_InitialTime};
                }
                else
                {
                    m_BattleDataPlayer[coordPair.m_Row, coordPair.m_Col].m_InflictedTileEffect = new InflictedTileEffect {m_TileEffect = startingTileEffect.m_InflictedTileEffect.m_TileEffect, m_InitialTime = startingTileEffect.m_InflictedTileEffect.m_InitialTime};
                }
                m_Target.SpawnTileEffects(startingTileEffect.m_GridType, coordPair, startingTileEffect.m_InflictedTileEffect);
            }
        }

        foreach (CoordPair coordPair in m_BattleSo.m_PlayerStartingTiles)
        {
            m_BattleDataPlayer[coordPair.m_Row, coordPair.m_Col].m_IsSetupTile = true;
            m_Target.SetSetupTile(coordPair);
        }

        if (m_SelectedCoordinates.HasValue)
        {
            m_Target.ToggleTileSelected(m_SelectedGrid, m_SelectedCoordinates.Value, false);
        }
        m_SelectedCoordinates = null;
    }

    private void RenderMap()
    {
        GUILayout.Space(10f);
        RenderEnemyGrid();
        GUILayout.Label("Enemy Grid");

        GUILayout.Space(2f);
        GUILayout.Label("Center Line");
        GUILayout.Space(2f);

        GUILayout.Label("Player Grid");
        RenderPlayerGrid();
    }

    private void RenderEnemyGrid()
    {
        // player side
        for (int r = MapData.NUM_ROWS - 1; r >= 0; --r)
        {
            GUILayout.BeginHorizontal();
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                CoordPair coordinates = new CoordPair(r, c);
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = m_SelectedGrid == GridType.ENEMY && m_SelectedCoordinates.Equals(coordinates) ? SELECTED_COLOR : ENEMY_COLOR;
                if (GUILayout.Button(string.Empty, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    if (m_SelectedCoordinates.HasValue)
                    {
                        m_Target.ToggleTileSelected(m_SelectedGrid, m_SelectedCoordinates.Value, false);
                    }
                    if (m_SelectedCoordinates.Equals(coordinates) && m_SelectedGrid == GridType.ENEMY)
                        m_SelectedCoordinates = null;
                    else
                    {
                        m_SelectedCoordinates = coordinates;
                        m_SelectedGrid = GridType.ENEMY;
                        m_Target.m_CurrTileEnemy = m_BattleDataEnemy[coordinates.m_Row, coordinates.m_Col];
                    }
                    m_Target.ToggleTileSelected(m_SelectedGrid, m_SelectedCoordinates.Value, true);
                }
                GUI.backgroundColor = oldColor;
            }
            GUILayout.EndHorizontal();
        }
    }

    private void RenderPlayerGrid()
    {
        // player side
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            GUILayout.BeginHorizontal();
            for (int c = MapData.NUM_COLS - 1; c >= 0; --c)
            {
                CoordPair coordinates = new CoordPair(r, c);
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = m_SelectedGrid == GridType.PLAYER && m_SelectedCoordinates.Equals(coordinates) ? SELECTED_COLOR : PLAYER_COLOR;
                if (GUILayout.Button(string.Empty, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    if (m_SelectedCoordinates.HasValue)
                    {
                        m_Target.ToggleTileSelected(m_SelectedGrid, m_SelectedCoordinates.Value, false);
                    }
                    if (m_SelectedCoordinates.Equals(coordinates) && m_SelectedGrid == GridType.PLAYER)
                        m_SelectedCoordinates = null;
                    else
                    {
                        m_SelectedCoordinates = coordinates;
                        m_SelectedGrid = GridType.PLAYER;
                        m_Target.m_CurrTilePlayer = m_BattleDataPlayer[coordinates.m_Row, coordinates.m_Col];
                    }
                    m_Target.ToggleTileSelected(m_SelectedGrid, m_SelectedCoordinates.Value, true);
                }
                GUI.backgroundColor = oldColor;
            }
            GUILayout.EndHorizontal();
        }
    }

    private void RenderTileData()
    {
        if (!m_SelectedCoordinates.HasValue)
            return;

        GUILayout.Space(10);

        GUILayout.Label($"{(m_SelectedGrid == GridType.PLAYER ? "Player" : "Enemy")} tile at row {m_SelectedCoordinates.Value.m_Row}, col {m_SelectedCoordinates.Value.m_Col}");

        if (m_SelectedGrid == GridType.ENEMY)
        {
            EditorGUILayout.PropertyField(m_EnemyTileDataProp, new GUIContent("Enemy Tile"));
        }
        else if (m_SelectedGrid == GridType.PLAYER)
        {
            EditorGUILayout.PropertyField(m_PlayerTileDataProp, new GUIContent("Player Tile"));
        }

        if (m_BattleSo == null)
            return;

        if (GUILayout.Button("Apply changes"))
        {
            if (m_SelectedGrid == GridType.ENEMY)
            {
                m_BattleDataEnemy[m_SelectedCoordinates.Value.m_Row, m_SelectedCoordinates.Value.m_Col] = m_Target.m_CurrTileEnemy;
                m_Target.UpdateTile(m_SelectedCoordinates.Value, m_Target.m_CurrTileEnemy);
            }
            else
            {
                m_BattleDataPlayer[m_SelectedCoordinates.Value.m_Row, m_SelectedCoordinates.Value.m_Col] = m_Target.m_CurrTilePlayer;
                m_Target.UpdateTile(m_SelectedCoordinates.Value, m_Target.m_CurrTilePlayer);
            }
        }
    }

    private void ClearBattle()
    {
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                m_BattleDataEnemy[r, c] = new BattleToolTileDataEnemy();
                m_BattleDataEnemy[r, c].Clear();
                m_BattleDataPlayer[r, c] = new BattleToolTileDataPlayer();
                m_BattleDataPlayer[r, c].Clear();
                m_Target.ClearBattle(new CoordPair(r, c));
            }
        }
    }

    private void SaveToSo()
    {
        Dictionary<InflictedTileEffect, List<CoordPair>> inflictedTileEffectGroup = new();
        List<EnemyUnitPlacement> enemyUnitPlacements = new();
        List<StartingTileEffect> startingTileEffects = new();
        List<CoordPair> setupTiles = new();

        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                CoordPair coordinates = new CoordPair(r, c);
                BattleToolTileDataEnemy battleToolTileDataEnemy = m_BattleDataEnemy[r, c];
                if (battleToolTileDataEnemy.m_BattleEnemyUnit.m_EnemyCharacter != null)
                {
                    enemyUnitPlacements.Add(battleToolTileDataEnemy.m_BattleEnemyUnit.GetEnemyUnitPlacement(coordinates));
                }
                if (battleToolTileDataEnemy.m_InflictedTileEffect.m_TileEffect != null)
                {
                    if (!inflictedTileEffectGroup.ContainsKey(battleToolTileDataEnemy.m_InflictedTileEffect))
                        inflictedTileEffectGroup[battleToolTileDataEnemy.m_InflictedTileEffect] = new();
                    
                    inflictedTileEffectGroup[battleToolTileDataEnemy.m_InflictedTileEffect].Add(coordinates);
                }
            }
        }

        foreach (KeyValuePair<InflictedTileEffect, List<CoordPair>> keyValuePair in inflictedTileEffectGroup)
        {
            startingTileEffects.Add(new StartingTileEffect{m_GridType = GridType.ENEMY, m_InflictedTileEffect = keyValuePair.Key, m_Coordinates = keyValuePair.Value});
        }
        inflictedTileEffectGroup.Clear();

        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                CoordPair coordinates = new CoordPair(r, c);
                BattleToolTileDataPlayer battleToolTileDataPlayer = m_BattleDataPlayer[r, c];
                if (battleToolTileDataPlayer.m_IsSetupTile)
                {
                    setupTiles.Add(coordinates);
                }
                if (battleToolTileDataPlayer.m_InflictedTileEffect.m_TileEffect != null)
                {
                    if (!inflictedTileEffectGroup.ContainsKey(battleToolTileDataPlayer.m_InflictedTileEffect))
                        inflictedTileEffectGroup[battleToolTileDataPlayer.m_InflictedTileEffect] = new();
                    
                    inflictedTileEffectGroup[battleToolTileDataPlayer.m_InflictedTileEffect].Add(coordinates);
                }
            }
        }

        foreach (KeyValuePair<InflictedTileEffect, List<CoordPair>> keyValuePair in inflictedTileEffectGroup)
        {
            startingTileEffects.Add(new StartingTileEffect{m_GridType = GridType.PLAYER, m_InflictedTileEffect = keyValuePair.Key, m_Coordinates = keyValuePair.Value});
        }

        Undo.RecordObject(m_BattleSo, "Save new changes to battleSO");
        m_BattleSo.m_EnemyUnitsToSpawn = enemyUnitPlacements;
        m_BattleSo.m_StartingTileEffects = startingTileEffects;
        m_BattleSo.m_PlayerStartingTiles = setupTiles;
        EditorUtility.SetDirty(m_BattleSo);
    }
}
#endif