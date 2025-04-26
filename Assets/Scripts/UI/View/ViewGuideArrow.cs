using System;
using UnityEngine;

namespace ETD.Scripts.UI.View
{
    public class ViewGuideArrow : MonoBehaviour
    {
        private void OnDisable()
        {
            gameObject.SetActive(false);
        }
    }
}