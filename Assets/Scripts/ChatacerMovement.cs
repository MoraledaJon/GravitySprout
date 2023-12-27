using Cinemachine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ChatacerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private SpriteRenderer rbSprite;

    [SerializeField]
    private UnityEngine.UI.Slider slider;

   
    public float jumpPower = 2;
    public float maxJump = 500;

    public CinemachineVirtualCamera virtualCamera;

    public float newYAngle;
    public float oldYAngle;

    private int currentRotation = 0;

    public Sprite[] sprites;
    private bool prepareJump = false;
    private bool isGrounded = false;
    private float fixedDeltaTime;

    public float currentCameraFov;
    public float maxCameraFox;
    private bool timestop = false;
    public float smooth;

    public ParticleSystem jumpEffect;
    public ParticleSystem fallEffect;
    public UnityEngine.UI.Image sliderimg;
    public GameObject gameoverimg;
    public GameObject animGameClear;
    public Animator animTitle;
    bool candeposit;
    bool playing = false;

    public Animator[] starAnimator;
    private int itemCount = 0;
    public GameObject pressto;
 
    private void FixedUpdate()
    {
        this.fixedDeltaTime = Time.fixedDeltaTime;
    }
    // Start is called before the first frame update
    void Start()
    {
        slider.maxValue = maxJump;
        rb = GetComponent<Rigidbody>();
        currentRotation = 1;
        rbSprite = GetComponent<SpriteRenderer>();
        rbSprite.sprite = sprites[0];
        slider.gameObject.SetActive(false);
        sliderimg.gameObject.transform.parent.gameObject.SetActive(false);

    }

    public void StartPlaying()
    {
        if(!playing)
        {

        
        animTitle.SetTrigger("start");
        StartCoroutine(animstart());
        }
    }

    IEnumerator animstart()
    {
        yield return new WaitForSeconds(1);
        Vector3 postoreach = transform.position + new Vector3(0, 0, -2);

        while(Vector3.Distance(transform.position, postoreach) > 0.1f)
            { 
            transform.position = Vector3.Lerp(transform.position, postoreach, Time.deltaTime*2);
            yield return null;
        }    
        playing = true;
        slider.gameObject.SetActive(true);
        StopAllCoroutines();
        sliderimg.gameObject.transform.parent.gameObject.SetActive(true);
    }


    // Update is called once per frame
    void Update()
    {
        if (!playing)
            return;

        if(transform.position.y < -10f || transform.position.y > 30 || transform.position.x > 150 || transform.position.x < -30)
        {
            //dead
            gameoverimg.SetActive(true);
            virtualCamera.Follow = null;
            slider.gameObject.SetActive(false);
            sliderimg.gameObject.transform.parent.gameObject.SetActive(false);
            
        }
        if(Input.GetKeyDown(KeyCode.A))
        {
            transform.localScale = new Vector3(-1,1,1);
        }
        else if(Input.GetKeyDown(KeyCode.D))
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        if(!timestop)
        rb.AddForce(transform.up * -2);

        if(Input.GetKeyDown(KeyCode.Escape)) { SceneManager.LoadScene(0); }

        //Vector3 smoothz = Vector3.Lerp(transform.eulerAngles, rotationDirection, Time.deltaTime);
        //transform.eulerAngles = smoothz;
        if(Input.GetKey(KeyCode.Space))
        {
            if (jumpPower < maxJump)
            {
                prepareJump = true;
                rbSprite.sprite = sprites[1];
                jumpPower += 2;
                
                
                if(virtualCamera.m_Lens.FieldOfView > maxCameraFox)
                {
                    virtualCamera.m_Lens.FieldOfView -= (smooth * Time.deltaTime);
                }

            }
        }
        else
        {
            if ((jumpPower > 0))
            {
                jumpPower -= Time.deltaTime * 1000;
            }
        }


        if(Input.GetKeyDown(KeyCode.E) && candeposit)
        {
            itemCount--;
            Debug.Log("Deposited one star now you have " + itemCount + " starts");
            starAnimator[itemCount].SetTrigger("chest");
            if(itemCount == 0) 
            {
                animGameClear.SetActive(true);
                pressto.SetActive(false);
            }
        }
        if(Input.GetKeyUp(KeyCode.Space))
        {
            prepareJump = false;
            isGrounded = false;
            rb.AddForce(transform.up * jumpPower, ForceMode.Force);
            rb.AddForce(transform.right * (transform.localScale.x > 0 ? jumpPower : -jumpPower), ForceMode.Force);
            sliderimg.transform.parent.GetComponent<Animator>().SetBool("vibrating", false);
            rbSprite.sprite = sprites[2];
            jumpEffect.Play();
           
        }

        if(isGrounded && !prepareJump)
        {
            rbSprite.sprite = sprites[0];
        }

        if (virtualCamera.m_Lens.FieldOfView < currentCameraFov && !prepareJump)
        {
            virtualCamera.m_Lens.FieldOfView += (smooth * 3  * Time.deltaTime);
        }

        slider.value = jumpPower;
        sliderimg.fillAmount = jumpPower / 500;
        if(sliderimg.fillAmount >= 1) 
        {
            sliderimg.transform.parent.GetComponent<Animator>().SetBool("vibrating", true);
        }


    }

    private void OnTriggerEnter(Collider other)
    {



        if (other.gameObject.tag == "rotate180")//rotate 180
        {
            rb.velocity = Vector3.zero;
            rbSprite.sprite = sprites[3];

            StopAllCoroutines();
         
            newYAngle = oldYAngle + 180f;
            StartCoroutine(Rotate(newYAngle));
            StartCoroutine(SlowTime());
            oldYAngle = newYAngle;
            Destroy(other.gameObject);
        }
        else if(other.gameObject.tag == "star")
        {
            starAnimator[itemCount].SetTrigger("start");
            itemCount++;
            Destroy(other.gameObject);
        }
        else if(other.gameObject.tag == "rotate90")//rotate 90
        {
            rbSprite.sprite = sprites[3];
            rb.velocity = Vector3.zero;

            StopAllCoroutines();
            newYAngle = oldYAngle + 90;
            StartCoroutine(Rotate(newYAngle));
            StartCoroutine(SlowTime());
            oldYAngle = newYAngle;
            Destroy(other.gameObject);
        }
        else if (other.gameObject.tag == "rotate-90")//rotate 90
        {
            rbSprite.sprite = sprites[3];

            rb.velocity = Vector3.zero;
            StopAllCoroutines();
            newYAngle = oldYAngle - 90;
            StartCoroutine(Rotate(newYAngle));
            StartCoroutine(SlowTime());
            oldYAngle = newYAngle;
            Destroy(other.gameObject);
        }
        else if (other.gameObject.tag == "pressto")
        {
            pressto.SetActive(true);
            candeposit = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "pressto")
        {
            pressto.SetActive(false);
            candeposit = false;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "floor")
        {
            isGrounded = true;
            fallEffect.Play();
        }
        if (collision.gameObject.tag == "fallaftertouch")
        {
            isGrounded = true;
            fallEffect.Play();
            collision.gameObject.GetComponent<Animator>().enabled = true;
        }

    }
    
    IEnumerator Rotate(float targetAngle)
    {
        while (transform.rotation.z != targetAngle)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, 0f, targetAngle), 5f * Time.deltaTime);
            yield return null;
        }
        transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);

        yield return null;
       
    }

    IEnumerator SlowTime()
    {
        timestop = true;
        Time.timeScale = 0.1555555f;

        yield return new WaitForSeconds(0.1f);

        Time.timeScale = 1f;
        timestop = false;
    }


}
