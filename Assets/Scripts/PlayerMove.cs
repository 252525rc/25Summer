using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using Unity.Burst.CompilerServices;


public class PlayerMove : MonoBehaviour
{
    public MazeGenerator mazeGen;
    public WallColor wallColor;
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
    public AudioClip swing;
    public AudioClip Goal;
    public AudioClip gameOver;

    public int brokenCount;
    public int maxAttack;
    public bool Attack = false;
    public bool hit;

    public bool canRotate = true;
    private float tiltX = 0f;
    private float tiltZ = 0f;
    private float tiltThreshold = 0.3f;
    // micro:bitのボタンの状態(0: なし、1: Aボタン、-1: Bボタン)
    private int buttonState = 0;

    public Text Finishtxt;
    public Text Counttxt;
    public GameObject retryButton;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Finishtxt.gameObject.SetActive(false);

        brokenCount = 3;
        maxAttack = 0;
        animator = GetComponent<Animator>();
        maze = mazeGen.GetMaze();
        cellSize = mazeGen.cellSize;
        currentPos = mazeGen.GetStartPos();
        Counttxt.text = "BROKEN:" + brokenCount;

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

    void HandleRotationInput()
    {
        float rotationAmount = 90f;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(SmoothRotate(-rotationAmount));
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            StartCoroutine(SmoothRotate(rotationAmount));
        }
    }

    void HandleTiltRotation()
    {
        float rotationAmount = 90f;

        if (canRotate)
        {
            if (tiltX > 0.4f)
            {
                StartCoroutine(SmoothRotate(-rotationAmount));
                canRotate = false;
            }
            else if (tiltX < -0.4f)
            {
                StartCoroutine(SmoothRotate(rotationAmount));
                canRotate = false;
            }
        }
        if (Mathf.Abs(tiltX) < 0.2f)
        {
            canRotate = true;
        }

        /*        if (canRotate)
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
                }*/
    }


    IEnumerator PerformTripleAttack()
    {
        Attack = true;

        for ( int i = 0; i < 3; i++)
        {
            animator.SetTrigger("isAttack");
            maxAttack++;
            if (hit)
            {
                AS.PlayOneShot(wallBroken);
            }
            else
            {
                AS.PlayOneShot(swing);
            }
            // 攻撃アニメーションの長さに応じてWait
            // 例：0.5秒のアニメーションなら
            yield return new WaitForSeconds(0.5f);
            animator.ResetTrigger("isAttack");
        }
        maxAttack = 0;
        Attack = false;
        Counttxt.text = "BROKEN:" + brokenCount;
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

    IEnumerator SmoothRotate(float angle)
    {
        isMoving = true;  // 回転中は移動できないようにする
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, angle, 0);
        float duration = 0.2f;  // 回転にかかる時間
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
        isMoving = false;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Finishtxt.text = "GameOver";
            Finishtxt.gameObject.SetActive(true);
            bgmAS.Stop();
            AS.PlayOneShot(gameOver);
            Time.timeScale = 0;
        }

        if(collision.gameObject.tag == "Goal")
        {
            bgmAS.Stop();
            AS.PlayOneShot(Goal);
            Finishtxt.text = "GameClear";
            Finishtxt.gameObject.SetActive(true);
            Time.timeScale = 0;
            Debug.Log("ゴールに到達しました！");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "wall") hit = true;
        //Debug.Log(hit);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "wall") hit = false;
        //Debug.Log(hit);
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
