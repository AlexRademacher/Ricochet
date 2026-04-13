
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunScript : MonoBehaviour
{
    public Transform firePoint;
    public float range = 100f;

    private int maxBounce = 2;
    private int currBounce;

    public InputActionProperty triggerAction;

    private LineRenderer laserLine;


    void Start()
    {
        laserLine = GetComponent<LineRenderer>();

    }

    void Update()
    {
        currBounce = 0;
        laserLine.positionCount = 1;
        laserLine.SetPosition(0, firePoint.position);
        laserLine.enabled = RayCast(new Ray(firePoint.position, firePoint.forward));

        if (triggerAction.action.WasPressedThisFrame())
        {
            Fire();
            Debug.Log("Shot Fired");
            
        }
    }

    private bool RayCast(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range) && currBounce <= maxBounce - 1)
        {
            currBounce++;
            var reflectAngle = UnityEngine.Vector3.Reflect(ray.direction, hit.normal);
            laserLine.positionCount = currBounce + 1;
            laserLine.SetPosition(currBounce, hit.point);
            RayCast(new Ray(hit.point, reflectAngle));
            return true;
        }
        laserLine.positionCount = currBounce + 2;
        laserLine.SetPosition(currBounce + 1, ray.GetPoint(range));
        return false;
    }

    void Fire()
{
    RaycastHit hit;

    if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range))
    {

        Debug.Log("Shoot");
    }
}
}