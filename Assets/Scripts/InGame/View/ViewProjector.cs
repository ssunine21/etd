using System;
using ETD.Scripts.Common;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.View
{
    public class ViewProjector : View, IRotatable
    {
        public Vector2 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public SpriteRenderer[] UnitSpriteRenderer => modelSpriteRenderers;
        
        [SerializeField] private SpriteRenderer[] modelSpriteRenderers;
        
        public void Rotate(Vector3 rotation)
        {
            try
            {
                modelSpriteRenderers[(int)EquippedPositionType.Active].transform.rotation = Quaternion.Euler(rotation);
                modelSpriteRenderers[(int)EquippedPositionType.Link].transform.rotation = Quaternion.Euler(rotation);
            }
            catch (Exception e)
            {
                var msg = e.Message;
            }
        }

        public void SubRotate(Vector3 rotation)
        {
            try
            {
                modelSpriteRenderers[(int)EquippedPositionType.Passive].transform.rotation = Quaternion.Euler(rotation);
            }
            catch (Exception e)
            {
                var msg = e.Message;
            }
        }
    }
}