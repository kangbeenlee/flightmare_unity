using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.PostProcessing;

//[RequireComponent(typeof(PostProcessingBehaviour))]

// Array ops
using System.Linq;

namespace MessageSpec
{

  // =============================
  // INTERNAL Message definitions
  // =============================
  // For storing unity's internal state
  // E.g. are objects initialized, should this frame be rendered, etc.
  public class UnityState_t
  {

    public Dictionary<string, Camera> camera_filters = new Dictionary<string, Camera>() { };

    // Initialization status
    public int initializationStep { get; set; } = 0;
    public int screenSkipFrames { get; set; } = 0;

    // List of landmark objects in the environment
    public Dictionary<string, GameObject> landmarkObjects;
    public Vector3 uavCenterOfMass;

    // Convenience getter function.
    public bool initialized { get { return (initializationStep < 0); } }
    public bool readyToRender { get { return (initialized && (screenSkipFrames == 0)); } }

    private Dictionary<string, ObjectState_t> objects;

    // Advanced getters/setters
    // Get Wrapper object, defaulting to a passed in template if it does not exist.
    public ObjectState_t getWrapperObject(string ID, GameObject template)
    {
      if (!objects.ContainsKey(ID))
      {
        // Create and save object from template
        objects[ID] = new ObjectState_t(template);
      }
      return objects[ID];
    }

    // Get Wrapper object, defaulting to a passed in template if it does not exist.
    public ObjectState_t getWrapperObject(string ID, string prefab_ID)
    {
      if (!objects.ContainsKey(ID))
      {
        // Create and save object from template
        GameObject template = Resources.Load(prefab_ID) as GameObject;
        objects[ID] = new ObjectState_t(template);
      }
      return objects[ID];
    }

    // Get gameobject from wrapper object
    public GameObject getGameobject(string ID, GameObject template)
    {
      return getWrapperObject(ID, template).gameObj;
    }
    public GameObject getGameobject(string ID, string templateID)
    {
      return getWrapperObject(ID, templateID).gameObj;
    }

    // Check if object is initialized
    public bool isInitialized(string ID)
    {
      bool isInitialized = false;
      if (objects.ContainsKey(ID))
      {
        isInitialized = objects[ID].initialized;
      }
      return isInitialized;
    }
    // Constructor
    public UnityState_t()
    {
      objects = new Dictionary<string, ObjectState_t>() { };
    }
  }

  // Keeps track of gameobjects and their initialization and instantiation.
  public class ObjectState_t
  {
    public bool initialized { get; set; } = false;
    public GameObject gameObj { get; set; }
    public GameObject template { get; set; }
    // public PostProcessingProfile postProcessingProfile { get; set; }
    // Constructor
    public ObjectState_t(GameObject template)
    {
      this.gameObj = GameObject.Instantiate(template);
      this.template = template;
    }

  }

  // Camera class for decoding the ZMQ messages.
  public class Camera_t
  {
    public string ID { get; set; }
    // Metadata
    public int channels { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public float fov { get; set; }
    public List<float> nearClipPlane { get; set; }
    public List<float> farClipPlane { get; set; }
    public float depthScale { get; set; }
    // transformation matrix
    public List<float> T_BC { get; set; }
    public bool isDepth { get; set; }
    public int outputIndex { get; set; }
    public List<bool> enabledLayers { get; set; }
    // Additional getters
    public bool isGrayscale { get { return (channels == 1) && (!isDepth); } }
  }

  public class Lidar_t
  {
    public string ID { get; set; }
    public int num_beams;
    public float max_distance;
    public float start_angle;
    public float end_angle;
    // transformation matrix with respect to center
    // of the vehicle
    public List<float> T_BS { get; set; }
  }

  // public class Vehicle_t
  // {
  //   public string ID { get; set; }
  //   public List<float> position { get; set; }
  //   public List<float> rotation { get; set; }
  //   public List<float> size { get; set; }
  //   public List<Camera_t> cameras { get; set; }
  //   public List<Lidar_t> lidars;
  //   public bool hasCollisionCheck = true;
  //   public bool hasVehicleCollision = false;
  // }

  public class Target_t
  {
    public string ID { get; set; }
    public List<float> position { get; set; }
    public List<float> rotation { get; set; }
    public List<float> size { get; set; }
    public List<Camera_t> cameras { get; set; }
    public List<Lidar_t> lidars;
    public bool hasCollisionCheck = true;
    public bool hasVehicleCollision = false;
  }

  public class Tracker_t
  {
    public string ID { get; set; }
    public List<float> position { get; set; }
    public List<float> rotation { get; set; }
    public List<float> size { get; set; }
    public List<Camera_t> cameras { get; set; }
    public List<Lidar_t> lidars;
    public bool hasCollisionCheck = true;
    public bool hasVehicleCollision = false;
  }

  // Generic object class for decoding the ZMQ messages.
  public class Object_t
  {
    public string ID { get; set; }
    public string prefabID { get; set; }
    public List<float> position { get; set; }
    public List<float> rotation { get; set; }
    // Metadata
    public List<float> size { get; set; }
  }

  // =============================
  // INCOMING Message definitions
  // =============================
  public class SettingsMessage_t
  {
    // Startup parameters. 
    // public bool sceneIsInternal { get; set; }
    public int scene_id { get; set; }

    // Object state update
    // public List<Vehicle_t> vehicles { get; set; }
    public List<Target_t> targets { get; set; }
    public List<Tracker_t> trackers { get; set; }
    public List<Object_t> objects { get; set; }
    // public List<Landmark_t> landmarksInView { get; set; } = new List<Landmark_t>(); // Must be initialized or will segfault.

    // ==============================================================================
    // Additional getters (for convenience)
    // ==============================================================================
    // public int numVehicles { get { return vehicles.Count(); } }
    public int numTargets { get { return targets.Count(); } }
    public int numTrackers { get { return trackers.Count(); } }
    // public Vehicle_t mainVehicle { get; set; }
    public Tracker_t mainTracker { get; set; }
    // we noly count the number of camera on the main vehicle. 
    public int numCameras { get; set; }
    public Camera_t mainTrackerCamera { get; set; }
    public Camera_t worldCamera { get; set; }
    // public List<Camera_t> cameras{ get; set; }
    public int camHeight { get; set; }
    public int camWidth { get; set; }
    public int screenWidth { get; set; }
    public int screenHeight { get; set; }
    public int worldCamHeight { get; set; }
    public int worldCamWidth { get; set; }
    public int worldScreenWidth { get; set; }
    public int worldScreenHeight { get; set; }
    public bool sceneIsDefault { get; set; }
    public void InitParamsters()
    {
      // World camera setting
      worldCamWidth = 1024;
      worldCamHeight = 764;
      worldScreenWidth = worldCamWidth;
      worldScreenHeight = worldCamHeight;

      // kind of ugly, the purpose is to handle the tracker that has no cameras. 
      if (numTrackers > 0)
      {
        mainTracker = trackers[(int)(numTrackers / 2)];
      }
      numCameras = mainTracker.cameras.Count();
      if (numCameras == 0)
      {
        camWidth = 1024;
        camHeight = 764;
        screenWidth = camWidth;
        screenHeight = camHeight;
      }
      else if (numCameras >= 1)
      {
        mainTrackerCamera = mainTracker.cameras[0];
        camWidth = mainTrackerCamera.width;
        camHeight = mainTrackerCamera.height;
        // enlarge the width if the main camera is stereo camera
        // screenWidth = camWidth * numCameras;
        screenWidth = camWidth;
        screenHeight = camHeight;
      }
    }
  }
  public class SubMessage_t
  {
    public Int64 frame_id { get; set; }
    // Object state update
    // public List<Vehicle_t> vehicles { get; set; }
    public List<Target_t> targets { get; set; }
    public List<Tracker_t> trackers { get; set; }
    public List<Object_t> objects { get; set; }
    // ==============================================================================
    // Additional getters (for convenience)
    // ==============================================================================
    // public Vehicle_t mainVehicle { get { return vehicles[(int)(vehicles.Count / 2)]; } }
    public Tracker_t mainTracker { get { return trackers[(int)(trackers.Count / 2)]; } }
  }

  // public class Pub_Vehicle_t
  // {
  //   public bool collision;
  //   public List<float> lidar_ranges;
  // }

  public class Pub_Target_t
  {
    public bool collision;
    public List<float> lidar_ranges;
  }

  public class Pub_Tracker_t
  {
    public bool collision;
    public List<float> lidar_ranges;
  }

  // =============================
  // OUTGOING Message definitions
  // =============================
  public class PubMessage_t
  {
    public Int64 frame_id { get; set; }
    // public List<Pub_Vehicle_t> pub_vehicles;
    public List<Pub_Target_t> pub_targets;
    public List<Pub_Tracker_t> pub_trackers;
    public PubMessage_t(SettingsMessage_t settings)
    {
      pub_targets = new List<Pub_Target_t>();
      foreach (var target_t in settings.targets)
      {
        var pub_i = new Pub_Target_t();
        pub_i.collision = false;
        pub_i.lidar_ranges = new List<float>();
        pub_targets.Add(pub_i);
      }
      pub_trackers = new List<Pub_Tracker_t>();
      foreach (var tracker_t in settings.trackers)
      {
        var pub_i = new Pub_Tracker_t();
        pub_i.collision = false;
        pub_i.lidar_ranges = new List<float>();
        pub_trackers.Add(pub_i);
      }
    }
  }
  public class ReadyMessage_t
  {
    public bool ready { get; set; }
    public ReadyMessage_t(bool r)
    {
      ready = r;
    }
  }

  public class PointCloudMessage_t
  {
    public List<float> range { get; set; }
    public List<float> origin { get; set; }
    public float resolution { get; set; }
    public string path { get; set; }
    public string file_name { get; set; }
  }

}
