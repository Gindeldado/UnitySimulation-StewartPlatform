using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class HttpClientExample : MonoBehaviour
{
    // The URL for the request (replace with your server URL)
    private string url = "http://127.0.0.1:5142?roll=0&pitch=0&yaw=0";

    // Array to store leg lengths
    [HideInInspector]
    public float[] legLengthsArray = new float[6];
    public GameObject topPlatformObj;

    void Start()
    {
        // Start the coroutine to make the HTTP request
        GetRequest(url);
        // StartCoroutine(GetRequest(url));
    }

    // Coroutine to handle the GET request
    public IEnumerator GetRequest(string uri)
    // public void GetRequest(string uri)
    {
        // Create a UnityWebRequest for the GET request
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Wait for the request to complete
            yield return webRequest.SendWebRequest();
            // webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.LogError("Network Error: " + webRequest.error);
                    break;

                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Data Processing Error: " + webRequest.error);
                    break;

                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Protocol Error: " + webRequest.error);
                    break;

                case UnityWebRequest.Result.Success:
                    Debug.Log("Request Success! Response: " + webRequest.downloadHandler.text);
                    break;
            }
            // Check if there was an error in the request
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Successfully received a response
                string response = webRequest.downloadHandler.text;

                // Log the received JSON response
                Debug.Log("Received Data: " + response);

                // Parse the JSON data into the LegLengths object
                // LegLengths legLens = JsonUtility.FromJson<LegLengths>(response);
                LegLengthsList legLensPython = JsonUtility.FromJson<LegLengthsList>(response);
                // Store the values in the array
                // EDCBAF is de volgorde van legs in Unity op basis van de excel 
                // BAFEDC in unity
                // 12356    leg nr in unity
                // CBAFED de volgorde die van python komt 
                legLengthsArray[5] = legLensPython.legs[0] /100;       
                legLengthsArray[0] = legLensPython.legs[1] / 100;
                legLengthsArray[1] = legLensPython.legs[2] / 100;
                legLengthsArray[2] = legLensPython.legs[3]/ 100;
                legLengthsArray[3] = legLensPython.legs[4]/ 100;
                legLengthsArray[4] = legLensPython.legs[5]/ 100;

                
                // !!! check here values and call change color if needed of leg
                for (int i = 0; i < legLengthsArray.Length; i++)
                {
                    Debug.Log((Mathf.Abs(legLengthsArray[i])*100)-988);
                    if((Mathf.Abs(legLengthsArray[i])*100)-988 > 200|| (Mathf.Abs(legLengthsArray[i])*100)-988< -200)   {
                        topPlatformObj.GetComponent<TopPlatform>().MakeLegRed(i);
                        continue;
                    }else{
                        topPlatformObj.GetComponent<TopPlatform>().MakeLegNormal(i);
                    }
                }
                topPlatformObj.GetComponent<TopPlatform>().adjustLegs();
                // topPlatformObj.
                // Log the extracted values from the array
                // Debug.Log("Leg lengths array: ");
                // for (int i = 0; i < legLengthsArray.Length; i++)
                // {
                //     // float n = float.Parse(legLengthsArray[i]);
                //     // float len = float.Parse(legLengthsArray[i], CultureInfo.InvariantCulture.NumberFormat);
                //     Debug.Log("Legs from python "+ i+": "+legLengthsArray[i]/100);
                // }
            }
        }
    }
}

[System.Serializable]
public class LegLengths
{
    // C
    public float l0; 
    public float l1;
    public float l2;
    public float l3;
    public float l4;
    public float l5;
}

[System.Serializable]
public class LegLengthsList
{
    // C
    public float[] legs;
}
