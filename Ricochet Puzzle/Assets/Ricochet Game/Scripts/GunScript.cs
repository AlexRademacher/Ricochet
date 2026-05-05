
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

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
    public GameObject bulletPrefab;
    private SplineContainer splinePath;
    private SplineAnimate splineAnim;

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
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
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

        if (Physics.Raycast(firePoint.position, -1 * firePoint.forward, out hit, range))
        {
            laserLine.SetPosition(1, hit.point);
        }
        else
        {
            laserLine.SetPosition(1, firePoint.position + (-1  * firePoint.forward * range));
        }
    }

    IEnumerator multiRayCast(Ray ray)
    {
        isFiring = true;
        RaycastHit hit;
        laserLine.enabled = false;
        if (Physics.Raycast(ray, out hit, range) && currBounce <= maxBounce)
        {
            currBounce++;
            var reflectAngle = Vector3.Reflect(ray.direction, hit.normal);
            laserLine.positionCount = currBounce + 1;
            laserLine.SetPosition(currBounce, hit.point);
            StartCoroutine(multiRayCast(new Ray(hit.point, reflectAngle)));
        }
        else
        {
            laserLine.positionCount = currBounce + 1;
            laserLine.SetPosition(currBounce + 2, new Vector3(laserLine.GetPosition(currBounce - 1).x, laserLine.GetPosition(currBounce - 1).y, laserLine.GetPosition(currBounce - 1).z));
        }
        yield return new WaitForSeconds(0);
    }

    public void Fire()
    {
        RaycastHit hit;

        if (Physics.Raycast(firePoint.position, -1 * firePoint.forward, out hit, range) && !isFiring)
        {
            currBounce = 0;
            laserLine.positionCount = 1;
            laserLine.SetPosition(0, firePoint.position);
            StartCoroutine(multiRayCast(new Ray(firePoint.position, -1 * firePoint.forward)));

            Time.timeScale = 0;
            GameObject bullet = Instantiate(bulletPrefab, new Vector3(firePoint.position.x, firePoint.position.y, firePoint.position.z), firePoint.rotation);
            splinePath = bullet.GetComponent<SplineContainer>();
            splineAnim = bullet.GetComponent<SplineAnimate>();

            Spline spline = splinePath.Spline;
            spline.Clear();
            for (int i = 0; i < laserLine.positionCount; i++)
            {
                spline.Add(new BezierKnot(laserLine.GetPosition(i)));           
            }
            spline.Closed = false;
            Time.timeScale = 1;
            
            StartCoroutine(afterShoot(bullet));
        }
    }

    public GameObject failureUI;
    public GameObject successUI;
    public GameObject player;
    private float distance = 1.0f;

    IEnumerator afterShoot(GameObject bullet)
    {
        splineAnim.Play();
        yield return new WaitUntil(() => splineAnim.IsPlaying == false);

        Destroy(bullet);
        currShot++;

        if (currShot > maxShot)
        {
            GameObject failUI = Instantiate(failureUI);
            failUI.transform.position = player.transform.position + (player.transform.forward * distance) - new Vector3(0,0.5f,0);
            failUI.transform.rotation = player.transform.rotation;
        }
        else if (GameObject.FindGameObjectsWithTag("Target").Length == 0)
        {
            GameObject winUI = Instantiate(successUI);
            winUI.transform.position = player.transform.position + (player.transform.forward * distance) - new Vector3(0,0.5f,0);
            winUI.transform.rotation = player.transform.rotation;

            GameObject.Find("GameManager").GetComponent<GameManager>().setTimeResult();
        }
        else
        {
            laserLine.enabled = true;
            isFiring = false;
        }
    }
}