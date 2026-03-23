using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BoundaryTile - Invisible trigger placed at the EDGE of valid tiles.
///
/// When the robot enters this zone, it means the player's code tried to move
/// the robot off the grid — an "out of bounds" operation. This triggers a
/// secure mode violation via SecureModeManager.
///
/// HOW TO USE IN UNITY:
///   1. Create an empty GameObject at each boundary edge of your level maze.
///   2. Add a BoxCollider2D set to "Is Trigger = true".
///   3. Attach this script.
///   4. Assign the SecureModeManager reference in the Inspector.
///
/// The robot will be blocked from moving further by the existing tileDetect()
/// raycast in robotD.cs — this script just registers the violation for UI/scoring.
/// </summary>
public class BoundaryTile : MonoBehaviour
{
    [SerializeField] private SecureModeManager secureModeManager = null;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "robot")
        {
            if (secureModeManager != null)
                secureModeManager.RegisterViolation();
            else
                Debug.LogWarning("BoundaryTile: SecureModeManager not assigned!");
        }
    }
}
