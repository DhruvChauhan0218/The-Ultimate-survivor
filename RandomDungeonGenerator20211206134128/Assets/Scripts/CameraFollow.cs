using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraFollow : MonoBehaviour
{
    #region Declarations

    public Transform targetToFollow; // Target object, which the camera should follow
    public float smoothing; // Value responsible for intensity of smoothing effect

    // Camera bound positions
    //private Vector2 minPos, maxPos;

    //[SerializeField] private Tilemap tilemap;

    #endregion

    #region Monobehaviour Callbacks

    private void Awake()
    {
        //minPos.x = tilemap.localBounds.min.x / 1.65f;
        //minPos.y = tilemap.localBounds.min.y / 1.77f;

        //maxPos.x = tilemap.localBounds.max.x / 1.65f;
        //maxPos.y = tilemap.localBounds.max.y / 1.77f;
    }

    void LateUpdate()
    {
        // Linearly interpolate from current position to player's position
        // For camera to render the view, it has to be at a certain distance from the objects, and therefore
        // to make sure it stays at a fixed distance from objects, we are setting the Z position of camera to -10.
        if(targetToFollow && transform.position != targetToFollow.position)
        {
            // Position holder, that could be used to modify positions as required.
            Vector3 targetPos = new Vector3(targetToFollow.position.x, targetToFollow.position.y, -10f);

            // Set the Bounds
            //targetPos.x = Mathf.Clamp(targetPos.x, minPos.x, maxPos.x);
            //targetPos.y = Mathf.Clamp(targetPos.y, minPos.y, maxPos.y);

            // Update Position
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothing);
        }
    }

    #endregion
}
