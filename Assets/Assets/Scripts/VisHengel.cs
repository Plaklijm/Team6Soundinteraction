using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using JSAM;

public class VisHengel : MonoBehaviour
{
    private SphereCollider sCol;
    private Vector3 initialPos;
    private int score;
    private bool canMoveFloat;
    private Vias currentFish;

    private void Awake()
    {
        sCol = GetComponentInChildren<SphereCollider>();
        sCol.enabled = false;
        AudioManager.PlayMusic(AudioLibraryMusic.Boat);
    }

    // Start is called before the first frame update
    void Start()
    {
        initialPos = sCol.transform.localPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }


    public void ThrowFishingLine()
    {
        sCol.transform.localPosition = initialPos;
        StartCoroutine(ThrowLine());
        sCol.enabled = true;
    }

    IEnumerator ThrowLine()
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
    }

    public void ReelFishingLineIn()
    {
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
            
        }
        VisManager.Instance.CatchFish(currentFish.gameObject);
        canMoveFloat = false;
        sCol.enabled = false;
        StopBliep();
        Debug.Log("Reel fishing line in");
        Debug.Log(score);
        AudioManager.PlaySound(AudioLibrarySounds.CaughtFish);
    }
    
    public void MoveFishingFloat(Vector2 _input)
    {
        if (canMoveFloat)
        {
            Vector3 currentPosition = sCol.transform.localPosition;
            Vector3 newPosition = currentPosition + new Vector3(_input.x, 0, _input.y) / 15;

            float maxXDistance = 60.0f; // Maximum allowed distance X axis
            float maxYDistance = 50.0f; // Maximum allowed distance Y axis
        
            if (Mathf.Abs(newPosition.x - initialPos.x) <= maxXDistance && Mathf.Abs(newPosition.z - initialPos.z) <= maxYDistance)
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
            case <= 10.0f:
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
