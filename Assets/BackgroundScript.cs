using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public CanvasGroup Canvases;
    private float _TransitionDirection = 0;
    public Logic.Timer Transition = new(10, 0,true);
    [Range(0,1f)] public float MinAlpha = 0;
    [Range(0, 1f)] public float MaxAlpha = 0;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _TransitionDirection = 1;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _TransitionDirection = 0;
    }


    // Update is called once per frame
    void Update()
    {
        Transition.Step(Time.deltaTime * ((_TransitionDirection == 0) ? -1 : 1));
        Canvases.alpha = Mathf.Lerp(MinAlpha, MaxAlpha, Transition.GetRatio());
    }
}
