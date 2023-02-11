using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarManager : MonoBehaviour
{
    float d2r = Mathf.PI / 180.0f;
    float r2d = 180.0f / Mathf.PI;

    // 서울 위도, 경도
    float local_latitude = 37.478f;
    float local_longitude = 127.148f;
    float standard_longitude = 135.0f;

    // 날짜, 시간
    float year = 2020;
    float month = 6;
    float day = 22;
    float local_hour = 12; // 방위각(정남 기준) 11시:61도, 12시:29도 <- 12:30분 -> 13시:(-)24도, 14시:(-)59도
    float local_min = 0;

    // Start is called before the first frame update
    void Start()
    {
        float day_of_year = 31 + 29 + 31 + 30 + 31 + 22;
        float B = (day_of_year - 1) * 360.0f / 365.0f;
        float EOT = 229.2f * (0.000075f
                             + 0.001868f * Mathf.Cos(d2r * B)
                             - 0.032077f * Mathf.Sin(d2r * B)
                             - 0.014615f * Mathf.Cos(d2r * 2 * B)
                             - 0.040890f * Mathf.Sin(d2r * 2 * B)
                             );

        Debug.Log($"[c] day_of_year: {day_of_year}, EOT: {EOT}");

        float local_hour_decimal = local_hour + local_min / 60.0f;
        float delta_longitude = local_longitude - standard_longitude;
        float solar_time_decimal = (local_hour_decimal * 60.0f + 4.0f * delta_longitude + EOT) / 60.0f;
        int solar_time_hour = (int)solar_time_decimal;
        int solar_time_min = (int)(solar_time_decimal * 60.0f) % 60;
        float hour_angle = (local_hour_decimal * 60.0f + 4 * delta_longitude + EOT) / 60.0f * 15.0f - 180.0f;

        Debug.Log($"[c] Solar Time {solar_time_hour}.{solar_time_min}, Hour angle: {hour_angle}");

        float solar_declination = 23.45f * Mathf.Sin(d2r * 360.0f/365.0f * (284.0f + day_of_year));

        Debug.Log($"[c] Solar Declination {solar_declination}");

        float term_1 = Mathf.Cos(d2r * local_latitude) * Mathf.Cos(d2r * solar_declination) * Mathf.Cos(d2r * hour_angle)
            + Mathf.Sin(d2r * local_latitude) * Mathf.Sin(d2r * solar_declination);
        float solar_altitude = r2d * Mathf.Asin(term_1);

        Debug.Log($"[c] Solar Altitude {solar_altitude}");

        float term_2 = (Mathf.Sin(d2r * solar_altitude) * Mathf.Sin(d2r * local_latitude) - Mathf.Sin(d2r * solar_declination))
            / (Mathf.Cos(d2r * solar_altitude) * Mathf.Cos(d2r * local_latitude));
        float solar_azimuth = r2d * Mathf.Acos(term_2);

        // 오전, 오후 방위각 변화
        // https://www.pveducation.org/ko/%ED%83%9C%EC%96%91%EA%B4%91/%EB%B0%A9%EC%9C%84%EA%B0%81-azimuth-angle
        if (hour_angle > 0)
        {
            solar_azimuth = -1 * solar_azimuth;
        }

        Debug.Log($"[c] Solar Azimuth {solar_azimuth}");

        float y_angle = 90.0f - solar_azimuth;
        float z_angle = solar_altitude;
        float x_angle = 0.0f;

        //float solar_zenith_angle = r2d * Mathf.Acos(Mathf.Sin(d2r * local_latitude) * Mathf.Sin(d2r * solar_declination) + Mathf.Cos(d2r * local_latitude) * Mathf.Cos(d2r * solar_declination) * Mathf.Cos(d2r * hour_angle));
        //float term_3 = (Mathf.Sin(d2r * solar_declination) * Mathf.Cos(d2r * local_latitude) - Mathf.Cos(d2r * hour_angle) * Mathf.Cos(d2r * solar_declination) * Mathf.Sin(d2r * local_latitude)) / Mathf.Sin(d2r * (90 - solar_altitude));
        //float solar_azimuth_2 = r2d * Mathf.Acos(term_3);

        //Debug.Log($"[c] solar_zenith_angle {solar_zenith_angle}, solar_azimuth_2 {solar_azimuth_2}");

        Quaternion solar_quaternion_y = Quaternion.Euler(0, y_angle, 0);
        Quaternion solar_quaternion_z = Quaternion.Euler(0, 0, z_angle);
        Quaternion solar_quaternion = solar_quaternion_y * solar_quaternion_z;

        // 일정거리(10)의 좌측벡터(10, 0, 0)으로 태양(Directional Light)의 rotation 계산
        Vector3 base_left_vec = new Vector3(10, 0, 0);
        Vector3 solar_directional_vector = solar_quaternion * base_left_vec;
        Debug.Log($"[c] Solar Position {solar_directional_vector}");

        GameObject directional_light = GameObject.Find("Directional Light");

        directional_light.transform.position = solar_directional_vector;
        directional_light.transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 0) - directional_light.transform.position);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
