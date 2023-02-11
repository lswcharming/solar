using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FXGear.LCT.Base
{
    using FXGear.LCT.Manager;
    using UniRx;
    using Zenject;

    public abstract class PresenterBase : MonoBehaviour
    {
        private const string Header = " [ PresenterBase ] ";

        protected CompositeDisposable Disposables { get; private set; } = new CompositeDisposable();

        [Header("[ Auto setting ] - Cores")]
        [SerializeField] protected SunManager sunManager;

        #region [ Inject ]
        [Inject]
        private void Inject(SunManager sunManager)
        {
            this.sunManager = sunManager;


            Initialize();
        }
        #endregion

        #region [ Unity events ]

        private void OnDestroy()
        {
            Dispose();
        }

        #endregion
        public abstract void Initialize();
        public virtual void Dispose()
        {
            if (Disposables != null)
                Disposables.Clear();
        }
    }

}
