
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace FXGear.LCT.Installer
{
    using FXGear.LCT.Manager;

    public class GlobalInstaller : MonoInstaller<GlobalInstaller>
    {

        private const string Header = " [ GlobalInstaller ] ";

        public override void InstallBindings()
        {
            InstallSunManager();
        }


        #region [ Install ]
        private void InstallSunManager()
            => Container.Bind<SunManager>()
            .FromNewComponentOnNewGameObject()
            .AsCached()
            .NonLazy();
        #endregion
    }
}
