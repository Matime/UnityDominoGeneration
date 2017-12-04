using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    public GameObject Domino;
    public GameObject Step;
    public Vector3 SpawnValues;
    public string TxtFile = "test";
    public float DistanceBetweenDominos = 1.625f;
    public int SegmentCountPerUnit = 200;

    private List<Vector3> _controlPoints;
    private List<Vector3> _interpolatedValues;
    private List<float> _distances;
    private List<StepAndSpawn> _spawnList;
    private const float DominoHeight = 1.875f;
    void SpawnDominos()
    {
        for (int i = 0; i < _spawnList.Count; i++)
        {
            GameObject obj = Instantiate(Domino, _spawnList[i].SpawnPoint, Quaternion.identity);
            if (i < _spawnList.Count - 1)
            {
                Vector3 targetOrientation = _spawnList[i + 1].SpawnPoint;
                targetOrientation.y = obj.transform.position.y;
                obj.transform.LookAt(targetOrientation);

                if (i == 0)
                    obj.transform.Rotate(new Vector3(1, 0, 0), 15.0f);
            }
            else
            {
                Vector3 targetOrientation = _spawnList[i - 1].SpawnPoint;
                targetOrientation.y = obj.transform.position.y;
                obj.transform.LookAt(targetOrientation);
                
            }
        }
    }
    void Start()
    {

    }
    private void InstantiateValues()
    {
        _controlPoints = new List<Vector3>();
        _interpolatedValues = new List<Vector3>();
        _distances = new List<float>();
        _spawnList = new List<StepAndSpawn>();

    }

    private void LoadControlPoints()
    {
        _controlPoints.Add(new Vector3());
        var controlPointObjects = GameObject.FindGameObjectsWithTag("ControlPoint");
        var sortedControlPointObjects = controlPointObjects.OrderBy(GetInitTimeFromControlPointObject).ToList(); ;
        foreach (var ctrlPoint in sortedControlPointObjects)
        {
            _controlPoints.Add(ctrlPoint.transform.position);
        }
        _controlPoints.Add(new Vector3());
    }

    private float GetInitTimeFromControlPointObject(GameObject obj)
    {
        ControlPointObject otherScript = obj.GetComponent(typeof(ControlPointObject)) as ControlPointObject;
        return otherScript.InitializationTime;
    }

    private void GenerateCatMullRom()
    {
        for (int i = 0; i < _controlPoints.Count - 3; i++)
        {
            float distanceBetweenControlPoints = Vector3.Distance(_controlPoints[i + 1], _controlPoints[i + 2]);
            int segmentCount = (int) (distanceBetweenControlPoints * SegmentCountPerUnit);
            for (int j = 0; j < segmentCount; j++)
            {
                float time = j / (float) segmentCount;
                Vector3 tempVector3 = CatMullRom(time, _controlPoints[i], _controlPoints[i + 1], _controlPoints[i + 2],
                    _controlPoints[i + 3]);
                tempVector3.y = 0.9375f;
                _interpolatedValues.Add(tempVector3);

            }
        }
        float totalDistance = 0.0f;
        _distances.Add(0.0f);
        for (int i = 0; i < _interpolatedValues.Count - 1; i++)
        {
            totalDistance += Vector3.Distance(_interpolatedValues[i], _interpolatedValues[i + 1]);
            _distances.Add(totalDistance);

        }
    }
    private void GenerateSpawnPoints()
    {
        float currentDistance;
        float targetDistance = 0.0f;
        for (int i = 0; i < _interpolatedValues.Count; i++)
        {
            currentDistance = _distances[i];
            if (currentDistance >= targetDistance)
            {
                StepAndSpawn temp = new StepAndSpawn();
                Vector3 tempVector3 = i == 0 ? _interpolatedValues[i] : _interpolatedValues[i - 1];
                tempVector3.y = 0.9375f;
                temp.SpawnPoint =tempVector3;
                _spawnList.Add(temp);
                targetDistance += DistanceBetweenDominos;
            }

        }
    }

    private void DetectIntersections()
    {
        for (int i = 0; i < _spawnList.Count; i++)
        {
            Vector3 tempVectorI = _spawnList[i].GetCopyOfSpawnPoint();
            tempVectorI.y = 0;
            for (int j = i + 1; j < _spawnList.Count; j++)
            {
                Vector3 tempVectorJ = _spawnList[j].GetCopyOfSpawnPoint();
                tempVectorJ.y = 0;

                if (Vector3.Distance(_spawnList[i].SpawnPoint, _spawnList[j].SpawnPoint) < DistanceBetweenDominos - 0.1f || ((Vector3.Distance(tempVectorI,tempVectorJ)<1)&&(Math.Abs(_spawnList[i].SpawnPoint.y-_spawnList[j].SpawnPoint.y)<1.875)))
                {
                    int stepCount = 11;
                    for (int k = 0; k < stepCount; k++)
                    {
                        int index = j - (stepCount - 1) / 2 + k;
                        if (!(index > _spawnList.Count - 1))
                        {
                            var toModify = _spawnList[index];
                            var toModifySpawnPoint = toModify.GetCopyOfSpawnPoint();
                            float modifier = 4 / 3f;
                            int stepCountHelper = (stepCount - 1) / 2 + 1;
                            int indexDistance = (Math.Abs(index - j));
                            modifier = modifier - ((float) indexDistance / stepCountHelper * modifier);
                            toModifySpawnPoint.y = DominoHeight * modifier + DominoHeight/2 +(Math.Abs(DominoHeight/2-toModifySpawnPoint.y));
                            toModify.SpawnPoint = toModifySpawnPoint;
                            _spawnList[index] = toModify;
                            SpawnStep(index, toModify, (index > _spawnList.Count - 1) ? _spawnList[index + 1] : _spawnList[index - 1]);
                        }
                    }
                }
            }
        }
    }


    private void SpawnStep(int index,StepAndSpawn position, StepAndSpawn lookAt)
    {
        Vector3 toPlace = new Vector3(position.SpawnPoint.x, position.SpawnPoint.y-DominoHeight/2, position.SpawnPoint.z);
        Vector3 targetPosition = lookAt.GetCopyOfSpawnPoint();
        targetPosition.y = toPlace.y;
        GameObject step = Instantiate(Step, toPlace, Quaternion.identity);
        step.transform.LookAt(targetPosition);
        if (position.Step)  
        {
            
            DestroyObject(position.Step);
            position.Step = null;
        }

        position.Step = step;
        _spawnList[index] = position;



    }

    private Vector3 CatMullRom(float t, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        float tSquared = t * t;
        float tCubed = tSquared * t;
        float u = 1.0f;
        float g = 0.5f;
        float[,] cm = new float[4, 4]{
            { -g, 2.0f - g, g - 2.0f, g},
            { 2.0f * g, g - 3.0f, 3.0f - 2.0f * g, -g },
            { -g, 0.0f, g, 0.0f },
            { 0, 1.0f, 0.0f, 0.0f }
        };
        float a = tCubed * cm[0, 0] + tSquared * cm[1, 0] + t * cm[2, 0] + u * cm[3, 0];
        float b = tCubed * cm[0, 1] + tSquared * cm[1, 1] + t * cm[2, 1] + u * cm[3, 1];
        float c = tCubed * cm[0, 2] + tSquared * cm[1, 2] + t * cm[2, 2] + u * cm[3, 2];
        float d = tCubed * cm[0, 3] + tSquared * cm[1, 3] + t * cm[2, 3] + u * cm[3, 3];
        Vector3 toReturn = new Vector3(
            a * v1.x + b * v2.x + c * v3.x + d * v4.x,
            a * v1.y + b * v2.y + c * v3.y + d * v4.y,
            a * v1.z + b * v2.z + c * v3.z + d * v4.z
            );
        return toReturn;
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void StartButton()
    {
        StopCameraScript();
        InstantiateValues();
        LoadControlPoints();
        GenerateCatMullRom();
        GenerateSpawnPoints();
        DestroyGeneratedObjects();
        DetectIntersections();
        SpawnDominos();

    }

    public void ResetButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // loads current scene
    }
    private void DestroyGeneratedObjects()
    {
        foreach (var domino in GameObject.FindGameObjectsWithTag("Domino"))
        {
            Destroy(domino);
        }
        foreach (var controlPoint in GameObject.FindGameObjectsWithTag("ControlPoint"))
        {
            Destroy(controlPoint);
        }
        foreach (var originalControlPoint in GameObject.FindGameObjectsWithTag("OriginalControlPoint"))
        {
            Destroy(originalControlPoint);
        }
        foreach(var step in GameObject.FindGameObjectsWithTag("Step")) { 
            Destroy(step);
        }
    }

    private void StartCameraScript()
    {
        var cameraScript = Camera.main.GetComponent(typeof(SpawnOnClick)) as SpawnOnClick;
        if (cameraScript != null) cameraScript.ResumeUpdate();
    }

    private void StopCameraScript()
    {
        var cameraScript = Camera.main.GetComponent(typeof(SpawnOnClick)) as SpawnOnClick;
        if (cameraScript != null) cameraScript.StopUpdate();
    }

    struct StepAndSpawn
    {
        private Vector3 _spawnPoint;
        private GameObject _Step;
        public Vector3 SpawnPoint
        {
            get { return _spawnPoint; }
            set { _spawnPoint = value;}
        }
        public GameObject Step
        {
            get { return _Step; }
            set { _Step = value; }
        }

        public Vector3 GetCopyOfSpawnPoint()
        {
            return new Vector3(_spawnPoint.x,_spawnPoint.y,_spawnPoint.z);
        }

    }
}
