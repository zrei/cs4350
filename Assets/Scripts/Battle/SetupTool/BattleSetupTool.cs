using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BattleManager))]
public class BattleSetupTool : MonoBehaviour
{
    [SerializeField] private GridSetupHelper m_EnemyGrid;
    [SerializeField] private GridSetupHelper m_PlayerGrid;

    [HideInInspector]
    [SerializeField] private BattleSetupToolEditor.BattleToolTileDataEnemy m_CurrTileEnemy;

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

        m_PlayerGrid.ClearTileEffects(coordPair);
        m_PlayerGrid.ClearUnit(coordPair);
        m_PlayerGrid.ResetTileColor(coordPair);
    }

    public void SpawnEnemy(CoordPair coordPair, UnitModelData unitModelData, WeaponInstanceSO weaponSO, ClassSO classSO)
    {
        m_EnemyGrid.SpawnUnit(coordPair, unitModelData, weaponSO, classSO);
    }

    public void SpawnTileEffects(GridType gridType, CoordPair coordPair, InflictedTileEffect inflictedTileEffect)
    {
        if (gridType == GridType.ENEMY)
            m_EnemyGrid.SetupTileEffects(coordPair, inflictedTileEffect.TileEffectObjs);
        else
            m_PlayerGrid.SetupTileEffects(coordPair, inflictedTileEffect.TileEffectObjs);
    }

    public void SetSetupTile(CoordPair coordPair)
    {
        m_PlayerGrid.SetTileAsSetupColor(coordPair);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BattleSetupTool))]
public class BattleSetupToolEditor : Editor
{
    [System.Serializable]
    public class BattleEnemyUnit
    {
        public EnemyCharacterSO m_EnemyCharacter;
        public Stats m_StatAugmnets;
        public EnemyTag m_EnemyTag = EnemyTag.Default;
    }

    [System.Serializable]
    public class BattleToolTileDataEnemy
    {
        public InflictedTileEffect m_InflictedTileEffect;
        public BattleEnemyUnit m_BattleEnemyUnit;

        public BattleToolTileDataEnemy()
        {
            m_InflictedTileEffect = new InflictedTileEffect {m_TileEffect = null, m_InitialTime = 0};
            m_BattleEnemyUnit = new BattleEnemyUnit {m_EnemyCharacter = null, m_StatAugmnets = new Stats(), m_EnemyTag = EnemyTag.Default};
        }

        public void Clear()
        {
            m_InflictedTileEffect = new InflictedTileEffect {m_TileEffect = null, m_InitialTime = 0};
            m_BattleEnemyUnit = new BattleEnemyUnit {m_EnemyCharacter = null, m_StatAugmnets = new Stats(), m_EnemyTag = EnemyTag.Default};
        }
    }

    [System.Serializable]
    private class BattleToolTileDataPlayer
    {
        public InflictedTileEffect m_InflictedTileEffect;
        public bool m_IsSetupTile;

        public BattleToolTileDataPlayer()
        {
            m_InflictedTileEffect = new InflictedTileEffect {m_TileEffect = null, m_InitialTime = 0};
            m_IsSetupTile = false;
        }

        public void Clear()
        {
            m_InflictedTileEffect = new InflictedTileEffect {m_TileEffect = null, m_InitialTime = 0};
            m_IsSetupTile = false;
        }
    }

    private BattleSetupTool m_Target;
    private BattleSO m_BattleSo;
    private BattleToolTileDataEnemy[,] m_BattleDataEnemy = new BattleToolTileDataEnemy[MapData.NUM_ROWS, MapData.NUM_COLS];
    private BattleToolTileDataPlayer[,] m_BattleDataPlayer = new BattleToolTileDataPlayer[MapData.NUM_ROWS, MapData.NUM_COLS];

    private CoordPair? m_SelectedCoordinates;
    private GridType m_SelectedGrid;

    #region Serialising current enemy character
    private EnemyCharacterSO m_CurrEnemyCharacter;
    private Stats m_CurrStatAugmnets;
    private EnemyTag m_CurrEnemyTag;

    SerializedProperty m_EnemyTileDataProp;
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
        ClearBattle();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

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

            if (GUILayout.Button("Save to SO"))
            {

            }
        }

        RenderMap();

        RenderTileData();
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

        m_SelectedCoordinates = null;
    }

    private void RenderMap()
    {
        GUILayout.Space(10f);
        GUILayout.Label("Enemy Grid");
        RenderEnemyGrid();

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
            for (int c = MapData.NUM_COLS - 1; c >= 0; --c)
            {
                CoordPair coordinates = new CoordPair(r, c);
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = m_SelectedGrid == GridType.ENEMY && m_SelectedCoordinates.Equals(coordinates) ? SELECTED_COLOR : ENEMY_COLOR;
                if (GUILayout.Button(string.Empty, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    if (m_SelectedCoordinates.Equals(coordinates) && m_SelectedGrid == GridType.ENEMY)
                        m_SelectedCoordinates = null;
                    else
                    {
                        m_SelectedCoordinates = coordinates;
                        m_SelectedGrid = GridType.ENEMY;
                    }
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
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                CoordPair coordinates = new CoordPair(r, c);
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = m_SelectedGrid == GridType.PLAYER && m_SelectedCoordinates.Equals(coordinates) ? SELECTED_COLOR : PLAYER_COLOR;
                if (GUILayout.Button(string.Empty, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    if (m_SelectedCoordinates.Equals(coordinates) && m_SelectedGrid == GridType.PLAYER)
                        m_SelectedCoordinates = null;
                    else
                    {
                        m_SelectedCoordinates = coordinates;
                        m_SelectedGrid = GridType.PLAYER;
                    }
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

        }
    }

    private void RenderEnemyFields()
    {
        m_CurrEnemyCharacter = EditorGUILayout.ObjectField("", m_CurrEnemyCharacter, typeof(EnemyCharacterSO), false) as EnemyCharacterSO;

    }

    private void ClearBattle()
    {
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                m_BattleDataEnemy[r, c] = new BattleToolTileDataEnemy();
                m_BattleDataPlayer[r, c] = new BattleToolTileDataPlayer();
                m_Target.ClearBattle(new CoordPair(r, c));
            }
        }
    }
}
#endif