using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    [Header("Movement")]
    public float AscendSpeed;
    public bool isMoving;
    public bool GenerationComplete;
    [Header("Children")]
    public GameObject Cabin;
    public GameObject Console;
    public Transform DarkEntrance;
    public Transform DarkLeave;
    [Header("Prefabs")]
    public GameObject pfLevel;
    public GameObject pfElevatorOut;
    public GameObject pfElevatorIn;
    [Header("Animators")]
    public Animator CabinDoorAnimator;
    [Header("Vectors - Should be private")]
    public GameObject CurrentLevel;
    public GameObject NextLevel;
    public Vector3 outFloorPos;

    private readonly int LEVEL_DESTROY_TIMER = 7;
    private readonly float LEVEL_GEN_DISTANCE = 1000f;
    private readonly float LEVEL_SKIP_TIMER = 2f;
    private float _playerInTimer;
    private bool _playerInCollider;

    // CAREFUL NOT TO CHANGE HIERARCHY, MIGHT MESS STUFF UP
    void Start()
    {
        // No idea why they change after instantiate alas no time to find out
        isMoving = false;
        GenerationComplete = false;
        Cabin.transform.localPosition = new Vector3(0,-0.1f,0);
        _playerInTimer = 0;
        _playerInCollider = false;
        //Cabin = transform.GetChild(0).gameObject;
        //Console = transform.GetChild(1).gameObject;
        //DarkEntrance = transform.GetChild(2);
        //DarkLeave = transform.GetChild(3);
    }


    private void OnTriggerStay(Collider other)
    {
        if (isMoving) return;
        if (other.CompareTag("Player"))
        {
            _playerInTimer += Time.deltaTime;
            if(_playerInTimer > LEVEL_SKIP_TIMER)
            {
                CabinDoorAnimator.SetTrigger("DoorClose");
                StartAscend();
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInTimer = 0;
        }
    }

    private void Update()
    {
        PlayerInElevator();

        if (Input.GetKeyDown(KeyCode.N))
        {
            StartAscend();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LevelGeneratorBase levelGenerator = GameManager.instance.levelGenerator;
            switch (levelGenerator.GenerationSpeed)
            {
                case LevelGenerationSpeed.Slow:
                    levelGenerator.GenerationSpeed = LevelGenerationSpeed.Fast;
                    Debug.Log("Set the level generation to fast.");
                    break;
                case LevelGenerationSpeed.Fast:
                    levelGenerator.GenerationSpeed = LevelGenerationSpeed.Slow;
                    Debug.Log("Set the level generation to slow.");
                    break;
                default:
                    Debug.LogAssertion("Level generation speed is invalid.");
                    break;
            }
        }
    }

    private void PlayerInElevator()
    {
        if (_playerInCollider)
        {
            _playerInTimer += Time.deltaTime;
            if (_playerInTimer > LEVEL_SKIP_TIMER)
            {
                CabinDoorAnimator.SetTrigger("DoorClose");
                StartAscend();
            }
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

        // Creating next level object
        if (NextLevel == null)
        {
            NextLevel = Instantiate(pfLevel, CurrentLevel.transform.position + Vector3.right * LEVEL_GEN_DISTANCE, Quaternion.identity);
            NextLevel.name = "Level " + GameManager.instance.Floor++;
            GameManager.instance.levelGenerator = NextLevel.GetComponentInChildren<LevelGeneratorBase>();
        }
        else
        {
            NextLevel.SetActive(true);
        }

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
        Vector3 elevatorOutPos = GameManager.instance.levelGenerator.GenerateRandomPositionOnGround(towardsMiddle: true) + Vector3.up * 3f;
        Vector3 elevatorInPos = GameManager.instance.levelGenerator.GenerateRandomPositionOnGround(towardsMiddle: true) + Vector3.up * 3f;

        // Creating elevator in object
        GameObject elevatorIn = Instantiate(pfElevatorIn, elevatorInPos, Quaternion.identity, NextLevel.transform);

        // Setting next elevator level
        elevatorIn.GetComponent<ElevatorController>().CurrentLevel = NextLevel;
        elevatorIn.GetComponent<ElevatorController>().NextLevel = null;

        // Creating elevator out object
        GameObject elevatorOut = Instantiate(pfElevatorOut, elevatorOutPos, Quaternion.identity, NextLevel.transform);
        Cabin.transform.parent = elevatorOut.transform;
        Vector3 entrancePos = elevatorOut.transform.GetChild(0).position; // Entrance
        outFloorPos = elevatorOut.transform.GetChild(1).position;
        TeleportToNextLevel(entrancePos);
        Debug.Log("Generation Complete");
    }
    public void Move()
    {
        Cabin.transform.position += new Vector3(0, AscendSpeed, 0) * Time.deltaTime;

        //MADE IT TO OTHER LEVEL
        if(Cabin.transform.position.y > outFloorPos.y && GenerationComplete)
        {
            isMoving = false;
            CabinDoorAnimator.SetTrigger("DoorOpen");
            Destroy(CurrentLevel,LEVEL_DESTROY_TIMER);
        }
        if(Cabin.transform.position.y > DarkLeave.position.y) 
        {
            Vector3 difference = new Vector3(0,DarkLeave.position.y - DarkEntrance.position.y,0);
            Cabin.transform.position -= difference;
            GameManager.instance.mainPlayer.transform.position = Cabin.transform.position + Vector3.up * 3f;
        }
    }

    public void TeleportToNextLevel(Vector3 entrancePos)
    {
        Cabin.transform.position = entrancePos;
        GameManager.instance.mainPlayer.transform.position = entrancePos + new Vector3(0,5,0);
    }
}
