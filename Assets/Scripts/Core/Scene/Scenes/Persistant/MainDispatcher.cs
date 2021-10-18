using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class MainDispatcher : MonoBehaviour
{
	private static int MAX_ACTIONS_PER_FRAME = 100;

	private static MainDispatcher _instance = null;
	public static MainDispatcher Instance
	{
		get
		{
			if (_instance != null)
				return _instance;

			Debug.LogWarning("Thread dispatcher is null.");

			return null;
		}

		private set => _instance = value;
	}

	private static readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();

	void Awake()
	{
		if (_instance == null)
			_instance = this;
	}

	void OnDestroy()
	{
		_instance = null;
	}

	public void Update()
	{
		int i = 0;
		while (_executionQueue.TryDequeue(out var action) && i++ < MAX_ACTIONS_PER_FRAME)
			action?.Invoke();
	}

	public void Enqueue(IEnumerator action)
	{
		_executionQueue.Enqueue(() => {
			StartCoroutine(action);
		});
	}

	public void Enqueue(Action action)
	{
		Enqueue(Coroutines.Immediate(action));
	}

	public Task EnqueueAsync(Action action)
	{
		var tcs = new TaskCompletionSource<bool>();

		void WrappedAction()
		{
			try
			{
				action();
				tcs.TrySetResult(true);
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		}

		Enqueue(Coroutines.Immediate(WrappedAction));
		return tcs.Task;
	}
}
