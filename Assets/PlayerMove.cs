using UnityEngine;
using System;
using TMPro;
using System.Collections;
using UnityEngine.UI;


public class PlayerMove : MonoBehaviour
{
    public MazeGenerator mazeGen;
    public float moveDuration = 0.2f;
    public float moveInterval = 0.3f;

    private int[,] maze;
    private Vector2Int currentPos;
    private float cellSize;
    private bool isMoving = false;
    private bool hasMoved = false;
    private Rigidbody rb;

    public Animator animator;
    public AudioSource AS;
    public AudioSource bgmAS;
    public AudioClip wallBroken;
    public AudioClip Goal;
    public AudioClip gameOver;

    public int brokenCount;
    public int maxAttack;
    public bool Attack = false;

    public bool canRotate = true;
    private float tiltX = 0f;
    private float tiltZ = 0f;
    private float tiltThreshold = 0.3f;
    // micro:bitのボタンの状態(0: なし、1: Aボタン、-1: Bボタン)
    private int buttonState = 0;

    public TextMeshProUGUI GameOvertxt;
    public GameObject retryButton;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        GameOvertxt.gameObject.SetActive(false);

        brokenCount = 3;
        maxAttack = 0;
        animator = GetComponent<Animator>();
        maze = mazeGen.GetMaze();
        cellSize = mazeGen.cellSize;
        currentPos = mazeGen.GetStartPos();

        transform.position = new Vector3(currentPos.x * cellSize, -1f, currentPos.y * cellSize);
    }

    void Update()
    {
        if (!isMoving && !Attack)
        {
            HandleRotationInput();

            // 長押しに対応：GetKey を使う
            if (GetForwardInput())
            {
                if (!hasMoved)
                {
                    hasMoved = true;
                    StartCoroutine(MoveContinuousLoop());
                }
            }
            else
            {
                hasMoved = false;
            }

        }

        if (Input.GetKeyUp(KeyCode.Q) && brokenCount > 0 && !Attack)
        {

            StartCoroutine(PerformTripleAttack());
            
        }else if (buttonState == 1 && brokenCount > 0 && !Attack)
        {
            StartCoroutine(PerformTripleAttack());
        }

        HandleTiltRotation();

    }

    void HandleTiltRotation()
    {
        if (canRotate)
        {
            if (tiltX > 0.4f)
            {
                transform.Rotate(0, 90f, 0);
                canRotate = false;
            }
            else if (tiltX < -0.4f)
            {
                transform.Rotate(0, -90f, 0);
                canRotate = false;
            }
        }

        // 中立に戻ったら再び回転許可
        if (Mathf.Abs(tiltX) < 0.2f)
        {
            canRotate = true;
        }
    }


    IEnumerator PerformTripleAttack()
    {
        Attack = true;

        for (int i = 0; i < 3; i++)
        {
            animator.SetTrigger("isAttack");
            AS.PlayOneShot(wallBroken); 
            maxAttack++;

            // 攻撃アニメーションの長さに応じてWait
            // 例：0.5秒のアニメーションなら
            yield return new WaitForSeconds(0.5f);

            animator.ResetTrigger("isAttack");
        }

        

        if (maxAttack == 3)
        {
            brokenCount--;
            maxAttack = 0;
        }

        Attack = false;
    }

    IEnumerator MoveContinuousLoop()
    {
        while (GetForwardInput())
        {
            Vector2Int direction = GetForwardDirection();
            Vector2Int target = currentPos + direction;

            if (IsWalkable(target))
            {
                Vector3 startPos = transform.position;
                Vector3 endPos = new Vector3(target.x * cellSize, -1f, target.y * cellSize);
                yield return MoveSmoothly(startPos, endPos, target);
            }

            yield return new WaitForSeconds(moveInterval);
        }
    }


    void HandleRotationInput()
    {
        float rotationAmount = 90f;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            transform.Rotate(0f, -rotationAmount, 0f);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            transform.Rotate(0f, rotationAmount, 0f);
        }
    }

    bool GetForwardInput()
    {
        return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || tiltZ < -tiltThreshold;
    }


    Vector2Int GetForwardDirection()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        if (Mathf.Abs(forward.z) > Mathf.Abs(forward.x))
        {
            return (forward.z > 0) ? new Vector2Int(0, 1) : new Vector2Int(0, -1);
        }
        else
        {
            return (forward.x > 0) ? new Vector2Int(1, 0) : new Vector2Int(-1, 0);
        }
    }


    bool IsWalkable(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < maze.GetLength(0) &&
               pos.y >= 0 && pos.y < maze.GetLength(1) &&
               maze[pos.x, pos.y] == 0;
    }


    IEnumerator MoveSmoothly(Vector3 start, Vector3 end, Vector2Int nextPos)
    {
        isMoving = true;
        animator.SetBool("isMoving", true);
        float elapsed = 0f;

        Vector3 direction = (end - start).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        currentPos = nextPos;
        isMoving = false;
        animator.SetBool("isMoving", false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            GameOvertxt.gameObject.SetActive(true);
            AS.PlayOneShot(gameOver);
            Time.timeScale = 0;
        }

        if(collision.gameObject.tag == "Goal")
        {
            bgmAS.Stop();
            AS.PlayOneShot(Goal);
            Time.timeScale = 0;
            Debug.Log("ゴールに到達しました！");
        }
    }

    // micro:bit 傾きデータ（X軸）
    public void OnAccelerometerChangedx(int x)
    {
        const float MAX_X = 1600f;
        tiltX = Mathf.Clamp(x / MAX_X, -1f, 1f);
    }

    // micro:bit 傾きデータ（Y軸 → Z移動）
    public void OnAccelerometerChangedy(int y)
    {
        const float MAX_Y = 1600f;
        tiltZ = Mathf.Clamp(y / MAX_Y, -1f, 1f);
    }

    public void OnButtonAChanged(int state)
    {
        buttonState = (state == 0 ? 0 : 1);
    }

}
