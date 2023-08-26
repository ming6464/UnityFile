using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CanvasGroup))]
public class LoadingTrasitionSceneUI : MonoBehaviour
{
    public static LoadingTrasitionSceneUI Ins {get => ins;}
    private static LoadingTrasitionSceneUI ins;
    
    [SerializeField] private Slider _sliderLoading;

    public static readonly string LoadingScene = "LoadingScene";
    public static readonly string MainMenuScene = "MainMenuScene";
    public static readonly string PlayingScene = "PlayingScene";


    private void Awake()
    {
        if (ins == null) ins = this;
    }
    private CanvasGroup _canvasGroup;

    
    public void LoadingToScene(String name)
    {
        ResetLoad();
        StartCoroutine(Loading(name));
    }

    private void Start()
    {
        ResetLoad();
    }

    public IEnumerator Loading(String name)
    {
        if (!_sliderLoading || _canvasGroup == null) yield break;
        float ratioSliderValue = _sliderLoading.maxValue;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;
        _canvasGroup.alpha = 1;

        var asyncScene = SceneManager.LoadSceneAsync(name);
        asyncScene.allowSceneActivation = false;

        while (asyncScene.isDone == false)
        {
            var progres = asyncScene.progress;

            if ((progres - _sliderLoading.value) > .1f)
            {
                progres = (progres + _sliderLoading.value) / 2f;
            }
            
            _sliderLoading.value = progres * ratioSliderValue;
            
            if(progres >= 0.8999f) break;
            
            yield return null;
        }

        _sliderLoading.value = ratioSliderValue;
        asyncScene.allowSceneActivation = true;
        yield return new WaitForSeconds(.1f);
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
        _canvasGroup.alpha = 0;
    }
    
    
    private IEnumerator UpdateLoading(String name)
    {
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;
        _canvasGroup.alpha = 1;
        var asyncOperation = SceneManager.LoadSceneAsync(name);
        asyncOperation.allowSceneActivation = false;
        while (!asyncOperation.isDone)
        {
            float progress = asyncOperation.progress;

            _sliderLoading.value = progress;

            yield return null;
        }
        asyncOperation.allowSceneActivation = true;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;
        _canvasGroup.alpha = 0;
    }

    public void ResetLoad()
    {
        _sliderLoading.value = 0;
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0;
    }
}