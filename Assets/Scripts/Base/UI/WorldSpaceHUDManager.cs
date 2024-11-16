using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.UI
{
    public class WorldSpaceHUDManager : Singleton<WorldSpaceHUDManager>
    {
        [SerializeField]
        private RectTransform root;

        private Dictionary<Transform, Func<Vector3>> huds = new();

        public bool AddHUD(Transform hud, Func<Vector3> worldPositionProducer)
        {
            Func<Vector3> hudPositionProducer;
            if (huds.TryAdd(hud, hudPositionProducer = () => WorldToHUDSpace(worldPositionProducer())))
            {
                hud.SetParent(root.transform, false);
                hud.localPosition = hudPositionProducer();
                return true;
            }
            return false;
        }

        public bool RemoveHUD(Transform hud)
        {
            return huds.Remove(hud);
        }

        private void Update()
        {
            foreach (var kvp in huds.ToList())
            {
                var transform = kvp.Key;
                var posProducer = kvp.Value;
                if (transform == null || !transform || posProducer == null) continue;
                transform.localPosition = kvp.Value();
            }
        }

        public Vector2 WorldToHUDSpace(Vector3 position)
        {
            var viewPosition = CameraManager.Instance.MainCamera.WorldToViewportPoint(position);
            var rootSize = root.rect.size;
            return new((viewPosition.x - 0.5f) * rootSize.x, (viewPosition.y - 0.5f) * rootSize.y);
        }
    }
}
