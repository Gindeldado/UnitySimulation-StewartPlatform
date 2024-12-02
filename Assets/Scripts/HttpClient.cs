using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
public class HttpClient : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Start the coroutine to make the HTTP GET request
        StartCoroutine(GetRequest("http://127.0.0.1:5000"));
    }

    // Coroutine to perform the GET request
    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Send the request and wait until a response is received
            yield return webRequest.SendWebRequest();

            // Check if the request encountered an error
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                // Successfully received a response
                Debug.Log("Response Code: " + webRequest.responseCode);
                Debug.Log("Received Data: " + webRequest.downloadHandler.text);
            }
        }
    }
}

