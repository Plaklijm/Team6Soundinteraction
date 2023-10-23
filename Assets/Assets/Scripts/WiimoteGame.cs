﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using System;
using UnityEngine.Serialization;
using WiimoteApi;

public class WiimoteGame : MonoBehaviour {

    public WiimoteModel model;

    private Quaternion initial_rotation;

    private Wiimote wiimote;

    private Vector2 scrollPosition;

    private Vector3 wmpOffset = Vector3.zero;

    void Start()
    {
        initial_rotation = model.mainRot.localRotation;
        
        // ENABLE DEBUG MESSAGES
        WiimoteManager.Debug_Messages = false;
        
        WiimoteManager.FindWiimotes();

        wiimote = WiimoteManager.Wiimotes[0];
        
        if (wiimote == null) { return; }

        // Set LED
        wiimote.SendPlayerLED(true, false, false, false);

        wiimote.ActivateWiiMotionPlus();

        wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_EXT8);
        wiimote.SetupIRCamera(IRDataType.BASIC);

        if (wiimote.current_ext == ExtensionController.MOTIONPLUS)
        {
            Debug.Log(wiimote.MotionPlus);
            MotionPlusData data = wiimote.MotionPlus;
            data.SetZeroValues();
        }
        
        // Register Data transfer

        
        model.mainRot.rotation = initial_rotation;
    }

    void Update () 
    {
        if (!WiimoteManager.HasWiimote()) { return; }

        wiimote = WiimoteManager.Wiimotes[0];

        int ret;
        do
        {
            Debug.Log("Do while");
            ret = wiimote.ReadWiimoteData();

            if (ret > 0 && wiimote.current_ext == ExtensionController.MOTIONPLUS) {
                Vector3 mainOffset = new Vector3(  0,
                                                -wiimote.MotionPlus.YawSpeed,
                                                0) / 95f; // Divide by 95Hz (average updates per second from wiimote)
                wmpOffset += mainOffset;
                
                //Debug.Log(wiimote.MotionPlus.YawSpeed);
                model.mainRot.Rotate(mainOffset, Space.Self);
            }
        } while (ret > 0);
        
        if (wiimote.Button.b && wiimote.MotionPlus.PitchSpeed > 20 && !wiimote.MotionPlus.PitchSlow)
        {
            Debug.Log("Throw fishing line");
        }

        if (wiimote.Button.a && wiimote.MotionPlus.PitchSpeed < -20 && !wiimote.MotionPlus.PitchSlow)
        {
            Debug.Log("Reel fishing line in");
        }

        /*model.a.enabled = wiimote.Button.a;
        model.b.enabled = wiimote.Button.b;
        model.one.enabled = wiimote.Button.one;
        model.two.enabled = wiimote.Button.two;
        model.d_up.enabled = wiimote.Button.d_up;
        model.d_down.enabled = wiimote.Button.d_down;
        model.d_left.enabled = wiimote.Button.d_left;
        model.d_right.enabled = wiimote.Button.d_right;
        model.plus.enabled = wiimote.Button.plus;
        model.minus.enabled = wiimote.Button.minus;
        model.home.enabled = wiimote.Button.home;*/

        if (wiimote.current_ext != ExtensionController.MOTIONPLUS)
            model.mainRot.localRotation = initial_rotation;
    }

    private Vector3 GetAccelVector()
    {
        float accel_x;
        float accel_y;
        float accel_z;

        float[] accel = wiimote.Accel.GetCalibratedAccelData();
        accel_x = accel[0];
        accel_y = -accel[2];
        accel_z = -accel[1];

        return new Vector3(accel_x, accel_y, accel_z).normalized;
    }

    void OnGUI()
    {
        GUIStyle bold = new GUIStyle(GUI.skin.button);
        bold.fontStyle = FontStyle.Bold;
        
        if (wiimote.current_ext == ExtensionController.MOTIONPLUS)
        {
            MotionPlusData data = wiimote.MotionPlus;
            if (GUILayout.Button("Reset Controller"))
            {
                data.SetZeroValues();
                wmpOffset = Vector3.zero;
            }
        }
    }

    [System.Serializable]
    public class WiimoteModel
    {
        public Transform mainRot;
        public Transform hengelRot;
        public Renderer a;
        public Renderer b;
        public Renderer one;
        public Renderer two;
        public Renderer d_up;
        public Renderer d_down;
        public Renderer d_left;
        public Renderer d_right;
        public Renderer plus;
        public Renderer minus;
        public Renderer home;
    }

	void OnApplicationQuit() {
		if (wiimote != null) {
			WiimoteManager.Cleanup(wiimote);
	        wiimote = null;
		}
	}
}
