using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;

public class ThreadedDataRequester : MonoBehaviour
{
	static ThreadedDataRequester instance =null;
	Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

	private void Awake()
	{
		if(instance == null)
		{
			//instance = FindObjectOfType<ThreadedDataRequester>();
			instance = this;
		}
	}

	public static void RequestData(Func<object> generateData, Action<object> callBack)
	{
		ThreadStart threadStart = delegate { instance.DataThread(generateData, callBack); };

		new Thread(threadStart).Start();
	}

	void DataThread(Func<object> generateData, Action<object> callBack)
	{
		object data = generateData();
			//HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine, heightMapSettings, center);
		lock (dataQueue)
		{
			dataQueue.Enqueue(new ThreadInfo(callBack, data));
		}
	}


	private void Update()
	{
		if (dataQueue.Count > 0)
		{
			for (int i = 0; i < dataQueue.Count; i++)
			{
				ThreadInfo threadInfo = dataQueue.Dequeue();
				threadInfo.callBack(threadInfo.parameter);
			}
		}

	}

	struct ThreadInfo
	{
		public readonly Action<object> callBack;
		public readonly object parameter;

		public ThreadInfo(Action<object> callBack, object parameter)
		{
			this.callBack = callBack;
			this.parameter = parameter;
		}
	}
}
