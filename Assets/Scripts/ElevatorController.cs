using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    [Header("Movement")]
    public float AscendSpeed;
    [Header("Children")]
    public GameObject Cabin;
    public GameObject Console;
    public Transform DarkEntrance;
    public Transform DarkLeave;
    [Header("Animators")]
    public Animator CabinDoorAnimator;
    [Header("Vectors - Should be private")]
    public GameObject CurrentLevel;
    public Vector3 outFloorPos;

    private bool _active;
    private bool _isMoving;

    private GameManager _gm;
    private LevelInfo _nextLevelInfo;

    private readonly float LEVEL_SKIP_TIMER = 2f;
    private float _playerInTimer;

    private bool _teleportedToNextLevel;

    void Start()
    {
        // No idea why they change after instantiate alas no time to find out
        _active = false;
        _isMoving = false;

        _teleportedToNextLevel = false;

        Cabin.transform.localPosition = new Vector3(0,-0.1f,0);

        _gm = GameManager.instance;

        _playerInTimer = 0;

        if (_gm.CurrentLevelInfo.LevelObject == CurrentLevel)
        {
            ActivateElevator();
        }
        else
        {
            _gm.OnLevelChanged += OnLevelChanged;
        }
    }

    private void OnDestroy()
    {
        if (_gm != null)
        {
            _gm.OnLevelChanged -= OnLevelChanged;
        }
    }

    private void OnLevelChanged(LevelInfo oldLevel, LevelInfo currentLevel)
    {
        if (currentLevel.LevelObject == CurrentLevel)
        {
            ActivateElevator();
            _gm.OnLevelChanged -= OnLevelChanged;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (_isMoving) return;
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
        if (_active)
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                StartAscend();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                LevelGeneratorBase levelGenerator = _nextLevelInfo.Generator;
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
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_isMoving)
        {
            Move();
        }
    }

    private void ActivateElevator()
    {
        _active = true;
        _nextLevelInfo = _gm.CreateNewLevel(startGeneration: true);
    }

    public void StartAscend()
    {
        _isMoving = true;

        StartCoroutine(WaitForLevelGenerationToCompleteToChangeLevel());
    }

    IEnumerator WaitForLevelGenerationToCompleteToChangeLevel()
    {
        yield return new WaitUntil(() => _nextLevelInfo.Generator.GenerationComplete);

        Cabin.transform.parent = _nextLevelInfo.ElevatorOut.transform;
        outFloorPos = _nextLevelInfo.ElevatorOut.transform.GetChild(1).position;
        // Last Operations.
        Vector3 entrancePos = _nextLevelInfo.ElevatorOut.transform.GetChild(0).position; // Entrance

        _gm.SetLevel(_nextLevelInfo);
        TeleportToNextLevel(entrancePos);
    }

    public void Move()
    {
        Cabin.transform.position += new Vector3(0, AscendSpeed, 0) * Time.deltaTime;

        //MADE IT TO OTHER LEVEL
        if (Cabin.transform.position.y > outFloorPos.y && _teleportedToNextLevel)
        {
            _isMoving = false;
            CabinDoorAnimator.SetTrigger("DoorOpen");
            _gm.DestroyOldLevel(CurrentLevel);
        }

        // Teleports player back if they fall down from elevator due to any physics bug.
        if (Cabin.transform.position.y > _gm.mainPlayer.transform.position.y)
        {
            _gm.mainPlayer.transform.position = Cabin.transform.position + Vector3.up * 3f;
        }

        if (Cabin.transform.position.y > DarkEntrance.position.y)
        {
            _nextLevelInfo.Generator.GenerationSpeed = LevelGenerationSpeed.Fast;
        }

        if(!_teleportedToNextLevel && Cabin.transform.position.y > DarkLeave.position.y) 
        {
            Vector3 difference = new Vector3(0,DarkLeave.position.y - DarkEntrance.position.y,0);
            Cabin.transform.position -= difference;
            _gm.mainPlayer.transform.position = Cabin.transform.position + Vector3.up * 3f;
        }
    }

    public void TeleportToNextLevel(Vector3 entrancePos)
    {
        Cabin.transform.position = entrancePos;
        _gm.mainPlayer.transform.position = entrancePos + new Vector3(0f, 5f, 0f);

        _teleportedToNextLevel = true;
    }
}
