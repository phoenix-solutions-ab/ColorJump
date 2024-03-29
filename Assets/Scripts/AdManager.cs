﻿using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdManager : MonoBehaviour
{
    public int gameOverCounter;

    public static AdManager instance;

    public string appId = "ca-app-pub-7325257911988484~7798253333";
    public string bannerUnitId = "ca-app-pub-7325257911988484/1238791832";
    public string fullscreenUnitId = "ca-app-pub-7325257911988484/5738425574";

    private BannerView bannerView;
    private InterstitialAd interstitialAd;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;        
        
        MobileAds.Initialize(appId);

        RequestBanner();

        interstitialAd = new InterstitialAd(fullscreenUnitId);
        AdRequest request = new AdRequest.Builder().AddTestDevice("d9ac1fc0b431150e881264fd73db780f").Build();
        
        interstitialAd.LoadAd(request);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode1)
    {
        //if (scene.name != "MainMenu")
        //{
        //    bannerView.Destroy();
        //}
    }

    public void RequestFullscreenAd()
    {
        gameOverCounter++;

        if (gameOverCounter % 10 == 3)
        {
            Debug.Log("REQUEST FULLSCREEN AD!");
            interstitialAd.Show();
        }
    }

    private void RequestBanner()
    {
        Debug.Log("Request banner!");

        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(bannerUnitId, AdSize.Banner, AdPosition.Top);

        // Called when an ad request has successfully loaded.
        bannerView.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        bannerView.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is clicked.
        bannerView.OnAdOpening += HandleOnAdOpened;
        // Called when the user returned from the app after an ad click.
        bannerView.OnAdClosed += HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        bannerView.OnAdLeavingApplication += HandleOnAdLeavingApplication;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().AddTestDevice("d9ac1fc0b431150e881264fd73db780f").Build();

        // Load the banner with the request.
        bannerView.LoadAd(request);

        bannerView.Show();

        Debug.Log("Banner loaded");
    }

    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        Debug.Log("HandleAdLoaded event received");
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log("HandleFailedToReceiveAd event received with message: "
                            + args.Message);
    }

    public void HandleOnAdOpened(object sender, EventArgs args)
    {
        Debug.Log("HandleAdOpened event received");
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        Debug.Log("HandleAdClosed event received");
    }

    public void HandleOnAdLeavingApplication(object sender, EventArgs args)
    {
        Debug.Log("HandleAdLeavingApplication event received");
    }
}
