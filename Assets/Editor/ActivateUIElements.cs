using UnityEngine;
using UnityEditor;

public class ActivateUIElements : EditorWindow
{
    [MenuItem("Tools/Activate Challenge UI")]
    public static void ActivateChallengeUI()
    {
        // 查找Canvas下的UI元素
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            // 激活挑战相关的UI元素
            Transform progressText = canvas.transform.Find("ProgressText");
            if (progressText != null)
            {
                progressText.gameObject.SetActive(true);
                Debug.Log("ProgressText activated");
            }

            Transform progressSlider = canvas.transform.Find("ProgressSlider");
            if (progressSlider != null)
            {
                progressSlider.gameObject.SetActive(true);
                Debug.Log("ProgressSlider activated");
            }

            Transform upcomingNotesText = canvas.transform.Find("UpcomingNotesText");
            if (upcomingNotesText != null)
            {
                upcomingNotesText.gameObject.SetActive(true);
                Debug.Log("UpcomingNotesText activated");
            }

            Transform countdownText = canvas.transform.Find("CountdownText");
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                Debug.Log("CountdownText activated");
            }

            Transform scoreText = canvas.transform.Find("ScoreText");
            if (scoreText != null)
            {
                scoreText.gameObject.SetActive(true);
                Debug.Log("ScoreText activated");
            }

            Transform noteDisplayText = canvas.transform.Find("NoteDisplayText");
            if (noteDisplayText != null)
            {
                noteDisplayText.gameObject.SetActive(true);
                Debug.Log("NoteDisplayText activated");
            }

            Transform octaveDisplayText = canvas.transform.Find("OctaveDisplayText");
            if (octaveDisplayText != null)
            {
                octaveDisplayText.gameObject.SetActive(true);
                Debug.Log("OctaveDisplayText activated");
            }

            Transform keyDisplayText = canvas.transform.Find("KeyDisplayText");
            if (keyDisplayText != null)
            {
                keyDisplayText.gameObject.SetActive(true);
                Debug.Log("KeyDisplayText activated");
            }

            Debug.Log("Challenge UI elements activation completed!");
        }
        else
        {
            Debug.LogError("Canvas not found!");
        }
    }
}