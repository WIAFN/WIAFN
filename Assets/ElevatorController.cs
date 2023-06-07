using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    public float AscendSpeed;
    public bool isMoving = false;
    public bool GenerationComplete = false;
    public GameObject Cabin;
    public GameObject Console;
    public Transform DarkEntrance;
    public Transform DarkLeave;
    public GameObject NextLevel;
    public GameObject pfElevatorOut;
    public Vector3 outFloorPos;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            StartAscend();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isMoving)
        {
            Move();
        }
    }

    public void StartAscend()
    {
        isMoving = true;
        NextLevel.SetActive(true);
        StartCoroutine(StartGenerationAsync());
    }

    IEnumerator StartGenerationAsync()
    {
        GameManager.instance.levelGenerator.OnGenerationCompleted += LevelGenerationComplete;
        while (!GenerationComplete)
        {
            yield return null;
        }
    }
    private void LevelGenerationComplete()
    {
        GenerationComplete = true;
        Vector3 startingPoint = GameManager.instance.levelGenerator.GenerateRandomPositionOnGround(towardsMiddle: true) + Vector3.up * 3f;
        GameObject elevatorOut = Instantiate(pfElevatorOut, startingPoint, Quaternion.identity);
        Vector3 entrancePos = elevatorOut.transform.GetChild(0).position; // Entrance
        outFloorPos = elevatorOut.transform.GetChild(1).position;
        TeleportToNextLevel(entrancePos);
        Debug.Log("Generation Complete");
    }
    public void Move()
    {
        Cabin.transform.position += new Vector3(0, AscendSpeed, 0) * Time.deltaTime;
        if(Cabin.transform.position.y > outFloorPos.y && GenerationComplete)
        {
            isMoving = false;
        }
        if(Cabin.transform.position.y > DarkLeave.position.y) 
        {
            Vector3 difference = new Vector3(0,DarkLeave.position.y - DarkEntrance.position.y,0);
            Cabin.transform.position -= difference;
            GameManager.instance.mainPlayer.transform.position -= difference;
        }
    }

    public void TeleportToNextLevel(Vector3 entrancePos)
    {
        Cabin.transform.position = entrancePos;
        GameManager.instance.mainPlayer.transform.position = entrancePos + new Vector3(0,5,0);
    }
}
