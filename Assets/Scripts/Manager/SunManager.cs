using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FXGear.LCT.Manager
{


    public class SunManager : MonoBehaviour
    {
        [Serializable]
        public class LocationData
        {
            public float latitude;
            public float longitude;

            public LocationData(float latitude, float longitude)
            {
                this.latitude = latitude;
                this.longitude = longitude;
            }

            public static implicit operator bool(LocationData a) => a != null;
        }

        [Serializable]
        public class ActionContainer
        {
            public Action onStartSimulation;
            public Action onEndSimulation;
            public Action<bool, SunData, DateTime> onTick;

            public static implicit operator bool(ActionContainer a) => a != null;
        }

        [Serializable]
        public class SunData
        {
            public double azimuth;
            public double altitude;
            public double sunrise;
            public double sunset;

            public Vector3 position;
            public Quaternion rotation;

            public float b;
            public float g;
            public float eot;
            public float tc;
            public float lst;
            public float hra;


            public static implicit operator bool(SunData a) => a != null;
        }

        [Serializable]
        public class TimeData
        {
            public int month;
            public int day;
            public int hour;
            public int minute;
            public double? inSeconds = null;

            public double GetInSeconds()
            {
                if (inSeconds == null)
                    inSeconds = hour * 60 + minute;

                return inSeconds.Value;
            }

            public double ToClock() => GetInSeconds() * 24f / 1440f;

            public static implicit operator bool(TimeData a) => a != null;
        }

        private const string Header = " [ SunManager ] ";

        [Header("[ Debugging Properties ]")]
        [SerializeField] private Light sun;
        [SerializeField] private LocationData locationData;
        [SerializeField] private ActionContainer actionContainer;

        [Space(20)]
        [SerializeField] private int dayOfYear;
        
        [Space(20)]
        // Value needed to calculate data of sun
        //[SerializeField] private int[] dayForMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        [SerializeField] private int[] dayForMonth = { 31, 29, 31, 30, 31, 22 };
        [SerializeField] private int lstm = 135;

        private Coroutine simulationRoutine;

        #region [ Public methods ]
        public void Initialize(Light sun, LocationData simulationTargetLocationData, ActionContainer actionContainer)
        { 
            this.locationData = simulationTargetLocationData;
            this.actionContainer = actionContainer;
            this.sun = sun;
        }

        public void StartSimulation(TimeData data)
        {
            IEnumerator SimulationIterator(DateTime curDate, DateTime targetDate)
            {
                var curSimulationTime = curDate;
                while (curSimulationTime < targetDate)
                {
                    bool isSuccess = TryCalculate(new()
                    {
                        month = curSimulationTime.Month,
                        day = curSimulationTime.Day,
                        hour = curSimulationTime.Hour,
                        minute = curSimulationTime.Minute,
                    }, out var result);

                    if (isSuccess)
                        curSimulationTime = curSimulationTime.AddMinutes(1);
                    else
                        yield break;

                    actionContainer?.onTick?.Invoke(isSuccess, result, curSimulationTime);

                    yield return null;
                }

                actionContainer?.onEndSimulation?.Invoke();
            }

            var curDate = DateTime.Now;
            var targetDate = new DateTime(curDate.Year, data.month, data.day, data.hour, data.minute, 0);
            if (curDate < targetDate)
            {
                StartCoroutine(SimulationIterator(curDate, targetDate));   
            }
        }

        //public bool TryCalculate(TimeData data, out SunData output)
        //{
        //    if (locationData == null ||
        //        sun == null)
        //    {
        //        output = null;
        //        return false;
        //    }

        //    var result = true;
        //    output = new();

        //    Debug.Log($"Start Calculation : {data.month} / {data.day} [ {data.hour} : {data.minute} ]");

        //    var clock = data.ToClock();//data.GetInSeconds() * 24f / 1440f;
        //    dayOfYear = 0;

        //    for (int i = 1; i < data.month; i++)
        //        dayOfYear += dayForMonth[i - 1];

        //    if (data.day > dayForMonth[data.month - 1] || data.day < 1)
        //    {
        //        result = false;
        //        output = null;

        //        return result;
        //    }
        //    else
        //        dayOfYear += data.day;

        //    output.b = (dayOfYear - 1) * 360.0f / 365.0f;
        //    output.g = Mathf.Rad2Deg * Mathf.Asin(Mathf.Sin((float)(23.45 * Mathf.Deg2Rad)) * Mathf.Sin((float)(output.b * Mathf.Deg2Rad)));
        //    output.eot = 9.87 * Mathf.Sin((float)(2 * output.b * Mathf.Deg2Rad)) - 7.53 * Mathf.Cos((float)(output.b * Mathf.Deg2Rad)) - 1.5 * Mathf.Sin((float)(output.b * Mathf.Deg2Rad));
        //    output.tc = 4 * (locationData.longitude - lstm) + output.eot;
        //    output.lst = clock + output.tc / 60;
        //    output.hra = 15 * (output.lst - 12);

        //    output.altitude = Mathf.Asin(Mathf.Sin((float)(output.g * Mathf.Deg2Rad)) 
        //        * Mathf.Sin((float)(locationData.latitude * Mathf.Deg2Rad)) 
        //        + Mathf.Cos((float)(output.g * Mathf.Deg2Rad)) 
        //        * Mathf.Cos((float)(locationData.latitude * Mathf.Deg2Rad)) 
        //        * Mathf.Cos((float)(output.hra * Mathf.Deg2Rad)));
        //    output.altitude = Mathf.Rad2Deg * output.altitude;

        //    output.azimuth = Mathf.Sin((float)(output.g * Mathf.Deg2Rad)) 
        //        * Mathf.Cos((float)(locationData.latitude * Mathf.Deg2Rad)) - Mathf.Cos((float)(output.g * Mathf.Deg2Rad)) 
        //        * Mathf.Sin((float)(locationData.latitude * Mathf.Deg2Rad)) * Mathf.Cos((float)(output.hra * Mathf.Deg2Rad));

        //    output.azimuth = Mathf.Acos((float)(output.azimuth / Mathf.Cos((float)(output.altitude * Mathf.Deg2Rad))));
        //    output.azimuth = Mathf.Rad2Deg * output.azimuth;

        //    output.sunrise = 12f - Mathf.Rad2Deg * Mathf.Acos(-Mathf.Tan((float)(locationData.latitude * Mathf.Deg2Rad)) * Mathf.Tan((float)(output.g * Mathf.Deg2Rad))) / 15f - output.tc / 60f;
        //    output.sunset = 12f + Mathf.Rad2Deg * Mathf.Acos(-Mathf.Tan((float)(locationData.latitude * Mathf.Deg2Rad)) * Mathf.Tan((float)(output.g * Mathf.Deg2Rad))) / 15f - output.tc / 60f;

        //    return result;
        //}

        public bool TryCalculate(TimeData data, out SunData output)
        {
            if (locationData == null ||
                sun == null)
            {
                output = null;
                return false;
            }

            var result = true;
            output = new();

            Debug.Log($"Start Calculation : {data.month} / {data.day} [ {data.hour} : {data.minute} ]");

            for (int i = 0; i < dayForMonth.Length; i++)
                dayOfYear += dayForMonth[i];

            output.b = (dayOfYear - 1) * 360f / 365f;
            output.eot = 229.2f * (0.000075f
                             + 0.001868f * Mathf.Cos(Mathf.Deg2Rad * output.b)
                             - 0.032077f * Mathf.Sin(Mathf.Deg2Rad * output.b)
                             - 0.014615f * Mathf.Cos(Mathf.Deg2Rad * 2 * output.b)
                             - 0.040890f * Mathf.Sin(Mathf.Deg2Rad * 2 * output.b)
                             );

            float localHourDecimal = data.hour + data.minute / 60f;
            float deltaLongitude = locationData.longitude - lstm;
            float solarTimeDecimal = (localHourDecimal * 60f + 4f * deltaLongitude + output.eot) / 60f;

            int solarTimeHour = (int)solarTimeDecimal;
            int solarTimeMin = (int)(solarTimeDecimal * 60f) % 60;

            float hourAngle = (localHourDecimal * 60.0f + 4 * deltaLongitude + output.eot) / 60.0f * 15.0f - 180.0f;

            float solarDeclination = 23.45f * Mathf.Sign(Mathf.Deg2Rad * 360f / 365f * (284.0f + dayOfYear));

            float term1 = Mathf.Cos(Mathf.Deg2Rad * locationData.latitude) 
                * Mathf.Cos(Mathf.Deg2Rad * solarDeclination) 
                * Mathf.Cos(Mathf.Deg2Rad * hourAngle) 
                + Mathf.Sin(Mathf.Deg2Rad * locationData.latitude) 
                * Mathf.Sin(Mathf.Deg2Rad * solarDeclination);
            float solarAltitude = Mathf.Rad2Deg * Mathf.Asin(term1);

            float term2 = (Mathf.Sin(Mathf.Deg2Rad * solarAltitude) * Mathf.Sin(Mathf.Deg2Rad * locationData.latitude) - Mathf.Sin(Mathf.Deg2Rad * solarDeclination))
                / (Mathf.Cos(Mathf.Deg2Rad * solarAltitude) * Mathf.Cos(Mathf.Deg2Rad * locationData.latitude));
            float solarAzimuth = Mathf.Rad2Deg * Mathf.Acos(term2);

            if (hourAngle > 0)
                solarAzimuth = -1 * solarAzimuth;

            var solarAngle = new Vector3() 
            {
                y = 90.0f - solarAzimuth,
                z = solarAltitude,
                x = 0f,
            };

            var solarQuaternionY = Quaternion.Euler(0, solarAngle.y, 0);
            var solarQuaternionZ = Quaternion.Euler(0, 0, solarAngle.z);
            var solarQuaternion = solarQuaternionY * solarQuaternionZ;

            var baseLeftVector = new Vector3(10, 0, 0);
            var solarDirectionalVector = solarQuaternion * baseLeftVector;

            output.position = solarDirectionalVector;
            output.rotation = Quaternion.LookRotation(Vector3.zero - sun.transform.position);

            return result;
        }

        #endregion
    }

}
