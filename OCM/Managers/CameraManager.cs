using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

// ReSharper disable InconsistentNaming
#pragma warning disable CS0618 // Type or member is obsolete

namespace OCM.Managers;

public class CameraManager : MonoBehaviour {
    public static CameraManager? Instance { get; private set; }

    public enum CameraMode {
        FirstPerson,
        ThirdPerson,
        FollowPlayer,
        Tablet,
    }
    public CameraMode Camera_Mode = CameraMode.Tablet;

    public Camera? Camera;
    
    public  RenderTexture? Texture;
    private Camera?        displayCamera;

    private Vector3    cameraPosition;
    private Quaternion cameraRotation;

    public Vector3 CameraOffset = new(0, 0, 0);
    
    public float FOV       = 90;
    public float NearClip  = 0.01f;
    public float Smoothing = 0.10f;
    public bool  RollLock;
    public bool  Flip;
    public bool  HideHead;

    private Vector3 defaultHeadScale;
    
    private void Awake() {
        Instance = this;
        
        StartCoroutine(OnLoad());

        defaultHeadScale = VRRig.LocalRig.headMesh.transform.localScale;
    }

    private IEnumerator OnLoad() {
        while (GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera") == null) yield return null;
        
        Camera = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera")
                           .GetComponent<Camera>();

        Camera.transform.Find("CM vcam1")
              .GetComponent<CinemachineVirtualCamera>()
              .enabled = false;

        Camera.fieldOfView   = FOV;
        Camera.nearClipPlane = NearClip;
        
        SetupRealView();
    }

    private void SetupRealView() {
        Texture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        Texture.Create();

        GameObject placeholder = new();
        displayCamera               = placeholder.AddComponent<Camera>();
        displayCamera.targetTexture = Texture;
    }

    private void LateUpdate() => Controller();

    private void Controller() {
        if (Camera                 == null || displayCamera                 == null ||
            TabletManager.Instance == null || TabletManager.Instance.Tablet == null ||
            !Plugin.Instance.FullyInitialized) return;
        
        Camera.fieldOfView   = FOV;
        Camera.nearClipPlane = NearClip;

        VRRig local = VRRig.LocalRig;

        local.headMesh.transform.localScale = HideHead ? 
                                                      Vector3.zero : 
                                                      defaultHeadScale;
        
        switch (Camera_Mode) {
            case CameraMode.FirstPerson:
                Transform player = GameObject
                                  .Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/Main Camera/Camera Follower")
                                  .transform;

                Quaternion rotation = RollLock
                                              ? Quaternion.Euler(player.transform.rotation.eulerAngles.x,
                                                      player.transform.rotation.eulerAngles.y, 0)
                                              : player.rotation;

                cameraPosition = player.position +
                                 player.forward * CameraOffset.z +
                                 player.up * CameraOffset.y;

                cameraRotation = rotation;

                Camera.transform.position = cameraPosition;
                Camera.transform.rotation = Quaternion.Lerp(Camera.transform.rotation, cameraRotation, Smoothing);
                
                break;
            case CameraMode.ThirdPerson:
                Transform head = local.headMesh.transform;

                float tracking = head.eulerAngles.x;

                Quaternion rot = Quaternion.Euler(tracking, head.eulerAngles.y, 0);

                Vector3 offset = rot           * Vector3.back * (1.75f * local.scaleFactor) +
                                 Vector3.right * (-0.25f               * local.scaleFactor);

                Vector3 position = local.transform.position + offset + local.transform.up * 0.20f;

                Quaternion look = Quaternion.LookRotation(local.transform.position - position);

                Camera.transform.position = Vector3.Lerp(Camera.transform.position, position, Smoothing);
                Camera.transform.rotation = Quaternion.Lerp(Camera.transform.rotation, look, Smoothing);
                
                break;
            case CameraMode.FollowPlayer:
                Vector3 headPos = local.headMesh.transform.position;
                float   dist    = Vector3.Distance(Camera.transform.position, headPos);

                Vector3 tabletTop = TabletManager.Instance.Tablet.transform.position +
                                    TabletManager.Instance.Tablet.transform.rotation * new Vector3(-0.10f, 0, 0.10f);

                Camera.transform.position = tabletTop;
                Camera.transform.rotation = TabletManager.Instance.Tablet.transform.rotation *
                                            Quaternion.Euler(180, 90, 90);

                if (dist > 2.50f)
                    TabletManager.Instance.Tablet.transform.position =
                            Vector3.Lerp(TabletManager.Instance.Tablet.transform.position, local.transform.position,
                                    0.025f);

                Quaternion tRot = Quaternion.LookRotation(headPos - TabletManager.Instance.Tablet.transform.position) *
                                  Quaternion.Euler(-90, 0, 90);
    
                TabletManager.Instance.Tablet.transform.rotation = Quaternion.Lerp(
                        TabletManager.Instance.Tablet.transform.rotation, tRot, Smoothing);

                break;
            case CameraMode.Tablet:
                Vector3 fix = TabletManager.Instance.Tablet.transform.rotation * new Vector3(-0.10f, 0, 0.10f);
                
                Camera.transform.position = TabletManager.Instance.Tablet.transform.position + fix;
                Camera.transform.rotation = TabletManager.Instance.Tablet.transform.rotation *
                                            Quaternion.Euler(180, 90, 90);
                
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
        
        displayCamera.transform.rotation = Camera.transform.rotation;
        displayCamera.transform.position = Camera.transform.position;

        displayCamera.fieldOfView = FOV;
        displayCamera.nearClipPlane = NearClip;
    }
}