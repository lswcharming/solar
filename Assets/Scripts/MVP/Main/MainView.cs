
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FXGear.LCT.MVP.Main
{
    using FXGear.LCT.Base;
    using UnityEngine.UI;
    using TMPro;
    using System;
    using Cinemachine;

    public class MainView : ViewBase
    {
        public enum InputFieldType
        {
            Month,
            Day,
            Hour,
            Minute,
        }

        public enum ButtonType
        {
            Run,
            ChangeStartingView,
            ChangeTopView,
            ChangeGroundView,
            Reset,
        }

        private const string Header = " [ MainView ] ";

        [Header("[ Editor setting ] - UI components")]
        [SerializeField] private CinemachineBlendListCamera blendListCamera;
        [SerializeField] private CinemachineVirtualCameraBase topViewVirtualCamera;
        [SerializeField] private CinemachineVirtualCameraBase startingViewVirtualCamera;
        [SerializeField] private CinemachineVirtualCameraBase groundViewVirtualCamera;

        [Space(20)]
        [SerializeField] private TMP_InputField monthInputField;
        [SerializeField] private TMP_InputField dayInputField;
        [SerializeField] private TMP_InputField hourInputField;
        [SerializeField] private TMP_InputField minuteInputField;

        [Space(20)]
        [SerializeField] private Button runButton;
        [SerializeField] private Button changeBottomViewButton;
        [SerializeField] private Button changeTopViewButton;
        [SerializeField] private Button changeGroundViewButton;
        [SerializeField] private Button resetButton;

        [Space(20)]
        [SerializeField] private TMP_Text currentTimeText;

        public Button this[ButtonType type]
        {
            get
            {
                Button result = default;
                switch (type)
                {
                    case ButtonType.Run:
                        result = runButton;
                        break;
                    case ButtonType.ChangeTopView:
                        result = changeTopViewButton;
                        break;
                    case ButtonType.ChangeStartingView:
                        result = changeBottomViewButton;
                        break;
                    case ButtonType.ChangeGroundView:
                        result = changeGroundViewButton;
                        break;
                    case ButtonType.Reset:
                        result = resetButton;
                        break;
                }
                
                return result;
            }
        }
        public TMP_InputField this[InputFieldType type]
        {
            get
            {
                TMP_InputField result = default;
                switch (type)
                {
                    case InputFieldType.Month:
                        result = monthInputField;
                        break;
                    case InputFieldType.Day:
                        result = dayInputField;
                        break;
                    case InputFieldType.Hour:
                        result = hourInputField;
                        break;
                    case InputFieldType.Minute:
                        result = minuteInputField;
                        break;
                }

                return result;
            }
        }

        public void RefreshCurrentTimeText(DateTime time)
            => currentTimeText.text = $"{time.Month}월 {time.Day}일 [ {time.ToString("HH")}시 {time.ToString("mm")}분 ]";
        

        public void SetInputTextWithoutNotify(InputFieldType type, string data)
            => this[type].SetTextWithoutNotify(data);

        public void SetInputTextWithoutNotify(DateTime time)
        {
            SetInputTextWithoutNotify(InputFieldType.Month, time.Month.ToString());
            SetInputTextWithoutNotify(InputFieldType.Day, time.Day.ToString());
            SetInputTextWithoutNotify(InputFieldType.Hour, time.Hour.ToString());
            SetInputTextWithoutNotify(InputFieldType.Minute, time.Minute.ToString());
        }

        public void ChangeTopView()
        {
            var curLiveChild = blendListCamera.LiveChild;
            blendListCamera.m_Instructions[0].m_VirtualCamera = curLiveChild.VirtualCameraGameObject.GetComponent<CinemachineVirtualCameraBase>();
            blendListCamera.m_Instructions[1].m_VirtualCamera = topViewVirtualCamera;

            blendListCamera.m_Instructions[1].m_Blend.m_Time = 1.5f;
            blendListCamera.m_Instructions[0].m_Hold = 1.0f;
        }

        public void ChangeStartingView()
        {
            var curLiveChild = blendListCamera.LiveChild;
            blendListCamera.m_Instructions[0].m_VirtualCamera = curLiveChild.VirtualCameraGameObject.GetComponent<CinemachineVirtualCameraBase>();
            blendListCamera.m_Instructions[1].m_VirtualCamera = startingViewVirtualCamera;

            blendListCamera.m_Instructions[1].m_Blend.m_Time = 1.5f;
            blendListCamera.m_Instructions[0].m_Hold = 1.0f;
        }

        public void ChangeGroundView()
        {
            var curLiveChild = blendListCamera.LiveChild;
            blendListCamera.m_Instructions[0].m_VirtualCamera = curLiveChild.VirtualCameraGameObject.GetComponent<CinemachineVirtualCameraBase>();
            blendListCamera.m_Instructions[1].m_VirtualCamera = groundViewVirtualCamera;

            blendListCamera.m_Instructions[1].m_Blend.m_Time = 1.5f;
            blendListCamera.m_Instructions[0].m_Hold = 1.0f;
        }

        public override void Hide()
        {
        }

        public override void HideImmediate()
        {
        }

        public override void Show()
        {
        }

        public override void ShowImmediate()
        {
            blendListCamera.m_Loop = false;
        }
    }

}
