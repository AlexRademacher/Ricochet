
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GunScript : MonoBehaviour
{
    public Transform firePoint;
    private float range = 50f;

    private float gunMoveRange = 1f;

    private int maxBounce = 3;
    private int currBounce = 0;

    private LineRenderer laserLine;

    private bool isFiring = false;

    private int currShot = 0;
    private int maxShot = 3;

    private Vector3 startPos;

    private XRBaseInteractable interactable;

    void Start()
    {
        laserLine = GetComponent<LineRenderer>();
        interactable = GetComponent<XRBaseInteractable>();

        startPos = transform.position;
    }

    void Update()
    {
        if (Mathf.Abs(transform.position.x - startPos.x) > gunMoveRange || Mathf.Abs(transform.position.z - startPos.z) > gunMoveRange) {
            interactable.enabled = false;
            transform.position = startPos;
            interactable.enabled = true;
        }

        if (!isFiring) {
            RayCast(new Ray(firePoint.position, firePoint.forward));
        }
    }

    private void RayCast(Ray ray)
    {
        RaycastHit hit;

        laserLine.positionCount = 2;
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
        if (Physics.Raycast(ray, out hit, range) && currBounce <= maxBounce)
        {
            currBounce++;
            var reflectAngle = Vector3.Reflect(ray.direction, hit.normal);
            laserLine.positionCount = currBounce + 1;
            laserLine.SetPosition(currBounce, hit.point);
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(multiRayCast(new Ray(hit.point, reflectAngle)));
        }
        else
        {
            laserLine.positionCount = currBounce + 1;
            laserLine.SetPosition(currBounce + 1, new Vector3(laserLine.GetPosition(currBounce - 1).x, laserLine.GetPosition(currBounce - 1).y, laserLine.GetPosition(currBounce - 1).z));
            yield return new WaitForSeconds(2);
            isFiring = false;
        }
    }

    public void Fire()
    {
        RaycastHit hit;

        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range) && !isFiring)
        {
            currBounce = 0;
            laserLine.positionCount = 1;
            laserLine.SetPosition(0, firePoint.position);
            StartCoroutine(multiRayCast(new Ray(firePoint.position, firePoint.forward)));
            if (GameObject.Find("GameManager").GetComponent<GameManager>().checkTargets()) {
                Debug.Log("All targets shot");
            }
        }
    }
}