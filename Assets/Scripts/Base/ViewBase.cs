using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FXGear.LCT.Base
{
    public abstract class ViewBase : MonoBehaviour
    {

        private const string Header = " [ ViewBase ] ";

        public abstract void Show();
        public abstract void ShowImmediate();
        public abstract void Hide();
        public abstract void HideImmediate();

    }

}
