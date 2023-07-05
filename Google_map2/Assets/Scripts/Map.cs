using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Android;
using System;

public class Map : MonoBehaviour
{
    public string apiKey;
    public int zoom = 14;
    public enum resolution { low = 1, high = 2 };
    public resolution mapResolution = resolution.low;
    public enum type { roadmap, satellite, gybrid, terrain };
    public type mapType = type.roadmap;
    private string url = "";
    private int mapWidth = 640;
    private int mapHeight = 640;
    private bool mapIsLoading = false; //not used. Can be used to know that the map is loading 
    private Rect rect;

    private string apiKeyLast;
    private float latLast = 0;
    private float lonLast = 0;
    private int zoomLast = 14;
    private resolution mapResolutionLast = resolution.low;
    private type mapTypeLast = type.roadmap;
    private bool updateMap = true;


    //위도 경도 변경
    public float latitude = 0;
    public float longitude = 0;
    float waitTime = 0;
    
    public float maxWaitTime = 10.0f;
    public float resendTime = 1.0f;
    public bool receiveGPS = false;



    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GPS_On());
        StartCoroutine(GetGoogleMap());
        rect = gameObject.GetComponent<RawImage>().rectTransform.rect;
        mapWidth = (int)Math.Round(rect.width);
        mapHeight = (int)Math.Round(rect.height);
    }

    // Update is called once per frame
    void Update()
    {
        if (updateMap && (apiKeyLast != apiKey || !Mathf.Approximately(latLast, latitude) || !Mathf.Approximately(lonLast, longitude) || zoomLast != zoom || mapResolutionLast != mapResolution || mapTypeLast != mapType))
        {
            rect = gameObject.GetComponent<RawImage>().rectTransform.rect;
            mapWidth = (int)Math.Round(rect.width);
            mapHeight = (int)Math.Round(rect.height);
            StartCoroutine(GetGoogleMap());
            updateMap = false;
        }
    }

    //GPS처리 함수
    public IEnumerator GPS_On()
    {
        //만일,GPS사용 허가를 받지 못했다면, 권한 허가 팝업을 띄움
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                yield return null;
            }
        }

        //만일 GPS 장치가 켜져 있지 않으면 위치 정보를 수신할 수 없다고 표시
        if (!Input.location.isEnabledByUser)
        {
           
            yield break;
        }

        //위치 데이터를 요청 -> 수신 대기
        Input.location.Start();

        //GPS 수신 상태가 초기 상태에서 일정 시간 동안 대기함
        while (Input.location.status == LocationServiceStatus.Initializing && waitTime < maxWaitTime)
        {
            yield return new WaitForSeconds(1.0f);
            waitTime++;
        }

        //수신 실패 시 수신이 실패됐다는 것을 출력
     
        //응답 대기 시간을 넘어가도록 수신이 없었다면 시간 초과됐음을 출력
     

        //수신된 GPS 데이터를 화면에 출력/

        LocationInfo li = Input.location.lastData;
        /*latitude = li.latitude;
       longitude = li.longitude;
       latitude_text.text = "위도 : " + latitude.ToString();
       longitude_text.text = "경도 : " + longitude.ToString();
       */
        //위치 정보 수신 시작 체크
        receiveGPS = true;

        //위치 데이터 수신 시작 이후 resendTime 경과마다 위치 정보를 갱신하고 출력
        while (receiveGPS)
        {
            li = Input.location.lastData;
            latitude = li.latitude;
            longitude = li.longitude;

            

            yield return new WaitForSeconds(resendTime);
        }
    }

    IEnumerator GetGoogleMap()
    {
        url = "https://maps.googleapis.com/maps/api/staticmap?center=" + latitude + "," + longitude + "&zoom=" + zoom + "&size=" + mapWidth + "x" + mapHeight + "&scale=" + mapResolution + "&maptype=" + mapType + "&key=" + apiKey;
        mapIsLoading = true;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("WWW ERROR: " + www.error);
        }
        else
        {
            mapIsLoading = false;
            gameObject.GetComponent<RawImage>().texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            apiKeyLast = apiKey;
            latLast = latitude;
            lonLast = longitude;
            zoomLast = zoom;
            mapResolutionLast = mapResolution;
            mapTypeLast = mapType;
            updateMap = true;
        }
    }

}


