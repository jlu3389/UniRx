﻿using UnityEngine;
using System.Collections;
using UniRx;
using System.Threading;
using System;

// test sandbox
public class NewBehaviourScript : ObservableMonoBehaviour
{
    IDisposable cancel;

    public override void Awake()
    {
        cancel = Observable.EveryUpdate()
            .Subscribe(_ => Debug.Log(DateTime.Now.ToString()));

        base.Awake();
    }

    public override void OnMouseDown()
    {
        Debug.Log("Start MouseDown");

        Observable.EveryUpdate()
            .Subscribe(_ => Debug.Log(DateTime.Now.ToString()));


        //Observable.Interval(TimeSpan.FromSeconds(3))

        //.ObserveOnMainThread()

        ObservableWWW.Get("http://google.co.jp/")
            .Subscribe(
                x => Debug.Log(x), // success
                ex => Debug.LogException(ex)); // onError


        // composing asynchronous sequence with LINQ query expressions
        var query = from google in ObservableWWW.Get("http://google.co.jp/")
                    from bing in ObservableWWW.Get("http://bing.co.jp/")
                    from unknown in ObservableWWW.Get(google + bing)
                    select new { google, bing, unknown };

        var cancel = query.Subscribe(x => Debug.Log(x));

        // Call Dispose is cancel.
        cancel.Dispose();


        // Observable.WhenAll is for parallel asynchronous operation
        // (It's like Observable.Zip but specialized for single async operations like Task.WhenAll)
        var parallel = Observable.WhenAll(
                ObservableWWW.Get("http://google.com/"),
                ObservableWWW.Get("http://bing.com/"),
                ObservableWWW.Get("http://yahoo.com/"));

        parallel.Subscribe(xs =>
        {
            Debug.Log(xs[0]); // google
            Debug.Log(xs[1]); // bing
            Debug.Log(xs[2]); // yahoo
        });


        // work on specified scheduler, default is ThreadPool
        var heavyMethod = Observable.Start(() =>
        {
            // heavy method...
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            return 10;
        });

        var heavyMethod2 = Observable.Start(() =>
        {
            // heavy method...
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
            return 10;
        });

        // Join and await two other thread values
        Observable.WhenAll(heavyMethod, heavyMethod2)
            .ObserveOnMainThread() // return to main thread
            .Subscribe(xs =>
            {
                // Unity can't touch GameObject from other thread
                // but use ObserveOnMainThread, you can touch GameObject naturally.
                (GameObject.Find("myGuiText")).guiText.text = xs[0] + ":" + xs[1];
            });







        // notifier for progress
        var progressNotifier = new ScheduledNotifier<float>();
        progressNotifier.Subscribe(x => Debug.Log(x)); // write www.progress

        // pass notifier to WWW.Get/Post
        ObservableWWW.Get("http://google.com/", progress: progressNotifier).Subscribe();





        //Observable.Return(10)
        //    .DelayFrame(180)
        //    .Subscribe(x =>
        //    {
        //        (GameObject.Find("myGuiText")).guiText.text = DateTime.Now.ToString();
        //    });

        //var a = Observable.Start(() =>
        //{
        //    //Thread.Sleep(TimeSpan.FromSeconds(1));
        //    return 100;
        //});

        //var b = Observable.Start(() =>
        //{
        //    //Thread.Sleep(TimeSpan.FromSeconds(2));
        //    return 200;
        //});

        //var notification = new UniRx.ScheduledNotifier<float>(Scheduler.MainThread);
        //notification.Subscribe(x => Debug.Log(x));

        //Observable.Zip(a, b, (x, y) =>
        //{
        //    return new { x, y };
        //})
        //.Zip(ObservableWWW.Get("http://google.co.jp/", progress: notification), (x, y) => y)
        //.ObserveOnMainThread()
        //.Subscribe(x =>
        //{
        //    Debug.Log("Subscribe Start");
        //    //var p = this.transform.position;
        //    //var np = this.transform.TransformPoint(p.x + 0.5f, p.y + 0.5f, p.z);
        //    //this.transform.position = np;

        //    (GameObject.Find("myGuiText")).guiText.text = Guid.NewGuid() + x;

        //    Debug.Log("End Transform");
        //});

        //Debug.Log("End MouseDown");


        base.OnMouseDown();
    }
}
