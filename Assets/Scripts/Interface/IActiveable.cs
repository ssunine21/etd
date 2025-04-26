using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.Interface
{
    public interface IActiveable
    {
        public void Selected(bool flag);
        public Button Button { get; }
    }
}