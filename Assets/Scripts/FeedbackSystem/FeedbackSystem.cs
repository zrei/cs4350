using UnityEngine;

public interface IFeedback
{
    void Play();
    void Stop();
}

public class FeedbackSystem : MonoBehaviour, IFeedback
{
    IFeedback[] feedbacks;

    private void Awake()
    {
        feedbacks = GetComponentsInChildren<IFeedback>();
    }

    public void Play()
    {
        foreach (IFeedback feedback in feedbacks)
        {
            if (feedback != null)
            {
                feedback.Play();
            }
        }
    }

    public void Stop()
    {
        foreach (IFeedback feedback in feedbacks)
        {
            if (feedback != null)
            {
                feedback.Stop();
            }
        }
    }
}
