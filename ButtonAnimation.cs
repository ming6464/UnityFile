using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour
{
    [SerializeField] Transform tf;
    [SerializeField] Ease ease = Ease.Linear;
    [SerializeField] LoopType loopType = LoopType.Yoyo;
    [SerializeField] float speed = 1f;
    [SerializeField] bool isScale;
    [SerializeField] Vector2 targetScale;
    [SerializeField] bool isScaleWhenClick;
    [SerializeField] bool autoShow;
    [SerializeField] float timeDelay;
    [SerializeField] GameObject objActive;

    [SerializeField] bool lightEffect;
    [SerializeField] float minX, maxX;
    [SerializeField] bool playSoundClick = true;
    Button button;
    private void Start()
    {
        LoadInit();
    }
    private void OnEnable()
    {
        button = GetComponent<Button>();
        if (autoShow)
        {
            objActive.SetActive(false);
            button.interactable = false;
            Invoke("Show", timeDelay);
        }
    }
    void LoadInit()
    {
        if (tf == null)
            tf = transform;
        
        if (isScale)
        {
            tf.localScale = Vector2.one;
            tf.DOScale(targetScale, speed).SetEase(ease).SetLoops(-1, loopType);
        }
        if (isScaleWhenClick)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate
            {
                if(playSoundClick)
                    AudioManager.PlaySound("Click UI");
                tf.DOScale(targetScale, speed).SetEase(ease).OnComplete(delegate
                {
                    tf.DOScale(Vector2.one, .1f);
                });
            });
        }
        if (lightEffect)
        {
            tf.localPosition = new Vector3(minX, 0, 0);
            Invoke("ShowFx", Random.Range(0, 3f));
        }
    }
    public void Scale()
    {
        tf.DOScale(targetScale, speed).SetEase(ease).OnComplete(delegate
        {
            tf.DOScale(Vector2.one, .1f);
        });
    }
    void ShowFx()
    {
        tf.DOLocalMoveX(maxX, speed).SetLoops(-1, loopType);
    }
    void Show()
    {
        objActive.SetActive(true);
        button.interactable = true;
    }
}
