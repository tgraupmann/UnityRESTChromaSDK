﻿using ChromaSDK;
using ChromaSDK.Api;
using ChromaSDK.ChromaPackage.Model;
using System.Collections.Generic;
using UnityEngine;

// Unity 3.X doesn't like namespaces
[ExecuteInEditMode]
public class ChromaSDKAnimation2D : ChromaSDKBaseAnimation
{
    public AnimationCurve Curve = new AnimationCurve();

    public ChromaDevice2DEnum Device = ChromaDevice2DEnum.Keyboard;

    public List<EffectArray2dInput> Frames = new List<EffectArray2dInput>();

    public delegate void ChomaOnComplete2D(ChromaSDKAnimation2D animation);

    // Callback when animation completes
    private ChomaOnComplete2D _mOnComplete = null;

    // Effects needs to be loaded before the animation can be played
    private bool _mIsLoaded = false;

    private bool _mIsPlaying = false;
    private float _mTime = 0f;
    private int _mCurrentFrame = 0;
    private List<EffectResponseId> _mEffects = new List<EffectResponseId>();

    /// <summary>
    /// Instance of the API
    /// </summary>
    private ChromaApi _mApiChromaInstance = null;

    /// <summary>
    /// Play the animation
    /// </summary>
    public void Play(ChromaApi chromaApi)
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        _mApiChromaInstance = chromaApi;

        if (!_mIsLoaded)
        {
            Debug.LogError("Play Animation has not been loaded!");
            return;
        }

        // clear on play to avoid unsetting on a loop
        _mOnComplete = null;

        _mTime = 0.0f;
        _mIsPlaying = true;
        _mCurrentFrame = 0;

        if (_mCurrentFrame < _mEffects.Count)
        {
            Debug.Log("SetEffect.");
            EffectResponseId effect = _mEffects[_mCurrentFrame];
            EffectIdentifierResponse result = ChromaUtils.SetEffect(_mApiChromaInstance, effect.Id);
            if (null == result ||
                result.Result != 0)
            {
                Debug.LogError("Play: Failed to set effect!");
            }
        }
    }

    /// <summary>
    /// Play the animation and fire the OnComplete event
    /// </summary>
    /// <param name="onComplete"></param>
    public void PlayWithOnComplete(ChromaApi chromaApi, ChomaOnComplete2D onComplete)
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        _mApiChromaInstance = chromaApi;

        if (!_mIsLoaded)
        {
            Debug.LogError("Animation has not been loaded!");
            return;
        }

        _mOnComplete = onComplete;

        _mTime = 0.0f;
        _mIsPlaying = true;
        _mCurrentFrame = 0;

        if (_mCurrentFrame < _mEffects.Count)
        {
            Debug.Log("SetEffect.");
            EffectResponseId effect = _mEffects[_mCurrentFrame];
            EffectIdentifierResponse result = ChromaUtils.SetEffect(_mApiChromaInstance, effect.Id);
            if (result.Result != 0)
            {
                Debug.LogError("Failed to set effect!");
            }
        }
    }

    /// <summary>
    /// Stop the animation
    /// </summary>
    public void Stop()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());
        _mIsPlaying = false;
        _mTime = 0.0f;
        _mCurrentFrame = 0;
    }

    /// <summary>
    /// Is the animation currently playing?
    /// </summary>
    /// <returns></returns>
    public bool IsPlaying()
    {
        return _mIsPlaying;
    }

    /// <summary>
    /// Load the effects before playing
    /// </summary>
    public void Load(ChromaApi chromaApi)
    {
        _mApiChromaInstance = chromaApi;

        if (_mIsLoaded)
        {
            Debug.LogError("Animation has already been loaded!");
            return;
        }

        for (int i = 0; i < Frames.Count; ++i)
        {
            EffectArray2dInput frame = Frames[i];
            EffectResponseId effect = ChromaUtils.CreateEffectCustom2D(_mApiChromaInstance, Device, frame);
            if (effect.Result != 0)
            {
                Debug.LogError("Failed to create effect!");
            }
            _mEffects.Add(effect);
        }

        _mIsLoaded = true;
    }

    /// <summary>
    /// Check if the animation has loaded
    /// </summary>
    /// <returns></returns>
    public bool IsLoaded()
    {
        return _mIsLoaded;
    }

    /// <summary>
    /// Unload the effects
    /// </summary>
    public void Unload(ChromaApi chromaApi)
    {
        _mApiChromaInstance = chromaApi;
        if (!_mIsLoaded)
        {
            Debug.LogError("Animation has already been unloaded!");
            return;
        }

        for (int i = 0; i < _mEffects.Count; ++i)
        {
            EffectResponseId effect = _mEffects[i];
            EffectIdentifierResponse result = ChromaUtils.RemoveEffect(_mApiChromaInstance, effect.Id);
            if (result.Result != 0)
            {
                Debug.LogError("Failed to delete effect!");
            }
        }
        _mEffects.Clear();
        _mIsLoaded = false;
    }

    private float GetTime(int index)
    {
        if (index >= 0 &&
        index < Curve.keys.Length)
        {
            return Curve.keys[index].time;
        }
        return 0.033f;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        _mTime += deltaTime;
        float nextTime = GetTime(_mCurrentFrame);
        if (nextTime < _mTime)
        {
            ++_mCurrentFrame;
            if (_mCurrentFrame < _mEffects.Count)
            {
                if (null != _mApiChromaInstance)
                {
                    Debug.Log("SetEffect.");
                    EffectResponseId effect = _mEffects[_mCurrentFrame];
                    EffectIdentifierResponse result = ChromaUtils.SetEffect(_mApiChromaInstance, effect.Id);
                    if (result.Result != 0)
                    {
                        Debug.LogError("Failed to set effect!");
                    }
                }
            }
            else
            {
                //UE_LOG(LogTemp, Log, TEXT("UChromaSDKPluginAnimation2DObject::Tick Animation Complete."));
                _mIsPlaying = false;
                _mTime = 0.0f;
                _mCurrentFrame = 0;

                // execute the complete event if set
                if (null != _mOnComplete)
                {
                    _mOnComplete.Invoke(this);
                }
            }
        }
    }
}
