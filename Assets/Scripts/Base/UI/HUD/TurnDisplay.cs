using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.UI
{
    public class TurnDisplay : Singleton<TurnDisplay>
    {
        public float radius = 60;

        private Dictionary<Unit, TurnDisplayUnit> mapping = new();
        private TurnDisplayUnit prefab;

        public TurnDisplayUnit InstantiateTurnDisplayUnit(Unit unit)
        {
            if (prefab == null)
            {
                prefab = Addressables
                    .LoadAssetAsync<GameObject>("TurnDisplayUnit")
                    .WaitForCompletion()
                    .GetComponent<TurnDisplayUnit>();
            }

            TurnDisplayUnit display;
            if (!mapping.ContainsKey(unit))
            {
                display = Instantiate(prefab);
                display.transform.SetParent(transform, false);
                display.Initialize(unit);
                mapping.Add(unit, display);
            }
            else
            {
                display = mapping[unit];
            }
            
            return display;
        }

        public void RemoveTurnDisplayUnit(Unit display)
        {
            mapping.Remove(display);
        }
    }
}
