using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LoadingScene : MonoBehaviour
{
    public static LoadingScene Instance {get => ins;}
    private static LoadingScene ins;
    
    [SerializeField] private Slider _sliderLoading;

    public static readonly string LoadingScene = "LoadingScene";
    public static readonly string MainMenuScene = "MainMenuScene";
    public static readonly string PlayingScene = "PlayingScene";


    private void Awake()
    {
        if (ins == null) ins = this;
    }

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
        if (!_sliderLoading) yield break;
        float ratioSliderValue = _sliderLoading.maxValue;

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
    }
    
    public void ResetLoad()
    {
        _sliderLoading.value = 0;
    }
}
