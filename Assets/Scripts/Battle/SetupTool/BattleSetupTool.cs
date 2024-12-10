using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BattleManager))]
public class BattleSetupTool : MonoBehaviour
{
    #if UNITY_EDITOR
    [SerializeField] private GridSetupHelper m_EnemyGrid;
    [SerializeField] private GridSetupHelper m_PlayerGrid;

    [Header("Ignore these fields")]
    [HideInInspector]
    public BattleSO m_BattleSo = null;
    public BattleSetupToolEditor.BattleToolTileDataEnemy[,] m_BattleDataEnemy = new BattleSetupToolEditor.BattleToolTileDataEnemy[MapData.NUM_ROWS, MapData.NUM_COLS];
    public BattleSetupToolEditor.BattleToolTileDataPlayer[,] m_BattleDataPlayer = new BattleSetupToolEditor.BattleToolTileDataPlayer[MapData.NUM_ROWS, MapData.NUM_COLS];
    [HideInInspector]
    public CoordPair? m_SelectedCoordinates = null; 
    [HideInInspector]
    public GridType m_SelectedGrid;
    public BattleSetupToolEditor.BattleToolTileDataEnemy m_CurrTileEnemy;
    public BattleSetupToolEditor.BattleToolTileDataPlayer m_CurrTilePlayer;

    /// <summary>
    /// Utility for adding the required children automatically
    /// </summary>
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

    public void ClearBattle(CoordPair coordPair)
    {
        m_EnemyGrid.ClearTileEffects(coordPair);
        m_EnemyGrid.ClearUnit(coordPair);
        m_EnemyGrid.ResetTileColor(coordPair);

        m_PlayerGrid.ClearTileEffects(coordPair);
        m_PlayerGrid.ClearUnit(coordPair);
        m_PlayerGrid.ResetTileColor(coordPair);

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
    #endif
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
    private BattleSO BattleSO {
        get {
            return m_Target.m_BattleSo;
        }
        set {
            m_Target.m_BattleSo = value;
        }
    }
    private BattleToolTileDataEnemy[,] BattleDataEnemy => m_Target.m_BattleDataEnemy;
    private BattleToolTileDataPlayer[,] BattleDataPlayer => m_Target.m_BattleDataPlayer;
    #endregion

    #region Selected Tile
    private CoordPair? SelectedCoordinates {
        get {
            return m_Target.m_SelectedCoordinates;
        }
        set {
            m_Target.m_SelectedCoordinates = value;
        }
    }
    private GridType SelectedGrid {
        get {
            return m_Target.m_SelectedGrid;
        }
        set {
            m_Target.m_SelectedGrid = value;
        }
    }
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

    #region Initialisation
    private void OnEnable()
    {
        m_Target = (BattleSetupTool) target;
        m_EnemyTileDataProp = serializedObject.FindProperty("m_CurrTileEnemy");
        m_PlayerTileDataProp = serializedObject.FindProperty("m_CurrTilePlayer");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10f);

        m_ShowAdditionalInstructions = EditorGUILayout.BeginFoldoutHeaderGroup(m_ShowAdditionalInstructions, "Additional Instructions");

        if (m_ShowAdditionalInstructions)
        {
            GUILayout.Label("Select a battle SO you want to edit, and use the setup map button to pre-input existing data.");
            GUILayout.Label("If you'd like to get a top-down view, right-click the DownwardView object in the hierarchy and select\nalign view to selected. You can also change the scene view camera from perspective to iso.");
            GUILayout.Label("Use the tile selection below to select a tile, inspect its current data, and edit it.");
            GUILayout.Label("Tile color changes and gizmo changes take a while to register, one tip\nis to click over to the console window to force the change.");
            GUILayout.Label("Once done, save to SO to register your changes.");
        }

        GUILayout.Space(10f);

        if (GUILayout.Button("Clear map"))
        {
            ClearBattle();
        }

        GUILayout.Space(10f);

        BattleSO = EditorGUILayout.ObjectField("", BattleSO, typeof(BattleSO), false) as BattleSO;
        if (BattleSO != null)
        {
            if (GUILayout.Button("Setup map using SO"))
            {
                SetMapData();
            }

            if (GUILayout.Button("Save to SO") && EditorUtility.DisplayDialog("Save data?", $"Save the data to the battleSO {BattleSO.name}? This can be undone immediately after",
                "Save", "Do not save"))
            {
                SaveToSo();
            }
        }

        RenderMap();

        RenderTileData();

        serializedObject.ApplyModifiedProperties();
    }
    #endregion

    #region Render Map
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
                GUI.backgroundColor = SelectedGrid == GridType.ENEMY && SelectedCoordinates.Equals(coordinates) ? SELECTED_COLOR : ENEMY_COLOR;
                if (GUILayout.Button(string.Empty, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    if (SelectedCoordinates.HasValue)
                    {
                        m_Target.ToggleTileSelected(SelectedGrid, SelectedCoordinates.Value, false);
                    }
                    if (SelectedCoordinates.Equals(coordinates) && SelectedGrid == GridType.ENEMY)
                        SelectedCoordinates = null;
                    else
                    {
                        SelectedCoordinates = coordinates;
                        SelectedGrid = GridType.ENEMY;
                        m_Target.m_CurrTileEnemy = BattleDataEnemy[coordinates.m_Row, coordinates.m_Col];
                    }
                    if (SelectedCoordinates.HasValue)
                        m_Target.ToggleTileSelected(SelectedGrid, SelectedCoordinates.Value, true);
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
                GUI.backgroundColor = SelectedGrid == GridType.PLAYER && SelectedCoordinates.Equals(coordinates) ? SELECTED_COLOR : PLAYER_COLOR;
                if (GUILayout.Button(string.Empty, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    if (SelectedCoordinates.HasValue)
                    {
                        m_Target.ToggleTileSelected(SelectedGrid, SelectedCoordinates.Value, false);
                    }
                    if (SelectedCoordinates.Equals(coordinates) && SelectedGrid == GridType.PLAYER)
                        SelectedCoordinates = null;
                    else
                    {
                        SelectedCoordinates = coordinates;
                        SelectedGrid = GridType.PLAYER;
                        m_Target.m_CurrTilePlayer = BattleDataPlayer[coordinates.m_Row, coordinates.m_Col];
                    }
                    if (SelectedCoordinates.HasValue)
                        m_Target.ToggleTileSelected(SelectedGrid, SelectedCoordinates.Value, true);
                }
                GUI.backgroundColor = oldColor;
            }
            GUILayout.EndHorizontal();
        }
    }
    #endregion

    #region Render Tile Data
    private void RenderTileData()
    {
        if (!SelectedCoordinates.HasValue)
            return;

        GUILayout.Space(10);

        GUILayout.Label($"{(SelectedGrid == GridType.PLAYER ? "Player" : "Enemy")} tile at row {SelectedCoordinates.Value.m_Row}, col {SelectedCoordinates.Value.m_Col}");

        if (SelectedGrid == GridType.ENEMY)
        {
            EditorGUILayout.PropertyField(m_EnemyTileDataProp, new GUIContent("Enemy Tile"));
        }
        else if (SelectedGrid == GridType.PLAYER)
        {
            EditorGUILayout.PropertyField(m_PlayerTileDataProp, new GUIContent("Player Tile"));
        }

        if (BattleSO == null)
            return;

        if (GUILayout.Button("Apply changes"))
        {
            if (SelectedGrid == GridType.ENEMY)
            {
                BattleDataEnemy[SelectedCoordinates.Value.m_Row, SelectedCoordinates.Value.m_Col] = m_Target.m_CurrTileEnemy;
                m_Target.UpdateTile(SelectedCoordinates.Value, m_Target.m_CurrTileEnemy);
            }
            else
            {
                BattleDataPlayer[SelectedCoordinates.Value.m_Row, SelectedCoordinates.Value.m_Col] = m_Target.m_CurrTilePlayer;
                m_Target.UpdateTile(SelectedCoordinates.Value, m_Target.m_CurrTilePlayer);
            }
        }
    }
    #endregion

    #region Handle Tile Data
    private void ClearBattle()
    {
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                BattleDataEnemy[r, c] = new BattleToolTileDataEnemy();
                BattleDataEnemy[r, c].Clear();
                BattleDataPlayer[r, c] = new BattleToolTileDataPlayer();
                BattleDataPlayer[r, c].Clear();
                m_Target.ClearBattle(new CoordPair(r, c));
            }
        }
    }

    private void SetMapData()
    {
        ClearBattle();
        foreach (EnemyUnitPlacement enemyUnitPlacement in BattleSO.m_EnemyUnitsToSpawn)
        {
            CoordPair coordinates = enemyUnitPlacement.m_Coordinates;
            BattleDataEnemy[coordinates.m_Row, coordinates.m_Col].m_BattleEnemyUnit = new BattleEnemyUnit{m_EnemyCharacter = enemyUnitPlacement.m_EnemyCharacterData, m_StatAugmnets = enemyUnitPlacement.m_StatAugments, m_EnemyTag = enemyUnitPlacement.m_EnemyTags};
            m_Target.SpawnEnemy(coordinates, enemyUnitPlacement.GetUnitModelData(), enemyUnitPlacement.m_EnemyCharacterData.m_EquippedWeapon, enemyUnitPlacement.m_EnemyCharacterData.m_EnemyClass);
        }

        foreach (StartingTileEffect startingTileEffect in BattleSO.m_StartingTileEffects)
        {
            foreach (CoordPair coordPair in startingTileEffect.m_Coordinates)
            {
                if (startingTileEffect.m_GridType == GridType.ENEMY)
                {
                    BattleDataEnemy[coordPair.m_Row, coordPair.m_Col].m_InflictedTileEffect = new InflictedTileEffect {m_TileEffect = startingTileEffect.m_InflictedTileEffect.m_TileEffect, m_InitialTime = startingTileEffect.m_InflictedTileEffect.m_InitialTime};
                }
                else
                {
                    BattleDataPlayer[coordPair.m_Row, coordPair.m_Col].m_InflictedTileEffect = new InflictedTileEffect {m_TileEffect = startingTileEffect.m_InflictedTileEffect.m_TileEffect, m_InitialTime = startingTileEffect.m_InflictedTileEffect.m_InitialTime};
                }
                m_Target.SpawnTileEffects(startingTileEffect.m_GridType, coordPair, startingTileEffect.m_InflictedTileEffect);
            }
        }

        foreach (CoordPair coordPair in BattleSO.m_PlayerStartingTiles)
        {
            BattleDataPlayer[coordPair.m_Row, coordPair.m_Col].m_IsSetupTile = true;
            m_Target.SetSetupTile(coordPair);
        }
    }
    #endregion

    #region Save
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
                BattleToolTileDataEnemy battleToolTileDataEnemy = BattleDataEnemy[r, c];
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
                BattleToolTileDataPlayer battleToolTileDataPlayer = BattleDataPlayer[r, c];
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

        Undo.RecordObject(BattleSO, "Save new changes to battleSO");
        BattleSO.m_EnemyUnitsToSpawn = enemyUnitPlacements;
        BattleSO.m_StartingTileEffects = startingTileEffects;
        BattleSO.m_PlayerStartingTiles = setupTiles;
        EditorUtility.SetDirty(BattleSO);
    }
    #endregion
}
#endif