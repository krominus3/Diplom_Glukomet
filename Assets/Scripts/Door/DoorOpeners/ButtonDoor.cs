using UnityEngine;

public class ButtonDoor : MonoBehaviour
{
    [Header("Door Reference")]
    public Door targetDoor;

    [Header("Button Settings")]
    public bool staysPressed = false; // Остается ли кнопка нажатой
    public float autoCloseDelay = 2f; // Задержка перед автоматическим закрытием

    private int objectsOnButton = 0;
    private bool isDoorOpen = false;
    private float closeTimer = 0f;

    void Update()
    {
        //if (!staysPressed && isDoorOpen && targetDoor != null)
        //{
        //    closeTimer += Time.deltaTime;
        //    if (closeTimer >= autoCloseDelay)
        //    {
        //        targetDoor.Close();
        //        isDoorOpen = false;
        //        closeTimer = 0f;
        //    }
        //}
    }

    void OnTriggerEnter(Collider other)
    {
        objectsOnButton++;
        Debug.Log($"Объект {other.gameObject.name} наступил на кнопку. Всего объектов: {objectsOnButton}");

        if (targetDoor != null && !isDoorOpen)
        {
            targetDoor.Open();
            isDoorOpen = true;
            closeTimer = 0f;
        }
    }

    void OnTriggerExit(Collider other)
    {
        objectsOnButton--;
        Debug.Log($"Объект {other.gameObject.name} покинул кнопку. Осталось объектов: {objectsOnButton}");

        if (staysPressed)
        {
            // Кнопка остается нажатой - ничего не делаем
        }
        else
        {
            // Кнопка отпускается - закрываем дверь если нет других объектов
            if (objectsOnButton <= 0 && isDoorOpen && targetDoor != null)
            {
                targetDoor.Close();
                isDoorOpen = false;
            }
        }
    }

    void OnDrawGizmos()
    {
        // Рисуем зону кнопки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);

        if (targetDoor != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, targetDoor.transform.position);
        }
    }
}