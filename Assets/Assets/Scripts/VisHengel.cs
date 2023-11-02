using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using JSAM;
using WiimoteApi;

public class VisHengel : MonoBehaviour
{
    private SphereCollider sCol;
    public Transform EndTransform;
    public Transform StartTransform;
    private Vector3 startPos;
    private Vector3 endPos;
    private int score;
    private bool canMoveFloat;
    private Vias currentFish;
    private Wiimote wiimoteRumble;

    public float mashDelay = .75f;

    private float mash;
    private bool pressed;
    private bool mashToFish;
    
    private void Awake()
    {
        sCol = GetComponentInChildren<SphereCollider>();
        sCol.enabled = false;
        AudioManager.PlayMusic(AudioLibraryMusic.Boat);
    }

    // Start is called before the first frame update
    void Start()
    {
        mash = mashDelay;
        startPos = StartTransform.position;
        mashToFish = false;
    }

    public void SetWiiMote(Wiimote mote)
    {
        wiimoteRumble = mote; 
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    private void Update()
    {
        
    }

    public void MashButton(bool a)
    {
        if (mashToFish)
        {
            mash -= Time.deltaTime;
            Debug.Log("start mashing");
            if (a && !pressed)
            {
                Debug.Log("mashing");
                pressed = true;
                mash = mashDelay;
            }
            else if (!a)
            {
                Debug.Log("unmashing");
                pressed = false;
            }

            if (mash <= 0)
            {
                Debug.Log(" not fast enough");
                mashToFish = false;
            }
        }
    }

    public void ThrowFishingLine()
    {
        sCol.transform.localPosition = startPos;
        StartCoroutine(ThrowLine());
    }

    private IEnumerator ThrowLine()
    {
        Debug.Log("Throw fishing line");
        if (Random.Range(0,10) > 4)
        {
            AudioManager.PlaySound(AudioLibrarySounds.ManThrow);
        }
        
        AudioManager.PlaySound(AudioLibrarySounds.Throw);
        AudioManager.PlaySound(AudioLibrarySounds.Whoosh);
        while (AudioManager.IsSoundPlaying(AudioLibrarySounds.Throw))
            yield return null;
        AudioManager.PlaySound(AudioLibrarySounds.Plons, sCol.transform);
        canMoveFloat = true;
        sCol.enabled = true;
        StartCoroutine(MoveFloatBack());
    }

    private IEnumerator MoveFloatBack()
    {
        AudioManager.PlaySound(AudioLibrarySounds.Reel);
        startPos = StartTransform.position;
        endPos = EndTransform.position;
        float elapsedTime = 0;
        float waitTime = 5f;
        
        while (elapsedTime < waitTime) 
        {
            if (currentFish)
            {
                mashToFish = true;
                AudioManager.StopSound(AudioLibrarySounds.Reel);
                StartCoroutine(Rumble());
                yield break;
                
            }
            sCol.transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;


            yield return null;
        }
        AudioManager.StopSound(AudioLibrarySounds.Reel);
        sCol.transform.position = endPos;
        yield return null;
    }

    private IEnumerator Rumble()
    {
        wiimoteRumble.RumbleOn = true;
        wiimoteRumble.SendWithType(OutputDataType.STATUS_INFO_REQUEST, new byte[] { 0x00 });
        yield return new WaitForSeconds(.75f);
        wiimoteRumble.RumbleOn = false;
        wiimoteRumble.SendWithType(OutputDataType.STATUS_INFO_REQUEST, new byte[] { 0x00 });
    }

    public void ReelFishingLineIn()
    {
        if (currentFish)
        {
            score = currentFish.score;
            switch (currentFish.score)
            {
                    case < 3:
                        AudioManager.PlaySound(AudioLibrarySounds.LowTierFish);
                        break;
                    case < 7:
                        AudioManager.PlaySound(AudioLibrarySounds.MidTierFish);
                        break;
                    case > 8:
                        AudioManager.PlaySound(AudioLibrarySounds.HighTierFish);
                        break;
                    default:
                        break;
                    
            }
        }

        AudioManager.PlaySound(AudioLibrarySounds.Whoosh);
        
        //VisManager.Instance.CatchFish(currentFish.gameObject);
        canMoveFloat = false;
        sCol.enabled = false;
        StopBliep();
        Debug.Log("Reel fishing line in");
        currentFish = null;
    }
    
    public void MoveFishingFloat(Vector2 _input)
    {
        if (canMoveFloat)
        {
            Vector3 currentPosition = sCol.transform.localPosition;
            Vector3 newPosition = currentPosition + new Vector3(_input.x, 0, _input.y) / 7.5f;

            float maxXDistance = 60.0f; // Maximum allowed distance X axis
            float maxYDistance = 50.0f; // Maximum allowed distance Y axis
        
            if (Mathf.Abs(newPosition.x - startPos.x) <= maxXDistance && Mathf.Abs(newPosition.z - startPos.z) <= maxYDistance)
            {
                Debug.Log("move");
                sCol.transform.localPosition = newPosition;
                if (_input.magnitude > 0)
                {
                    if (!AudioManager.IsSoundPlaying(AudioLibrarySounds.Reel))
                    {
                        AudioManager.PlaySound(AudioLibrarySounds.Reel);
                        if (Random.Range(0, 1000) > 930 && !AudioManager.IsSoundPlaying(AudioLibrarySounds.ManSearching))
                        {
                            AudioManager.PlaySound(AudioLibrarySounds.ManSearching);
                        }
                    }
                }
                else
                {
                    AudioManager.StopSound(AudioLibrarySounds.Reel);
                }
            }
            else
            {
                if (AudioManager.IsSoundPlaying(AudioLibrarySounds.Reel))
                {
                    AudioManager.StopSound(AudioLibrarySounds.Reel);
                }
            }
        }
    }


    private void OnTriggerStay(Collider other)
    {
        currentFish = other.GetComponent<Vias>();
        Debug.Log(currentFish);
        if (!currentFish) return;
        
        float distance = Vector3.Distance(other.transform.position, sCol.transform.position);

        AudioLibrarySounds selectedAudioSource;

        switch (distance)
        {
            case <= 1.25f:
                selectedAudioSource = AudioLibrarySounds.BliepClose;
                break;
            case <= 2.5f:
                selectedAudioSource = AudioLibrarySounds.BliepMidClose;
                break;
            case <= 5.0f:
                selectedAudioSource = AudioLibrarySounds.BliepMid;
                break;
            case <= 12.5f:
                selectedAudioSource = AudioLibrarySounds.BliepMidFar;
                break;
            default:
                selectedAudioSource = AudioLibrarySounds.BliepFar;
                break;
        }
        
        PlayAudio(selectedAudioSource);
    }

    private void OnTriggerExit(Collider other)
    {
        currentFish = null;
        StopBliep();
    }

    private void PlayAudio(AudioLibrarySounds audio)
    {
        if (!AudioManager.IsSoundPlaying(audio))
        {
            StopBliep();
            AudioManager.PlaySound(audio);
        }
    }

    private void StopBliep()
    {
        AudioManager.StopSound(AudioLibrarySounds.BliepClose);
        AudioManager.StopSound(AudioLibrarySounds.BliepMidClose);
        AudioManager.StopSound(AudioLibrarySounds.BliepMid);
        AudioManager.StopSound(AudioLibrarySounds.BliepMidFar);
        AudioManager.StopSound(AudioLibrarySounds.BliepFar);
    }
    
}
