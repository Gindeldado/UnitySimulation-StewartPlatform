using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Leg4Upper, Center point: (-4.96, 8.21, 7.01)
//Leg2Upper, Center point: (-13.91, 8.21, 16.11)
//Leg6Upper, Center point: (-1.55, 8.21, 19.32)
//Leg5Upper, Center point: (0.30, 8.21, 16.11)
//Leg3Upper, Center point: (-8.65, 8.21, 7.01)
//Leg1Upper, Center point: (-12.06, 8.21, 19.32)

//Upper Platform center is -6.8 ,9.17, 14.15
//Leg 1 Lower-cross is -8.65, 1.15, 21.28 ||
//Leg 2 Lower-cross is -13.91, 1.15, 12.18
//Leg 4 Lower-cross is -1.55, 1.15, 8.98
//Leg 3 Lower-cross is -12.06, 1.15, 8.98
//Leg 5 Lower-cross is 0.30, 1.15 ,12.18
//Leg 6 Lower-cross is -4.96, 1.15, 21.28


public class TopPlatform : MonoBehaviour
{
    public List<GameObject> upperCrosses = new List<GameObject>();
    public List<GameObject> legs = new List<GameObject>();
    public List<GameObject> upperLegs = new List<GameObject>();

    //Uppercross 
    private List<Vector3> originalCrossPos = new List<Vector3>();
    [Range(-90, 90)]
    public float desiredPitch;
    [Range(-90, 90)]
    public float desiredRoll;
    [Range(-90, 90)]
    public float desiredYaw;

    public float[] prevRot;
    public float startLenOfLegs = 9.88f;
    private float currentPitch,currentRoll,currentYaw = 0;
    private float targetPitch, targetRoll,targetYaw;

    public Material limitIdicatorMaterial;
    public Material baseMaterial;

    public GameObject CallServerObj;
    
    public TCPConnection tcpScript;
    private Dictionary<string, Vector3> rotPositions = new Dictionary<string, Vector3>
    {
        { "UpperPlatform", new Vector3(-6.8f, 11.17f, 14.15f) },
        { "Leg1", new Vector3(-8.65f, 1.15f, 21.28f) },
        { "Leg2", new Vector3(-13.91f, 1.15f, 12.18f) },
        { "Leg4", new Vector3(-1.55f, 1.15f, 8.98f) },
        { "Leg3", new Vector3(-12.06f, 1.15f, 8.98f) },
        { "Leg6", new Vector3(-4.96f, 1.15f, 21.28f) },
        { "Leg5", new Vector3(0.30f, 1.15f, 12.18f) }
    };

    void Start(){
        setOriginalCrossPos();
        gameObject.transform.position = new Vector3(0, 2, 0);
        for (int i = 0; i <= 5; i++)
        {
            Vector3 upperPoint;
            Renderer renderer = upperCrosses[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                upperPoint = renderer.bounds.center;
                int index = i + 1;
                Vector3 lowerPoint = rotPositions["Leg" + index];
                Vector3 targetVector = upperPoint - lowerPoint;
                Vector3 currentVector = originalCrossPos[i] - lowerPoint;
                float deltaLength =targetVector.magnitude-currentVector.magnitude;

                // uitschuif van python / 100 
                Vector3 direction = currentVector.normalized;
                upperLegs[i].transform.position += direction * deltaLength;
                Quaternion rotation = Quaternion.FromToRotation(currentVector, targetVector);
                rotation.ToAngleAxis(out float angle, out Vector3 axis);
                legs[i].transform.RotateAround(lowerPoint, axis, angle);
            }
            
        }
        setOriginalCrossPos();
}
    
    void Update()
    {
        if (Input.GetKey(KeyCode.T))
        {
            Debug.Log($"Turning to {desiredRoll} degree roll, {desiredPitch} degree pitch, {desiredYaw} degree yaw");
            desiredRoll = 0;
            desiredPitch = 15;
            desiredYaw = 10;
            MakeLegRed(0);

            MakeLegRed(4);
        }

        if(Input.GetKey(KeyCode.S)){
            tcpScript.stopThread = true;
        }

        targetPitch = desiredPitch-currentPitch;
        currentPitch = desiredPitch;
        targetRoll = desiredRoll - currentRoll;
        currentRoll = desiredRoll;
        targetYaw = desiredYaw-currentYaw;
        currentYaw = desiredYaw;

        if(targetPitch != 0 || targetRoll != 0 || targetYaw != 0){
            // - send desired roll,yaw,pitch to python server
            // if target pitch,roll,yw != 0 ???
            // if(targetPitch != 0 || targetRoll != 0 || targetYaw != 0){
                HttpClientExample c = CallServerObj.GetComponent<HttpClientExample>();
                string req = string.Format($"http://127.0.0.1:5142?roll={desiredRoll}&pitch={desiredPitch}&yaw={desiredYaw}");
                // c.GetRequest(req);
                StartCoroutine(c.GetRequest(req));
                Debug.Log($"{req}");
            // }
            // - the server sends back the leg lengths 
            // -check if one of the legs abs(length) > 200
            // if yes call MakeLegRed() else check of leg red is and turn it to base 
        }
        // Debug.Log($"TargetPitch: {targetPitch}");
        gameObject.transform.RotateAround(rotPositions["UpperPlatform"], Vector3.left, targetPitch);
        gameObject.transform.RotateAround(rotPositions["UpperPlatform"], Vector3.forward, targetRoll);
        gameObject.transform.RotateAround(rotPositions["UpperPlatform"], Vector3.down, targetYaw);
        
        // adjustLegs();
    }

    public void adjustLegs()
    {
        // if(targetPitch != 0 || targetRoll != 0 || targetYaw != 0){
        for (int i = 0; i <= 5; i++)
        {
            Vector3 upperPoint;
            Renderer renderer = upperCrosses[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                upperPoint = renderer.bounds.center;
                int index = i + 1;
                Vector3 lowerPoint = rotPositions["Leg" + index];
                Vector3 targetVector = upperPoint - lowerPoint;
                Vector3 currentVector = originalCrossPos[i] - lowerPoint;
                float deltaLength =CallServerObj.GetComponent<HttpClientExample>().legLengthsArray[i]-currentVector.magnitude;
                float deltaTemp =  targetVector.magnitude-currentVector.magnitude;
                // uitschuif van python / 100 
                deltaLength += deltaTemp-deltaLength;
                Vector3 direction = currentVector.normalized;
                upperLegs[i].transform.position += direction * deltaLength;
                Quaternion rotation = Quaternion.FromToRotation(currentVector, targetVector);
                rotation.ToAngleAxis(out float angle, out Vector3 axis);
                legs[i].transform.RotateAround(lowerPoint, axis, angle);
            }
        }
        setOriginalCrossPos();
        CallServerObj.GetComponent<HttpClientExample>().legLengthsArray = new float[6];
    }

    void setOriginalCrossPos()
    {
        originalCrossPos.Clear();
        for (int i = 0; i <= 5; i++)
        {
            Renderer renderer = upperCrosses[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                originalCrossPos.Add(renderer.bounds.center);
            }
        }
    }


    public void MakeLegRed(int val){
        Transform leg = legs[val].transform;
        Transform legLowerPart = leg.GetComponentsInChildren<Transform>()[1];
        Transform legHigherPart = leg.GetComponentsInChildren<Transform>()[4];

        Transform[] legLowerParts = legLowerPart.GetComponentsInChildren<Transform>();

        Transform[] legHigherParts = legHigherPart.GetComponentsInChildren<Transform>();

        Transform[] allParts = legLowerParts.Concat(legHigherParts).ToArray();
        foreach (var part in allParts)
        {
            if(part.GetComponent<MeshRenderer>() != null)
                part.GetComponent<MeshRenderer>().material = limitIdicatorMaterial;
        }
    }

    public void MakeLegNormal(int val){
        Transform leg = legs[val].transform;
        Transform legLowerPart = leg.GetComponentsInChildren<Transform>()[1];
        Transform legHigherPart = leg.GetComponentsInChildren<Transform>()[4];

        Transform[] legLowerParts = legLowerPart.GetComponentsInChildren<Transform>();

        Transform[] legHigherParts = legHigherPart.GetComponentsInChildren<Transform>();

        Transform[] allParts = legLowerParts.Concat(legHigherParts).ToArray();
        foreach (var part in allParts)
        {
            if(part.GetComponent<MeshRenderer>() != null)
                part.GetComponent<MeshRenderer>().material = baseMaterial;
        }
    }
}