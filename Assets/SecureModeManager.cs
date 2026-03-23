using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SecureModeManager - New Level (Level 6: Memory Safe Maze)
/// 
/// Inspired by CWE-119: Improper Restriction of Operations Within the Bounds
/// of a Memory Buffer. In this level, the robot must navigate a maze without
/// ever attempting to move into a wall (out-of-bounds tile). If the robot tries
/// to move forward when no valid tile exists ahead, it triggers a "buffer
/// overflow" violation — the level resets and the violation counter increments.
/// 
/// This teaches students that in safe programming, you must validate before
/// you act — not just react to failure after the fact.
/// 
/// SETUP IN UNITY INSPECTOR:
///   - Attach this script to an empty GameObject called "SecureModeManager"
///   - Assign the violationCountText UI Text element
///   - Assign the violationPanel (shown briefly on violation)
///   - Assign the resetButton reference
///   - Set maxViolations (default 3 — after 3 violations, level fully resets)
/// </summary>
public class SecureModeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text violationCountText = null;
    [SerializeField] private GameObject violationPanel = null;
    [SerializeField] private Text violationMessageText = null;

    [Header("Settings")]
    [SerializeField] private int maxViolations = 3;
    [Tooltip("How long (seconds) the violation panel stays visible")]
    [SerializeField] private float violationDisplayTime = 2.0f;

    [Header("References")]
    [SerializeField] private ResetButton resetButton = null;

    // Tracks how many out-of-bounds moves were attempted this run
    private int violationCount = 0;

    // Whether secure mode is currently active
    private bool secureModeActive = true;

    void Start()
    {
        violationCount = 0;
        UpdateViolationUI();

        if (violationPanel != null)
            violationPanel.SetActive(false);
    }

    /// <summary>
    /// Called by SecureTile when the robot attempts to walk off the defined
    /// grid boundary (i.e. a "memory violation").
    /// </summary>
    public void RegisterViolation()
    {
        if (!secureModeActive) return;

        violationCount++;
        UpdateViolationUI();

        Debug.Log($"[SecureMode] Violation #{violationCount} — Out of bounds move attempted.");

        // Show feedback panel
        if (violationPanel != null)
        {
            string msg = $"Violation #{violationCount}: Out-of-bounds move blocked!\n" +
                         $"In safe programming, always check bounds before acting.\n" +
                         $"(CWE-119)";
            if (violationMessageText != null)
                violationMessageText.text = msg;

            StopAllCoroutines();
            StartCoroutine(ShowViolationPanel());
        }

        // Trigger reset
        if (resetButton != null)
            resetButton.resetScene();

        // If max violations hit, show a stronger warning (optional extension)
        if (violationCount >= maxViolations)
        {
            Debug.Log("[SecureMode] Maximum violations reached. Consider showing a hint.");
            // You could disable the play button here or show a hint panel
        }
    }

    /// <summary>
    /// Returns the current number of violations — used by LevelManager
    /// when calculating star rating at level completion.
    /// 0 violations = 3 stars, 1 = 2 stars, 2+ = 1 star
    /// </summary>
    public int GetViolationCount()
    {
        return violationCount;
    }

    /// <summary>
    /// Resets violation count — call this when the player restarts the level
    /// from the level complete screen.
    /// </summary>
    public void ResetViolations()
    {
        violationCount = 0;
        UpdateViolationUI();
    }

    private void UpdateViolationUI()
    {
        if (violationCountText != null)
            violationCountText.text = $"Violations: {violationCount}/{maxViolations}";
    }

    private IEnumerator ShowViolationPanel()
    {
        violationPanel.SetActive(true);
        yield return new WaitForSeconds(violationDisplayTime);
        violationPanel.SetActive(false);
    }
}
