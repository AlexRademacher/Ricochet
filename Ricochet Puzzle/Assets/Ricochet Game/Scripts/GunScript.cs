
using System.Collections;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunScript : MonoBehaviour
{
    public Transform firePoint;
    private float range = 50f;

    private int maxBounce = 3;
    private int currBounce = 0;

    private LineRenderer laserLine;

    private bool isFiring = false;

    void Start()
    {
        laserLine = GetComponent<LineRenderer>();
    }

    void Update()
    {
        RayCast(new Ray(firePoint.position, firePoint.forward));
    }

    private void RayCast(Ray ray)
    {
        RaycastHit hit;

        laserLine.SetPosition(0, firePoint.position);

        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range))
        {
            laserLine.SetPosition(1, hit.point);
        }
        else
        {
            laserLine.SetPosition(1, firePoint.position + firePoint.forward * range);
        }
    }

    IEnumerator multiRayCast(Ray ray)
    {
        isFiring = true;
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range) && currBounce <= maxBounce + 1)
        {
            currBounce++;
            var reflectAngle = UnityEngine.Vector3.Reflect(ray.direction, hit.normal);
            laserLine.positionCount = currBounce + 1;
            laserLine.SetPosition(currBounce, hit.point);
            multiRayCast(new Ray(hit.point, reflectAngle));
        }
        else
        {
            laserLine.positionCount = currBounce + 1;
            laserLine.SetPosition(currBounce, new UnityEngine.Vector3(laserLine.GetPosition(currBounce - 1).x, laserLine.GetPosition(currBounce - 1).y, laserLine.GetPosition(currBounce - 1).z));
        }
        yield return new WaitForSeconds(1);
        isFiring = false;
    }

    public void Fire()
    {
        RaycastHit hit;

        if (!(GameObject.Find("LevelManager").GetComponent<LevelData>().updateShots()))
        {
            return;
        }

        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range) && !isFiring)
        {
            currBounce = 0;
            laserLine.positionCount = 0;
            StartCoroutine(multiRayCast(new Ray(firePoint.position, firePoint.forward)));
        }
    }
}