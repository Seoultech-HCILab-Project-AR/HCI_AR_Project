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


    //���� �浵 ����
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

    //GPSó�� �Լ�
    public IEnumerator GPS_On()
    {
        //����,GPS��� �㰡�� ���� ���ߴٸ�, ���� �㰡 �˾��� ���
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                yield return null;
            }
        }

        //���� GPS ��ġ�� ���� ���� ������ ��ġ ������ ������ �� ���ٰ� ǥ��
        if (!Input.location.isEnabledByUser)
        {
           
            yield break;
        }

        //��ġ �����͸� ��û -> ���� ���
        Input.location.Start();

        //GPS ���� ���°� �ʱ� ���¿��� ���� �ð� ���� �����
        while (Input.location.status == LocationServiceStatus.Initializing && waitTime < maxWaitTime)
        {
            yield return new WaitForSeconds(1.0f);
            waitTime++;
        }

        //���� ���� �� ������ ���еƴٴ� ���� ���
     
        //���� ��� �ð��� �Ѿ���� ������ �����ٸ� �ð� �ʰ������� ���
     

        //���ŵ� GPS �����͸� ȭ�鿡 ���/

        LocationInfo li = Input.location.lastData;
        /*latitude = li.latitude;
       longitude = li.longitude;
       latitude_text.text = "���� : " + latitude.ToString();
       longitude_text.text = "�浵 : " + longitude.ToString();
       */
        //��ġ ���� ���� ���� üũ
        receiveGPS = true;

        //��ġ ������ ���� ���� ���� resendTime ������� ��ġ ������ �����ϰ� ���
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


