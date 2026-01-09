using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Image fillImage;
    private float duration;
    private float timer;
    private bool running;

    void Update()
    {
        if (!running) return;

        timer += Time.deltaTime;
        fillImage.fillAmount = 1 - (timer / duration);

        if (timer >= duration)
            running = false;
    }

    public void StartProgress(float time)
    {
        duration = time;
        timer = 0f;
        running = true;
        fillImage.fillAmount = 1;
    }
}
