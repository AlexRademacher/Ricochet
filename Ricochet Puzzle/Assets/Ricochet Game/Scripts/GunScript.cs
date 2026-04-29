
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GunScript : MonoBehaviour
{
    [Header("Gun Stats")]
    [SerializeField]
    private Transform firePoint;
    [Range(0, 50), SerializeField]
    private float range = 50f;

    private float gunMoveRange = 1f;

    [Range(0, 5), SerializeField]
    private int maxBounce = 3;
    private int currBounce = 0;

    private LineRenderer laserLine;

    private bool isFiring = false;

    [Range(0, 10), SerializeField]
    private int maxShot = 3;
    private int currShot = 0;

    private Vector3 startPos;
    private Quaternion startRot;

    private XRBaseInteractable interactable;
    [Header("Other Prefabs")]
    [SerializeField]
    private GameObject bulletPrefab;
    private SplineContainer splinePath;
    private SplineAnimate splineAnim;

    [SerializeField]
    private GameObject failureUI;
    [SerializeField]
    private GameObject successUI;
    [SerializeField]
    private GameObject player;
    private float distance = 1.0f;

    void Start()
    {
        laserLine = GetComponent<LineRenderer>();
        interactable = GetComponent<XRBaseInteractable>();

        startPos = transform.position;
        startRot = transform.rotation;
    }

    void Update()
    {
        if (Mathf.Abs(transform.position.x - startPos.x) > gunMoveRange || Mathf.Abs(transform.position.z - startPos.z) > gunMoveRange) {
            interactable.enabled = false;
            transform.position = startPos;
            transform.rotation = startRot;
            interactable.enabled = true;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

        if (!isFiring) {
            RayCast(new Ray(firePoint.position, firePoint.forward));
        }

        if (!isFiring && Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
    }

    /// <summary>
    /// Creates an Aiming Line for shooting
    /// </summary>
    /// <param name="ray"> The Ray that is used for aiming the bullets</param>
    private void RayCast(Ray ray)
    {

        laserLine.positionCount = 2;
        laserLine.SetPosition(0, firePoint.position);

        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, range))
        {
            laserLine.SetPosition(1, hit.point);
        }
        else
        {
            laserLine.SetPosition(1, firePoint.position + (firePoint.forward * range));
        }
    }

    /// <summary>
    /// Ricochets the Shooting Line off the Walls till the <c>maxBounce</c>
    /// </summary>
    /// <param name="ray">The Line that we are shooting along</param>
    /// <returns></returns>
    IEnumerator MultiRayCast(Ray ray)
    {
        // remove firing again
        isFiring = true;

        // hides Laser Line
        laserLine.enabled = false;

        // Uses recursion hitting things and ricocheting back till the maxBounce is reached
        if (Physics.Raycast(ray, out RaycastHit hit, range) && currBounce <= maxBounce)
        {
            // Updates bounce num
            currBounce++; 

            // Get new angle
            var reflectAngle = Vector3.Reflect(ray.direction, hit.normal);

            // Update Laser Line
            laserLine.positionCount = currBounce + 1;
            laserLine.SetPosition(currBounce, hit.point);

            // Loops
            StartCoroutine(MultiRayCast(new Ray(hit.point, reflectAngle)));
        }
        else
        {
            // updates Laser Line
            laserLine.positionCount = currBounce + 1;
            //laserLine.SetPosition(currBounce + 1, new Vector3(laserLine.GetPosition(currBounce - 1).x, laserLine.GetPosition(currBounce - 1).y, laserLine.GetPosition(currBounce - 1).z));
            // Is out of bounds with the laser line
        }
        yield return new WaitForSeconds(0);
    }

    /// <summary>
    /// Called to shoot the gun
    /// </summary>
    public void Fire()
    {
        // Makes a ray that points forward if not shooting already
        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, range) && !isFiring)
        {
            // set bounce cound to 0
            currBounce = 0;

            // Sets up Laser Line
            laserLine.positionCount = 1;
            laserLine.SetPosition(0, firePoint.position);

            // Shoots the RayCast out and has it bounce on all of the walls with recursion
            StartCoroutine(MultiRayCast(new Ray(firePoint.position, firePoint.forward)));

            // Setting up spline
            Time.timeScale = 0;

            if (bulletPrefab != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation); // <--- Transform.postion doesn't work correctly with spline Animate look into setting spline animate in code after instantiate
                splinePath = bullet.GetComponent<SplineContainer>();
                splineAnim = bullet.transform.GetChild(0).GetComponent<SplineAnimate>();

                if (splinePath != null)
                {
                    Spline spline = splinePath.Spline;
                    spline.Clear();

                    for (int i = 0; i < laserLine.positionCount; i++)
                    {
                        spline.Add(new BezierKnot(laserLine.GetPosition(i)));
                    }

                    spline.Closed = false;
                    Time.timeScale = 1;

                    StartCoroutine(AfterShoot(bullet));
                }
                else
                    Debug.LogError("Spline Path is missing");
            }
            else
                Debug.LogError("Bullet Prefab is missing");
        }
    }


    IEnumerator AfterShoot(GameObject bullet)
    {
        // Starts spline
        if (splineAnim != null)
        {
            splineAnim.Play();
            yield return new WaitUntil(() => splineAnim.IsPlaying == false);
        }
        else
            Debug.LogError("Spline Animate is missing");

        // Removes bullet
        Destroy(bullet);
        currShot++;

        // Checks if player lost, hit all targets, or continues shooting
        if (currShot > maxShot)
        {
            if (failureUI != null)
            {
                GameObject failUI = Instantiate(failureUI);
                failUI.transform.SetPositionAndRotation(player.transform.position + (player.transform.forward * distance) - new Vector3(0, 0, 0.5f), player.transform.rotation);
            }
            else
                Debug.LogError("Failure UI is missing");
        }
        else if (GameObject.FindGameObjectsWithTag("Target").Length == 0)
        {
            if (successUI)
            {
                GameObject winUI = Instantiate(successUI);
                winUI.transform.SetPositionAndRotation(player.transform.position + (player.transform.forward * distance) - new Vector3(0, 0, 0.5f), player.transform.rotation);
            }
            else
                Debug.LogError("Success UI is missing");
        }
        else
        {
            laserLine.enabled = true;
            isFiring = false;
        }
    }
}