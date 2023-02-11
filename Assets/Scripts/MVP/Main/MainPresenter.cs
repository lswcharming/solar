using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FXGear.LCT.MVP.Main
{
    using UniRx;

    using FXGear.LCT.Base;
    using System;
    using FXGear.LCT.Global;
    using FXGear.LCT.Manager;
    using System.Numerics;
    using System.Security.Cryptography;

    public class MainPresenter : PresenterBase
    {
        private const string Header = " [ MainPresenter ] ";

        [Header("[ Auto setting ] - View")]
        [SerializeField] private MainView view;

        [Header("[ Editor setting ] - Objects")]
        [SerializeField] private Light sun;

        public override void Initialize()
        {
            if (TryGetComponent<MainView>(out view))
            {
                ResetSun();

                InitializeView();

                view.RefreshCurrentTimeText(DateTime.Now);

                RegisterPlaySimulationButtonClicked();
                RegisterViewChangeButtonClicked();
                RegisterResetButtonClicked();

                view.ShowImmediate();
            }
        }

        #region [ Unity event ]
        private void Update()
        {
            view.RefreshCurrentTimeText(DateTime.Now);
        }
        #endregion

        #region [ Private methods ]
        private void InitializeView()
        {
            view.SetInputTextWithoutNotify(DateTime.Now);
        }

        private void ResetSun()
        {
            sunManager.Initialize(sun, simulationTargetLocationData: new(37.478f, 127.148f), actionContainer: GetSunManagerActionContainer());

            SunManager.TimeData curTimeData = new()
            {
                month = DateTime.Now.Month,
                day = DateTime.Now.Day,
                hour = DateTime.Now.Hour,
                minute = DateTime.Now.Minute,
            };

            if (sunManager.TryCalculate(curTimeData, out var output))
            {
                sun.transform.position = output.position;
                sun.transform.rotation = output.rotation;
            }

            view.SetInputTextWithoutNotify(DateTime.Now);
        }
        #endregion

        #region [ Register events ]
        private SunManager.ActionContainer GetSunManagerActionContainer()
            => new() 
            {
                onTick = (isSuccess, resultSunData, curSimulationTime) => 
                {
                    if (isSuccess)
                    {
                        view.SetInputTextWithoutNotify(curSimulationTime);


                        sun.transform.rotation = UnityEngine.Quaternion.Lerp(sun.transform.rotation, resultSunData.rotation, Time.deltaTime);//resultSunData.rotation;
                        sun.transform.position = UnityEngine.Vector3.Lerp(sun.transform.position, resultSunData.position, Time.deltaTime);//resultSunData.position;
                    }
                },
            };

        private void RegisterResetButtonClicked()
            => view[MainView.ButtonType.Reset].OnClickAsObservable()
            .ThrottleFirst(GlobalConstants.ToThrottleSeconds)
            .Subscribe(_ => 
            {
                ResetSun();
            })
            .AddTo(Disposables);

        private void RegisterViewChangeButtonClicked()
        {
            var bottomViewButtonObserver = view[MainView.ButtonType.ChangeStartingView].OnClickAsObservable().ThrottleFirst(GlobalConstants.ToThrottleSeconds).Select(_ => MainView.ButtonType.ChangeStartingView);
            var topViewButtonObserver = view[MainView.ButtonType.ChangeTopView].OnClickAsObservable().ThrottleFirst(GlobalConstants.ToThrottleSeconds).Select(_ => MainView.ButtonType.ChangeTopView);
            var groundViewButtonObserver = view[MainView.ButtonType.ChangeGroundView].OnClickAsObservable().ThrottleFirst(GlobalConstants.ToThrottleSeconds).Select(_ => MainView.ButtonType.ChangeGroundView);

            Observable.Merge(bottomViewButtonObserver, topViewButtonObserver, groundViewButtonObserver)
                .Subscribe(clickButtonType =>
                {
                    switch (clickButtonType)
                    {
                        case MainView.ButtonType.ChangeTopView:
                            {
                                view.ChangeTopView();
                            }
                            break;
                        case MainView.ButtonType.ChangeStartingView:
                            {
                                view.ChangeStartingView();
                            }
                            break;
                        case MainView.ButtonType.ChangeGroundView:
                            {
                                view.ChangeGroundView();
                            }
                            break;
                    }
                })
                .AddTo(Disposables);
        }

        private void RegisterPlaySimulationButtonClicked()
            => view[MainView.ButtonType.Run].OnClickAsObservable()
            .ThrottleFirst(GlobalConstants.ToThrottleSeconds)
            .Subscribe(_ => 
            {
                var month = view[MainView.InputFieldType.Month].text;
                var day = view[MainView.InputFieldType.Day].text;
                var hour = view[MainView.InputFieldType.Hour].text;
                var minute = view[MainView.InputFieldType.Minute].text;

                sunManager.Initialize(sun, simulationTargetLocationData: new(37.478f, 127.148f), actionContainer: GetSunManagerActionContainer());

                sunManager.StartSimulation(new()
                {
                    month = int.Parse(month),
                    day = int.Parse(day),
                    hour = int.Parse(hour),
                    minute = int.Parse(minute),
                });
                //if (sunManager.TryCalculate(new()
                //{
                //    month = int.Parse(month),
                //    day = int.Parse(day),
                //    hour = int.Parse(hour),
                //    minute = int.Parse(minute),
                //}, out var output))
                //{
                //    sun.transform.position = output.position;
                //    sun.transform.rotation = output.rotation;
                //}
            })
            .AddTo(Disposables);
        
        //private void RegisterInputFieldsValueChanged()
        //{
        //    var monthObserver = view[MainView.InputFieldType.Month].OnValueChangedAsObservable().Select(_ => MainView.InputFieldType.Month);
        //    var dayObserver = view[MainView.InputFieldType.Day].OnValueChangedAsObservable().Select(_ => MainView.InputFieldType.Day);
        //    var hourObserver = view[MainView.InputFieldType.Hour].OnValueChangedAsObservable().Select(_ => MainView.InputFieldType.Hour);
        //    var minuteObserver = view[MainView.InputFieldType.Minute].OnValueChangedAsObservable().Select(_ => MainView.InputFieldType.Minute);

        //    Observable.Merge(monthObserver, dayObserver, hourObserver, minuteObserver)
        //        .Subscribe(inputType => 
        //        {
        //        })
        //        .AddTo(Disposables);
        //}
        #endregion
    }
}
