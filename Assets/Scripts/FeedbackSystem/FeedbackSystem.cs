using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IFeedback
{
    bool IsPlaying { get; }

    void Play();
    void Stop();
}

public class FeedbackSystem : MonoBehaviour, IFeedback
{
    public List<IFeedback> feedbacks;

    public bool IsPlaying { get => feedbacks.Any(x => x.IsPlaying); }

    private void Awake()
    {
        feedbacks = new(GetComponentsInChildren<IFeedback>());
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
