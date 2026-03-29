using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    [Header("Door Movement Settings")]
    public Transform openPosition; // Точка, куда дверь перемещается при открытии
    public Transform closedPosition; // Точка, где дверь находится в закрытом состоянии
    public float moveSpeed = 2f; // Скорость движения двери
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Кривая движения

    [Header("Door State")]
    public bool isOpen = false; // Открыта ли дверь сейчас
    public bool isMoving = false; // Движется ли дверь сейчас

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onDoorOpen; // Событие при открытии
    public UnityEngine.Events.UnityEvent onDoorClose; // Событие при закрытии
    public UnityEngine.Events.UnityEvent onDoorMoveStart; // Событие при начале движения
    public UnityEngine.Events.UnityEvent onDoorMoveComplete; // Событие при завершении движения

    private Coroutine moveCoroutine;
    private Vector3 startPosition;
    private Vector3 targetPosition;

    void Start()
    {
        // Если позиции не заданы, используем текущую как закрытую
        if (closedPosition == null)
        {
            GameObject closedPoint = new GameObject("ClosedPosition");
            closedPoint.transform.position = transform.position;
            closedPoint.transform.parent = transform.parent;
            closedPosition = closedPoint.transform;
        }

        // Если открытая позиция не задана, создаем смещение
        if (openPosition == null)
        {
            GameObject openPoint = new GameObject("OpenPosition");
            openPoint.transform.position = transform.position + Vector3.right * 2f;
            openPoint.transform.parent = transform.parent;
            openPosition = openPoint.transform;
        }

        // Устанавливаем начальную позицию
        transform.position = closedPosition.position;
    }


    // Открыть дверь
    public void Open()
    {
        //if (isOpen || isMoving) return;

        isOpen = true;
        StartMovement(openPosition.position);
        onDoorOpen?.Invoke();
        print("door open");
    }


    // Закрыть дверь
    public void Close()
    {
        //if (!isOpen || isMoving) return;

        isOpen = false;
        StartMovement(closedPosition.position);
        onDoorClose?.Invoke();
        print("door close");
    }


    // Переключить состояние двери (открыть/закрыть)
    public void Toggle()
    {
        print($"doort togle: {isOpen}");
        if (isOpen)
            Close();
        else
            Open();
    }

    // Начать движение к целевой позиции
    private void StartMovement(Vector3 targetPos)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        startPosition = transform.position;
        targetPosition = targetPos;
        moveCoroutine = StartCoroutine(MoveDoor());
    }


    // Корутина движения двери
    private IEnumerator MoveDoor()
    {
        isMoving = true;
        onDoorMoveStart?.Invoke();

        float distance = Vector3.Distance(startPosition, targetPosition);
        float duration = distance / moveSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float curveValue = moveCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
        onDoorMoveComplete?.Invoke();

        moveCoroutine = null;
    }


    // Мгновенно установить дверь в открытое состояние
    public void InstantOpen()
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        transform.position = openPosition.position;
        isOpen = true;
        isMoving = false;
        onDoorOpen?.Invoke();
        onDoorMoveComplete?.Invoke();
    }


    // Мгновенно установить дверь в закрытое состояние
    public void InstantClose()
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        transform.position = closedPosition.position;
        isOpen = false;
        isMoving = false;
        onDoorClose?.Invoke();
        onDoorMoveComplete?.Invoke();
    }


    // Установить новую открытую позицию
    public void SetOpenPosition(Transform newOpenPosition)
    {
        openPosition = newOpenPosition;
    }


    // Установить новую закрытую позицию
    public void SetClosedPosition(Transform newClosedPosition)
    {
        closedPosition = newClosedPosition;
    }

    void OnDrawGizmos()
    {
        if (closedPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(closedPosition.position, 0.2f);
            Gizmos.DrawLine(transform.position, closedPosition.position);
        }

        if (openPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(openPosition.position, 0.2f);
            Gizmos.DrawLine(transform.position, openPosition.position);
        }
    }
}