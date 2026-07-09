using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    [SerializeField] private Camera cMainCamera;
    [SerializeField] private LayerMask lEnemyLayer;
    [SerializeField] private float fRayDistance = 100f;

    protected void InitInput()
    {
        cMainCamera = Camera.main;
    }

    void Start()
    {
        InitInput();
    }

    void Update()
    {
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (cMainCamera == null) return; 

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ProcessRaycast(Mouse.current.position.ReadValue());
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            ProcessRaycast(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    private void ProcessRaycast(Vector2 vScreenPosition)
    {
        Ray rRay = cMainCamera.ScreenPointToRay(vScreenPosition);
        RaycastHit rHit;

        if (Physics.Raycast(rRay, out rHit, fRayDistance, lEnemyLayer))
        {
            EnemyController eEnemy = rHit.collider.GetComponent<EnemyController>();
            
            if (eEnemy != null)
            {
                Destroy(rHit.collider.gameObject);
                Debug.Log("Nemico intercettato e rimosso!");
            }
        }
    }
}